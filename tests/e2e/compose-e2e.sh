#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

cleanup() {
  docker compose down -v --remove-orphans || true
}

trap cleanup EXIT

wait_for_http() {
  local url="$1"
  local timeout_seconds="$2"
  local started_at
  started_at="$(date +%s)"

  until curl -fsS "$url" >/dev/null 2>&1; do
    if (( "$(date +%s)" - started_at >= timeout_seconds )); then
      echo "Timed out waiting for $url" >&2
      return 1
    fi

    sleep 2
  done
}

wait_for_internal_http() {
  local url="$1"
  local timeout_seconds="$2"
  local started_at
  started_at="$(date +%s)"

  until docker compose exec -T merchandise-services bash -lc "curl -fsS '$url' >/dev/null 2>&1"; do
    if (( "$(date +%s)" - started_at >= timeout_seconds )); then
      echo "Timed out waiting for internal URL $url" >&2
      return 1
    fi

    sleep 2
  done
}

docker compose down -v --remove-orphans || true
docker compose up -d --build

wait_for_http "http://localhost:8080/health/ready" 180
wait_for_http "http://localhost:8080/metrics" 180
wait_for_http "http://localhost:9090/-/ready" 180
wait_for_internal_http "http://grafana:3000/api/health" 180
wait_for_internal_http "http://loki:3100/ready" 180
wait_for_internal_http "http://tempo:3200/ready" 180
wait_for_internal_http "http://fluent-bit:2020/api/v1/metrics/prometheus" 180

SWAGGER_STATUS="$(curl -s -o /tmp/swagger.out -w '%{http_code}' http://localhost:8080/swagger)"
if [[ "$SWAGGER_STATUS" != "301" ]]; then
  echo "Expected /swagger to return 301 redirect, got $SWAGGER_STATUS." >&2
  cat /tmp/swagger.out >&2
  exit 1
fi

SWAGGER_SPEC="$(curl -fsS http://localhost:8080/swagger/v1/swagger.json)"
if [[ "$SWAGGER_SPEC" != *"\"openapi\""* ]]; then
  echo "Swagger spec is not available on /swagger/v1/swagger.json." >&2
  echo "$SWAGGER_SPEC" >&2
  exit 1
fi

PRESET_SQL_RESULT="$(docker compose exec -T merchandise-services-db psql -U postgres -d merchandise-services -At <<'SQL'
SELECT
  sp.id || '|' || string_agg(sps.sku_id::text, ',' ORDER BY sps.sku_id)
FROM sku_presets sp
INNER JOIN sku_preset_skus sps ON sps.sku_preset_id = sp.id
WHERE sp.preset_type_id = 4
GROUP BY sp.id
HAVING COUNT(*) FILTER (WHERE sps.sku_id IN (7, 13)) = 2
ORDER BY sp.id
LIMIT 1;
SQL
)"

if [[ -z "${PRESET_SQL_RESULT// }" ]]; then
  echo "Could not resolve probation preset for E2E scenario." >&2
  exit 1
fi

PRESET_ID="${PRESET_SQL_RESULT%%|*}"
SKU_IDS_CSV="${PRESET_SQL_RESULT#*|}"
EMAIL="shell-e2e-$(date +%s)@example.com"
MESSAGE_ID="shell-e2e-$(date +%s)"

docker compose exec -T merchandise-services-db psql -U postgres -d merchandise-services <<SQL
INSERT INTO merchandise_requests (sku_preset_id, merchandise_request_status, created_at, clothing_size, employee_email)
VALUES (${PRESET_ID}, 'processing', CURRENT_TIMESTAMP, 'XS', '${EMAIL}');
SQL

docker compose exec -T stock-api-db psql -U postgres -d stock-api <<SQL
UPDATE stocks
SET quantity = GREATEST(quantity, 10), minimal_quantity = 1
WHERE sku_id IN (${SKU_IDS_CSV});
SQL

docker compose exec -T broker bash -lc "printf '%s\n' '{\"type\":[{\"sku\":7,\"itemTypeId\":0,\"itemTypeName\":\"\",\"clothingSize\":null},{\"sku\":13,\"itemTypeId\":0,\"itemTypeName\":\"\",\"clothingSize\":null}]}' | kafka-console-producer --bootstrap-server broker:29092 --topic stock_replenished_event >/dev/null"

REQUEST_STATUS=""
for _ in {1..30}; do
  REQUEST_STATUS="$(docker compose exec -T merchandise-services-db psql -U postgres -d merchandise-services -tAc "SELECT merchandise_request_status FROM merchandise_requests WHERE employee_email = '${EMAIL}' ORDER BY id DESC LIMIT 1;")"
  REQUEST_STATUS="${REQUEST_STATUS// /}"

  if [[ "$REQUEST_STATUS" == "done" ]]; then
    break
  fi

  sleep 2
done

if [[ "$REQUEST_STATUS" != "done" ]]; then
  echo "Pending request did not transition to done." >&2
  echo "Last status: $REQUEST_STATUS" >&2
  exit 1
fi

OUTBOX_PROCESSED="$(docker compose exec -T merchandise-services-db psql -U postgres -d merchandise-services -tAc "SELECT COUNT(*) FROM integration_outbox_messages WHERE topic = 'email_notification_event' AND processed_on_utc IS NOT NULL;")"
if [[ "${OUTBOX_PROCESSED// /}" == "0" ]]; then
  echo "Outbox message for email_notification_event was not processed." >&2
  exit 1
fi

PUBLISHED_EVENT="$(docker compose exec -T broker bash -lc "kafka-console-consumer --bootstrap-server broker:29092 --topic email_notification_event --from-beginning --max-messages 1 --timeout-ms 20000")"
if [[ "$PUBLISHED_EVENT" != *"$EMAIL"* ]]; then
  echo "Did not observe email_notification_event for ${EMAIL}." >&2
  echo "$PUBLISHED_EVENT" >&2
  exit 1
fi

PROM_TARGETS="$(docker compose exec -T merchandise-services bash -lc "curl -fsS http://prometheus:9090/api/v1/targets")"
if [[ "$PROM_TARGETS" != *"merchandise-service"* ]] || [[ "$PROM_TARGETS" != *"fluent-bit"* ]] || [[ "$PROM_TARGETS" != *"health\":\"up\""* ]]; then
  echo "Prometheus is not scraping required targets successfully." >&2
  echo "$PROM_TARGETS" >&2
  exit 1
fi

METRICS_PAYLOAD="$(curl -fsS http://localhost:8080/metrics)"
if [[ "$METRICS_PAYLOAD" != *"http_requests_received_total"* ]]; then
  echo "Application metrics endpoint does not expose HTTP metrics." >&2
  exit 1
fi

LOKI_RESULT=""
for _ in {1..30}; do
  LOKI_RESULT="$(docker compose exec -T merchandise-services bash -lc "END=\$(date +%s); START=\$((END-600)); curl -fsS -G http://loki:3100/loki/api/v1/query_range --data-urlencode 'query={job=\"fluent-bit\"} |= \"OzonEdu.MerchandiseService\"' --data-urlencode limit=20 --data-urlencode start=\${START}000000000 --data-urlencode end=\${END}000000000" || true)"
  if [[ "$LOKI_RESULT" == *"OzonEdu.MerchandiseService"* ]]; then
    break
  fi

  sleep 2
done

if [[ "$LOKI_RESULT" != *"OzonEdu.MerchandiseService"* ]]; then
  echo "Loki did not receive merchandise-service logs from Fluent Bit." >&2
  echo "$LOKI_RESULT" >&2
  exit 1
fi

TRACE_ID="$(docker compose logs --no-color merchandise-services | grep -o '"TraceId":"[^"]*"' | tail -n1 | cut -d'"' -f4)"
if [[ -z "$TRACE_ID" ]]; then
  echo "Could not extract TraceId from merchandise-service logs." >&2
  exit 1
fi

TEMPO_TRACE=""
for _ in {1..30}; do
  TEMPO_TRACE="$(docker compose exec -T merchandise-services bash -lc "curl -fsS http://tempo:3200/api/traces/$TRACE_ID" || true)"
  if [[ "$TEMPO_TRACE" == *"\"batches\""* ]]; then
    break
  fi

  sleep 2
done

if [[ "$TEMPO_TRACE" != *"\"batches\""* ]]; then
  echo "Tempo did not return trace $TRACE_ID." >&2
  echo "$TEMPO_TRACE" >&2
  exit 1
fi

echo "Compose E2E passed."

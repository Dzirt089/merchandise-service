#!/bin/bash

set -euo pipefail

SERVICE_HTTP_URL="${SERVICE_HTTP_URL:-http://merchandise-services}"
JAEGER_URL="${JAEGER_URL:-http://jaeger:16686}"
PROMETHEUS_URL="${PROMETHEUS_URL:-http://prometheus:9090}"
ELASTICSEARCH_URL="${ELASTICSEARCH_URL:-http://elasticsearch:9200}"
STARTUP_TIMEOUT_SECONDS="${STARTUP_TIMEOUT_SECONDS:-180}"

wait_for_http() {
	local url="$1"
	local deadline=$((SECONDS + STARTUP_TIMEOUT_SECONDS))

	while (( SECONDS < deadline )); do
		if curl --silent --fail "$url" >/dev/null; then
			return 0
		fi
		sleep 3
	done

	echo "Timed out waiting for ${url}" >&2
	return 1
}

echo "Waiting for merchandise-service readiness"
wait_for_http "${SERVICE_HTTP_URL}/health/ready"

echo "Waiting for Jaeger"
wait_for_http "${JAEGER_URL}/"

echo "Waiting for Prometheus"
wait_for_http "${PROMETHEUS_URL}/-/ready"

echo "Waiting for Elasticsearch"
wait_for_http "${ELASTICSEARCH_URL}/_cluster/health"

dotnet test /src/tests/OzonEdu.MerchandiseService.E2ETests/OzonEdu.MerchandiseService.E2ETests.csproj -c Release --no-build --no-restore --logger "console;verbosity=normal"

#!/bin/bash

set -euo pipefail

DB_HOST="${DB_HOST:-merchandise-services-db}"
DB_PORT="${DB_PORT:-5432}"
HTTP_PORT="${HTTP_PORT:-80}"
STARTUP_TIMEOUT_SECONDS="${STARTUP_TIMEOUT_SECONDS:-120}"

wait_for_tcp() {
	local host="$1"
	local port="$2"
	local deadline=$((SECONDS + STARTUP_TIMEOUT_SECONDS))

	while (( SECONDS < deadline )); do
		if bash -c ":</dev/tcp/${host}/${port}" 2>/dev/null; then
			return 0
		fi
		sleep 2
	done

	echo "Timed out waiting for ${host}:${port}" >&2
	return 1
}

wait_for_http() {
	local url="$1"
	local deadline=$((SECONDS + STARTUP_TIMEOUT_SECONDS))

	while (( SECONDS < deadline )); do
		if curl --silent --fail "$url" >/dev/null; then
			return 0
		fi
		sleep 2
	done

	echo "Timed out waiting for ${url}" >&2
	return 1
}

echo "Waiting for Postgres at ${DB_HOST}:${DB_PORT}"
wait_for_tcp "$DB_HOST" "$DB_PORT"

echo "Running MerchandiseService migrations"
dotnet OzonEdu.MerchandiseService.Migrator.dll --no-build -v d

echo "Starting MerchandiseService"
dotnet OzonEdu.MerchandiseService.dll --no-build -v d &
APP_PID=$!

trap 'kill -TERM $APP_PID 2>/dev/null || true; wait $APP_PID' TERM INT

wait_for_http "http://127.0.0.1:${HTTP_PORT}/health/live"
wait $APP_PID

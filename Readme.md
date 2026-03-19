# merchandise-service

## Быстрый старт

Поднять локальный контур:

```bash
docker compose up -d --build
```

Остановить контур:

```bash
docker compose down
```

## Shell E2E

Основной e2e-путь теперь shell-based и совпадает по подходу с `stock-api`:

```bash
bash tests/e2e/compose-e2e.sh
```

Скрипт поднимает полный Docker-контур, проверяет HTTP, Kafka, Prometheus, Fluent Bit, Loki и Tempo, а затем удаляет окружение.

## Локальные проверки

- readiness: `curl http://localhost:8080/health/ready`
- metrics: `curl http://localhost:8080/metrics`
- HTTP список мерча: `curl http://localhost:8080/Merchandise/GetAllMerch`
- HTTP получение по `id`: `curl http://localhost:8080/Merchandise/GetById/1`
- логи сервиса: `docker compose logs -f merchandise-services`

## Observability URLs

- Swagger: `http://localhost:8080/swagger`
- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000`
- Loki API: `http://localhost:5310`
- Tempo API: `http://localhost:5320`

Grafana по умолчанию:

- логин: `admin`
- пароль: `admin`

## Порты

- `merchandise-service` HTTP: `8080`
- `merchandise-service` gRPC: `5062`
- `stock-api` HTTP: `5070`
- `stock-api` gRPC: `5072`
- Prometheus: `9090`
- Grafana: `3000`
- Loki: `5310`
- Tempo: `5320`
- Kafka broker: `9092`

## Что проверять в observability

В Prometheus:

- таргеты `merchandise-service` и `fluent-bit` должны быть `up`

В Grafana:

- datasource’ы `Prometheus`, `Loki`, `Tempo`
- dashboard `Merchandise Service Observability`

В Loki:

- логи `merchandise-service`, пришедшие через Fluent Bit

В Tempo:

- trace после HTTP или Kafka-driven сценария

## Secondary .NET E2E

`.NET` e2e-проект сохранён как дополнительный слой и может запускаться отдельно, если локально доступен `dotnet`:

```bash
dotnet test tests/OzonEdu.MerchandiseService.E2ETests/OzonEdu.MerchandiseService.E2ETests.csproj
```

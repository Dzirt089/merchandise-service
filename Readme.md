# merchandise-service

## Быстрый старт

Поднять весь Docker-контур:

```bash
docker compose up -d --build
```

Остановить контур:

```bash
docker compose down
```

Если нужно пересобрать только сервис и лог-шейпер:

```bash
docker compose up -d --build merchandise-services fluent-bit
```

## E2E-тесты

Собрать тестовый образ:

```bash
docker compose --profile test build test-runner
```

Запустить полный e2e-прогон:

```bash
docker compose --profile test run --rm test-runner
```

Тесты проверяют:

- HTTP endpoints
- gRPC endpoints
- взаимодействие с `stock-api`
- inbound Kafka события
- outbound Kafka события
- traces, metrics и logs внутри Docker-контура

## Что открывать в браузере

- Swagger: `http://localhost:8080/swagger`
- Jaeger: `http://localhost:16686`
- Kibana: `http://localhost:5601`
- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000`

Grafana по умолчанию:

- логин: `admin`
- пароль: `admin`

## Порты сервисов

- `merchandise-service` HTTP: `8080`
- `merchandise-service` gRPC: `5062`
- `stock-api` HTTP: `5070`
- `stock-api` gRPC: `5072`
- Elasticsearch: `9200`
- Kibana: `5601`
- Jaeger UI: `16686`
- Prometheus: `9090`
- Grafana: `3000`
- Kafka broker: `9092`

## Полезные проверки руками

Проверить readiness:

```bash
curl http://localhost:8080/health/ready
```

Проверить метрики:

```bash
curl http://localhost:8080/metrics
```

Проверить HTTP endpoint:

```bash
curl http://localhost:8080/Merchandise/GetAllMerch
```

Посмотреть логи сервиса:

```bash
docker compose logs -f merchandise-services
```

Посмотреть логи `stock-api`:

```bash
docker compose logs -f stock-api
```

Посмотреть логи shipper'а логов:

```bash
docker compose logs -f fluent-bit
```

## Что смотреть в observability

В Jaeger:

- HTTP traces сервиса
- gRPC server traces
- gRPC client вызовы в `stock-api`

В Kibana:

- индекс `merchandise-service-logs-*`

В Prometheus / Grafana:

- readiness и app metrics
- HTTP latency / request count
- runtime метрики процесса

## Примечания

- Kafka topics создаются контейнером `kafka-init`
- файловые логи сервиса пишутся в `./.docker/logs`
- e2e-тесты запускаются только против Docker-контура, не против локального `dotnet run`

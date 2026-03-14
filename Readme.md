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

## HTTP

Проверить список мерча:

```bash
curl http://localhost:8080/Merchandise/GetAllMerch
```

Проверить получение сущности по `id`:

```bash
curl http://localhost:8080/Merchandise/GetById/1
```

Для ручной проверки через браузер удобнее использовать Swagger:

- `http://localhost:8080/swagger`

## gRPC

gRPC сервис доступен на:

- `localhost:5062`

Что проверять:

- `GiveOutMerchandise`
- `GetRequestsByEmployee`

Внутри e2e это уже покрыто контейнером `test-runner`. Для ручной проверки можно использовать `grpcurl` или Postman с поддержкой gRPC.

## Kafka

Используемые топики:

- `stock_replenished_event`
- `employee_notification_event`
- `email_notification_event`

Что проверяется автоматически в e2e:

- входящий `employee_notification_event`
- входящий `stock_replenished_event`
- исходящий `email_notification_event`

Что проверять руками через логи:

```bash
docker compose logs -f merchandise-services
```

При необходимости можно смотреть состояние Kafka через контейнер `broker`.

## Traces

Открыть Jaeger:

- `http://localhost:16686`

Что должно быть видно:

- HTTP trace при вызове controller endpoint
- gRPC trace при вызове gRPC метода
- внутренние span'ы handler/repository
- исходящий gRPC client вызов в `stock-api`

Минимальный smoke-сценарий:

1. Открыть `http://localhost:8080/swagger`
2. Вызвать `GetAllMerch`
3. Перейти в Jaeger
4. Выбрать сервис `OzonEdu.MerchandiseService`
5. Найти свежий trace

## Logs

Открыть Kibana:

- `http://localhost:5601`

Индекс логов:

- `merchandise-service-logs-*`

Где лежат файловые логи сервиса локально:

- `./.docker/logs`

Что смотреть:

- HTTP request logs
- gRPC interceptor logs
- ошибки Kafka consumer/publisher
- correlation по `TraceId` и `SpanId`

## Metrics

Проверить endpoint метрик:

```bash
curl http://localhost:8080/metrics
```

Открыть Prometheus:

- `http://localhost:9090`

Открыть Grafana:

- `http://localhost:3000`

Что смотреть:

- `up` по сервису
- HTTP latency
- request count
- runtime/process metrics

Базовый smoke-сценарий:

1. Вызвать HTTP endpoint сервиса
2. Проверить `http://localhost:8080/metrics`
3. Убедиться, что Prometheus видит job `merchandise-service`
4. Открыть Grafana и проверить dashboard

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

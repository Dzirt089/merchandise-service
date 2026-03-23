# merchandise-service

`merchandise-service` отвечает за выдачу мерча сотрудникам и хранение истории заявок. Сервис работает в связке с `stock-api`: при выдаче мерча он проверяет наличие SKU, обрабатывает Kafka-driven события о пополнении склада и публикует уведомления через outbox.

Текущий сервис ориентирован на gRPC и событийную интеграцию. HTTP в проекте используется как инфраструктурный слой для health checks, metrics и Swagger.

## Что умеет сервис

- Выдать мерч сотруднику по gRPC через `GiveOutMerchandise`.
- Вернуть историю заявок сотрудника по gRPC через `GetRequestsByEmployee`.
- Обрабатывать входящий Kafka event `stock_replenished_event` и повторно завершать ожидающие заявки.
- Публиковать исходящий Kafka event `email_notification_event` через outbox publisher.
- Экспортировать health checks, Prometheus metrics, logs и traces в observability-контур.

## Технологии и зависимости

- `.NET 9`, ASP.NET Core, gRPC, MediatR
- PostgreSQL
- Kafka + ZooKeeper
- `stock-api` как внешний сервис проверки остатков
- OpenTelemetry Collector
- Prometheus
- Loki
- Tempo
- Grafana
- Fluent Bit
- Docker Compose для локального контура и shell E2E

## Структура проекта

- `src/OzonEdu.MerchandiseService` - основной ASP.NET/gRPC entrypoint
- `src/OzonEdu.MerchandiseService.Application` - команды, запросы, handlers
- `src/OzonEdu.MerchandiseService.Domain` - доменная модель
- `src/OzonEdu.MerchandiseService.DataAccess.EntityFramework` - EF Core слой
- `src/OzonEdu.Infrastructure` - репозитории и инфраструктурные реализации
- `src/OzonEdu.MerchandiseService.Migrator` - FluentMigrator migrations
- `tests/OzonEdu.MerchandiseService.DomainTests` - domain tests
- `tests/OzonEdu.MerchandiseService.E2ETests` - secondary .NET E2E
- `tests/e2e/compose-e2e.sh` - основной shell E2E сценарий
- `observability/` - конфиги Collector, Loki, Tempo, Grafana, Prometheus, Fluent Bit

## Внешние интерфейсы

### gRPC

Сервис публикует два основных RPC:

- `GiveOutMerchandise` - инициирует выдачу мерча сотруднику
- `GetRequestsByEmployee` - возвращает список заявок сотрудника

gRPC сервис описан в [`merchandise-service.proto`](/home/suoza/code/merchandise-service/src/OzonEdu.MerchandiseService.Grpc/merchandise-service.proto).

### Kafka

- входящий topic: `stock_replenished_event`
- исходящий topic: `email_notification_event`

### HTTP infrastructure endpoints

- readiness: `http://localhost:8080/health/ready`
- liveness: `http://localhost:8080/health/live`
- metrics: `http://localhost:8080/metrics`
- Swagger UI: `http://localhost:8080/swagger`
- Swagger spec: `http://localhost:8080/swagger/v1/swagger.json`

## Локальный запуск

Полный локальный контур поднимается одной командой:

```bash
docker compose up -d --build
```

Команда поднимает:

- `merchandise-service`
- PostgreSQL для `merchandise-service`
- `stock-api` и его PostgreSQL
- Kafka + ZooKeeper
- observability stack: OTel Collector, Prometheus, Loki, Tempo, Grafana, Fluent Bit

Остановить контур:

```bash
docker compose down
```

Если нужен полный сброс томов и состояния:

```bash
docker compose down -v --remove-orphans
```

## Быстрые проверки после старта

После запуска имеет смысл проверить:

- readiness:

```bash
curl http://localhost:8080/health/ready
```

- metrics:

```bash
curl http://localhost:8080/metrics
```

- Swagger:

```bash
curl http://localhost:8080/swagger/v1/swagger.json
```

- логи сервиса:

```bash
docker compose logs -f merchandise-services
```

Если readiness не проходит, сначала проверьте логи `merchandise-services`, затем состояние БД и Kafka зависимостей через `docker compose ps`.

## Основные порты

- `merchandise-service` HTTP: `8080`
- `merchandise-service` gRPC: `5062`
- `stock-api` HTTP: `5070`
- `stock-api` gRPC: `5072`
- PostgreSQL `merchandise-service`: `5436`
- PostgreSQL `stock-api`: `5426`
- Kafka broker: `9092`
- Prometheus: `9090`
- Grafana: `3000`
- Loki: `5310`
- Tempo: `5320`
- Fluent Bit metrics: `5240`

Grafana по умолчанию:

- логин: `admin`
- пароль: `admin`

## Тестирование

### Основной E2E путь

Основной acceptance-сценарий для проекта - shell E2E:

```bash
bash tests/e2e/compose-e2e.sh
```

Сценарий поднимает чистый compose-контур, проверяет:

- readiness `merchandise-service`
- HTTP infrastructure endpoints
- Kafka-driven retry flow
- публикацию `email_notification_event`
- scrape в Prometheus
- доставку логов в Loki
- наличие trace в Tempo

После завершения сценарий удаляет окружение.

### Secondary .NET E2E

`.NET` E2E оставлены как дополнительный слой:

```bash
dotnet test tests/OzonEdu.MerchandiseService.E2ETests/OzonEdu.MerchandiseService.E2ETests.csproj
```

Этот путь полезен, если локально установлен `dotnet` и нужен дополнительный интеграционный прогон поверх compose-зависимостей.

### Domain tests

Для быстрой локальной проверки доменной логики:

```bash
dotnet test tests/OzonEdu.MerchandiseService.DomainTests/OzonEdu.MerchandiseService.DomainTests.csproj
```

## Observability

Локальный observability-контур доступен сразу после `docker compose up -d --build`.

### Что смотреть

В Prometheus:

- `http://localhost:9090`
- targets `merchandise-service` и `fluent-bit` должны быть `up`

В Grafana:

- `http://localhost:3000`
- datasource'ы `Prometheus`, `Loki`, `Tempo`
- dashboard `Merchandise Service Observability`

В Loki:

- `http://localhost:5310`
- логи `OzonEdu.MerchandiseService`, пришедшие через Fluent Bit

В Tempo:

- `http://localhost:5320`
- trace после HTTP infrastructure запроса или Kafka-driven сценария

## Модель данных

Ключевые таблицы:

- `merchandise_requests` - заявки на выдачу мерча сотрудникам
- `sku_presets` и `sku_preset_skus` - пресеты выдачи и привязанные SKU
- `integration_outbox_messages` - исходящие интеграционные сообщения
- `integration_inbox_messages` - учёт обработанных входящих сообщений

Это важно для понимания retry flow и outbox-публикации уведомлений.

## Частые проблемы

### `health/ready` не отвечает

- проверьте `docker compose ps`
- проверьте логи `docker compose logs --tail=200 merchandise-services`
- убедитесь, что `merchandise-services-db`, `broker` и `stock-api` действительно поднялись

### Не проходит shell E2E

- убедитесь, что Docker daemon доступен
- убедитесь, что порты `8080`, `9090`, `3000`, `5310`, `5320`, `9092` не заняты
- повторите запуск на чистом контуре через `docker compose down -v --remove-orphans`

### `.NET` тесты не запускаются локально

В этом окружении `dotnet` должен быть установлен отдельно. Если его нет, используйте shell E2E как основной acceptance path.

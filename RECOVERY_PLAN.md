# Merchandise Service Migration Recovery Plan

Этот файл обязателен к чтению перед продолжением работы после любого сбоя, прерывания или нового запуска агента.

## Цель

Привести `merchandise-service` к operational standard, совместимому с текущим `stock-api`:

- `OpenTelemetry Collector`
- `Loki`
- `Tempo`
- `Prometheus`
- `Grafana`
- `Fluent Bit`
- shell E2E вместо `profile test` + `test-runner`

## Что уже сделано

- Удален `test-runner` stage из `Dockerfile`.
- Удален старый `entrypoint.e2e.sh`.
- В `docker-compose.yml` начата миграция с `Jaeger + Elasticsearch/Kibana` на `otel-collector + loki + tempo`.
- Из `docker-compose.yml` удален `test-runner` и `profile test`.
- `merchandise-services` и `stock-api` переведены на docker logging driver `fluentd`.
- `merchandise-services` переведен на конфиг `Telemetry__OtlpEndpoint`.
- В `HostBuilderExtensions` удален `ConsoleExporter`; OTLP endpoint теперь берется из `Telemetry:OtlpEndpoint` с fallback.
- Обновлены observability-конфиги:
  - `observability/fluent-bit/fluent-bit.conf`
  - `observability/prometheus/prometheus.yml`
  - `observability/grafana/provisioning/datasources/datasources.yml`
  - `observability/grafana/provisioning/dashboards/json/merchandise-service-overview.json`
- Добавлены новые файлы:
  - `observability/otel-collector-config.yaml`
  - `observability/loki/config.yaml`
  - `observability/tempo/tempo.yaml`
- Обновлен `merchandise-service.sln` под новые observability-файлы и удаление `entrypoint.e2e.sh`.

## Что делаю прямо сейчас

Этап 1 почти завершен: нужно проверить текущий diff на синтаксические/структурные ошибки в `docker-compose.yml` и связных конфигах, затем сделать промежуточный git commit.

## Что осталось сделать по плану

### 1. Завершить и зафиксировать observability migration

- Проверить `docker-compose.yml` после замены сервисов.
- Проверить, что Grafana mounts и datasource paths совпадают с реальными файлами.
- Проверить, что `stock-api` как зависимый сервис не остался с Jaeger-specific env.
- Сделать первый промежуточный git commit.

### 2. Добавить shell E2E

- Создать `tests/e2e/compose-e2e.sh`.
- Shell E2E должен:
  - поднимать compose-контур,
  - ждать readiness `merchandise-service`, `Prometheus`, `Grafana`, `Loki`, `Tempo`, `Fluent Bit`,
  - проверять HTTP endpoints,
  - проверять Kafka-driven сценарий,
  - проверять outbound Kafka event,
  - проверять scrape в Prometheus,
  - проверять попадание логов в Loki,
  - проверять retrieval trace из Tempo.
- Обновить workflow запуска в README.
- Сделать второй промежуточный git commit.

### 3. Адаптировать .NET E2E как secondary layer

- Обновить `tests/OzonEdu.MerchandiseService.E2ETests/MerchandiseEnvironmentFixture.cs`:
  - убрать Jaeger/Elasticsearch clients,
  - добавить Tempo/Loki clients,
  - заменить проверки observability на Tempo/Loki.
- Обновить `MerchandiseServiceE2ETests.cs`, если потребуется для новой observability semantics.
- Сделать третий промежуточный git commit.

### 4. Обновить документацию

- Переписать `Readme.md` под новый стек:
  - убрать Jaeger/Kibana/Elastic,
  - добавить Loki/Tempo и shell E2E,
  - обновить ручные smoke checks и observability URLs.
- При необходимости обновить команды и примеры.

### 5. Проверка и финализация

- Посмотреть `git diff --stat`.
- По возможности прогнать хотя бы доступные проверки/скрипты.
- Сформулировать оставшиеся риски, если среда не позволит выполнить полную проверку.

## Обязательное правило восстановления

Если работа прерывается, следующий запуск должен начать с чтения этого файла и сверки:

1. текущего `git status`
2. незакоммиченных изменений
3. последнего завершенного пункта из раздела `Что осталось сделать по плану`

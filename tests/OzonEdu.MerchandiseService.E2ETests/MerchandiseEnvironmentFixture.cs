using CSharpCourse.Core.Lib.Enums;
using CSharpCourse.Core.Lib.Events;
using CSharpCourse.Core.Lib.Models;
using System.Text.Json.Nodes;

namespace OzonEdu.MerchandiseService.E2ETests;

public sealed class MerchandiseEnvironmentFixture : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpClient HttpClient { get; }
    public HttpClient TempoClient { get; }
    public HttpClient PrometheusClient { get; }
    public HttpClient LokiClient { get; }
    public string DatabaseConnectionString { get; }
    public string KafkaBootstrapServers { get; }
    public string EmailNotificationTopic { get; } = "email_notification_event";
    public string EmployeeNotificationTopic { get; } = "employee_notification_event";
    public string StockReplenishedTopic { get; } = "stock_replenished_event";

    private MerchandiseServiceGrpc.MerchandiseServiceGrpcClient MerchandiseGrpcClient { get; }
    private StockApiGrpc.StockApiGrpcClient StockApiGrpcClient { get; }

    public MerchandiseEnvironmentFixture()
    {
        var serviceHttpUrl = Environment.GetEnvironmentVariable("SERVICE_HTTP_URL") ?? "http://merchandise-services";
        var serviceGrpcUrl = Environment.GetEnvironmentVariable("SERVICE_GRPC_URL") ?? "http://merchandise-services:82";
        var stockApiGrpcUrl = Environment.GetEnvironmentVariable("STOCK_API_GRPC_URL") ?? "http://stock-api:82";
        var tempoUrl = Environment.GetEnvironmentVariable("TEMPO_URL") ?? "http://tempo:3200";
        var prometheusUrl = Environment.GetEnvironmentVariable("PROMETHEUS_URL") ?? "http://prometheus:9090";
        var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL") ?? "http://loki:3100";

        DatabaseConnectionString = Environment.GetEnvironmentVariable("SERVICE_DB_CONNECTION_STRING")
            ?? "Host=merchandise-services-db;Port=5432;Database=merchandise-services;Username=postgres;Password=merchandiseServicesPassword";
        KafkaBootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "broker:29092";

        HttpClient = new HttpClient { BaseAddress = new Uri(serviceHttpUrl) };
        TempoClient = new HttpClient { BaseAddress = new Uri(tempoUrl) };
        PrometheusClient = new HttpClient { BaseAddress = new Uri(prometheusUrl) };
        LokiClient = new HttpClient { BaseAddress = new Uri(lokiUrl) };

        var merchChannel = GrpcChannel.ForAddress(serviceGrpcUrl);
        MerchandiseGrpcClient = new MerchandiseServiceGrpc.MerchandiseServiceGrpcClient(merchChannel);

        var stockChannel = GrpcChannel.ForAddress(stockApiGrpcUrl);
        StockApiGrpcClient = new StockApiGrpc.StockApiGrpcClient(stockChannel);
    }

    public async Task InitializeAsync()
    {
        await WaitForHttpAsync(() => HttpClient.GetAsync("/health/ready"), "merchandise-service readiness");
        await WaitForAsync(async () =>
        {
            try
            {
                await StockApiGrpcClient.GetAllStockItemsAsync(new Empty());
                return true;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromSeconds(60), "stock-api gRPC readiness");
        await WaitForHttpAsync(() => TempoClient.GetAsync("/ready"), "tempo");
        await WaitForHttpAsync(() => PrometheusClient.GetAsync("/-/ready"), "prometheus");
        await WaitForHttpAsync(() => LokiClient.GetAsync("/ready"), "loki");
        await WaitForAsync(async () =>
        {
            await EnsureSkuPresetsSeededAsync();
            return true;
        }, TimeSpan.FromSeconds(30), "seeded sku presets");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task<PresetCandidate> GetAvailablePresetAsync()
    {
        var stockItems = await StockApiGrpcClient.GetAllStockItemsAsync(new Empty());
        var stockQuantities = stockItems.Items.ToDictionary(x => x.Sku, x => x.Quantity);
        var candidates = await LoadPresetCandidatesAsync();

        var candidate = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                MinimumQuantity = candidate.SkuIds.Min(skuId => stockQuantities.TryGetValue(skuId, out var quantity) ? quantity : 0)
            })
            .Where(x => x.MinimumQuantity > 0)
            .OrderByDescending(x => x.MinimumQuantity)
            .Select(x => x.Candidate)
            .FirstOrDefault();

        return candidate ?? throw new InvalidOperationException("No preset with positive stock quantities was found in stock-api.");
    }

    public async Task<long> InsertProcessingRequestAsync(string email, PresetCandidate presetCandidate)
    {
        await using var connection = new NpgsqlConnection(DatabaseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
			INSERT INTO merchandise_requests (sku_preset_id, merchandise_request_status, created_at, clothing_size, employee_email)
			VALUES (@skuPresetId, 'processing', CURRENT_TIMESTAMP, @clothingSize, @employeeEmail)
			RETURNING id;
			""";
        command.Parameters.AddWithValue("skuPresetId", presetCandidate.SkuPresetId);
        command.Parameters.AddWithValue("clothingSize", presetCandidate.ClothingSize);
        command.Parameters.AddWithValue("employeeEmail", email);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result);
    }

    public async Task<GiveOutMerchandiseResponse> GiveOutMerchandiseAsync(string email, PresetCandidate presetCandidate)
    {
        return await MerchandiseGrpcClient.GiveOutMerchandiseAsync(new GiveOutMerchandiseRequest
        {
            MerchRequestUnit = new GiveOutMerchandiseRequestUnit
            {
                Email = email,
                Type = presetCandidate.TypeName,
                ClothingSize = presetCandidate.ClothingSize
            }
        });
    }

    public async Task<GetRequestsByEmployeeResponse> GetRequestsByEmployeeAsync(string email)
    {
        return await MerchandiseGrpcClient.GetRequestsByEmployeeAsync(new GetRequestsByEmployeeRequest
        {
            Email = email
        });
    }

    public async Task<NotificationEvent> WaitForEmailNotificationEventAsync(string email, TimeSpan timeout)
    {
        using var consumer = CreateConsumer($"e2e-email-{Guid.NewGuid():N}", EmailNotificationTopic);
        var deadline = DateTimeOffset.UtcNow + timeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            var result = consumer.Consume(TimeSpan.FromSeconds(2));
            if (result is null)
            {
                continue;
            }

            var notification = JsonSerializer.Deserialize<NotificationEvent>(result.Message.Value, JsonSerializerOptions);
            if (notification?.EmployeeEmail == email)
            {
                return notification;
            }
        }

        throw new TimeoutException($"Timed out waiting for outbound email notification event for '{email}'.");
    }

    public async Task PublishEmployeeNotificationEventAsync(string email, PresetCandidate presetCandidate)
    {
        var notification = new NotificationEvent
        {
            EventType = EmployeeEventType.MerchDelivery,
            EmployeeEmail = email,
            ManagerEmail = string.Empty,
            Payload = new MerchDeliveryEventPayload
            {
                MerchType = MapMerchType(presetCandidate.TypeId),
                ClothingSize = MapExternalClothingSize(presetCandidate.ClothingSize)
            }
        };

        await PublishJsonMessageAsync(EmployeeNotificationTopic, email, notification);
    }

    public async Task PublishStockReplenishedEventAsync(IEnumerable<long> skuIds)
    {
        var replenishedSkuIds = skuIds.Distinct().ToArray();
        var stockReplenishedEvent = new StockReplenishedEvent
        {
            Type = replenishedSkuIds
                .Select(skuId => new StockReplenishedItem
                {
                    Sku = skuId,
                    ItemTypeId = 0,
                    ItemTypeName = string.Empty,
                    ClothingSize = null
                })
                .ToList()
        };

        await PublishJsonMessageAsync(StockReplenishedTopic, string.Join(",", replenishedSkuIds), stockReplenishedEvent);
    }

    public async Task WaitForRequestStatusAsync(string email, string expectedStatus, TimeSpan timeout)
    {
        await WaitForAsync(async () =>
        {
            var response = await GetRequestsByEmployeeAsync(email);
            return response.Requests.Any(x => string.Equals(x.Status, expectedStatus, StringComparison.OrdinalIgnoreCase));
        }, timeout, $"request status '{expectedStatus}' for '{email}'");
    }

    public async Task WaitForTraceAsync(TimeSpan timeout)
    {
        await WaitForAsync(async () =>
        {
            using var response = await TempoClient.GetAsync("/api/search?tags=service.name%3DOzonEdu.MerchandiseService&limit=20");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            return payload?["traces"] is JsonArray data && data.Count > 0;
        }, timeout, "tempo traces");
    }

    public async Task WaitForPrometheusMetricsAsync(TimeSpan timeout)
    {
        await WaitForAsync(async () =>
        {
            using var response = await PrometheusClient.GetAsync("/api/v1/query?query=up%7Bjob%3D%22merchandise-service%22%7D");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            return payload?["data"]?["result"] is JsonArray result && result.Count > 0;
        }, timeout, "prometheus metrics");
    }

    public async Task WaitForIndexedLogsAsync(TimeSpan timeout)
    {
        await WaitForAsync(async () =>
        {
            var end = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var start = end - 600;
            using var response = await LokiClient.GetAsync($"/loki/api/v1/query_range?query=%7Bjob%3D%22fluent-bit%22%7D%20%7C%3D%20%22OzonEdu.MerchandiseService%22&limit=20&start={start}000000000&end={end}000000000");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var payload = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            return payload?["data"]?["result"] is JsonArray result && result.Count > 0;
        }, timeout, "loki logs");
    }

    private async Task EnsureSkuPresetsSeededAsync()
    {
        await using var connection = new NpgsqlConnection(DatabaseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sku_presets";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        if (count <= 0)
        {
            throw new InvalidOperationException("sku_presets table is empty.");
        }
    }

    private async Task<IReadOnlyCollection<PresetCandidate>> LoadPresetCandidatesAsync()
    {
        var candidates = new List<PresetCandidate>();

        await using var connection = new NpgsqlConnection(DatabaseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
			SELECT
				sp.id,
				sp.preset_type_id,
				MAX(COALESCE(s.clothing_size, 0)) AS clothing_size,
				ARRAY_AGG(sps.sku_id ORDER BY sps.sku_id) AS sku_ids
			FROM sku_presets sp
			INNER JOIN sku_preset_skus sps ON sps.sku_preset_id = sp.id
			INNER JOIN skus s ON s.id = sps.sku_id
			GROUP BY sp.id, sp.preset_type_id
			ORDER BY sp.id;
			""";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var presetTypeId = reader.GetInt32(1);
            var clothingSizeId = reader.GetInt32(2);

            candidates.Add(new PresetCandidate(
                SkuPresetId: reader.GetInt64(0),
                TypeId: presetTypeId,
                TypeName: MapPresetType(presetTypeId),
                ClothingSize: MapClothingSize(clothingSizeId),
                SkuIds: reader.GetFieldValue<long[]>(3)));
        }

        return candidates;
    }

    private async Task PublishJsonMessageAsync<T>(string topic, string key, T payload)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = KafkaBootstrapServers
        };

        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
        var messageId = Guid.NewGuid().ToString("N");

        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = JsonSerializer.Serialize(payload, JsonSerializerOptions),
            Headers = new Headers
            {
                { "x-message-id", Encoding.UTF8.GetBytes(messageId) }
            }
        });

        producer.Flush(TimeSpan.FromSeconds(5));
    }

    private IConsumer<string, string> CreateConsumer(string groupId, string topic)
    {
        var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = KafkaBootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        }).Build();

        consumer.Subscribe(topic);
        return consumer;
    }

    private static async Task WaitForHttpAsync(Func<Task<HttpResponseMessage>> action, string description)
    {
        await WaitForAsync(async () =>
        {
            try
            {
                using var response = await action();
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromMinutes(2), description);
    }

    private static async Task WaitForAsync(Func<Task<bool>> predicate, TimeSpan timeout, string description)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        Exception? lastException = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                if (await predicate())
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                lastException = exception;
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        throw new TimeoutException($"Timed out waiting for {description}.", lastException);
    }

    private static string MapPresetType(int presetTypeId) => presetTypeId switch
    {
        1 => "welcome_pack",
        2 => "conference_listener_pack",
        3 => "conference_speaker_pack",
        4 => "probation_period_ending_pack",
        5 => "veteran_pack",
        _ => throw new InvalidOperationException($"Unknown preset type id '{presetTypeId}'.")
    };

    private static string MapClothingSize(int clothingSizeId) => clothingSizeId switch
    {
        1 => "XS",
        2 => "S",
        3 => "M",
        4 => "L",
        5 => "XL",
        6 => "XXL",
        _ => throw new InvalidOperationException($"Unknown clothing size id '{clothingSizeId}'.")
    };

    private static MerchType MapMerchType(int presetTypeId) => presetTypeId switch
    {
        1 => MerchType.WelcomePack,
        2 => MerchType.ConferenceListenerPack,
        3 => MerchType.ConferenceSpeakerPack,
        4 => MerchType.ProbationPeriodEndingPack,
        5 => MerchType.VeteranPack,
        _ => throw new InvalidOperationException($"Unknown preset type id '{presetTypeId}'.")
    };

    private static ClothingSize MapExternalClothingSize(string clothingSize) => clothingSize.ToUpperInvariant() switch
    {
        "XS" => ClothingSize.XS,
        "S" => ClothingSize.S,
        "M" => ClothingSize.M,
        "L" => ClothingSize.L,
        "XL" => ClothingSize.XL,
        "XXL" => ClothingSize.XXL,
        _ => throw new InvalidOperationException($"Unknown clothing size '{clothingSize}'.")
    };
}

public sealed record PresetCandidate(
    long SkuPresetId,
    int TypeId,
    string TypeName,
    string ClothingSize,
    IReadOnlyCollection<long> SkuIds);

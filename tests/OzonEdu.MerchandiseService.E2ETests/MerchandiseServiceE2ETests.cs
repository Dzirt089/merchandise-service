using System.Text.Json.Nodes;

namespace OzonEdu.MerchandiseService.E2ETests;

public sealed class MerchandiseServiceE2ETests : IClassFixture<MerchandiseEnvironmentFixture>
{
    private readonly MerchandiseEnvironmentFixture _fixture;

    public MerchandiseServiceE2ETests(MerchandiseEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HttpEndpoints_ReturnInfrastructurePayloads()
    {
        using var readinessResponse = await _fixture.HttpClient.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, readinessResponse.StatusCode);

        using var metricsResponse = await _fixture.HttpClient.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.OK, metricsResponse.StatusCode);
        var metricsPayload = await metricsResponse.Content.ReadAsStringAsync();
        Assert.Contains("http_requests_received_total", metricsPayload);

        using var swaggerRedirectResponse = await _fixture.HttpClient.GetAsync("/swagger");
        Assert.Equal(HttpStatusCode.MovedPermanently, swaggerRedirectResponse.StatusCode);

        using var swaggerSpecResponse = await _fixture.HttpClient.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, swaggerSpecResponse.StatusCode);

        var swaggerSpec = JsonNode.Parse(await swaggerSpecResponse.Content.ReadAsStringAsync());
        Assert.NotNull(swaggerSpec?["openapi"]?.GetValue<string>());
    }

    [Fact]
    public async Task GrpcEndpoints_GiveOutMerchandiseAndReadRequests()
    {
        var presetCandidate = await _fixture.GetAvailablePresetAsync();
        var email = $"grpc-{Guid.NewGuid():N}@example.com";

        var giveOutResponse = await _fixture.GiveOutMerchandiseAsync(email, presetCandidate);
        Assert.True(giveOutResponse.ResponseCheck);

        await _fixture.WaitForRequestStatusAsync(email, "done", TimeSpan.FromSeconds(30));

        var requests = await _fixture.GetRequestsByEmployeeAsync(email);
        Assert.Contains(requests.Requests, x => string.Equals(x.Type, presetCandidate.TypeName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SuccessfulGiveOut_PublishesEmailNotificationEvent()
    {
        var presetCandidate = await _fixture.GetAvailablePresetAsync();
        var email = $"outbox-{Guid.NewGuid():N}@example.com";

        var giveOutResponse = await _fixture.GiveOutMerchandiseAsync(email, presetCandidate);
        Assert.True(giveOutResponse.ResponseCheck);

        var notification = await _fixture.WaitForEmailNotificationEventAsync(email, TimeSpan.FromSeconds(60));
        Assert.Equal(email, notification.EmployeeEmail);
        Assert.NotNull(notification.Payload);
    }

    [Fact]
    public async Task EmployeeNotificationEvent_ProcessesPendingRequest()
    {
        var presetCandidate = await _fixture.GetAvailablePresetAsync();
        var email = $"employee-notification-{Guid.NewGuid():N}@example.com";

        await _fixture.PublishEmployeeNotificationEventAsync(email, presetCandidate);

        await _fixture.WaitForRequestStatusAsync(email, "done", TimeSpan.FromSeconds(60));
    }

    [Fact]
    public async Task StockReplenishedEvent_RetriesPendingRequest()
    {
        var presetCandidate = await _fixture.GetAvailablePresetAsync();
        var email = $"stock-replenished-{Guid.NewGuid():N}@example.com";

        await _fixture.InsertProcessingRequestAsync(email, presetCandidate);
        await _fixture.PublishStockReplenishedEventAsync(presetCandidate.SkuIds);

        await _fixture.WaitForRequestStatusAsync(email, "done", TimeSpan.FromSeconds(60));
    }

    [Fact]
    public async Task ObservabilityStack_CollectsTracesMetricsAndLogs()
    {
        using var response = await _fixture.HttpClient.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await _fixture.WaitForTraceAsync(TimeSpan.FromSeconds(60));
        await _fixture.WaitForPrometheusMetricsAsync(TimeSpan.FromSeconds(60));
        await _fixture.WaitForIndexedLogsAsync(TimeSpan.FromSeconds(60));
    }
}

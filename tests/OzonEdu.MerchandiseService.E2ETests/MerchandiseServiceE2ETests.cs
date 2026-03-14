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
    public async Task HttpEndpoints_ReturnExpectedPayload()
    {
        using var getAllResponse = await _fixture.HttpClient.GetAsync("/Merchandise/GetAllMerch");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        var items = JsonNode.Parse(await getAllResponse.Content.ReadAsStringAsync())?.AsArray();
        Assert.NotNull(items);
        Assert.NotEmpty(items!);

        using var getByIdResponse = await _fixture.HttpClient.GetAsync("/Merchandise/GetById/1");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        using var notFoundResponse = await _fixture.HttpClient.GetAsync("/Merchandise/GetById/999999");
        Assert.Equal(HttpStatusCode.NotFound, notFoundResponse.StatusCode);
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
        using var response = await _fixture.HttpClient.GetAsync("/Merchandise/GetAllMerch");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await _fixture.WaitForTraceAsync(TimeSpan.FromSeconds(60));
        await _fixture.WaitForPrometheusMetricsAsync(TimeSpan.FromSeconds(60));
        await _fixture.WaitForIndexedLogsAsync(TimeSpan.FromSeconds(60));
    }
}

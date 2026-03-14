namespace OzonEdu.MerchandiseService.Application.Abstractions.Integration
{
    public interface IIntegrationOutboxWriter
    {
        Task AddAsync<TMessage>(string topic, string key, TMessage message, CancellationToken cancellationToken);
    }
}

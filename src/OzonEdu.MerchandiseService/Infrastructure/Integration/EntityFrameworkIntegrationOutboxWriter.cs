using OzonEdu.MerchandiseService.Application.Abstractions.Integration;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models;
using System.Text.Json;

namespace OzonEdu.MerchandiseService.Infrastructure.Integration
{
    public sealed class EntityFrameworkIntegrationOutboxWriter : IIntegrationOutboxWriter
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly MerchandiseDbContext _dbContext;

        public EntityFrameworkIntegrationOutboxWriter(MerchandiseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task AddAsync<TMessage>(string topic, string key, TMessage message, CancellationToken cancellationToken)
        {
            var outboxMessage = new IntegrationOutboxMessage
            {
                Id = Guid.NewGuid(),
                Topic = topic,
                Key = key,
                Type = typeof(TMessage).FullName ?? typeof(TMessage).Name,
                Payload = JsonSerializer.Serialize(message, SerializerOptions),
                OccurredOnUtc = DateTimeOffset.UtcNow
            };

            return _dbContext.IntegrationOutboxMessages.AddAsync(outboxMessage, cancellationToken).AsTask();
        }
    }
}

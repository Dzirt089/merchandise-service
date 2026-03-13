using Confluent.Kafka;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;

namespace OzonEdu.MerchandiseService.Infrastructure.Kafka
{
	public sealed class KafkaOutboxPublisherBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IOptions<KafkaOptions> _kafkaOptions;
		private readonly ILogger<KafkaOutboxPublisherBackgroundService> _logger;

		public KafkaOutboxPublisherBackgroundService(
			IServiceScopeFactory serviceScopeFactory,
			IOptions<KafkaOptions> kafkaOptions,
			ILogger<KafkaOutboxPublisherBackgroundService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_kafkaOptions = kafkaOptions;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var options = _kafkaOptions.Value;
			if (!options.Enabled)
			{
				_logger.LogInformation("Kafka outbox publisher is disabled.");
				return;
			}

			var producerConfig = new ProducerConfig
			{
				BootstrapServers = options.BootstrapServers,
				Acks = Acks.All,
				EnableIdempotence = true
			};

			using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _serviceScopeFactory.CreateScope();
					var dbContext = scope.ServiceProvider.GetRequiredService<MerchandiseDbContext>();

					var messages = await dbContext.IntegrationOutboxMessages
						.Where(x => x.ProcessedOnUtc == null)
						.OrderBy(x => x.OccurredOnUtc)
						.Take(options.OutboxBatchSize)
						.ToListAsync(stoppingToken);

					if (messages.Count == 0)
					{
						await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
						continue;
					}

					foreach (var message in messages)
					{
						try
						{
							var kafkaMessage = new Message<string, string>
							{
								Key = message.Key,
								Value = message.Payload,
								Headers = new Headers
								{
									{ KafkaMessageHeaders.MessageId, System.Text.Encoding.UTF8.GetBytes(message.Id.ToString("N")) }
								}
							};

							await producer.ProduceAsync(message.Topic, kafkaMessage, stoppingToken);
							message.ProcessedOnUtc = DateTimeOffset.UtcNow;
							message.Error = null;
						}
						catch (Exception exception)
						{
							message.Error = exception.Message;
							_logger.LogError(exception, "Failed to publish outbox message {OutboxMessageId} to topic {Topic}", message.Id, message.Topic);
						}
					}

					await dbContext.SaveChangesAsync(stoppingToken);
				}
				catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
				{
					break;
				}
				catch (Exception exception)
				{
					_logger.LogError(exception, "Outbox publisher iteration failed.");
					await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
				}
			}
		}
	}
}

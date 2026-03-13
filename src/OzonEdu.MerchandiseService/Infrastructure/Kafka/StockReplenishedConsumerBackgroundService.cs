using System.Text;
using System.Text.Json;

using Confluent.Kafka;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using CSharpCourse.Core.Lib.Events;

using OzonEdu.MerchandiseService.Application.Commands.NewMerchandiseAppeared;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models;

namespace OzonEdu.MerchandiseService.Infrastructure.Kafka
{
	public sealed class StockReplenishedConsumerBackgroundService : BackgroundService
	{
		private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
		{
			PropertyNameCaseInsensitive = true
		};

		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IOptions<KafkaOptions> _kafkaOptions;
		private readonly ILogger<StockReplenishedConsumerBackgroundService> _logger;

		public StockReplenishedConsumerBackgroundService(
			IServiceScopeFactory serviceScopeFactory,
			IOptions<KafkaOptions> kafkaOptions,
			ILogger<StockReplenishedConsumerBackgroundService> logger)
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
				_logger.LogInformation("Stock replenished Kafka consumer is disabled.");
				return;
			}

			// Yield startup back to the host before entering the blocking Kafka poll loop.
			await Task.Yield();

			var consumerConfig = new ConsumerConfig
			{
				BootstrapServers = options.BootstrapServers,
				GroupId = options.ConsumerGroups.StockReplenished,
				EnableAutoCommit = false,
				AutoOffsetReset = AutoOffsetReset.Earliest
			};

			using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
			consumer.Subscribe(options.Topics.StockReplenishedEvent);

			while (!stoppingToken.IsCancellationRequested)
			{
				ConsumeResult<string, string>? consumeResult = null;

				try
				{
					consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(options.PollTimeoutMs));
					if (consumeResult is null)
					{
						continue;
					}

					var eventPayload = JsonSerializer.Deserialize<StockReplenishedEvent>(consumeResult.Message.Value, SerializerOptions);
					if (eventPayload is null)
					{
						consumer.Commit(consumeResult);
						continue;
					}

					using var scope = _serviceScopeFactory.CreateScope();
					var dbContext = scope.ServiceProvider.GetRequiredService<MerchandiseDbContext>();
					var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
					var messageId = ResolveMessageId(consumeResult);

					var alreadyProcessed = await dbContext.IntegrationInboxMessages.AnyAsync(x => x.Id == messageId, stoppingToken);
					if (alreadyProcessed)
					{
						consumer.Commit(consumeResult);
						continue;
					}

					await dbContext.IntegrationInboxMessages.AddAsync(new IntegrationInboxMessage
					{
						Id = messageId,
						Topic = consumeResult.Topic,
						Type = nameof(StockReplenishedEvent),
						ReceivedOnUtc = DateTimeOffset.UtcNow,
						ProcessedOnUtc = DateTimeOffset.UtcNow
					}, stoppingToken);

					await mediator.Send(new NewMerchandiseAppearedCommand
					{
						SkuCollection = eventPayload.Type.Select(x => x.Sku).ToArray()
					}, stoppingToken);

					consumer.Commit(consumeResult);
				}
				catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
				{
					break;
				}
				catch (Exception exception)
				{
					_logger.LogError(exception, "Failed to process stock replenished event from topic {Topic}", consumeResult?.Topic);
					await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
				}
			}
		}

		private static string ResolveMessageId(ConsumeResult<string, string> consumeResult)
		{
			var header = consumeResult.Message.Headers?.FirstOrDefault(x => x.Key == KafkaMessageHeaders.MessageId);
			var messageIdHeader = header is null
				? null
				: Encoding.UTF8.GetString(header.GetValueBytes());

			return string.IsNullOrWhiteSpace(messageIdHeader)
				? $"{consumeResult.Topic}:{consumeResult.Partition}:{consumeResult.Offset}"
				: messageIdHeader;
		}
	}
}

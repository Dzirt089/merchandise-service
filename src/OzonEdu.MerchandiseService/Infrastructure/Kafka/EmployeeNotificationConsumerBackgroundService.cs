using Confluent.Kafka;
using CSharpCourse.Core.Lib.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OzonEdu.MerchandiseService.Application.Commands.ProcessEmployeeNotification;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models;
using System.Text;
using System.Text.Json;

namespace OzonEdu.MerchandiseService.Infrastructure.Kafka
{
    public sealed class EmployeeNotificationConsumerBackgroundService : BackgroundService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<KafkaOptions> _kafkaOptions;
        private readonly ILogger<EmployeeNotificationConsumerBackgroundService> _logger;

        public EmployeeNotificationConsumerBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<KafkaOptions> kafkaOptions,
            ILogger<EmployeeNotificationConsumerBackgroundService> logger)
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
                _logger.LogInformation("Employee notification Kafka consumer is disabled.");
                return;
            }

            // Yield startup back to the host before entering the blocking Kafka poll loop.
            await Task.Yield();

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = options.BootstrapServers,
                GroupId = options.ConsumerGroups.EmployeeNotification,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(options.Topics.EmployeeNotificationEvent);

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

                    var eventPayload = JsonSerializer.Deserialize<NotificationEvent>(consumeResult.Message.Value, SerializerOptions);
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
                        Type = nameof(NotificationEvent),
                        ReceivedOnUtc = DateTimeOffset.UtcNow,
                        ProcessedOnUtc = DateTimeOffset.UtcNow
                    }, stoppingToken);

                    await mediator.Send(new ProcessEmployeeNotificationCommand
                    {
                        Email = eventPayload.EmployeeEmail,
                        Type = MapType(eventPayload.Payload),
                        ClothingSize = MapClothingSize(eventPayload.Payload)
                    }, stoppingToken);

                    consumer.Commit(consumeResult);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to process employee notification event from topic {Topic}", consumeResult?.Topic);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private static string MapType(object payload)
        {
            var merchPayload = ParsePayload(payload);
            return merchPayload.MerchType switch
            {
                CSharpCourse.Core.Lib.Enums.MerchType.WelcomePack => "welcome_pack",
                CSharpCourse.Core.Lib.Enums.MerchType.ConferenceListenerPack => "conference_listener_pack",
                CSharpCourse.Core.Lib.Enums.MerchType.ConferenceSpeakerPack => "conference_speaker_pack",
                CSharpCourse.Core.Lib.Enums.MerchType.ProbationPeriodEndingPack => "probation_period_ending_pack",
                CSharpCourse.Core.Lib.Enums.MerchType.VeteranPack => "veteran_pack",
                _ => throw new InvalidOperationException($"Unsupported merch type '{merchPayload.MerchType}'.")
            };
        }

        private static string MapClothingSize(object payload)
        {
            var merchPayload = ParsePayload(payload);
            return merchPayload.ClothingSize switch
            {
                CSharpCourse.Core.Lib.Enums.ClothingSize.XS => "XS",
                CSharpCourse.Core.Lib.Enums.ClothingSize.S => "S",
                CSharpCourse.Core.Lib.Enums.ClothingSize.M => "M",
                CSharpCourse.Core.Lib.Enums.ClothingSize.L => "L",
                CSharpCourse.Core.Lib.Enums.ClothingSize.XL => "XL",
                CSharpCourse.Core.Lib.Enums.ClothingSize.XXL => "XXL",
                _ => throw new InvalidOperationException($"Unsupported clothing size '{merchPayload.ClothingSize}'.")
            };
        }

        private static MerchDeliveryEventPayload ParsePayload(object payload)
        {
            if (payload is JsonElement jsonElement)
            {
                var deserialized = jsonElement.Deserialize<MerchDeliveryEventPayload>(SerializerOptions);
                return deserialized ?? throw new InvalidOperationException("Notification payload is empty.");
            }

            if (payload is MerchDeliveryEventPayload merchPayload)
            {
                return merchPayload;
            }

            throw new InvalidOperationException($"Unsupported notification payload type '{payload?.GetType().FullName}'.");
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

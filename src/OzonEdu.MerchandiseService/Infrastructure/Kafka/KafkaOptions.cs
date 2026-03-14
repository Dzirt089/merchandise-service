namespace OzonEdu.MerchandiseService.Infrastructure.Kafka
{
    public sealed class KafkaOptions
    {
        public bool Enabled { get; set; }

        public string BootstrapServers { get; set; } = "localhost:9092";

        public int PollTimeoutMs { get; set; } = 1000;

        public int OutboxBatchSize { get; set; } = 20;

        public KafkaTopicOptions Topics { get; set; } = new();

        public KafkaConsumerGroupOptions ConsumerGroups { get; set; } = new();
    }

    public sealed class KafkaTopicOptions
    {
        public string StockReplenishedEvent { get; set; } = "stock_replenished_event";

        public string EmployeeNotificationEvent { get; set; } = "employee_notification_event";

        public string EmailNotificationEvent { get; set; } = "email_notification_event";
    }

    public sealed class KafkaConsumerGroupOptions
    {
        public string StockReplenished { get; set; } = "merchandise-service-stock-replenished";

        public string EmployeeNotification { get; set; } = "merchandise-service-employee-notification";
    }
}

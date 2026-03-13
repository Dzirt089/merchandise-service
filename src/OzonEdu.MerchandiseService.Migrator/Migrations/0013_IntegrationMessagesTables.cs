using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(13)]
	public class IntegrationMessagesTables : Migration
	{
		public override void Up()
		{
			Create
				.Table(CommonConstants.IntegrationOutboxMessagesTable)
				.WithColumn("id").AsGuid().PrimaryKey()
				.WithColumn("topic").AsString(255).NotNullable()
				.WithColumn("message_key").AsString(255).NotNullable()
				.WithColumn("message_type").AsString(512).NotNullable()
				.WithColumn("payload").AsCustom("text").NotNullable()
				.WithColumn("occurred_on_utc").AsDateTimeOffset().NotNullable()
				.WithColumn("processed_on_utc").AsDateTimeOffset().Nullable()
				.WithColumn("error").AsCustom("text").Nullable();

			Create.Index("ix_integration_outbox_messages_processed_on_utc")
				.OnTable(CommonConstants.IntegrationOutboxMessagesTable)
				.OnColumn("processed_on_utc");

			Create.Index("ix_integration_outbox_messages_topic_occurred_on_utc")
				.OnTable(CommonConstants.IntegrationOutboxMessagesTable)
				.OnColumn("topic").Ascending()
				.OnColumn("occurred_on_utc").Ascending();

			Create
				.Table(CommonConstants.IntegrationInboxMessagesTable)
				.WithColumn("id").AsString(255).PrimaryKey()
				.WithColumn("topic").AsString(255).NotNullable()
				.WithColumn("message_type").AsString(512).NotNullable()
				.WithColumn("received_on_utc").AsDateTimeOffset().NotNullable()
				.WithColumn("processed_on_utc").AsDateTimeOffset().Nullable();

			Create.Index("ix_integration_inbox_messages_processed_on_utc")
				.OnTable(CommonConstants.IntegrationInboxMessagesTable)
				.OnColumn("processed_on_utc");
		}

		public override void Down()
		{
			Delete.Table(CommonConstants.IntegrationInboxMessagesTable);
			Delete.Table(CommonConstants.IntegrationOutboxMessagesTable);
		}
	}
}

namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models
{
	public class IntegrationOutboxMessage
	{
		public Guid Id { get; set; }

		public string Topic { get; set; } = string.Empty;

		public string Key { get; set; } = string.Empty;

		public string Type { get; set; } = string.Empty;

		public string Payload { get; set; } = string.Empty;

		public DateTimeOffset OccurredOnUtc { get; set; }

		public DateTimeOffset? ProcessedOnUtc { get; set; }

		public string? Error { get; set; }
	}
}

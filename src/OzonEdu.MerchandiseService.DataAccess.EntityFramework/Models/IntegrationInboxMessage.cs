namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models
{
	public class IntegrationInboxMessage
	{
		public string Id { get; set; } = string.Empty;

		public string Topic { get; set; } = string.Empty;

		public string Type { get; set; } = string.Empty;

		public DateTimeOffset ReceivedOnUtc { get; set; }

		public DateTimeOffset? ProcessedOnUtc { get; set; }
	}
}

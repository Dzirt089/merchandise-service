namespace OzonEdu.MerchandiseService.Application.Models.DTOs
{
	public sealed record EmployeeNotificationItemDto
	{
		public long SkuId { get; init; }

		public int Quantity { get; init; }
	}
}

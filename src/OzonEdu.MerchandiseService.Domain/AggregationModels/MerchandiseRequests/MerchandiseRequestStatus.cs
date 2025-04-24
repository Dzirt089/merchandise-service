using OzonEdu.MerchandiseService.Domain.Root;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests
{
	/// <summary>
	/// Статус заявки на выдачу мерча
	/// </summary>
	public class MerchandiseRequestStatus : Enumeration
	{
		/// <summary>
		/// Новая заявка
		/// </summary>
		public static MerchandiseRequestStatus New = new(1, "new");

		/// <summary>
		/// Заявка в обработке
		/// </summary>
		public static MerchandiseRequestStatus Processing = new(2, "processing");

		/// <summary>
		/// Заявка выполнена
		/// </summary>
		public static MerchandiseRequestStatus Done = new(3, "done");

		/// <summary>
		/// Заявка отклонена
		/// </summary>
		public static MerchandiseRequestStatus Declined = new(4, "declined");

		public MerchandiseRequestStatus(int id, string name) : base(id, name)
		{
		}
	}
}

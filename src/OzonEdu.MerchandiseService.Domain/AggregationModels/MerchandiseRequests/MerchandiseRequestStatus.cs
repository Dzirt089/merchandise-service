using OzonEdu.MerchandiseService.Domain.Root;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

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

		/// <summary>
		/// Преобразование строки в тип пресета
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="DomainException"></exception>
		public static MerchandiseRequestStatus Parse(string name) => name?.ToUpper() switch
		{
			"new" => New,
			"processing" => Processing,
			"done" => Done,
			"declined" => Declined,
			_ => throw new DomainException("Unknown preset type name")
		};
	}
}

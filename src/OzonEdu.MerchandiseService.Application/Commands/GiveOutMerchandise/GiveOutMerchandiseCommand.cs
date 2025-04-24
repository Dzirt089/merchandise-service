using MediatR;

namespace OzonEdu.MerchandiseService.Application.Commands.GiveOutMerchandise
{
	/// <summary>
	/// Команда на создание запроса на выдачу мерча
	/// </summary>
	public record GiveOutMerchandiseCommand : IRequest<bool>
	{
		/// <summary>
		/// Email сотрудника
		/// </summary>
		public string Email { get; init; }

		/// <summary>
		/// Размер одежды
		/// </summary>
		public string ClothinSize { get; init; }

		/// <summary>
		/// Тип набора выдаваемого мерча
		/// </summary>
		public string Type { get; init; }
	}
}

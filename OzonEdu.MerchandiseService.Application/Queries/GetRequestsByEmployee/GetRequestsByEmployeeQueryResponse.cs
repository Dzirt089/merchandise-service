using OzonEdu.MerchandiseService.Application.Models.DTOs;

namespace OzonEdu.MerchandiseService.Application.Queries.GetRequestsByEmployee
{
	public class GetRequestsByEmployeeQueryResponse
	{
		public IReadOnlyCollection<MerchandiseRequestDataDto> Items { get; set; }
	}
}

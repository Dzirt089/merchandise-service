using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;

namespace OzonEdu.MerchandiseService.Application.Contracts
{
	public interface IEmailService
	{
		Task SendEmail(Email email, object obj);
	}
}
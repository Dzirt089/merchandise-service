using MediatR;

using CSharpCourse.Core.Lib.Enums;
using CSharpCourse.Core.Lib.Events;
using CSharpCourse.Core.Lib.Models;

using OzonEdu.MerchandiseService.Application.Abstractions.Integration;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public sealed class MerchandiseRequestGiveOutHandler : INotificationHandler<MerchandiseRequestGiveOutDomainEvent>
	{
		private const string EmailNotificationTopic = "email_notification_event";
		private readonly IIntegrationOutboxWriter _integrationOutboxWriter;

		public MerchandiseRequestGiveOutHandler(IIntegrationOutboxWriter integrationOutboxWriter)
		{
			_integrationOutboxWriter = integrationOutboxWriter;
		}

		public Task Handle(MerchandiseRequestGiveOutDomainEvent notification, CancellationToken cancellationToken)
		{
			var integrationEvent = new NotificationEvent
			{
				EventType = EmployeeEventType.MerchDelivery,
				EmployeeEmail = notification.Employee.Email.Value,
				ManagerEmail = string.Empty,
				Payload = new MerchDeliveryEventPayload
				{
					MerchType = MapMerchType(notification.SkuPreset.Type.Id),
					ClothingSize = MapClothingSize(notification.Employee.ClothingSize.Id)
				}
			};

			return _integrationOutboxWriter.AddAsync(
				EmailNotificationTopic,
				notification.Employee.Email.Value,
				integrationEvent,
				cancellationToken);
		}

		private static MerchType MapMerchType(int presetTypeId) => presetTypeId switch
		{
			1 => MerchType.WelcomePack,
			2 => MerchType.ConferenceListenerPack,
			3 => MerchType.ConferenceSpeakerPack,
			4 => MerchType.ProbationPeriodEndingPack,
			5 => MerchType.VeteranPack,
			_ => throw new InvalidOperationException($"Unsupported preset type id '{presetTypeId}'.")
		};

		private static CSharpCourse.Core.Lib.Enums.ClothingSize MapClothingSize(int clothingSizeId) => clothingSizeId switch
		{
			1 => CSharpCourse.Core.Lib.Enums.ClothingSize.XS,
			2 => CSharpCourse.Core.Lib.Enums.ClothingSize.S,
			3 => CSharpCourse.Core.Lib.Enums.ClothingSize.M,
			4 => CSharpCourse.Core.Lib.Enums.ClothingSize.L,
			5 => CSharpCourse.Core.Lib.Enums.ClothingSize.XL,
			6 => CSharpCourse.Core.Lib.Enums.ClothingSize.XXL,
			_ => throw new InvalidOperationException($"Unsupported clothing size id '{clothingSizeId}'.")
		};
	}
}

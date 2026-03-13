using CSharpCourse.Core.Lib.Enums;
using CSharpCourse.Core.Lib.Events;

namespace OzonEdu.MerchandiseService.Application.Integration
{
	internal static class NotificationEventFactory
	{
		public static NotificationEvent CreateMerchDelivery(string employeeEmail, int presetTypeId, int clothingSizeId)
		{
			return new NotificationEvent
			{
				EventType = EmployeeEventType.MerchDelivery,
				EmployeeEmail = employeeEmail,
				ManagerEmail = string.Empty,
				Payload = new MerchDeliveryEventPayload
				{
					MerchType = MapMerchType(presetTypeId),
					ClothingSize = MapClothingSize(clothingSizeId)
				}
			};
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

		private static ClothingSize MapClothingSize(int clothingSizeId) => clothingSizeId switch
		{
			1 => ClothingSize.XS,
			2 => ClothingSize.S,
			3 => ClothingSize.M,
			4 => ClothingSize.L,
			5 => ClothingSize.XL,
			6 => ClothingSize.XXL,
			_ => throw new InvalidOperationException($"Unsupported clothing size id '{clothingSizeId}'.")
		};
	}
}

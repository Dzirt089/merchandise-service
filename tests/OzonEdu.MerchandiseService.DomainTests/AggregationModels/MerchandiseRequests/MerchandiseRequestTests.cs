using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

using Xunit;

using Assert = Xunit.Assert;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.Tests
{
	public class MerchandiseRequestTests
	{
		private SkuPreset CreateTestSkuPreset()
		{
			// Создаём тестовый SKU и Preset
			var sku = Sku.Create(1);
			var skus = new List<Sku> { sku };
			return new SkuPreset(100, skus, PresetType.WelcomePack);
		}

		private Employee CreateTestEmployee()
		{
			// Создаём тестового сотрудника с Email и ClothingSize
			var email = Email.Create("test@test.com");
			return new Employee(email, ClothingSize.M);
		}

		[Fact()]
		public void MerchandiseRequestTest()
		{
			//Arrange
			var skuPreset = CreateTestSkuPreset();
			var employee = CreateTestEmployee();
			var alreadyExisted = new List<MerchandiseRequest>();
			var createdAt = DateTimeOffset.UtcNow;

			// Act
			var request = MerchandiseRequest.Create(skuPreset, employee, alreadyExisted, createdAt);

			// Assert
			Assert.Equal(skuPreset, request.SkuPreset);
			Assert.Equal(employee, request.Employee);
			Assert.Equal(MerchandiseRequestStatus.New, request.Status);
			Assert.Equal(createdAt, request.CreatedAt);
			Assert.Null(request.GiveOutAt);
		}

		[Fact]
		public void GiveOut_WhenAvailable_UpdatesStatusToDoneAndSetsGiveOutAt()
		{
			// Arrange
			var skuPreset = CreateTestSkuPreset();
			var employee = CreateTestEmployee();
			var alreadyExisted = new List<MerchandiseRequest>();
			var createdAt = DateTimeOffset.UtcNow;
			var request = MerchandiseRequest.Create(skuPreset, employee, alreadyExisted, createdAt);

			var giveOutAt = DateTimeOffset.UtcNow.AddHours(1);

			// Act
			var status = request.GiveOut(true, giveOutAt);

			// Assert
			Assert.Equal(MerchandiseRequestStatus.Done, status);
			Assert.Equal(MerchandiseRequestStatus.Done, request.Status);
			Assert.Equal(giveOutAt, request.GiveOutAt);
			// Проверяем, что доменное событие о выдаче добавлено.
			Assert.True(request.DomainEvents.Any(e => e is MerchandiseRequestGiveOut));
		}

		[Fact]
		public void GiveOut_WhenNotAvailable_UpdatesStatusToProcessing()
		{
			// Arrange
			var skuPreset = CreateTestSkuPreset();
			var employee = CreateTestEmployee();
			var alreadyExisted = new List<MerchandiseRequest>();
			var createdAt = DateTimeOffset.UtcNow;
			var request = MerchandiseRequest.Create(skuPreset, employee, alreadyExisted, createdAt);
			var giveOutAt = DateTimeOffset.UtcNow.AddHours(1);

			// Act
			var status = request.GiveOut(false, giveOutAt);

			// Assert
			Assert.Equal(MerchandiseRequestStatus.Processing, status);
			Assert.Equal(MerchandiseRequestStatus.Processing, request.Status);
			Assert.Null(request.GiveOutAt);
			// Не должно быть события выдачи, так как мерч не выдан
			Assert.False(request.DomainEvents.Any(e => e is MerchandiseRequestGiveOut));
		}

		[Fact]
		public void GiveOut_WhenStatusNotNewOrProcessing_ThrowsException()
		{
			// Arrange
			var skuPreset = CreateTestSkuPreset();
			var employee = CreateTestEmployee();
			var alreadyExisted = new List<MerchandiseRequest>();
			var createdAt = DateTimeOffset.UtcNow;
			// Создаём запрос и выдаём мерч, переводя его в статус Done
			var request = MerchandiseRequest.Create(skuPreset, employee, alreadyExisted, createdAt);
			var giveOutAt = DateTimeOffset.UtcNow.AddHours(1);
			request.GiveOut(true, giveOutAt);

			// Act & Assert: повторная попытка выдачи при статусе Done
			var ex = Assert.Throws<DomainException>(() => request.GiveOut(true, giveOutAt.AddHours(1)));
			Assert.Contains("Unable to give out merchandise", ex.Message);
		}

		[Fact]
		public void Decline_WhenStatusIsNew_UpdatesStatusToDeclinedAndAddsDomainEvent()
		{
			// Arrange
			var skuPreset = CreateTestSkuPreset();
			var employee = CreateTestEmployee();
			var alreadyExisted = new List<MerchandiseRequest>();
			var createdAt = DateTimeOffset.UtcNow;
			var request = MerchandiseRequest.Create(skuPreset, employee, alreadyExisted, createdAt);

			// Act
			request.Decline();

			// Assert
			Assert.Equal(MerchandiseRequestStatus.Declined, request.Status);
			// Проверяем, что событие Declined добавлено
			Assert.True(request.DomainEvents.Any(e => e is MerchandiseRequestDeclined));
		}

		[Fact]
		public void Decline_WhenStatusIsDone_ThrowsException()
		{
			// Arrange
			var skuPreset = CreateTestSkuPreset();
			var employee = CreateTestEmployee();
			var alreadyExisted = new List<MerchandiseRequest>();
			var createdAt = DateTimeOffset.UtcNow;
			var request = MerchandiseRequest.Create(skuPreset, employee, alreadyExisted, createdAt);
			// переводим запрос в статус Done
			var giveOutAt = DateTimeOffset.UtcNow.AddHours(1);
			request.GiveOut(true, giveOutAt);

			// Act & Assert
			var ex = Assert.Throws<DomainException>(() => request.Decline());
			Assert.Contains("Unable to decline request", ex.Message);
		}

		[Fact]
		public void Decline_WhenStatusIsDeclined_ThrowsException()
		{
			// Arrange
			var skuPreset = CreateTestSkuPreset();
			var employee = CreateTestEmployee();
			var alreadyExisted = new List<MerchandiseRequest>();
			var createdAt = DateTimeOffset.UtcNow;
			var request = MerchandiseRequest.Create(skuPreset, employee, alreadyExisted, createdAt);
			// переводим запрос в статус Declined
			request.Decline();

			// Act & Assert
			var ex = Assert.Throws<DomainException>(() => request.Decline());
			Assert.Contains("Unable to decline request", ex.Message);
		}
	}
}
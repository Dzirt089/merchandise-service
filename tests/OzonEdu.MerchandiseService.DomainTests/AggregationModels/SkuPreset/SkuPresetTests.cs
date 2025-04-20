using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

using Xunit;

using Assert = Xunit.Assert;

namespace OzonEdu.MerchandiseService.DomainTests.AggregationModels.SkuPresets
{
	public class SkuPresetTests
	{
		[Fact()]
		public void SkuPresetTest()
		{
			//Arrange
			var sku1 = Sku.Create(1);
			var sku2 = Sku.Create(2);
			var skus = new List<Sku>() { sku1, sku2 };
			var presetType = PresetType.WelcomePack;
			var id = 1L;

			//Act
			var preset = new SkuPreset(id, skus, presetType);

			//Assert
			Assert.Equal(id, preset.Id);
			Assert.Equal(skus, preset.SkuCollection);
			Assert.Equal(presetType, preset.Type);

			Assert.Contains(sku1, preset.SkuCollection);
			Assert.Contains(sku2, preset.SkuCollection);
		}

		[Fact()]
		public void AddToPresetTest()
		{
			//Arrange
			var sku1 = Sku.Create(1);
			var sku2 = Sku.Create(2);
			var skus1 = new List<Sku>() { sku1, sku2 };
			var presetType = PresetType.WelcomePack;
			var id = 1L;
			var preset = new SkuPreset(id, skus1, presetType);

			var sku3 = Sku.Create(3);
			var newSkus = new List<Sku>() { sku3 };

			//Act
			preset.AddToPreset(newSkus);

			//Assert
			Assert.Equal(3, preset.SkuCollection.Count);
			Assert.Contains(sku3, preset.SkuCollection);

			//Arrenge
			var dublicateSkus = new List<Sku>() { Sku.Create(1) };

			//Act & Assert
			var exception = Assert.Throws<DomainException>(() => preset.AddToPreset(dublicateSkus));
			Assert.Contains("already added", exception.Message);
		}

		[Fact()]
		public void DeleteFromPresetTest()
		{
			//Arrange
			var sku1 = Sku.Create(1);
			var sku2 = Sku.Create(2);

			var skus = new List<Sku>() { sku1, sku2 };
			var preset = new SkuPreset(100, skus, PresetType.WelcomePack);

			//Act
			preset.DeleteFromPreset(skus);

			//Assert
			Assert.Empty(preset.SkuCollection);

			//Arrange
			preset = new SkuPreset(101, skus, PresetType.WelcomePack);
			var deleteSkus = new List<Sku> { Sku.Create(3) };

			//Act & Assert
			var exception = Assert.Throws<DomainException>(() => preset.DeleteFromPreset(deleteSkus));
			Assert.Contains("not exists", exception.Message);
		}
	}
}
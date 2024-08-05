using OzonEdu.MerchandiseService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchPackAggregate
{
	public class MerchPackPreset : Entity
	{
		public MerchPackPreset(
			long id,
			PackType merchPack, 
			IReadOnlyCollection<Sku> skuCollection)
		{
			Id = id;
			PackType = merchPack;
			SkuCollection = skuCollection;
		}

		public PackType PackType { get; }
		public IReadOnlyCollection<Sku> SkuCollection { get; private set; }

	}
}

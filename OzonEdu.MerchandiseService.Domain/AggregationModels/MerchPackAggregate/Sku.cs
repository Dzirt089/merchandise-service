using OzonEdu.MerchandiseService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchPackAggregate
{
	/// <summary>
	/// Идентификатор нового товара
	/// </summary>
	public class Sku : ValueObject
	{
		public Sku(long value)
		{
			Value = value;
		}

		public long Value { get;}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Value;
		}
	}
}

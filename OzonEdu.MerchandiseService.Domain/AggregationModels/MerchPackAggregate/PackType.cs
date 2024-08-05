using OzonEdu.MerchandiseService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchPackAggregate
{
	public class PackType : Enumeration
	{
		/// <summary>Стандартный набор мерча для сотрудника, выдаётся 1 раз в год </summary>
		public static PackType StarterPack = new(1, nameof(StarterPack));

		/// <summary>Набор мерча для проведения конференций</summary>
		public static PackType Conference = new(2, nameof(Conference));

		/// <summary>
		/// Можем создавать и другие объекты мерча через открытый конструктор, который принимает 
		/// </summary>
		/// <param name="id">ID новой позиции</param>
		/// <param name="name">Наименование новой позиции</param>
		public PackType(int id, string name) : base(id, name)
		{
		}
	}
}

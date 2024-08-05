using OzonEdu.MerchandiseService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.EmployeeAggregate
{
	/// <summary>
	/// Определяем как тип перечислителя, определяющий размер одежды
	/// Размеры уже предопределены (Или можно их получать из БД)
	/// </summary>
	public class ClothingSize : Enumeration
	{
		public static ClothingSize XS = new(1, nameof(XS));
		public static ClothingSize S = new(2, nameof(S));
		public static ClothingSize M = new(3, nameof(M));
		public static ClothingSize L = new(4, nameof(L));
		public static ClothingSize XL = new(5, nameof(XL));
		public static ClothingSize XXL = new(6, nameof(XXL));

		public ClothingSize(int id, string name) : base(id, name)
		{
		}
	}
}

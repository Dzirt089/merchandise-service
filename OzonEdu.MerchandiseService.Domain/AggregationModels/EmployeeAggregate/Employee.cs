using OzonEdu.MerchandiseService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.EmployeeAggregate
{
	/// <summary>
	/// Сотрудник с данными почты и размера одежды
	/// </summary>
	public class Employee : ValueObject
	{
		/// <summary>
		/// Информация о сотруднике
		/// </summary>
		/// <param name="email">Email для сотрудника</param>
		/// <param name="clothingSize">Размер одежды сотрудника</param>
		public Employee(
			Email email, 
			ClothingSize clothingSize)
		{
			Email = email;
			ClothingSize = clothingSize;
		}

		/// <summary>
		/// Email для сотрудника
		/// </summary>
		public Email Email { get; }
		/// <summary>
		/// Размер одежды сотрудника
		/// </summary>
		public ClothingSize ClothingSize { get; }

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Email;
			yield return ClothingSize;
		}
	}
}

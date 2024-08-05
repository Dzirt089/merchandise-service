using OzonEdu.MerchandiseService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.EmployeeAggregate
{
	/// <summary>
	/// Email для сотрудника
	/// </summary>
	public class Email : ValueObject
	{
		private Email(string emailString)
		{
			Value = emailString;
		}

		/// <summary>
		/// Создаём для сотрудника email с проверками
		/// </summary>
		/// <param name="emailString">строка с email</param>
		/// <returns>экземпляр класса <see cref="Email"/></returns>
		/// <exception cref="Exception"></exception>
		public static Email Create(string emailString)
		{
			if(emailString != null && IsValidEmail(emailString))
				return new Email(emailString);

			throw new Exception(@$"Email is null or is invalid: {emailString}");
		}
		/// <summary>
		/// Хранит емайл
		/// </summary>
		public string Value { get; }

		//TODO: Сделать тест на проверку емайла
		public static bool IsValidEmail(string emailString)
		{
			return Regex.IsMatch(emailString, @"[a-zA-Z0-9\.-_]+@[a-zA-Z0-9\.-_]+");
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Value;
		}
	}
}

using OzonEdu.MerchandiseService.Domain.Root;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests
{
	/// <summary>
	/// Информация о сотруднике
	/// </summary>
	public class Employee : ValueObject
	{
		private Employee() { }
		public Employee(Email email, ClothingSize clothingSize)
		{
			Email = email;
			ClothingSize = clothingSize;
		}

		/// <summary>
		/// Email сотрудника
		/// </summary>
		public Email Email { get; private set; }

		/// <summary>
		/// Размер одежды сотрудника
		/// </summary>
		public ClothingSize ClothingSize { get; private set; }

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Email;
			yield return ClothingSize;
		}
	}
}

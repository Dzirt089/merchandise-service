
using OzonEdu.MerchandiseService.Models;

namespace OzonEdu.MerchandiseService.Services
{
	public class MerchService : IMerchService
	{
		/// <summary>
		/// Пример притянут за уши, естественно данные будем получать из запроса в БД. 
		/// </summary>
		private readonly List<MerchItem> MerchItems =
		[
			new MerchItem(1, "Кепка", 150),
			new MerchItem(2, "Футболка", 230),
			new MerchItem(3, "Толстовка", 70),
			new MerchItem(4, "Перчатки", 250),
		];

		/// <summary>
		/// Получаем все остатки мерча на складе 
		/// </summary>
		/// <param name="token">токен отмены</param>
		/// <returns>список мерча</returns>
		public async Task<List<MerchItem>> GetAll(CancellationToken token) => MerchItems;

		/// <summary>
		/// Получаем конкретный остаток для определенного мерча
		/// </summary>
		/// <param name="id">Идентификатор мерча</param>
		/// <param name="token">токен отмены</param>
		public async Task<MerchItem?> GetById(long id, CancellationToken token) => MerchItems.FirstOrDefault(x => x.Id == id);

	}
}

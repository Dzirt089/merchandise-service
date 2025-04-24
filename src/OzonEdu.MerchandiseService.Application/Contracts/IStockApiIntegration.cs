namespace OzonEdu.MerchandiseService.Application.Contracts
{
	/// <summary>
	/// Интерфейс для интеграции со Stock API склада
	/// </summary>
	public interface IStockApiIntegration
	{
		/// <summary>
		/// Запрашиваем выдачу мерча со склада
		/// </summary>
		/// <param name="skuCollection"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<bool> RequestGiveOutAsync(IEnumerable<long> skuCollection, CancellationToken cancellationToken);
	}
}

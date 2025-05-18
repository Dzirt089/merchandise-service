namespace OzonEdu.MerchandiseService.Application.Contracts
{
	/// <summary>
	/// Конфигурации подключения к сервису StockApi
	/// </summary>
	public class StockApiGrpcServiceConfiguration
	{
		/// <summary>
		/// Адрес сервера
		/// </summary>
		public string ServerAddress { get; set; }
	}
}

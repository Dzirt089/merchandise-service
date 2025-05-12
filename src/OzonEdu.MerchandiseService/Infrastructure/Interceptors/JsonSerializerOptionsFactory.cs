using System.Text.Json;

namespace OzonEdu.MerchandiseService.Infrastructure.Interceptors
{
	/// <summary>
	/// Фабрика для создания настроек сериализации JSON.
	/// </summary>
	internal class JsonSerializerOptionsFactory
	{
		public static JsonSerializerOptions Default = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true
		};
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Http.Services
{
    /// <summary>
    /// Создаем для наглядности HttpClient, который будет работать с нашей апишкой. Чтобы было понимание, что принимает и возвращает.
    /// </summary>
    /// <param name="client">HttpClient</param>
    public class HttpService(HttpClient client) : IHttpService
    {
        private readonly HttpClient _client = client;
        private readonly string ACTION = "Merchandise";
        private readonly string URL = "http://localhost:5222"; //TODO: адрес забить в appsettings.json

        /// <summary>
        /// Дергаем апишку, чтобы получить все данные по мерчу
        /// </summary>
        /// <param name="token">токен отмены</param>
        /// <returns>список данных мерча по складу</returns>
        public async Task<List<ItemPosition>> GetAll(CancellationToken token)
        {
            using var response = await _client.GetAsync(@$"{URL}/{ACTION}/GetAllMerch", token);
            var body = await response.Content.ReadAsStringAsync(token);
            var result = JsonSerializer.Deserialize<List<ItemPosition>>(body);
            return result;
        }

        /// <summary>
        /// Получаем инфу по одной конкретной складской позиции
        /// </summary>
        /// <param name="id">идентификатор товара</param>
        /// <param name="token">токен отмены</param>
        /// <returns>инфа товара</returns>
        public async Task<ItemPosition> GetById(long id, CancellationToken token)
        {
            using var response = await _client.GetAsync(@$"{URL}/{ACTION}/GetById/{id}", token);
            var body = await response.Content.ReadAsStringAsync(token);
            var result = JsonSerializer.Deserialize<ItemPosition>(body);
            return result;
        }
    }
}

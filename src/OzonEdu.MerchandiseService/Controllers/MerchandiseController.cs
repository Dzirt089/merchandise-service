using Microsoft.AspNetCore.Mvc;
using OzonEdu.MerchandiseService.Http.Services;
using OzonEdu.MerchandiseService.Services;

namespace OzonEdu.MerchandiseService.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class MerchandiseController(IMerchService merch) : ControllerBase
    {
        private readonly IMerchService _merchService = merch;

        /// <summary>
        /// Получаем все данные со склада по всему мерчу
        /// </summary>
        /// <param name="token">токен отмены</param>
        /// <returns>Список данных по мерчу со склада</returns>
        [HttpGet("[action]")]
        public async Task<ActionResult<List<HttpItem>>> GetAllMerch(CancellationToken token)
        {
            var result = await _merchService.GetAll(token);
            List<HttpItem> items = new List<HttpItem>();
            foreach (var item in result)
            {
                items.Add(new HttpItem
                {
                    IdPosition = item.Id,
                    ItemNamePosition = item.ItemName,
                    QuantityPosition = item.Quantity,
                });
            }
            return Ok(items);
        }

        /// <summary>
        /// Получаем инфу по конкретному мерчу со склада
        /// </summary>
        /// <param name="token">токен отмены</param>
        /// <param name="id">идентификатор мерча для склада</param>
        /// <returns>данные по одному мерчу со склада</returns>
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<HttpItem>> GetById(long id, CancellationToken token)
        {
            var result = await _merchService.GetById(id, token);
            if (result is null) return NotFound();
            return Ok(new HttpItem
            {
                IdPosition = result.Id,
                ItemNamePosition = result.ItemName,
                QuantityPosition = result.Quantity,
            });
        }
    }
}

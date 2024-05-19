using Microsoft.AspNetCore.Mvc;
using OzonEdu.MerchandiseService.Services;

namespace OzonEdu.MerchandiseService.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("v1/api/merch")]
    public class MerchandiseController(IMerchService merch) : ControllerBase
    {
        private readonly IMerchService _merchService = merch;

        /// <summary>
        /// Получаем все данные со склада по всему мерчу
        /// </summary>
        /// <param name="token">токен отмены</param>
        /// <returns>Список данных по мерчу со склада</returns>
        [HttpGet]
        public async Task<ActionResult<List<MerchItem>>> GetAllMerch(CancellationToken token) => Ok(await _merchService.GetAll(token));

        /// <summary>
        /// Получаем инфу по конкретному мерчу со склада
        /// </summary>
        /// <param name="token">токен отмены</param>
        /// <param name="id">идентификатор мерча для склада</param>
        /// <returns>данные по одному мерчу со склада</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<MerchItem>> GetById(CancellationToken token, long id)
        {
            var result = await _merchService.GetById(id, token);
            if (result is null) return NotFound();
            return Ok(result);
        }
    }
}

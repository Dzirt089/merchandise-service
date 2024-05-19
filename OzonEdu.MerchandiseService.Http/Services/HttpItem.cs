using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Http.Services
{
    public class HttpItem
    {
        public HttpItem(long id, string itemName, int quantity)
        {
            IdClient = id;
            ItemNameClient = itemName;
            QuantityClient = quantity;
        }
        public HttpItem() { }

        /// <summary>
        /// Идентификатор мерча
        /// </summary>
        [JsonPropertyName("IdClient")]
        public long IdClient { get; set; }


        /// <summary>
        /// Наименования меча
        /// </summary>
        [JsonPropertyName("ItemNameClient")]
        public string ItemNameClient { get; set; }

        /// <summary>
        /// Остаток на складе позиции мерча
        /// </summary>
        [JsonPropertyName("QuantityClient")]
        public int QuantityClient { get; set; }
    }
}

﻿using System;
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
            IdPosition = id;
            ItemNamePosition = itemName;
            QuantityPosition = quantity;
        }
        public HttpItem() { }

        /// <summary>
        /// Идентификатор мерча
        /// </summary>
        [JsonPropertyName("IdPosition")]
        public long IdPosition { get; set; }


        /// <summary>
        /// Наименования меча
        /// </summary>
        [JsonPropertyName("ItemNamePosition")]
        public string ItemNamePosition { get; set; }

        /// <summary>
        /// Остаток на складе позиции мерча
        /// </summary>
        [JsonPropertyName("QuantityPosition")]
        public int QuantityPosition { get; set; }

        public override string ToString() => $"{IdPosition}, {ItemNamePosition}, {QuantityPosition}";
    }
}

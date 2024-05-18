namespace OzonEdu.MerchandiseService.Services
{
    /// <summary>
    /// Модель мерча (перчатки, фуутболка и т.д.)
    /// </summary>
    public class MerchItem
    {
        public MerchItem(long id, string itemName, int quantity)
        {
            Id = id;
            ItemName = itemName;
            Quantity = quantity;
        }

        /// <summary>
        /// Идентификатор мерча
        /// </summary>
        public long Id { get; }


        /// <summary>
        /// Наименования меча
        /// </summary>
        public string ItemName { get; }

        /// <summary>
        /// Остаток на складе позиции мерча
        /// </summary>
        public int Quantity { get; }
    }
}

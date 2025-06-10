namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models
{
	public class SkuDbModel
	{
		public long Id { get; set; }
		public string Name { get; set; }        // если нужно, можно не читать
		public int ItemTypeId { get; set; }
		public int? ClothingSize { get; set; }
		public int PresetTypeId { get; set; }
	}
}

using OzonEdu.MerchandiseService.Domain.Root;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPreset
{
	public class PresetType : Enumeration
	{
		public PresetType(int id, string name) : base(id, name)
		{


		}
	}
}

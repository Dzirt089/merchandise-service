using System.Diagnostics;

namespace OzonEdu.MerchandiseService.Domain.Root.Diagnostics
{
	public static class MerchandiseTelemetry
	{
		public const string SourceName = "OzonEdu.MerchandiseService";

		public static ActivitySource ActivitySource { get; } = new(SourceName);
	}
}

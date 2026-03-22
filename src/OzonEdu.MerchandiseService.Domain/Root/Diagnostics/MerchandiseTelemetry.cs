using System.Diagnostics;

namespace OzonEdu.MerchandiseService.Domain.Root.Diagnostics
{
    //TODO: Глянуть по глубже. Для чего, почему именно такая реализация, что это даёт?
    public static class MerchandiseTelemetry
    {
        public const string SourceName = "OzonEdu.MerchandiseService";

        public static ActivitySource ActivitySource { get; } = new(SourceName);
    }
}

using Confluent.Kafka;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using OzonEdu.MerchandiseService.Infrastructure.Kafka;

namespace OzonEdu.MerchandiseService.Infrastructure.HealthChecks
{
	public sealed class KafkaHealthCheck : IHealthCheck
	{
		private readonly IOptions<KafkaOptions> _kafkaOptions;

		public KafkaHealthCheck(IOptions<KafkaOptions> kafkaOptions)
		{
			_kafkaOptions = kafkaOptions;
		}

		public Task<HealthCheckResult> CheckHealthAsync(
			HealthCheckContext context,
			CancellationToken cancellationToken = default)
		{
			var options = _kafkaOptions.Value;
			if (!options.Enabled)
			{
				return Task.FromResult(HealthCheckResult.Healthy("Kafka integration is disabled."));
			}

			try
			{
				var adminConfig = new AdminClientConfig
				{
					BootstrapServers = options.BootstrapServers,
					SocketTimeoutMs = 5000
				};

				using var adminClient = new AdminClientBuilder(adminConfig).Build();
				var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

				return Task.FromResult(metadata.Brokers.Count > 0
					? HealthCheckResult.Healthy("Kafka broker is reachable.")
					: HealthCheckResult.Unhealthy("Kafka broker metadata is empty."));
			}
			catch (Exception exception)
			{
				return Task.FromResult(HealthCheckResult.Unhealthy("Kafka health check failed.", exception));
			}
		}
	}
}

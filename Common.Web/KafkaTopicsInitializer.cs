using Confluent.Kafka;
using Confluent.Kafka.Admin;

using Microsoft.Extensions.Logging;

namespace Common.Web;

public class KafkaTopicsInitializer
{
    private readonly IAdminClient _adminClient;
    private readonly ILogger<KafkaTopicsInitializer> _logger;

    public KafkaTopicsInitializer(string bootstrapServers, ILogger<KafkaTopicsInitializer> logger)
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        };

        _adminClient = new AdminClientBuilder(config).Build();
        _logger = logger;
    }

    public async Task EnsureTopicExistsAsync(string topicName, int partitions = 1, short replicationFactor = 1)
    {
        try
        {
            var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            bool exists = metadata.Topics.Any(t => t.Topic == topicName);

            if (exists)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation($"[Kafka] Topic '{topicName}' already exists.");
                return;
            }

            await _adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = partitions,
                    ReplicationFactor = replicationFactor
                }
            });

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"[Kafka] Topic '{topicName}' created.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[Kafka] Failed to create topic '{topicName}': {ex.Message}");
        }
    }
}

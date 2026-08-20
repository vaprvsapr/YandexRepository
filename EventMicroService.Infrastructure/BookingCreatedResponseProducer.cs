using Confluent.Kafka;
using Contracts;
using System.Text.Json;

namespace EventMicroService.Infrastructure;

public class BookingCreatedResponseProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic = Topics.BookingCreatedResponseTopic;
    public BookingCreatedResponseProducer(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config)
            .Build();
    }
    public async Task PublishAsync(BookingCreatedResponse bookingCreatedResponse)
    {
        var result = await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = bookingCreatedResponse.BookingId.ToString(),
            Value = JsonSerializer.Serialize(bookingCreatedResponse)
        });
    }
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}


using Confluent.Kafka;
using Contracts;
using System.Text.Json;

namespace EventMicroService.Infrastructure;

public class BookingProcessResponseProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic = Topics.BookingProcessResponse;
    public BookingProcessResponseProducer(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config)
            .Build();
    }
    public async Task PublishAsync(BookingProcessResponse bookingProcessResponse)
    {
        var result = await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = bookingProcessResponse.BookingId.ToString(),
            Value = JsonSerializer.Serialize(bookingProcessResponse)
        });
    }
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}


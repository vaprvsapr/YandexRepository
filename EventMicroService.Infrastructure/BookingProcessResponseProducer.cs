using Confluent.Kafka;
using Common;
using System.Text.Json;
using EventMicroService.Application;

namespace EventMicroService.Infrastructure;

public class BookingProcessResponseProducer: IDisposable
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
        await _producer.ProduceAsync(_topic, new Message<string, string>
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


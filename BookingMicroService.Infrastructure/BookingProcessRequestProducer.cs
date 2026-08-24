using BookingMicroService.Application;
using Confluent.Kafka;
using Common.Contracts;
using System.Text.Json;

namespace BookingMicroService.Infrastructure;

public class BookingProcessRequestProducer : IBookingProcessRequestProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic = Topics.BookingProcessRequest;
    public BookingProcessRequestProducer(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config)
            .Build();
    }
    public async Task PublishAsync(BookingProcessRequest bookingCreated)
    {
        await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = bookingCreated.EventId.ToString(),
            Value = JsonSerializer.Serialize(bookingCreated)
        });
    }
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
        GC.SuppressFinalize(this);
    }
}

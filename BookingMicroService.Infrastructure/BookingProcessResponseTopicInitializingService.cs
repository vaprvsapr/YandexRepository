using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BookingMicroService.Infrastructure;

internal class BookingProcessResponseTopicInitializingService(
    KafkaTopicsInitializer initializer) : IHostedService
{
    private readonly KafkaTopicsInitializer _initializer = initializer;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _initializer.EnsureTopicExistsAsync(Topics.BookingProcessResponse);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
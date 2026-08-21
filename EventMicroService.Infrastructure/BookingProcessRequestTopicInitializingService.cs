using Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EventMicroService.Infrastructure;

internal class BookingProcessRequestTopicInitializingService(
    KafkaTopicsInitializer initializer) : IHostedService
{
    private readonly KafkaTopicsInitializer _initializer = initializer;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _initializer.EnsureTopicExistsAsync(Topics.BookingProcessRequest);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
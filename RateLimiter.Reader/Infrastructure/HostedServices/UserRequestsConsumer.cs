using System.Text.Json;
using Confluent.Kafka;
using RateLimiter.Reader.Application.Abstractions.Services;
using RateLimiter.Reader.Domain;

namespace RateLimiter.Reader.Infrastructure.HostedServices;

public class UserRequestsConsumer(IConfiguration configuration, IRequestProcessingService processingService) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = configuration["Kafka:BootstrapServers"],
                    GroupId = configuration["Kafka:GroupId"],
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };
                
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                
                var topic = configuration["Kafka:Topic"];
                consumer.Subscribe(topic);

                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var message = consumer.Consume(cancellationToken);

                        var request = JsonSerializer.Deserialize<UserRequestEvent>(message.Message.Value) ??
                                      throw new JsonException("Пустое событие.");

                        await processingService.ProcessAsync(request, cancellationToken);

                        consumer.Commit(message);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {

                }
                finally
                {
                    consumer.Close();
                }
            }
            , cancellationToken);
    }
}
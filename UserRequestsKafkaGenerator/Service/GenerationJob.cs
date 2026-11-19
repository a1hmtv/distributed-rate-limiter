using System.Text.Json;
using Confluent.Kafka;
using UserRequestsKafkaGenerator.Models;

namespace UserRequestsKafkaGenerator.Service;

public class GenerationJob(
    IProducer<Null, string> producer, 
    GenerationSettings settings,
    string topic,
    CancellationToken cancellationToken)
{
    public async Task SendAsync()
    {
        if (settings.Rpm <= 0) throw new ArgumentException("RPM должен быть больше нуля.");
        
        var interval = TimeSpan.FromSeconds(60.0 / settings.Rpm);

        var message = new UserRequestEvent()
        {
            UserId = settings.UserId,
            Endpoint = settings.Endpoint
        };

        var value = JsonSerializer.Serialize(message);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await producer.ProduceAsync(
                    topic, 
                    new Message<Null, string>
                    {
                        Value = value
                    },
                    cancellationToken);
            }
            catch (ProduceException<Null, string> exception)
            {
                Console.WriteLine(exception.Error.Reason);
            }

            await Task.Delay(interval, cancellationToken);
        }
    }
}
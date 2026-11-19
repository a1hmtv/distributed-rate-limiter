using System.Globalization;
using Confluent.Kafka;
using UserRequestsKafkaGenerator.Models;
using UserRequestsKafkaGenerator.Service;

var config = new ProducerConfig
{
    BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092", MessageTimeoutMs = 5000
};

using var producer = new ProducerBuilder<Null, string>(config).Build();
var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ?? "user-requests";
var service = new GeneratorService(producer, topic);
using var shutdown = new CancellationTokenSource();

ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};
Console.CancelKeyPress += cancelHandler;

try
{
    Console.WriteLine($"Kafka Messages Generator: {config.BootstrapServers}, topic: {topic}");
    PrintHelp();

    while (!shutdown.IsCancellationRequested)
    {
        Console.Write("> ");
        var line = await Console.In.ReadLineAsync().WaitAsync(shutdown.Token);
        if (line is null) break;

        var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) continue;

        try
        {
            switch (parts[0].ToLowerInvariant())
            {
                case "add":
                    RequireArguments(parts, 5, "add <id> <user_id> <endpoint> <rpm>");
                    service.AddJob(parts[1], ParseSettings(parts));
                    Console.WriteLine($"Задание '{parts[1]}' добавлено.");
                    break;

                case "update":
                    RequireArguments(parts, 5, "update <id> <user_id> <endpoint> <rpm>");
                    await service.UpdateJobAsync(parts[1], ParseSettings(parts));
                    Console.WriteLine($"Задание '{parts[1]}' обновлено.");
                    break;

                case "remove":
                    RequireArguments(parts, 2, "remove <id>");
                    await service.RemoveJobAsync(parts[1]);
                    Console.WriteLine($"Задание '{parts[1]}' удалено.");
                    break;

                case "list":
                    RequireArguments(parts, 1, "list");
                    var jobs = service.GetAll();
                    if (jobs.Count == 0)
                    {
                        Console.WriteLine("Заданий нет.");
                        break;
                    }

                    Console.WriteLine("ID\tUSER_ID\tENDPOINT\tRPM");
                    foreach (var (id, settings) in jobs.OrderBy(pair => pair.Key))
                    {
                        Console.WriteLine($"{id}\t{settings.UserId}\t{settings.Endpoint}\t{settings.Rpm}");
                    }
                    break;

                case "help":
                    RequireArguments(parts, 1, "help");
                    PrintHelp();
                    break;

                case "exit":
                    RequireArguments(parts, 1, "exit");
                    shutdown.Cancel();
                    break;

                default:
                    Console.WriteLine("Неизвестная команда. Введите help для справки.");
                    break;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
        }
    }
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    // Контрл Ц стопает ожидание ввода
}
finally
{
    Console.WriteLine("Остановка генератора...");
    try
    {
        await service.StopAllAsync();
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Ошибка при остановке заданий: {exception.Message}");
        Environment.ExitCode = 1;
    }
    finally
    {
        try
        {
            var pending = producer.Flush(TimeSpan.FromSeconds(5));
            if (pending > 0)
            {
                Console.Error.WriteLine($"Не подтверждена доставка сообщений: {pending}.");
                Environment.ExitCode = 1;
            }
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
        }
    }
}

static GenerationSettings ParseSettings(string[] parts)
{
    if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId))
        throw new ArgumentException("user_id должен быть целым числом в диапазоне Int32.");

    if (!int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var rpm)
        || rpm <= 0)
        throw new ArgumentException("RPM должен быть целым числом больше нуля.");

    return new GenerationSettings(userId, parts[3], rpm);
}

static void RequireArguments(string[] parts, int count, string usage)
{
    if (parts.Length != count)
        throw new ArgumentException($"Формат команды: {usage}");
}

static void PrintHelp()
{
    Console.WriteLine("""
        add <id> <user_id> <endpoint> <rpm>     Добавить задание
        update <id> <user_id> <endpoint> <rpm>  Изменить задание
        remove <id>                            Удалить задание
        list                                   Показать задания
        help                                   Показать справку
        exit                                   Остановить задания и выйти
        Пример: add job1 123 GetUserById 10
        ID задания уникален; один user_id может использоваться в нескольких заданиях.
        Endpoint указывается одним словом без пробелов.
        """);
}

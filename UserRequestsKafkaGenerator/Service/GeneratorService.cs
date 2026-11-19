using Confluent.Kafka;
using UserRequestsKafkaGenerator.Models;

namespace UserRequestsKafkaGenerator.Service;

public class GeneratorService(
    IProducer<Null, string> producer, 
    string topic)
{
    private record RunningJob(
        GenerationSettings Settings,
        CancellationTokenSource Stop,
        Task Task);
    
    private readonly Dictionary<string, RunningJob> _jobs = new();


    public void AddJob(string id, GenerationSettings settings)
    {
        Validate(id, settings);

        if (_jobs.ContainsKey(id)) throw new ArgumentException($"Задание '{id}' уже существует.");

        var stop = new CancellationTokenSource();
        
        var job = new GenerationJob(producer, settings, topic, stop.Token);
        
        var task = RunJobAsync(job, stop.Token);
        
        _jobs.Add(id, new RunningJob(settings, stop, task));
    }
    
    public async Task RemoveJobAsync(string id)
    {
        if (!_jobs.TryGetValue(id, out var job)) throw new ArgumentException($"Задание '{id}' не найдено.");

        job.Stop.Cancel();

        try
        {
            await job.Task;
        }
        finally
        {
            job.Stop.Dispose();
            _jobs.Remove(id);
        }
    }

    public async Task UpdateJobAsync(
        string id,
        GenerationSettings settings)
    {
        Validate(id, settings);
        
        await RemoveJobAsync(id);
        
        AddJob(id, settings);
    }
        
    public IReadOnlyDictionary<string, GenerationSettings> GetAll()
    {
        return _jobs.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Settings);
    }
    
    public async Task StopAllAsync()
    {
        foreach (var job in _jobs.Values)
        {
            job.Stop.Cancel();
        }

        try
        {
            await Task.WhenAll(_jobs.Values.Select(job => job.Task));
        }
        finally
        {
            foreach (var job in _jobs.Values)
            {
                job.Stop.Dispose();
            }

            _jobs.Clear();
        }
    }
    
    
    
    private static async Task RunJobAsync(
        GenerationJob job,
        CancellationToken cancellationToken)
    {
        try
        {
            await job.SendAsync();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested){}
        
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }
    }
    
    private static void Validate(
        string id,
        GenerationSettings settings)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id не должен быть пустым.");

        if (settings.Rpm <= 0)
            throw new ArgumentException("RPM должен быть больше нуля.");

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
            throw new ArgumentException("Endpoint не должен быть пустым.");
    }   
    
}
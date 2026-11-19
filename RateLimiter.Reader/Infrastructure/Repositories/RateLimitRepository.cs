using System.Runtime.CompilerServices;
using MongoDB.Driver;
using RateLimiter.Reader.Application.Abstractions.Repositories;
using RateLimiter.Reader.Domain;
using RateLimiter.Reader.Infrastructure.DbModels;
using RateLimiter.Reader.Infrastructure.Factories;

namespace RateLimiter.Reader.Infrastructure.Repositories;

public class RateLimitRepository(
    IMongoCollection<DbRateLimitModel> rateLimits,
    LimitMappingFactory mapper,
    ChangeMappingFactory changeMapper) : IRateLimitRepository
{
    public async IAsyncEnumerable<RateLimit> GetAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var filter = Builders<DbRateLimitModel>.Filter
            .Eq(x => x.IsDeleted, false);

        var options = new FindOptions<DbRateLimitModel>
        {
            BatchSize = 1000,
        };

        using var cursor = await rateLimits.FindAsync(
            filter,
            options,
            cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var limit in cursor.Current)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return mapper.ToDomain(limit);
            }
        }
    }

    public async IAsyncEnumerable<RateLimitChange> WatchChangesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var pipeline =
            new EmptyPipelineDefinition<ChangeStreamDocument<DbRateLimitModel>>()
                .Match(change =>
                    change.OperationType == ChangeStreamOperationType.Insert ||
                    change.OperationType == ChangeStreamOperationType.Update);

        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
        };

        using var cursor = await rateLimits.WatchAsync(
            pipeline,
            options,
            cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var change in cursor.Current)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var model = change.FullDocument;

                yield return changeMapper.ToDomain(model);
            }
        }
    }
}
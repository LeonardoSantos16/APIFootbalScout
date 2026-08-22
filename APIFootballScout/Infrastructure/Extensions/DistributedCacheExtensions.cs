using Microsoft.Extensions.Caching.Distributed;
using Refit;
using System.Text.Json;

namespace APIFootballScout.Infrastructure.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T?> GetOrFetchAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<IApiResponse<T>>> fetch,
        TimeSpan? expiration = null) where T : class
        {

            if (expiration == null)
            {
                var directResponse = await fetch();
                return directResponse.IsSuccessStatusCode ? directResponse.Content : null;
            }

            var cached = await cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<T>(cached);

            var response = await fetch();
            if (!response.IsSuccessStatusCode || response.Content == null)
                return null;

            await cache.SetStringAsync(key, JsonSerializer.Serialize(response.Content),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration.Value });

            return response.Content;
        }
    }
}

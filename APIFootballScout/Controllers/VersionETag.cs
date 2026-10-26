using Microsoft.Net.Http.Headers;

namespace APIFootballScout.Controllers
{
    internal static class VersionETag
    {
        public static void Set(HttpResponse response, long version)
            => response.Headers.ETag = new EntityTagHeaderValue($"\"{version}\"").ToString();

        public static bool TryParse(string? ifMatch, out long version)
        {
            version = 0;

            return EntityTagHeaderValue.TryParse(ifMatch, out var etag)
                && !etag.IsWeak
                && long.TryParse(etag.Tag.Value?.Trim('"'), out version);
        }
    }
}
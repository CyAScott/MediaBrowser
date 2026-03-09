using System.Net.Http.Headers;
using System.Net.Http.Json;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace MediaBrowser.Media;

public class MediaClient(HttpClient client)
{
    public Task<HttpResponseMessage<MediaReadModel>> Get(Guid id) =>
        client.GetAsync<MediaReadModel>($"/api/media/{id}");

    public Task<HttpResponseMessage<MediaReadModel>> Update(Guid id, UpdateMediaRequest request) =>
        client.PutAsync<MediaReadModel, UpdateMediaRequest>($"/api/media/{id}", request);

    public Task<HttpResponseMessage<SearchResponse>> Search(SearchRequest request)
    {
        var query = new Dictionary<string, object?>
        {
            {nameof(request.Cast), request.Cast},
            {nameof(request.Descending), request.Descending},
            {nameof(request.Directors), request.Directors},
            {nameof(request.Genres), request.Genres},
            {nameof(request.Keywords), request.Keywords},
            {nameof(request.Producers), request.Producers},
            {nameof(request.Skip), request.Skip},
            {nameof(request.Sort), request.Sort},
            {nameof(request.Take), request.Take},
            {nameof(request.Writers), request.Writers}
        };
        var queryString = string.Join('&', query
            .Where(it => it.Value != null)
            .Select(it => $"{it.Key}={Uri.EscapeDataString(it.Value!.ToString()!)}"));
        return client.GetAsync<SearchResponse>($"/api/media/search?{queryString}");
    }

    public Task<HttpResponseMessage<IReadOnlyList<string>>> GetAll(TagType tagType) =>
        client.GetAsync<IReadOnlyList<string>>($"/api/media/{tagType}s");

    public Task<HttpResponseMessage> GetThumbnail(TagType tagType, string name, DateTimeOffset? lastModified = null) =>
        client.SendAsync(new(HttpMethod.Get, $"/api/media/{tagType}/{name}/thumbnail")
        {
            Headers =
            {
                IfModifiedSince = lastModified
            }
        });

    public async Task<HttpResponseMessage> Stream(Guid id,
        DateTimeOffset? lastModified = null,
        RangeHeaderValue? rangeHeaderValue = null,
        string? etag = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/media/{id}/file")
        {
            Headers =
            {
                IfModifiedSince = lastModified,
                Range = rangeHeaderValue
            }
        };
        if (etag != null)
        {
            request.Headers.Add(HeaderNames.IfNoneMatch, etag);
        }
        return await client.SendAsync(request);
    }

    public async Task<HttpResponseMessage> StreamFanartThumbnail(Guid id, DateTimeOffset? lastModified = null) =>
        await client.SendAsync(new(HttpMethod.Get, $"/api/media/{id}/file/thumbnail-fanart")
        {
            Headers =
            {
                IfModifiedSince = lastModified
            }
        });

    public Task<HttpResponseMessage> UpdateFanartThumbnailWithTimestamp(Guid id, UpdateThumbnailRequest request) =>
        client.PostAsJsonAsync($"/api/media/{id}/file/thumbnail-fanart", request);

    public async Task<HttpResponseMessage> StreamThumbnail(Guid id, DateTimeOffset? lastModified = null) =>
        await client.SendAsync(new(HttpMethod.Get, $"/api/media/{id}/file/thumbnail")
        {
            Headers =
            {
                IfModifiedSince = lastModified
            }
        });

    public Task<HttpResponseMessage> UpdateThumbnailWithTimestamp(Guid id, UpdateThumbnailRequest request) =>
        client.PostAsJsonAsync($"/api/media/{id}/file/thumbnail", request);

    public Task<HttpResponseMessage> UpdateThumbnailWithFile(Guid id, Stream thumbnail, string thumbnailFileName, bool isPrimary) =>
        client.PostAsync($"/api/media/{id}/file/thumbnail/file", new MultipartFormDataContent
        {
            {new StreamContent(thumbnail), "thumbnail", thumbnailFileName},
            {new StringContent(isPrimary.ToString()), "is_primary"}
        });
}
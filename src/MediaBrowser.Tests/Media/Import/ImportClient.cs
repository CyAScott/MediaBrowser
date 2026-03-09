namespace MediaBrowser.Media.Import;

public class ImportClient(HttpClient client)
{
    public Task<HttpResponseMessage<IReadOnlyList<ImportFileInfo>>> GetFiles() =>
        client.GetAsync<IReadOnlyList<ImportFileInfo>>("/api/import/files");

    public async Task<HttpResponseMessage> ReadFile(string name, DateTimeOffset? lastModified = null) =>
        await client.SendAsync(new(HttpMethod.Get, $"/api/import/file/{HttpUtility.UrlPathEncode(name)}")
        {
            Headers =
            {
                IfModifiedSince = lastModified
            }
        });

    public Task<HttpResponseMessage<ImportFileInfo>> ReadFileInfo(string name) =>
        client.GetAsync<ImportFileInfo>($"/api/import/file/{HttpUtility.UrlPathEncode(name)}/info");

    public Task<HttpResponseMessage<MediaReadModel>> Import(string name, ImportMediaRequest request) =>
        client.PostAsync<MediaReadModel, ImportMediaRequest>($"/api/import/file/{HttpUtility.UrlPathEncode(name)}", request);
}
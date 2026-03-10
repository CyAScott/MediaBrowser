namespace MediaBrowser;

[DebuggerStepThrough]
public static class HttpClientExtensions
{
    extension(HttpClient client)
    {
        public async Task<HttpResponseMessage<TResponse>> GetAsync<TResponse>(string requestUri)
            where TResponse : class
        {
            var message = await client.GetAsync(requestUri);
            return new(message, message.IsSuccessStatusCode ? await message.Content.ReadFromJsonAsync<TResponse>().ShouldNotBeNull() : null);
        }

        public async Task<HttpResponseMessage<TResponse>> PostAsync<TResponse, TRequest>(string requestUri, TRequest request)
            where TResponse : class
        {
            var message = await client.PostAsJsonAsync(requestUri, request);
            return new(message, message.IsSuccessStatusCode ? await message.Content.ReadFromJsonAsync<TResponse>().ShouldNotBeNull() : null);
        }

        public async Task<HttpResponseMessage<TResponse>> PutAsync<TResponse, TRequest>(string requestUri, TRequest request)
            where TResponse : class
        {
            var message = await client.PutAsJsonAsync(requestUri, request);
            return new(message, message.IsSuccessStatusCode ? await message.Content.ReadFromJsonAsync<TResponse>().ShouldNotBeNull() : null);
        }
    }
}

[DebuggerStepThrough]
public class HttpResponseMessage<TResponse>(HttpResponseMessage response, TResponse? content = default) : IDisposable
{
    public HttpResponseHeaders Headers => response.Headers;
    public HttpStatusCode StatusCode => response.StatusCode;
    public TResponse? Content => content;
    public async Task EnsureSuccessStatusCode() => response.StatusCode.ShouldBeOneOf([HttpStatusCode.OK, HttpStatusCode.NoContent], await response.Content.ReadAsStringAsync());
    public void Dispose() => response.Dispose();
}
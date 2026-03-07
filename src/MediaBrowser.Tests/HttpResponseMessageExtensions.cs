using System.Text.Json;

namespace MediaBrowser;

public static class HttpResponseMessageExtensions
{
    extension(HttpResponseMessage response)
    {
        public async Task<T> Read<T>()
            where T : class =>
            JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync()).ShouldNotBeNull();
    }
}
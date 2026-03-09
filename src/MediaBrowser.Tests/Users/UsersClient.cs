using System.Net.Http.Json;
using Microsoft.Net.Http.Headers;

namespace MediaBrowser.Users;

public class UsersClient(HttpClient client)
{
    public Task<HttpResponseMessage<UserReadModel>> Login(UserLoginRequest request) =>
        client.PostAsync<UserReadModel, UserLoginRequest>("/api/users/login", request);

    public Task<HttpResponseMessage> Logout(string jwt)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users/logout")
        {
            Headers =
            {
                {HeaderNames.Cookie, $"{JwtCookieMiddleware.CookieName}={jwt}"}
            }
        };
        return client.PostAsync("/api/users/logout", null);
    }

    public Task<HttpResponseMessage<UserReadModel>> Me() =>
        client.GetAsync<UserReadModel>("/api/users/me");

    public Task<HttpResponseMessage<UserReadModel>> Register(UserRegisterRequest request) =>
        client.PostAsync<UserReadModel, UserRegisterRequest>("/api/users/register", request);

    public Task<HttpResponseMessage> ChangePassword(ChangePasswordRequest request) =>
        client.PutAsJsonAsync("/api/users/me/password", request);

    public Task<HttpResponseMessage<IReadOnlyList<UserReadModel>>> GetUsers() =>
        client.GetAsync<IReadOnlyList<UserReadModel>>("/api/users");

    public Task<HttpResponseMessage> DeleteUser(Guid id) =>
        client.DeleteAsync($"/api/users/{id}");
}
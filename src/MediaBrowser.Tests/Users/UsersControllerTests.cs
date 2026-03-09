using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;

// ReSharper disable AccessToDisposedClosure

namespace MediaBrowser.Users;

[TestFixture]
public class UsersControllerTests
{
    [Test(Description = "Test CRUD user APIs.")]
    public async Task Test()
    {
        await using var factory = new MediaBrowserWebApplicationFactory();
        await factory.StartServerAsync();

        using var client = factory.CreateClient();
        using var clientForLogin = factory.CreateClient();

        var usersClient = new UsersClient(client);
        var usersClientForLogin = new UsersClient(clientForLogin);

        var adminUser = await GetAdminUser();
        async Task<UserReadModel> GetAdminUser()
        {
            using var scope = factory.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            var user = (await db.Users.FirstAsync()).ToReadModel();
            client.DefaultRequestHeaders.Authorization = new("Bearer", factory.GetJwtForTestUser(user));
            return user;
        }

        await MeTest();
        async Task MeTest()
        {
            var response = await usersClient.Me();
            await response.EnsureSuccessStatusCode();
            response.Content.ShouldNotBeNull().Username.ShouldBe(adminUser.Username);
        }

        const string newUserName = "new-user";
        var newUser = await RegisterUserTest();
        async Task<UserReadModel> RegisterUserTest()
        {
            var response = await usersClient.Register(new()
            {
                Username = newUserName,
                Password = _validPassword
            });
            await response.EnsureSuccessStatusCode();
            response.Content.ShouldNotBeNull().Username.ShouldBe(newUserName);
            response.Content.Id.ShouldNotBe(Guid.Empty);
            return response.Content;
        }

        await DuplicateRegisterTest();
        async Task DuplicateRegisterTest()
        {
            var response = await usersClient.Register(new()
            {
                Username = newUserName,
                Password = _validPassword
            });
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        }

        var newUserJwt = await LoginTest();
        async Task<string> LoginTest()
        {
            var response = await usersClientForLogin.Login(new()
            {
                Username = newUserName,
                Password = _validPassword
            });
            await response.EnsureSuccessStatusCode();
            response.Content.ShouldNotBeNull();
            response.Content.ShouldBe(newUser);

            var (jwt, expiresOn) = GetCookie(response.Headers);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var jwtExp = token.ValidTo;
            jwtExp.ShouldBe(expiresOn.UtcDateTime, "JWT expiration should match cookie expiration");

            return jwt;
        }

        await InvalidLoginTest();
        async Task InvalidLoginTest()
        {
            var response = await usersClientForLogin.Login(new()
            {
                Username = newUserName,
                Password = $"wrong{_validPassword}"
            });
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        await LogoutTest();
        async Task LogoutTest()
        {
            var response = await usersClientForLogin.Logout(newUserJwt);
            response.EnsureSuccessStatusCode();
            var (cookie, expiresOn) = GetCookie(response.Headers);
            cookie.ShouldBe("logout");
            expiresOn.ShouldBeLessThan(DateTimeOffset.UtcNow);
        }

        await ChangePasswordTest();
        async Task ChangePasswordTest()
        {
            var response = await usersClient.ChangePassword(new()
            {
                OldPassword = _validPassword,
                NewPassword = $"new-{_validPassword}"
            });
            response.EnsureSuccessStatusCode();
        }

        await InvalidChangePasswordTest();
        async Task InvalidChangePasswordTest()
        {
            var response = await usersClient.ChangePassword(new()
            {
                OldPassword = $"wrong-{_validPassword}",
                NewPassword = _validPassword
            });
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        await GetUsersTest();
        async Task GetUsersTest()
        {
            var response = await usersClient.GetUsers();
            await response.EnsureSuccessStatusCode();
            response.Content.ShouldNotBeNull().Count.ShouldBe(2, "There should be the admin user and the test user created in this test.");
            response.Content.ShouldContain(adminUser);
            response.Content.ShouldContain(newUser);
        }

        await DeleteUserTest();
        async Task DeleteUserTest()
        {
            var response = await usersClient.DeleteUser(newUser.Id);
            response.EnsureSuccessStatusCode();
        }

        await DeleteNonExistentUserTest();
        async Task DeleteNonExistentUserTest()
        {
            var response = await usersClient.DeleteUser(Guid.NewGuid());
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }
    }

    (string Jwt, DateTimeOffset ExpiresOn) GetCookie(HttpResponseHeaders header)
    {
        header.TryGetValues(HeaderNames.SetCookie, out var cookies).ShouldBeTrue();
        var cookie = cookies.ShouldNotBeNull().SingleOrDefault(c => c.StartsWith(JwtCookieMiddleware.CookieName)).ShouldNotBeNull();
        var parts = cookie.Split('=', 2).Skip(1).SingleOrDefault().ShouldNotBeNull().Split(';', StringSplitOptions.TrimEntries);
        parts.ShouldNotBeEmpty();

        var jwt = parts[0];
        var expiresOn = DateTimeOffset.Parse(parts.SingleOrDefault(p => p.StartsWith("Expires=", StringComparison.OrdinalIgnoreCase))
            .ShouldNotBeNull()
            .Split('=', 2)
            .LastOrDefault().ShouldNotBeNull(), CultureInfo.InvariantCulture);

        return (jwt, expiresOn);
    }

    const string _validPassword = "P@ssword1234";
    async Task<UserReadModel> AddTestUser(UsersClient usersClient)
    {
        var registerResponse = await usersClient.Register(new()
        {
            Username = "test-user",
            Password = _validPassword
        });
        await registerResponse.EnsureSuccessStatusCode();
        return registerResponse.Content.ShouldNotBeNull();
    }
}
using System.Net;
using System.Net.Http.Json;
using MemoBoard.Core.Dtos;

namespace MemoBoard.Api.Tests
{
    public class AccountsTests : IClassFixture<ApiFactory>
    {
        private readonly HttpClient _client;

        public AccountsTests(ApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private Task<HttpResponseMessage> RegisterAsync(string userName, string password) =>
            _client.PostAsJsonAsync("/api/accounts", new { userName, password });

        private Task<HttpResponseMessage> LoginAsync(string userName, string password) =>
            _client.PostAsJsonAsync("/api/accounts/login", new { userName, password });

        [Fact]
        public async Task Registering_answers_201_and_the_name_is_kept_in_lower_case()
        {
            var created = await RegisterAsync("Alice.One", "correct horse battery");
            var login = await LoginAsync("alice.one", "correct horse battery");

            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var session = await login.Content.ReadFromJsonAsync<SessionDto>();
            Assert.Equal("alice.one", session!.UserName);
        }

        [Fact]
        public async Task A_name_already_taken_answers_409_whatever_its_case()
        {
            await RegisterAsync("bob", "correct horse battery");

            var again = await RegisterAsync("BOB", "another password");

            Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        }

        [Theory]
        [InlineData("ab", "long enough password")]
        [InlineData("has space", "long enough password")]
        [InlineData("fine.name", "short")]
        [InlineData("", "long enough password")]
        public async Task A_bad_name_or_password_answers_400(string userName, string password)
        {
            var response = await RegisterAsync(userName, password);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Logging_in_returns_a_token_that_opens_the_memos()
        {
            await RegisterAsync("carol", "correct horse battery");

            var login = await LoginAsync("carol", "correct horse battery");
            var session = await login.Content.ReadFromJsonAsync<SessionDto>();

            Assert.NotNull(session);
            Assert.False(string.IsNullOrEmpty(session.Token));
            Assert.True(session.ExpiresAt > DateTime.UtcNow.AddMinutes(50));
            Assert.NotNull(session.LastLoginAt);

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/memos");
            request.Headers.Authorization = new("Bearer", session.Token);
            var memos = await _client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, memos.StatusCode);
        }

        [Fact]
        public async Task A_wrong_password_and_an_unknown_name_get_the_same_401()
        {
            await RegisterAsync("dave", "correct horse battery");

            var wrongPassword = await LoginAsync("dave", "wrong password");
            var unknownName = await LoginAsync("nobody", "wrong password");

            Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, unknownName.StatusCode);
            Assert.Equal(
                await wrongPassword.Content.ReadAsStringAsync(),
                await unknownName.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Too_many_login_attempts_answer_429()
        {
            using var factory = new ApiFactory { LoginAttemptsPerMinute = 3 };
            var client = factory.CreateClient();

            var statuses = new List<HttpStatusCode>();
            for (var i = 0; i < 4; i++)
            {
                var response = await client.PostAsJsonAsync("/api/accounts/login", new { userName = "x", password = "y" });
                statuses.Add(response.StatusCode);
            }

            Assert.Equal(
                new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.TooManyRequests },
                statuses);
        }

        [Fact]
        public async Task Every_answer_carries_the_hardening_headers()
        {
            var response = await _client.PostAsJsonAsync("/api/accounts/login", new { userName = "x", password = "y" });

            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
            Assert.Equal("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
            Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        }
    }
}

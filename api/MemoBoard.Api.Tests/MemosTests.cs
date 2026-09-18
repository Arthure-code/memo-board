using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MemoBoard.Core.Dtos;

namespace MemoBoard.Api.Tests
{
    // Two accounts, each with its own client carrying its own token, so
    // that every test can check one cannot reach the other.
    public class MemosTests : IClassFixture<ApiFactory>
    {
        private const string Passphrase = "correct horse battery";
        private readonly ApiFactory _factory;

        public MemosTests(ApiFactory factory)
        {
            _factory = factory;
        }

        private async Task<HttpClient> SignedInAsync(string userName)
        {
            var client = _factory.CreateClient();
            await client.PostAsJsonAsync("/api/accounts", new { userName, password = Passphrase });
            var login = await client.PostAsJsonAsync("/api/accounts/login", new { userName, password = Passphrase });
            var session = await login.Content.ReadFromJsonAsync<SessionDto>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.Token);
            return client;
        }

        private static async Task<MemoDto> AddAsync(HttpClient client, string title, string body = "text")
        {
            var response = await client.PostAsJsonAsync("/api/memos", new { title, body });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<MemoDto>())!;
        }

        [Fact]
        public async Task Without_a_token_every_memo_route_answers_401()
        {
            var anonymous = _factory.CreateClient();

            Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/memos")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/api/memos", new { title = "t", body = "b" })).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.DeleteAsync("/api/memos/1")).StatusCode);
        }

        [Fact]
        public async Task A_forged_token_is_refused()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxIn0.forged");

            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/memos")).StatusCode);
        }

        [Fact]
        public async Task Creating_answers_201_and_the_memo_shows_in_the_list_oldest_first()
        {
            var erin = await SignedInAsync("erin");

            var first = await AddAsync(erin, "First");
            await Task.Delay(20);
            var second = await AddAsync(erin, "Second");
            var list = await erin.GetFromJsonAsync<List<MemoDto>>("/api/memos");

            Assert.NotNull(list);
            Assert.Equal(new[] { first.Id, second.Id }, list.Select(m => m.Id));
            Assert.Equal("First", list[0].Title);
            Assert.True(list[0].CreatedAt <= list[1].CreatedAt);
        }

        [Fact]
        public async Task Each_account_sees_only_its_own_memos()
        {
            var frank = await SignedInAsync("frank");
            var grace = await SignedInAsync("grace");
            await AddAsync(frank, "Mine");
            await AddAsync(grace, "Hers");

            var franksList = await frank.GetFromJsonAsync<List<MemoDto>>("/api/memos");
            var gracesList = await grace.GetFromJsonAsync<List<MemoDto>>("/api/memos");

            Assert.Equal(new[] { "Mine" }, franksList!.Select(m => m.Title));
            Assert.Equal(new[] { "Hers" }, gracesList!.Select(m => m.Title));
        }

        [Fact]
        public async Task A_title_is_unique_per_account_and_free_for_another()
        {
            var henry = await SignedInAsync("henry");
            var iris = await SignedInAsync("iris");
            await AddAsync(henry, "Groceries");

            var again = await henry.PostAsJsonAsync("/api/memos", new { title = "Groceries", body = "b" });
            var hers = await iris.PostAsJsonAsync("/api/memos", new { title = "Groceries", body = "b" });

            Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
            Assert.Equal(HttpStatusCode.Created, hers.StatusCode);
        }

        [Theory]
        [InlineData("", "body")]
        [InlineData("title", "")]
        public async Task An_empty_title_or_body_answers_400(string title, string body)
        {
            var jack = await SignedInAsync("jack");

            var response = await jack.PostAsJsonAsync("/api/memos", new { title, body });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Deleting_my_memo_answers_204_then_404()
        {
            var kate = await SignedInAsync("kate");
            var memo = await AddAsync(kate, "Gone");

            var first = await kate.DeleteAsync($"/api/memos/{memo.Id}");
            var second = await kate.DeleteAsync($"/api/memos/{memo.Id}");

            Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        }

        [Fact]
        public async Task Deleting_someone_elses_memo_answers_404_and_leaves_it_there()
        {
            var liam = await SignedInAsync("liam");
            var mia = await SignedInAsync("mia");
            var hers = await AddAsync(mia, "Private");

            var attempt = await liam.DeleteAsync($"/api/memos/{hers.Id}");
            var stillThere = await mia.GetFromJsonAsync<List<MemoDto>>("/api/memos");

            Assert.Equal(HttpStatusCode.NotFound, attempt.StatusCode);
            Assert.Contains(stillThere!, m => m.Id == hers.Id);
        }
    }
}

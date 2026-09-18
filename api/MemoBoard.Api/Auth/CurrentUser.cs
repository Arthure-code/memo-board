using System.Security.Claims;

namespace MemoBoard.Api.Auth
{
    public static class CurrentUser
    {
        // The account id the token was issued for. Only reachable behind
        // [Authorize], so the claim is always there.
        public static int AccountId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}

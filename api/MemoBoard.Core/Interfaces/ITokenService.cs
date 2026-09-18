using MemoBoard.Core.Entities;

namespace MemoBoard.Core.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) Issue(Account account);
    }
}

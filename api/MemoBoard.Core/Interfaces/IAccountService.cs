using MemoBoard.Core.Dtos;
using MemoBoard.Core.Entities;

namespace MemoBoard.Core.Interfaces
{
    public enum RegisterOutcome
    {
        Created,
        NameTaken,
    }

    public interface IAccountService
    {
        Task<RegisterOutcome> RegisterAsync(RegisterRequest request);

        // The account when the name and password match, null otherwise;
        // the caller never learns which of the two was wrong.
        Task<Account?> AuthenticateAsync(LoginRequest request);
    }
}

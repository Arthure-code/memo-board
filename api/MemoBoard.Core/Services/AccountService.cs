using MemoBoard.Core.Dtos;
using MemoBoard.Core.Entities;
using MemoBoard.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MemoBoard.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAsyncRepository<Account> _accounts;
        private readonly IPasswordHasher<Account> _hasher;

        public AccountService(IAsyncRepository<Account> accounts, IPasswordHasher<Account> hasher)
        {
            _accounts = accounts;
            _hasher = hasher;
        }

        public async Task<RegisterOutcome> RegisterAsync(RegisterRequest request)
        {
            var userName = Normalize(request.UserName);
            if (await _accounts.AnyAsync(a => a.UserName == userName)) return RegisterOutcome.NameTaken;

            var account = new Account { UserName = userName, CreatedAt = DateTime.UtcNow };
            account.PasswordHash = _hasher.HashPassword(account, request.Password);
            await _accounts.AddAsync(account);
            return RegisterOutcome.Created;
        }

        public async Task<Account?> AuthenticateAsync(LoginRequest request)
        {
            var userName = Normalize(request.UserName);
            var matches = await _accounts.ListAsync(a => a.UserName == userName);
            var account = matches.Count == 0 ? null : matches[0];
            if (account is null) return null;

            var result = _hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed) return null;

            account.LastLoginAt = DateTime.UtcNow;
            await _accounts.UpdateAsync(account);
            return account;
        }

        private static string Normalize(string userName)
        {
            return userName.Trim().ToLowerInvariant();
        }
    }
}

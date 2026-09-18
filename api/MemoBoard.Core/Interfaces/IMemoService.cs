using MemoBoard.Core.Dtos;

namespace MemoBoard.Core.Interfaces
{
    public enum CreateMemoOutcome
    {
        Created,
        TitleTaken,
    }

    // Every operation takes the account it acts for: a memo is only ever
    // read, created or deleted by its owner.
    public interface IMemoService
    {
        Task<IReadOnlyList<MemoDto>> ListAsync(int accountId);
        Task<(CreateMemoOutcome Outcome, MemoDto? Memo)> CreateAsync(int accountId, NewMemoRequest request);
        Task<bool> DeleteAsync(int accountId, int memoId);
    }
}

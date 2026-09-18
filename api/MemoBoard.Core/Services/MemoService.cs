using MemoBoard.Core.Dtos;
using MemoBoard.Core.Entities;
using MemoBoard.Core.Interfaces;

namespace MemoBoard.Core.Services
{
    public class MemoService : IMemoService
    {
        private readonly IAsyncRepository<Memo> _memos;

        public MemoService(IAsyncRepository<Memo> memos)
        {
            _memos = memos;
        }

        public async Task<IReadOnlyList<MemoDto>> ListAsync(int accountId)
        {
            var memos = await _memos.ListAsync(m => m.AccountId == accountId);
            return memos.OrderBy(m => m.CreatedAt).Select(ToDto).ToList();
        }

        // A title is unique for its owner, not for the whole board.
        public async Task<(CreateMemoOutcome Outcome, MemoDto? Memo)> CreateAsync(int accountId, NewMemoRequest request)
        {
            var title = request.Title.Trim();
            if (await _memos.AnyAsync(m => m.AccountId == accountId && m.Title == title))
            {
                return (CreateMemoOutcome.TitleTaken, null);
            }

            var memo = new Memo
            {
                AccountId = accountId,
                Title = title,
                Body = request.Body.Trim(),
                CreatedAt = DateTime.UtcNow,
            };
            await _memos.AddAsync(memo);
            return (CreateMemoOutcome.Created, ToDto(memo));
        }

        // A memo that belongs to another account does not exist as far as
        // this one can tell: the answer is the same as for a missing id.
        public async Task<bool> DeleteAsync(int accountId, int memoId)
        {
            var memo = await _memos.GetByIdAsync(memoId);
            if (memo is null || memo.AccountId != accountId) return false;

            await _memos.DeleteAsync(memo);
            return true;
        }

        private static MemoDto ToDto(Memo memo)
        {
            return new MemoDto { Id = memo.Id, Title = memo.Title, Body = memo.Body, CreatedAt = memo.CreatedAt };
        }
    }
}

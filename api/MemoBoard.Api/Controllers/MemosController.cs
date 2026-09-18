using MemoBoard.Api.Auth;
using MemoBoard.Core.Dtos;
using MemoBoard.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemoBoard.Api.Controllers
{
    // Every route needs a valid token, and acts only on the memos of the
    // account that token names.
    [ApiController]
    [Authorize]
    [Route("api/memos")]
    [Produces("application/json")]
    public class MemosController : ControllerBase
    {
        private readonly IMemoService _memos;

        public MemosController(IMemoService memos)
        {
            _memos = memos;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<MemoDto>>> List()
        {
            return Ok(await _memos.ListAsync(User.AccountId()));
        }

        [HttpPost]
        public async Task<ActionResult<MemoDto>> Create(NewMemoRequest request)
        {
            var (outcome, memo) = await _memos.CreateAsync(User.AccountId(), request);
            return outcome == CreateMemoOutcome.TitleTaken
                ? Conflict(new { message = "You already have a memo with that title." })
                : StatusCode(StatusCodes.Status201Created, memo);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _memos.DeleteAsync(User.AccountId(), id) ? NoContent() : NotFound();
        }
    }
}

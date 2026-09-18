namespace MemoBoard.Core.Entities
{
    // A note that belongs to one account and is never shown to another.
    public class Memo : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
    }
}

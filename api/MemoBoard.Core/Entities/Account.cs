namespace MemoBoard.Core.Entities
{
    // A person who signs in. Only a hash of the password is ever kept.
    public class Account : BaseEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<Memo> Memos { get; set; } = new();
    }
}

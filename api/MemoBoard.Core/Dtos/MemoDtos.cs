using System.ComponentModel.DataAnnotations;

namespace MemoBoard.Core.Dtos
{
    public class NewMemoRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Body { get; set; } = string.Empty;
    }

    public class MemoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

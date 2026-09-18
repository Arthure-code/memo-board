using System.ComponentModel.DataAnnotations;

namespace MemoBoard.Core.Dtos
{
    // What it takes to open an account: a short handle and a password of
    // at least eight characters. The handle is stored in lower case.
    public class RegisterRequest
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        [RegularExpression("^[A-Za-z0-9._-]+$", ErrorMessage = "Letters, digits, dots, dashes and underscores only.")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    // What a successful login returns: the signed token the client sends
    // back on every call, and when it stops being valid.
    public class SessionDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}

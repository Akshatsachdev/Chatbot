using System.ComponentModel.DataAnnotations; // <-- Important for [Required]

namespace MedicalChatBot.Api.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty; // ✅ Default value to fix CS8618 warning

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty; // ✅ Default value to fix CS8618 warning
    }
}

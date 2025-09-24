using System.ComponentModel.DataAnnotations; // <-- Important for [Required]

namespace MedicalChatBot.Api.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty; // ✅

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty; // ✅

        [Required(ErrorMessage = "Confirm Password is required.")]
        public string ConfirmPassword { get; set; } = string.Empty; // ✅
    }
}

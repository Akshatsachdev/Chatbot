using System;

namespace MedicalChatBot.Api.Models
{
    public class ChatHistory
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        
        public string SessionId { get; set; } = Guid.NewGuid().ToString();  // ✅ Important!
    }
}

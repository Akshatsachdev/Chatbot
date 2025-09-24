using Microsoft.AspNetCore.Mvc;
using MedicalChatBot.Api.Data;
using MedicalChatBot.Api.Models;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using UglyToad.PdfPig;

namespace MedicalChatBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // ✅ Ask a question to the bot
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest message)
        {
            if (string.IsNullOrWhiteSpace(message.UserMessage) || string.IsNullOrWhiteSpace(message.Username))
                return BadRequest(new { message = "Username and message cannot be empty." });

            var sessionId = message.SessionId ?? Guid.NewGuid().ToString(); // Generate session if not sent

            var client = _httpClientFactory.CreateClient();
            var ollamaRequest = new
            {
                model = "llama3",
                prompt = message.UserMessage,
                stream = false,
                temperature = 0.7,
                top_p = 0.9
            };

            var content = new StringContent(JsonSerializer.Serialize(ollamaRequest), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("http://localhost:11434/api/generate", content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to connect to Ollama model.", error = ex.Message });
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { message = $"Ollama error: {errorBody}" });
            }

            var body = await response.Content.ReadAsStringAsync();
            string botReply = "";

            try
            {
                using var doc = JsonDocument.Parse(body);
                botReply = doc.RootElement.GetProperty("response").GetString() ?? "No response.";
            }
            catch
            {
                return StatusCode(500, new { message = "Failed to parse Ollama response." });
            }

            var chat = new ChatHistory
            {
                Username = message.Username,
                Message = message.UserMessage,
                Response = botReply,
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                _context.ChatHistories.Add(chat);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Save Error: {ex.Message}");
                // Do not block the response if DB fails
            }

            return Ok(new { response = botReply, sessionId = sessionId });
        }

        // ✅ Upload a file (.pdf / .txt)
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            string extractedText;

            try
            {
                if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    extractedText = await ExtractTextFromPdf(file);
                }
                else if (file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    extractedText = await reader.ReadToEndAsync();
                }
                else
                {
                    return BadRequest(new { message = "Only PDF and TXT files are supported." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error processing file: {ex.Message}" });
            }

            var client = _httpClientFactory.CreateClient();
            var ollamaRequest = new
            {
                model = "llama3",
                prompt = $"Summarize this document:\n{extractedText.Substring(0, Math.Min(4000, extractedText.Length))}",
                stream = false
            };

            var content = new StringContent(JsonSerializer.Serialize(ollamaRequest), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("http://localhost:11434/api/generate", content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to connect to Ollama model.", error = ex.Message });
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { message = $"Ollama error: {errorBody}" });
            }

            var body = await response.Content.ReadAsStringAsync();
            string botReply = "";

            try
            {
                using var doc = JsonDocument.Parse(body);
                botReply = doc.RootElement.GetProperty("response").GetString() ?? "No response.";
            }
            catch
            {
                return StatusCode(500, new { message = "Failed to parse Ollama response." });
            }

            return Ok(new { response = botReply });
        }

        // ✅ Get all sessions for a user
        [HttpGet("sessions/{username}")]
        public async Task<IActionResult> GetUserSessions(string username)
        {
            var sessions = await _context.ChatHistories
                .Where(x => x.Username == username)
                .GroupBy(x => x.SessionId)
                .Select(g => new
                {
                    SessionId = g.Key,
                    Title = g.OrderBy(x => x.Timestamp).Select(x => x.Message).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(sessions);
        }

        // ✅ Get full chat history of a session
        [HttpGet("history/{sessionId}")]
        public async Task<IActionResult> GetSessionHistory(string sessionId)
        {
            var chats = await _context.ChatHistories
                .Where(x => x.SessionId == sessionId)
                .OrderBy(x => x.Timestamp)
                .Select(x => new
                {
                    message = x.Message,
                    botReply = x.Response
                })
                .ToListAsync();

            return Ok(chats);
        }

        // 📚 Helper method to extract text from PDFs
       private async Task<string> ExtractTextFromPdf(IFormFile file)
{
    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);
    memoryStream.Position = 0; // Reset stream position to beginning

    StringBuilder textBuilder = new StringBuilder();

    using (var pdf = UglyToad.PdfPig.PdfDocument.Open(memoryStream))
    {
        foreach (var page in pdf.GetPages())
        {
            textBuilder.AppendLine(page.Text); // 📄 Extract text page by page
        }
    }

    return textBuilder.ToString();
}


    }

    // DTO class
    public class ChatRequest
    {
        public string Username { get; set; } = string.Empty;
        public string UserMessage { get; set; } = string.Empty;
        public string? SessionId { get; set; }
    }
}

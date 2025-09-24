public class ChatMessage
{
    public string Username { get; set; }
    public string UserMessage { get; set; }
    public string BotResponse { get; set; }

    // Constructor initializing the properties
    public ChatMessage(string username, string userMessage, string botResponse)
    {
        Username = username;
        UserMessage = userMessage;
        BotResponse = botResponse;
    }
}

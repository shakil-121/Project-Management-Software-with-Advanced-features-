using System;
using System.ComponentModel.DataAnnotations;

namespace FastPMS.Models
{
    public class AIChatModel
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Message { get; set; }

        public string Response { get; set; }
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public class DeepSeekRequest
    {
        public string model { get; set; } = "deepseek-chat";
        public List<Message> messages { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class DeepSeekResponse
    {
        public List<Choice> choices { get; set; }
    }

    public class Choice
    {
        public Message message { get; set; }
    }
}
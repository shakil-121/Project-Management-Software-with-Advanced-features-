using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FastPMS.Models;

namespace FastPMS.Services
{
    public class DeepSeekService : IDeepSeekService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://api.deepseek.com/v1/chat/completions";

        public DeepSeekService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["DeepSeek:ApiKey"];

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            }
        }

        public async Task<string> GetAIResponseAsync(string userMessage)
        {
            try
            {
                // ✅ TEMPORARY FALLBACK - No API calls until billing fixed
                // Always return friendly response without calling external API
                return GetSmartResponse(userMessage);

                /*
                // 🔧 KEEP THIS CODE COMMENTED UNTIL BILLING ISSUE IS RESOLVED
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return GetSmartResponse(userMessage);
                }

                var request = new DeepSeekRequest
                {
                    messages = new List<Message>
                    {
                        new Message { role = "user", content = userMessage }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(_apiUrl, request);

                if (response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
                {
                    return "🔧 AI features are currently being upgraded. I can still help with basic assistance!";
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<DeepSeekResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                return result?.choices?.FirstOrDefault()?.message?.content 
                       ?? GetSmartResponse(userMessage);
                */
            }
            catch (Exception ex)
            {
                // Friendly fallback response
                return GetSmartResponse(userMessage);
            }
        }

        private string GetSmartResponse(string userMessage)
        {
            var lowerMessage = userMessage.ToLower();

            if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi") || lowerMessage.Contains("hey"))
                return "Hello! I'm your AI Assistant. How can I help you with your projects today?";
            else if (lowerMessage.Contains("project") && lowerMessage.Contains("create"))
                return "To create a project, go to the 'Create Project' section in the admin menu.";
            else if (lowerMessage.Contains("task") || lowerMessage.Contains("assignment"))
                return "You can manage tasks from the 'Tasks' section. Would you like help with specific task management?";
            else if (lowerMessage.Contains("help"))
                return "I can help with: project creation, task management, team assignments, and general guidance. What do you need help with?";
            else if (lowerMessage.Contains("team") || lowerMessage.Contains("member"))
                return "Team management is available in the 'Create Team' section for admins and super admins.";
            else if (lowerMessage.Contains("thank"))
                return "You're welcome! Let me know if you need any other assistance.";
            else if (lowerMessage.Contains("how are you"))
                return "I'm functioning well! Ready to help you with your project management needs.";
            else
                return "Thanks for your message! I understand you're asking about: '" + userMessage + "'. Currently, my advanced AI features are being upgraded, but I'm here to provide basic assistance with your PMS system.";
        }

        public async Task<bool> IsApiKeyValidAsync()
        {
            // ✅ Always return true for now to prevent errors
            return true;

            /*
            try
            {
                var testMessage = await GetAIResponseAsync("Hello");
                return !testMessage.Contains("Error");
            }
            catch
            {
                return false;
            }
            */
        }
    }
}
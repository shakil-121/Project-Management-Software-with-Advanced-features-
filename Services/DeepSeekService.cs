using FastPMS.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
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

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<string> GetAIResponseAsync(string userMessage)
        {
            try
            {
                var request = new DeepSeekRequest
                {
                    messages = new List<Message>
                    {
                        new Message { role = "user", content = userMessage }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(_apiUrl, request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<DeepSeekResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return result?.choices?.FirstOrDefault()?.message?.content
                           ?? "Sorry, I couldn't process your request.";
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Error connecting to AI service: {ex.Message}";
            }
        }

        public async Task<bool> IsApiKeyValidAsync()
        {
            try
            {
                var testMessage = await GetAIResponseAsync("Hello");
                return !testMessage.Contains("Error");
            }
            catch
            {
                return false;
            }
        }
    }
}
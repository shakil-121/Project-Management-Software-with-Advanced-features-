using System.Threading.Tasks;
namespace FastPMS.Services
{
    public interface IDeepSeekService
    {
        Task<string> GetAIResponseAsync(string userMessage);
        Task<bool> IsApiKeyValidAsync();
    }
}
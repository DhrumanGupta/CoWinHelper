using System.Net.Http;
using System.Threading.Tasks;

namespace CoWinDiscord.Services
{
    public class HttpRequestHandler
    {
        private readonly HttpClient _client = new();

        public async Task<string> GetAsync(string uri)
        {
            var response = await _client.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return string.Empty;
            
            return await response.Content.ReadAsStringAsync();
        }
    }
}
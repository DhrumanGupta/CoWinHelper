using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace CoWinDiscord.Services
{
    public class HttpRequestHandler
    {
        private HttpClient _client = new();

        public async Task<string> GetAsync(string uri)
        {
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();

#if DEBUG
            Console.WriteLine(response.ReasonPhrase);
#endif
            return string.Empty;
        }
    }
}
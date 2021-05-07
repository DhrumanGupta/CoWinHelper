using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CowinChecker
{
    public class RequestHttpManager
    {
        private readonly HttpClient _client = new();

        public async Task<string> GetAsync(string uri)
        {
            var response = await _client.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return string.Empty;
            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
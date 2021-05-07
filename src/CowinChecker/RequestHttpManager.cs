using System;
using System.IO;
using System.Net;

namespace CowinChecker
{
    public class RequestHttpManager : IHttpManager
    {
        public string Get(string uri)
        { 
            var request = WebRequest.Create(uri);
            request.Method = "GET";
            request.Headers[":authority"] = "cdn-api.co-vin.in";

            using var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream();

            using var reader = new StreamReader(webStream);
            var data = reader.ReadToEnd();

            #if DEBUG
            Console.WriteLine(data);
            #endif
            
            return data;
        }
    }
}
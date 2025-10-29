using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#if UNITY_WSA && !UNITY_EDITOR
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
#endif

namespace YARG.Assets.Script.Helpers
{
    public static class HttpHelper
    {
        public static async Task<string> GetURL(string url)
        {
#if UNITY_WSA && !UNITY_EDITOR
            var filter = new HttpBaseProtocolFilter();
            using var httpClient = new HttpClient(filter);
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("YARG");

            var response = await httpClient.GetAsync(new Uri(url));
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
#else
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.UserAgent = "YARG";
            request.Timeout = 10000;
            using var response = await request.GetResponseAsync();
            using var reader = new StreamReader(response.GetResponseStream()!, Encoding.UTF8);
            return await reader.ReadToEndAsync();
#endif
        }
    }
}
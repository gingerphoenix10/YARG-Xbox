using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using YARG.Core.Logging;
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


        public static async Task<string> PostURL(string url, string postData, string contentType = "application/json")
        {
#if UNITY_WSA && !UNITY_EDITOR
            var filter = new HttpBaseProtocolFilter();
            using var httpClient = new HttpClient(filter);
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("YARG");

            var content = new HttpStringContent(postData, Windows.Storage.Streams.UnicodeEncoding.Utf8, contentType);
            var response = await httpClient.PostAsync(new Uri(url), content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
#else
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = "POST";
            request.UserAgent = "YARG";
            request.ContentType = contentType;
            request.Timeout = 10000;

            using (var stream = await request.GetRequestStreamAsync())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(postData);
            }

            using var response = await request.GetResponseAsync();
            using var reader = new StreamReader(response.GetResponseStream()!, Encoding.UTF8);
            return await reader.ReadToEndAsync();
#endif
        }

        public static async UniTask<byte[]> GetStreamingAsset(string path)
        {
            // On Android, must use UnityWebRequest because StreamingAssets is inside APK.
            // Might just use this for non-android too, idk. We'll see
            await UniTask.SwitchToMainThread();
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                var request = www.SendWebRequest();

                while (!request.isDone)
                    await Task.Yield();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    YargLogger.LogError("Failed to load streamed asset: " + www.error);
                    return null;
                }

                return www.downloadHandler.data;
            }
        }

        public static async UniTask<string> GetStreamingAssetText(string path)
        {
            await UniTask.SwitchToMainThread();

            using var www = UnityWebRequest.Get(path);
            var op = www.SendWebRequest();

            while (!op.isDone)
                await UniTask.Yield();

            if (www.result != UnityWebRequest.Result.Success)
                return null;

            byte[] data = www.downloadHandler.data;

            // Detect BOM manually
            if (data.Length >= 3 &&
                data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
            {
                // UTF-8 with BOM
                return Encoding.UTF8.GetString(data, 3, data.Length - 3);
            }
            else if (data.Length >= 2 &&
                     data[0] == 0xFF && data[1] == 0xFE)
            {
                // UTF-16 LE
                return Encoding.Unicode.GetString(data, 2, data.Length - 2);
            }
            else if (data.Length >= 2 &&
                     data[0] == 0xFE && data[1] == 0xFF)
            {
                // UTF-16 BE
                return Encoding.BigEndianUnicode.GetString(data, 2, data.Length - 2);
            }
            else
            {
                // Assume UTF-8
                return Encoding.UTF8.GetString(data);
            }
        }

    }
}
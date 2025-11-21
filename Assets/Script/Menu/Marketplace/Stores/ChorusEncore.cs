using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YARG.Assets.Script.Helpers;
using YARG.Core;
using YARG.Core.Logging;

namespace YARG.Menu.Marketplace.Stores
{
    class ChorusEncore : MarketplaceStore
    {
        public override string Name => "Chorus Encore";
        public override Sprite Icon => Sprite.Create(new Texture2D(0,0), new Rect(0,0,0,0), new Vector2(0,0));
        public override async Task<List<SetlistItem>> Search(string term)
        {
            List<SetlistItem> result = new();
            Dictionary<string, object> postData = new()
            {
                { "difficulty", "expert" },
                { "drumsReviewed", false },
                { "drumType", null },
                { "instrument", "guitar" },
                { "page", 1 },
                { "search", term.Replace(" ","")==""?"*":term },
                { "source", "bridge" }
            };
            string data = JsonConvert.SerializeObject(postData);
            JObject returned = JObject.Parse(await HttpHelper.PostURL("https://api.enchor.us/search", data));
            foreach (JObject songObject in returned["data"])
            {
                UnityWebRequest textureReq = UnityWebRequestTexture.GetTexture($"https://files.enchor.us/{songObject["albumArtMd5"]}.jpg");
                UnityWebRequestAsyncOperation op = textureReq.SendWebRequest();
                while (!op.isDone)
                    await Task.Yield(); // this probably sucks but I don't really care
                result.Add(new SetlistItem
                {
                    Name = songObject["name"]!.ToString(),
                    Cover = textureReq.result == UnityWebRequest.Result.Success ? DownloadHandlerTexture.GetContent(textureReq) : new Texture2D(0,0),
                    Identifier = songObject["md5"]!.ToString(),
                    Info = new SetlistInfo
                    {
                        Description = songObject["loading_phrase"]!.ToString() + $"\nAlbum: {songObject["album"]!.ToString()} - Genre: {songObject["genre"]!.ToString()} - Year: {songObject["year"]!.ToString()}",
                        Songs = new List<SetlistSong>
                        {
                            new SetlistSong
                            {
                                Name = songObject["name"]!.ToString(),
                                Artist = songObject["artist"]!.ToString(),
                                Length = TimeSpan.FromMilliseconds(int.Parse(songObject["song_length"]!.ToString()))
                            }
                        },
                        SetlistURL = $"https://files.enchor.us/{songObject["md5"]!.ToString()}.sng",
                        SetlistHeaders = new()
                        {
                            { "mode", "cors" },
                            { "referrer-policy", "no-referrer" }
                        }
                    }
                });
            }
            return result;
        }
        public override async Task<SetlistInfo> GetInfo(string Identifier)
        {
            return new();
        }
        public override async Task<List<SetlistItem>> GetSetlists()
        {
            return await Search("*");
        }
    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using YARG.Assets.Script.Helpers;
using YARG.Core.Logging;

namespace YARG.Assets.Script.Menu.Marketplace.Stores
{
    class YARN : MarketplaceStore
    {
        public override string Name => "YARN";
        public override Sprite Icon => Addressables.LoadAssetAsync<Sprite>($"TabIcons[Songs]").WaitForCompletion();

        private readonly string SETLIST_URL = "https://releases.yarg.in/profiles/";
        private readonly string PROFILES_URL = "https://releases.yarg.in/profiles/index.json";
        private readonly string ICON_URL = "https://media.githubusercontent.com/media/YARC-Official/YARC-Launcher/master/public/profileAssets";
        public List<SetlistItem> getCache = new();

        public override async Task<List<SetlistItem>> GetSetlists()
        {
            List<SetlistItem> items = new();

            try
            {
                JObject profiles = JObject.Parse(await HttpHelper.GetURL(PROFILES_URL));
                foreach (JObject profile in profiles["profiles"])
                {
                    if (profile["category"]!.ToString() != "yarn_setlist")
                        continue;

                    string iconUrl = profile["iconUrl"]!.ToString();
                    if (iconUrl.StartsWith("@"))
                        iconUrl = ICON_URL + iconUrl.Substring(1);

                    UnityWebRequest textureReq = UnityWebRequestTexture.GetTexture(iconUrl);
                    UnityWebRequestAsyncOperation op = textureReq.SendWebRequest();
                    while (!op.isDone)
                        await Task.Yield(); // this probably sucks but I don't really care

                    items.Add(new SetlistItem
                    {
                        Name = profile["name"]!.ToString(),
                        Cover = DownloadHandlerTexture.GetContent(textureReq),
                        Identifier = profile["uuid"]!.ToString()
                    });
                }
                getCache = items;
                return items;
            }
            catch (Exception e)
            {
                YargLogger.LogException(e);
                getCache = new();
                return null;
            }
        }

        public override async Task<SetlistInfo> GetSongs(string id)
        {
            SetlistInfo info = new()
            {
                Description = "",
                Songs = new()
            };

            try
            {
                JObject setlistInfo = JObject.Parse(await HttpHelper.GetURL(SETLIST_URL + id + "/index.json"));
                info.Description = setlistInfo["metadata"]["description"]!.ToString();
                foreach (JObject song in setlistInfo["metadata"]["songs"])
                {
                    info.Songs.Add(new SetlistSong
                    {
                        Name = song["title"]!.ToString(),
                        Artist = song["artist"]!.ToString(),
                        Length = TimeSpan.FromMilliseconds(int.Parse(song["length"]!.ToString())),
                    });
                }
                return info;
            }
            catch (Exception e)
            {
                YargLogger.LogException(e);
                return null;
            }
        }

        public override Task<List<SetlistItem>> Search(string term)
        {
            List<SetlistItem> matchedItems = new();
            foreach (SetlistItem setlist in getCache)
            {
                if (SearchHelper.Similarity(setlist.Name, term) >= 0.25 || setlist.Name.ToLower().Contains(term.ToLower()))
                    matchedItems.Add(setlist);
            }
            TaskCompletionSource<List<SetlistItem>> taskCompletion = new();
            taskCompletion.SetResult(matchedItems);
            return taskCompletion.Task;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using YARG.Core.Logging;
using YARG.Menu.Persistent;

namespace YARG.Menu.Marketplace
{

    public enum SetlistDownloadState
    {
        Downloading,
        Installing,
        DownloadFailed,
        Finished,
        Uninstalling
    }

    public delegate void DownloadStateChange(MarketplaceStore store, string Identifier, SetlistDownloadState Status);
    class DownloadsHandler
    {
        public static DownloadStateChange changedEvent;
        public static Dictionary<Tuple<string, string>, Tuple<SetlistDownloadState, float>> downloadStates = new();
        public DownloadsHandler(MarketplaceStore store, SetlistInfo info, string Identifier, bool uninstall = false)
        {
            if (uninstall)
                HandleUninstall(store, info, Identifier);
            else
                HandleDownload(store, info, Identifier);
        }

        private async void HandleDownload(MarketplaceStore store, SetlistInfo info, string Identifier)
        {
            string storeFolder = Path.Combine(MarketplaceMenu.SONGS_PATH, store.GetType().Name);
            string setlistFolder = Path.Combine(storeFolder, Identifier);
            string downloadFileName = Identifier + $".{info.SetlistURL.Split(".").Last()}"; /* this sucks */

            Tuple<string, string> dictKey = new(store.GetType().Name, Identifier);

            downloadStates.Add(dictKey, new(SetlistDownloadState.Downloading, 0));
            changedEvent?.Invoke(store, Identifier, SetlistDownloadState.Downloading);
            string archivePath = await DownloadHelper.Download(storeFolder, downloadFileName, info.SetlistURL, info.SetlistHeaders, (float Progress) =>
            {
                downloadStates[dictKey] = new(SetlistDownloadState.Downloading, Progress);
                changedEvent?.Invoke(store, Identifier, SetlistDownloadState.Downloading);
            });
            if (archivePath == null)
            {
                downloadStates[dictKey] = new(SetlistDownloadState.DownloadFailed, downloadStates[dictKey].Item2);
                changedEvent.Invoke(store, Identifier, SetlistDownloadState.DownloadFailed);
                return;
            }
            downloadStates[dictKey] = new(SetlistDownloadState.Installing, 1);
            changedEvent.Invoke(store, Identifier, SetlistDownloadState.Installing);
            if (info.SetlistURL.EndsWith(".7z"))
                await DownloadHelper.ExtractSevenZip(setlistFolder, archivePath);
            else if (info.SetlistURL.EndsWith(".zip"))
                await DownloadHelper.ExtractZip(setlistFolder, archivePath);
            else // Made for sng files, even though none of the official stores use it. Whateverrr
            {
                if (!Directory.Exists(setlistFolder))
                {
                    Directory.CreateDirectory(setlistFolder);
                    File.Move(archivePath, Path.Combine(setlistFolder, Path.GetFileName(archivePath)));
                }
            }
            if (downloadStates.ContainsKey(dictKey))
                downloadStates.Remove(dictKey);
            changedEvent.Invoke(store, Identifier, SetlistDownloadState.Finished);
        }

        private async void HandleUninstall(MarketplaceStore store, SetlistInfo info, string Identifier)
        {
            string storeFolder = Path.Combine(MarketplaceMenu.SONGS_PATH, store.GetType().Name);
            string setlistFolder = Path.Combine(storeFolder, Identifier);
            string downloadFileName = Identifier + $".{info.SetlistURL.Split(".").Last()}"; /* this sucks */
            string downloadPath = Path.Combine(storeFolder, downloadFileName);

            Tuple<string, string> dictKey = new(store.GetType().Name, Identifier);

            downloadStates.Add(dictKey, new(SetlistDownloadState.Uninstalling,0));
            changedEvent?.Invoke(store, Identifier, SetlistDownloadState.Uninstalling);
            try
            {
                MusicPlayer player = HelpBar.Instance?.MusicPlayer;
                if (player != null)
                    player.enabled = false;
                GC.Collect();
                if (File.Exists(downloadPath))
                    File.Delete(downloadPath);
                if (Directory.Exists(setlistFolder))
                    Directory.Delete(setlistFolder, true);
            } catch (Exception e)
            {
                YargLogger.LogFormatError("Failed to uninstall: {0}", e.ToString());
                await Task.Delay(2000);
                HandleUninstall(store, info, Identifier);
            }

            if (downloadStates.ContainsKey(dictKey))
                downloadStates.Remove(dictKey);
            changedEvent.Invoke(store, Identifier, SetlistDownloadState.Finished);
        }
    }
}

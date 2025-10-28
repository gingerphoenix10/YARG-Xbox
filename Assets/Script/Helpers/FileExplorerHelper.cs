using System;
using SFB;
using YARG.Core.Logging;

using System.Diagnostics;

#if UNITY_WSA && !UNITY_EDITOR
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
#endif

using UnityEngine;
using YARG.Menu.Persistent;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.WSA;

namespace YARG.Helpers
{
    public static class FileExplorerHelper
    {
        public static async Task OpenChooseFolder(string startingDir, Action<string> callback)
        {
#if UNITY_WSA && !UNITY_EDITOR
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var picker = new FolderPicker();
                picker.SuggestedStartLocation = PickerLocationId.Desktop;
                picker.FileTypeFilter.Add("*");

                StorageFolder path = await picker.PickSingleFolderAsync();
                if (path == null)
                    return;

                try
                {
                    callback(path.Path);
                }
                catch (Exception ex)
                {
                    YargLogger.LogException(ex, $"Error when handling folder {path.Path}!");
                }
            });
#else
            StandaloneFileBrowser.OpenFolderPanelAsync("Choose Folder", startingDir, false, (files) =>
            {
                if (files is not { Length: > 0 })
                    return;

                string path = files[0];
                try
                {
                    callback(path);
                }
                catch (Exception ex)
                {
                    YargLogger.LogException(ex, $"Error when handling folder {path}!");
                }
            });
#endif
        }

        public static void OpenChooseFile(string startingDir, string extension, Action<string> callback)
        {
            StandaloneFileBrowser.OpenFilePanelAsync("Choose File", startingDir, extension, false, (files) =>
            {
                if (files is not { Length: > 0 })
                    return;

                string path = files[0];
                try
                {
                    callback(path);
                }
                catch (Exception ex)
                {
                    YargLogger.LogException(ex, $"Error when handling file {path}!");
                }
            });
        }

        public static void OpenSaveFile(string startingDir, string defaultName, string extension,
            Action<string> callback)
        {
            StandaloneFileBrowser.SaveFilePanelAsync("Save File", startingDir, defaultName, extension, (path) =>
            {
                if (string.IsNullOrEmpty(path))
                    return;

                try
                {
                    callback(path);
                }
                catch (Exception ex)
                {
                    YargLogger.LogException(ex, $"Error when saving file {path}!");
                }
            });
        }

        public static void OpenFolder(string folderPath)
        {
#if UNITY_STANDALONE_WIN
            Process.Start("explorer.exe", folderPath);
#elif UNITY_STANDALONE_OSX
            Process.Start("open", $"\"{folderPath}\"");
#elif UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", folderPath);
#else
            GUIUtility.systemCopyBuffer = folderPath;
            DialogManager.Instance.ShowMessage(
                "Path Copied To Clipboard",
                "Your system does not support the opening of the file explorer dialog, so the path of the folder has " +
                "been copied to your clipboard.");
#endif
        }

        public static void OpenToFile(string filePath)
        {
#if UNITY_STANDALONE_WIN
            Process.Start("explorer.exe", $"/select, \"{filePath}\"");
#elif UNITY_STANDALONE_OSX
            Process.Start("open", $"-R \"{filePath}\"");
#elif UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", Path.GetDirectoryName(filePath));
#else
            GUIUtility.systemCopyBuffer = filePath;
            DialogManager.Instance.ShowMessage(
                "Path Copied To Clipboard",
                "Your system does not support the opening of the file explorer dialog, so the path of the folder has " +
                "been copied to your clipboard.");
#endif
        }
    }
}

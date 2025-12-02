using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YARG.Assets.Script.Helpers;
using YARG.Core.Logging;
#if UNITY_WSA && !UNITY_EDITOR
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
#endif


namespace YARG.Menu.Marketplace
{
    public static class DownloadHelper
    {
        public static async Task<string> Download(string path, string filename, string url, Dictionary<string, string> urlHeaders = null, Action<float> Progress = null)
        {
            if (urlHeaders == null)
                urlHeaders = new();

            try
            {
#if UNITY_WSA && !UNITY_EDITOR
                StorageFolder targetFolder = await StorageFolder.GetFolderFromPathAsync(path);

                string songPath = Path.Combine(path, filename);
                if (File.Exists(songPath))
                    return songPath;

                StorageFile songFile = await targetFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                using (var httpClient = new HttpClient())
                {
                    if (urlHeaders != null)
                    {
                        foreach (var header in urlHeaders)
                        {
                            if (header.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                            {
                                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(header.Value);
                            }
                            else
                            {
                                httpClient.DefaultRequestHeaders.TryAppendWithoutValidation(header.Key, header.Value);
                            }
                        }
                    }

                    HttpResponseMessage response = await httpClient.GetAsync(new Uri(url));
                    response.EnsureSuccessStatusCode();

                    ulong? contentLength = response.Content.Headers.ContentLength;

                    using (IInputStream inputStream = await response.Content.ReadAsInputStreamAsync())
                    using (IRandomAccessStream outputStream = await songFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        const uint bufferSize = 81920; // 80 KB
                        IBuffer buffer = new Windows.Storage.Streams.Buffer(bufferSize);
                        ulong totalRead = 0;

                        while (true)
                        {
                            buffer = await inputStream.ReadAsync(buffer, bufferSize, InputStreamOptions.None);
                            if (buffer.Length == 0)
                                break; // EOF

                            await outputStream.WriteAsync(buffer);
                            totalRead += buffer.Length;

                            if (contentLength.HasValue)
                            {
                                if (Progress != null)
                                    Progress((float)totalRead / contentLength.Value);
                            }
                        }

                        await outputStream.FlushAsync();
                    }
                }

                return songPath;
#else
                string songPath = Path.Combine(path, filename);
                if (!File.Exists(songPath))
                {
                    using (var client = new WebClient())
                    {
                        foreach (KeyValuePair<string, string> header in urlHeaders)
                            client.Headers.Set(header.Key, header.Value);

                        var tcs = new TaskCompletionSource<object?>();

                        client.DownloadProgressChanged += (s, e) =>
                        {
                            if (Progress != null)
                                Progress(e.ProgressPercentage/100f);
                        };

                        client.DownloadFileCompleted += (s, e) =>
                        {
                            if (e.Error != null) tcs.SetException(e.Error);
                            else if (e.Cancelled) tcs.SetCanceled();
                            else tcs.SetResult(null);
                        };

                        client.DownloadFileAsync(new Uri(url), songPath);
                        await tcs.Task;
                    }
                }
                return songPath;
#endif
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to download setlist");
            }
            return null;
        }

        public static async Task ExtractZip(string extractionPath, string filePath)
        {
            try {
                if (Directory.Exists(extractionPath))
                    Directory.Delete(extractionPath, true);

                Directory.CreateDirectory(extractionPath);
                await UniTask.RunOnThreadPool(() =>
                {
                    ZipFile.ExtractToDirectory(filePath, extractionPath);
                });
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                YargLogger.LogException(e, "Failed to install zip setlist.");
            }
        }

        public static async Task ExtractSevenZip(string extractionPath, string filePath, string password = null)
        {
            if (Directory.Exists(extractionPath))
                Directory.Delete(extractionPath, true);

            Directory.CreateDirectory(extractionPath);

            try {
                await UniTask.RunOnThreadPool(() =>
                {
                    byte[] archiveBytes = File.ReadAllBytes(filePath);
                    using var memoryStream = new MemoryStream(archiveBytes);

                    var readerOptions = new ReaderOptions();
                    if (!string.IsNullOrEmpty(password))
                        readerOptions.Password = password;

                    using var archive = SevenZipArchive.Open(memoryStream, readerOptions);

                    bool isSolid = archive.IsSolid;
                    bool isEncrypted = archive.Entries.Any(e => e.IsEncrypted);

                    bool canParallel = !isSolid && !isEncrypted;

                    var entries = archive.Entries.Where(e => !e.IsDirectory).ToList();

                    if (canParallel)
                    {
                        Parallel.ForEach(
                            entries,
                            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                            entry => ExtractEntry(entry, extractionPath));
                    }
                    else
                    {
                        foreach (var entry in entries)
                        {
                            ExtractEntry(entry, extractionPath);
                        }
                    }
                });

                File.Delete(filePath);
            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(password))
                {
                    const string LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    var sb = new StringBuilder();

                    for (int i = 0; i < 64; i++)
                    {
                        byte a = (byte) (5 + unchecked(i * 104729));
                        byte b = (byte) (9 + unchecked(i * 224737));
                        byte c = (byte) (a % b % 52);
                        sb.Append(LETTERS[c]);
                    }
                    YargLogger.LogInfo("7z Extraction Failed. Trying encrypted");
                    await ExtractSevenZip(extractionPath, filePath, sb.ToString());
                }
                else
                    YargLogger.LogException(e, "7z Encrypted extraction failed.");
            }
        }
        private static void ExtractEntry(SharpCompress.Archives.IArchiveEntry entry, string extractionPath)
        {
            string outputPath = Path.Combine(extractionPath, entry.Key);
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var entryStream = entry.OpenEntryStream();
            using var fileStream = File.Create(outputPath);
            entryStream.CopyTo(fileStream);
        }
    }
}

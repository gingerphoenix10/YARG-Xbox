using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;
using YARG.Core.Input;
using YARG.Core.Logging;
using YARG.Helpers;
using YARG.Helpers.Extensions;
using YARG.Localization;
using YARG.Menu.Marketplace.Stores;
using YARG.Menu.Navigation;
using YARG.Settings;
using YARG.Settings.Customization;
using YARG.Settings.Metadata;
using YARG.Song;

namespace YARG.Menu.Marketplace
{
    [DefaultExecutionOrder(-10000)]
    public class MarketplaceMenu : MonoBehaviour
    {
        [SerializeField]
        private HeaderTabs _headerTabs;
        [SerializeField]
        private Transform _settingsContainer;
        [SerializeField]
        private NavigationGroup _settingsNavGroup;
        [SerializeField]
        private ScrollRect _scrollRect;

        [Space]
        [SerializeField]
        private GameObject _searchBarContainer;
        [SerializeField]
        private TMP_InputField _searchBar;
        [SerializeField]
        private TextMeshProUGUI _searchHeaderText;

        [Space]
        [SerializeField]
        private TextMeshProUGUI _settingName;
        [SerializeField]
        private TextMeshProUGUI _settingDescription;
        [SerializeField]
        private GameObject _songsContainer;
        [SerializeField]
        private TextMeshProUGUI _songsText;

        public string SearchQuery => _searchBar.text;

        public List<MarketplaceStore> _stores = new() { new YARC(), new YARN(), /*new Enchorus()*/ };
        public Dictionary<MarketplaceStore, List<SetlistItem>> marketplaceCache = new();
        public MarketplaceStore CurrentStore;

        private static GameObject _downloadButtonPrefab;

        public static string SONGS_PATH;

        private async void Start()
        {
            _downloadButtonPrefab = Addressables
                .LoadAssetAsync<GameObject>("MarketplaceTab/Setlist")
                .WaitForCompletion();
            var tabs = new List<HeaderTabs.TabInfo>();

            foreach (MarketplaceStore store in _stores)
            {
                tabs.Add(new HeaderTabs.TabInfo
                {
                    Icon = store.Icon,
                    Id = store.GetType().Name,
                    DisplayName = store.Name
                });
            }

            _headerTabs.Tabs = tabs;
        }

        private void OnEnable()
        {
            _settingsNavGroup.SelectionChanged += OnSelectionChanged;

            // Set navigation scheme
            Navigator.Instance.PushScheme(new NavigationScheme(new()
            {
                NavigationScheme.Entry.NavigateSelect,
                new NavigationScheme.Entry(MenuAction.Red, "Menu.Common.Back", MenuManager.Instance.PopMenu),
                NavigationScheme.Entry.NavigateUp,
                NavigationScheme.Entry.NavigateDown,
                _headerTabs.NavigateNextTab,
                _headerTabs.NavigatePreviousTab
            }, true));

            //Refresh();
            _headerTabs.TabChanged += OnTabChanged;
        }

        private async void OnTabChanged(string tab)
        {
            YargLogger.LogFormatInfo("Tab changed to {0}", tab);
            foreach (MarketplaceStore store in _stores)
            {
                if (store.GetType().Name == tab)
                    CurrentStore = store;
            }
            Refresh();

            _searchBar.text = string.Empty;
        }
        public void SelectSettingByIndex(int index)
        {
            // Force it to be the navigation selection type so the scroll view properly updates
            _settingsNavGroup.SelectAt(index, SelectionOrigin.Navigation);
        }

        private async void OnSelectionChanged(NavigatableBehaviour selected, SelectionOrigin selectionOrigin)
        {
            if (selected == null || !(selected is SetlistButton))
            {
                _settingName.text = string.Empty;
                _settingDescription.text = string.Empty;
                _songsText.text = string.Empty;
                _songsContainer.SetActive(false);
                return;
            }
            SetlistButton buttonObj = (SetlistButton)selected;

            _settingName.text = buttonObj.title.text;

            if (buttonObj.setlist.Info == null)
                buttonObj.setlist.Info = await CurrentStore.GetInfo(buttonObj.setlist.Identifier);
            if (buttonObj.setlist.Info == null)
                return;

            _settingDescription.text = buttonObj.setlist.Info.Description;

            string songs = "";
            foreach (SetlistSong song in buttonObj.setlist.Info.Songs) {
                string time;
                if (song.Length.Hours > 0)
                    time = $"{song.Length.Hours:D2}:{song.Length.Minutes:D2}:{song.Length.Seconds:D2}";
                else
                    time = $"{song.Length.Minutes:D2}:{song.Length.Seconds:D2}";

                songs += $"{song.Artist} - {song.Name}  (<color=#00aaff>{time}</color>)\n";
            }

            _songsContainer.SetActive(true);
            _songsText.text = songs;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_songsText.GetComponent<RectTransform>());
        }

        public void Refresh()
        {
            YargLogger.LogInfo("Refreshing");
            UpdateSettings(true);
        }

        public void RefreshAndKeepPosition()
        {
            // Everything gets recreated, so we must cache the index before hand
            int? beforeIndex = _settingsNavGroup.SelectedIndex;

            UpdateSettings(false);

            // Restore selection
            _settingsNavGroup.SelectAt(beforeIndex);
        }
        public async void UpdateSettings(bool resetScroll)
        {
            _settingsNavGroup.ClearNavigatables();
            _settingsContainer.DestroyChildren();

            // Build the settings tab
            if (CurrentStore == null || !_stores.Contains(CurrentStore))
            {
                foreach (MarketplaceStore store in _stores)
                {
                    if (store.GetType().Name == _headerTabs.SelectedTabId)
                        CurrentStore = store;
                }
            }
            if (CurrentStore == null)
                return;
            MarketplaceStore LoadingStore = CurrentStore;
            List<SetlistItem> setlistsToCreate = null;
            if (!marketplaceCache.TryGetValue(LoadingStore, out setlistsToCreate))
            {
                setlistsToCreate = await LoadingStore.GetSetlists();
                marketplaceCache[LoadingStore] = setlistsToCreate;
            }
            if (setlistsToCreate == null || setlistsToCreate.Count == 0)
                return;

            foreach (SetlistItem setlist in setlistsToCreate)
            {
                setlist.store = LoadingStore;
                CreateSetlistItem(setlist, LoadingStore);
            }

            if (resetScroll)
            {
                // Make the settings nav group the main one
                _settingsNavGroup.SelectFirst();

                // Reset scroll rect
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public async void OnSearchBarFinish()
        {
            string term = _searchBar.text.ToLower();
            if (term.Replace(" ","") == "")
            {
                _searchBar.text = "";
                UpdateSettings(false);
                return;
            }

            _settingsNavGroup.ClearNavigatables();
            _settingsContainer.DestroyChildren();
            if (CurrentStore == null || !_stores.Contains(CurrentStore))
            {
                foreach (MarketplaceStore store in _stores)
                {
                    if (store.GetType().Name == _headerTabs.SelectedTabId)
                        CurrentStore = store;
                }
            }
            if (CurrentStore == null)
                return;
            List<SetlistItem> setlistsToCreate = await CurrentStore.Search(term);
            foreach (SetlistItem setlist in setlistsToCreate)
                CreateSetlistItem(setlist, CurrentStore);

            // Make the settings nav group the main one
            _settingsNavGroup.SelectFirst();

            // Reset scroll rect
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private void CreateSetlistItem(SetlistItem setlist, MarketplaceStore store)
        {
            if (store != CurrentStore)
                return;
            GameObject newButton = Instantiate(_downloadButtonPrefab, _settingsContainer);
            SetlistButton button = newButton.GetComponent<SetlistButton>();
            _settingsNavGroup.AddNavigatable(button);
            button.icon.sprite = Sprite.Create(setlist.Cover, new Rect(0, 0, setlist.Cover.width, setlist.Cover.height), new Vector2(0, 0));
            button.title.text = setlist.Name;
            button.setlist = setlist;
            button.setlist.store = store;

            if (SONGS_PATH == null)
                SONGS_PATH = Path.Combine(SongContainer.internalSongsPath, "Marketplace Songs");

            if (!Directory.Exists(SONGS_PATH))
                Directory.CreateDirectory(SONGS_PATH);

            string storeFolder = Path.Combine(SONGS_PATH, store.GetType().Name);

            if (!Directory.Exists(storeFolder))
                Directory.CreateDirectory(storeFolder);

            string setlistFolder = Path.Combine(storeFolder, setlist.Identifier);

            Tuple<string, string> statesKey = new(store.GetType().Name, setlist.Identifier);
            if (DownloadsHandler.downloadStates.ContainsKey(statesKey))
                DownloadStateChanged(store, button.setlist.Identifier, DownloadsHandler.downloadStates[statesKey].Item1, button, setlist, true);
                else
            {
                if (Directory.Exists(setlistFolder))
                {
                    button._downloadButton.GetComponent<Button>().interactable = false;
                    button._downloadButton.GetComponentInChildren<TextMeshProUGUI>().text = "Already Installed";
                    button._downloadButton.gameObject.SetActive(false);
                    button._uninstallButton.gameObject.SetActive(true);
                    button.GetComponentInChildren<NavigationGroup>().AddNavigatable(button._uninstallButton);
                }
                else
                    button.GetComponentInChildren<NavigationGroup>().AddNavigatable(button._downloadButton);
            }

            DownloadsHandler.changedEvent += (MarketplaceStore store, string Identifier, SetlistDownloadState state) =>
            {
                DownloadStateChanged(store, Identifier, state, button, setlist);
            };

            button._downloadButton.GetComponent<Button>().onClick.AddListener(async () =>
            {
                if (button.setlist.Info == null)
                    button.setlist.Info = await store.GetInfo(button.setlist.Identifier);

                new DownloadsHandler(store, button.setlist.Info, button.setlist.Identifier);
            });

            button._uninstallButton.GetComponent<Button>().onClick.AddListener(async () =>
            {
                if (button.setlist.Info == null)
                    button.setlist.Info = await store.GetInfo(button.setlist.Identifier);

                new DownloadsHandler(store, button.setlist.Info, button.setlist.Identifier, true);
            });
        }

        private void DownloadStateChanged(MarketplaceStore store, string Identifier, SetlistDownloadState state, SetlistButton button, SetlistItem setlist, bool init = false)
        {
            if (Identifier != setlist.Identifier || store.GetType().Name != setlist.store.GetType().Name || button == null)
                return;
            Tuple<string, string> key = new(store.GetType().Name, Identifier);
            RectTransform progressBar = button._navGroup.GetComponent<RectTransform>();
            switch (state)
            {
                case SetlistDownloadState.Downloading:
                    if (button._downloadButton == null)
                        return;
                    if (button._uninstallButton != null)
                        button._uninstallButton.gameObject.SetActive(false);
                    button._downloadButton.gameObject.SetActive(true);
                    button._downloadButton.GetComponent<Button>().interactable = false;
                    button._downloadButton.GetComponentInChildren<TextMeshProUGUI>().text = "Downloading...";
                    button._downloadButton.GetComponent<Image>().color = new Color(152,152,152);
                    button._downloadButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(128,172,207);
                    button.progress.padding = new Vector4(0, 0, progressBar.rect.width+2 - (progressBar.rect.width+2) * DownloadsHandler.downloadStates[key].Item2, 0);
                    if (init)
                        button.GetComponentInChildren<NavigationGroup>().AddNavigatable(button._downloadButton);
                    break;
                case SetlistDownloadState.Installing:
                    if (button._downloadButton == null)
                        return;
                    if (button._uninstallButton != null)
                        button._uninstallButton.gameObject.SetActive(false);
                    button._downloadButton.gameObject.SetActive(true);
                    button._downloadButton.gameObject.SetActive(true);
                    button._downloadButton.GetComponent<Button>().interactable = false;
                    button._downloadButton.GetComponentInChildren<TextMeshProUGUI>().text = "Installing...";
                    button._downloadButton.GetComponent<Image>().color = new Color(152,152,152);
                    button._downloadButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(128,172,207);
                    button.progress.padding = new Vector4(0, 0, 0, 0);
                    if (init)
                        button.GetComponentInChildren<NavigationGroup>().AddNavigatable(button._downloadButton);
                    break;
                case SetlistDownloadState.DownloadFailed:
                    if (button._downloadButton == null)
                        return;
                    if (button._uninstallButton != null)
                        button._uninstallButton.gameObject.SetActive(false);
                    button._downloadButton.gameObject.SetActive(true);
                    button._downloadButton.GetComponent<Button>().interactable = true;
                    button._downloadButton.GetComponentInChildren<TextMeshProUGUI>().text = "Download Failed";
                    button._downloadButton.GetComponent<Image>().color = new Color(9,222,123);
                    button._downloadButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0,88,108);
                    button.progress.padding = new Vector4(0, 0, progressBar.rect.width + 2 - (progressBar.rect.width + 2) * DownloadsHandler.downloadStates[key].Item2, 0);
                    if (init)
                        button.GetComponentInChildren<NavigationGroup>().AddNavigatable(button._downloadButton);
                    break;
                case SetlistDownloadState.Finished:
                    UpdateSettings(false);
                    break;
                case SetlistDownloadState.Uninstalling:
                    if (button._uninstallButton == null)
                        return;
                    if (button._downloadButton != null)
                        button._downloadButton.gameObject.SetActive(false);
                    button._uninstallButton.gameObject.SetActive(true);
                    button._uninstallButton.GetComponent<Button>().interactable = true;
                    button._uninstallButton.GetComponentInChildren<TextMeshProUGUI>().text = "Uninstalling...";
                    if (init)
                        button.GetComponentInChildren<NavigationGroup>().AddNavigatable(button._uninstallButton);
                    break;
            }
        }

        private void OnDisable()
        {
            if (Navigator.Instance != null)
                Navigator.Instance.PopScheme();
            _headerTabs.TabChanged -= OnTabChanged;

            _settingsNavGroup.SelectionChanged -= OnSelectionChanged;
        }
    }
}
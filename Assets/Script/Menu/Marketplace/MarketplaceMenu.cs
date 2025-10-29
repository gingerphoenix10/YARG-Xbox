using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using YARG.Assets.Script.Menu.Marketplace;
using YARG.Assets.Script.Menu.Marketplace.Stores;
using YARG.Core.Input;
using YARG.Core.Logging;
using YARG.Helpers;
using YARG.Helpers.Extensions;
using YARG.Localization;
using YARG.Menu.Navigation;
using YARG.Settings;
using YARG.Settings.Customization;
using YARG.Settings.Metadata;

namespace YARG.Menu.Settings
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

        public List<MarketplaceStore> _stores = new() { new YARC(), new YARN() };
        public Dictionary<MarketplaceStore, List<SetlistItem>> marketplaceCache = new();
        public MarketplaceStore CurrentStore;

        private static GameObject _buttonPrefab;

        private async void Start()
        {
            _buttonPrefab = Addressables
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

            List<SetlistItem> store1 = await _stores[0].GetSetlists();
            foreach (SetlistItem setlist in store1)
            {
                YargLogger.LogFormatInfo("Found setlist: {0}", setlist.Name);
            }
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
                buttonObj.setlist.Info = await CurrentStore.GetSongs(buttonObj.setlist.Identifier);
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
        private async void UpdateSettings(bool resetScroll)
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
            List<SetlistItem> setlistsToCreate = null;
            if (!marketplaceCache.TryGetValue(CurrentStore, out setlistsToCreate))
            {
                setlistsToCreate = await CurrentStore.GetSetlists();
                marketplaceCache.Add(CurrentStore, setlistsToCreate);
            }
            if (setlistsToCreate == null || setlistsToCreate.Count == 0)
                return;

            foreach (SetlistItem setlist in setlistsToCreate)
            {
                GameObject newButton = Instantiate(_buttonPrefab, _settingsContainer);
                SetlistButton button = newButton.GetComponent<SetlistButton>();
                _settingsNavGroup.AddNavigatable(button);
                button.icon.sprite = Sprite.Create(setlist.Cover, new Rect(0,0,setlist.Cover.width,setlist.Cover.height), new Vector2(0,0));
                button.title.text = setlist.Name;
                button.setlist = setlist;
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
            {
                GameObject newButton = Instantiate(_buttonPrefab, _settingsContainer);
                SetlistButton button = newButton.GetComponent<SetlistButton>();
                _settingsNavGroup.AddNavigatable(button);
                button.icon.sprite = Sprite.Create(setlist.Cover, new Rect(0, 0, setlist.Cover.width, setlist.Cover.height), new Vector2(0, 0));
                button.title.text = setlist.Name;
                button.setlist = setlist;
            }

            // Make the settings nav group the main one
            _settingsNavGroup.SelectFirst();

            // Reset scroll rect
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private void OnDisable()
        {

            Navigator.Instance.PopScheme();
            _headerTabs.TabChanged -= OnTabChanged;

            _settingsNavGroup.SelectionChanged -= OnSelectionChanged;
        }
    }
}
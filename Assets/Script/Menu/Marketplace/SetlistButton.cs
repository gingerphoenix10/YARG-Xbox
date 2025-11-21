using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YARG.Core.Input;
using YARG.Localization;
using YARG.Menu.Navigation;
using YARG.Settings;

namespace YARG.Menu.Marketplace
{
    public class SetlistButton : NavigatableBehaviour
    {
        public Image icon;
        public TextMeshProUGUI title;
        public SetlistItem setlist;
        public NavigatableUnityButton _downloadButton;
        public NavigatableUnityButton _uninstallButton;
        public NavigationGroup _navGroup;
        public RectMask2D progress;

        private bool _focused;

        public override void Confirm()
        {
            var scheme = new NavigationScheme(new()
            {
                NavigationScheme.Entry.NavigateSelect,
                new NavigationScheme.Entry(MenuAction.Red, "Menu.Common.Back", () =>
                {
                    Navigator.Instance.PopScheme();
                }),
                NavigationScheme.Entry.NavigateUp,
                NavigationScheme.Entry.NavigateDown
            }, true);

            scheme.PopCallback = () =>
            {
                _focused = false;
                _navGroup.SelectLastNavGroup();
            };

            Navigator.Instance.PushScheme(scheme);

            _focused = true;
            _navGroup.PushNavGroupToStack();
            _navGroup.SelectFirst();
        }

        protected override void OnSelectionChanged(bool selected)
        {
            base.OnSelectionChanged(selected);
            OnDisable();
        }

        private void OnDisable()
        {
            // If the visual's nav scheme is still in the stack, make sure to pop it.
            if (_focused)
            {
                Navigator.Instance.PopScheme();
            }
        }
    }
}
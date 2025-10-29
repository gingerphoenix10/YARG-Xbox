using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YARG.Assets.Script.Menu.Marketplace;
using YARG.Core.Input;
using YARG.Localization;
using YARG.Menu.Navigation;
using YARG.Settings;

namespace YARG.Menu.Settings
{
    public class SetlistButton : NavigatableUnityButton
    {
        public Image icon;
        public TextMeshProUGUI title;
        public SetlistItem setlist;
    }
}
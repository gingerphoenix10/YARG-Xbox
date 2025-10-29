using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YARG.Assets.Script.Menu.Marketplace
{
    public abstract class MarketplaceStore
    {
        public abstract string Name { get; }
        public abstract Sprite Icon { get; }
        public abstract Task<List<SetlistItem>> GetSetlists();
        public abstract Task<List<SetlistItem>> Search(string term);
        public abstract Task<SetlistInfo> GetSongs(string identifier);
    }

    public class SetlistItem
    {
        public string Name;
        public Texture2D Cover;
        public string Identifier;
        public SetlistInfo Info;
    }
    public class SetlistInfo
    {
        public string Description;
        public List<SetlistSong> Songs;
    }

    public class SetlistSong
    {
        public string Name;
        public string Artist;
        public TimeSpan Length;
    }
}

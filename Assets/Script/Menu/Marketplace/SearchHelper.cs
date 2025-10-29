// I'm happy to say that I did not write any of this. I am not putting this much effort into a search bar
using System;
using System.Collections.Generic;
using System.Text;

namespace YARG.Assets.Script.Menu.Marketplace
{
    public static class SearchHelper
    {
        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
                return string.IsNullOrEmpty(t) ? 0 : t.Length;
            if (string.IsNullOrEmpty(t))
                return s.Length;

            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= t.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1,
                                 d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[s.Length, t.Length];
        }

        public static double Similarity(string s, string t)
        {
            s = s.ToLower();
            t = t.ToLower();
            int maxLen = Math.Max(s.Length, t.Length);
            if (maxLen == 0) return 1.0;
            return 1.0 - (double) LevenshteinDistance(s, t) / maxLen;
        }
    }

}

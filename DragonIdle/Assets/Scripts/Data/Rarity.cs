using UnityEngine;

namespace DragonIdle
{
    /// <summary>孵化のたびに抽選されるレアリティ。生産倍率とティア（強さの段階）を決める。</summary>
    public enum Rarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3,
        Mythic = 4
    }

    public static class Rarities
    {
        public const int Count = 5;

        public static string Name(Rarity r)
        {
            switch (r)
            {
                case Rarity.Common: return "コモン";
                case Rarity.Rare: return "レア";
                case Rarity.Epic: return "エピック";
                case Rarity.Legendary: return "レジェンド";
                default: return "神話";
            }
        }

        public static Color Tint(Rarity r)
        {
            switch (r)
            {
                case Rarity.Common: return new Color32(0x8E, 0x93, 0xA8, 0xFF);
                case Rarity.Rare: return new Color32(0x4F, 0xA8, 0xE8, 0xFF);
                case Rarity.Epic: return new Color32(0xB3, 0x88, 0xFF, 0xFF);
                case Rarity.Legendary: return new Color32(0xF2, 0xC1, 0x4E, 0xFF);
                default: return new Color32(0xFF, 0x6F, 0xA5, 0xFF);
            }
        }

        /// <summary>レアリティそのものの生産倍率。</summary>
        public static double Multiplier(Rarity r)
        {
            switch (r)
            {
                case Rarity.Common: return 1.0;
                case Rarity.Rare: return 2.2;
                case Rarity.Epic: return 5.0;
                case Rarity.Legendary: return 12.0;
                default: return 30.0;
            }
        }

        /// <summary>そのレアリティで出うる種族ティアの範囲（1〜5）。</summary>
        public static void TierRange(Rarity r, out int min, out int max)
        {
            switch (r)
            {
                case Rarity.Common: min = 1; max = 2; break;
                case Rarity.Rare: min = 2; max = 3; break;
                case Rarity.Epic: min = 3; max = 4; break;
                case Rarity.Legendary: min = 4; max = 5; break;
                default: min = 5; max = 5; break;
            }
        }
    }
}

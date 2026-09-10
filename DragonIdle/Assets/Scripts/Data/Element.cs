using UnityEngine;

namespace DragonIdle
{
    /// <summary>ドラゴンの属性。巣に何種類そろっているかがシナジー倍率になる。</summary>
    public enum Element
    {
        Fire = 0,
        Water = 1,
        Wind = 2,
        Earth = 3,
        Thunder = 4,
        Light = 5,
        Dark = 6
    }

    public static class Elements
    {
        public const int Count = 7;

        public static string Name(Element e)
        {
            switch (e)
            {
                case Element.Fire: return "火";
                case Element.Water: return "水";
                case Element.Wind: return "風";
                case Element.Earth: return "土";
                case Element.Thunder: return "雷";
                case Element.Light: return "光";
                default: return "闇";
            }
        }

        public static Color Tint(Element e)
        {
            switch (e)
            {
                case Element.Fire: return new Color32(0xFF, 0x6B, 0x3D, 0xFF);
                case Element.Water: return new Color32(0x3F, 0xA9, 0xF5, 0xFF);
                case Element.Wind: return new Color32(0x5F, 0xD9, 0xA8, 0xFF);
                case Element.Earth: return new Color32(0xC7, 0x9A, 0x5B, 0xFF);
                case Element.Thunder: return new Color32(0xF5, 0xD2, 0x3F, 0xFF);
                case Element.Light: return new Color32(0xFF, 0xE9, 0xA3, 0xFF);
                default: return new Color32(0x9B, 0x6B, 0xFF, 0xFF);
            }
        }
    }
}

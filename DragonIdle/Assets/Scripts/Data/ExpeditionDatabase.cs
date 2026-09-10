using System.Collections.Generic;

namespace DragonIdle
{
    /// <summary>
    /// 探索先。行き先ごとに所要時間と実入りが決まっていて、留守にするあいだ
    /// そのドラゴンは巣で稼がない。長い旅ほど割がよく、帰ると経験も積んでいる。
    /// </summary>
    public class ExpeditionDef
    {
        public readonly string Name;
        public readonly string Description;
        public readonly double Hours;
        public readonly double Multiplier;

        public ExpeditionDef(string name, string description, double hours, double multiplier)
        {
            Name = name;
            Description = description;
            Hours = hours;
            Multiplier = multiplier;
        }

        public double Seconds { get { return Hours * 3600.0; } }

        /// <summary>帰還時に無料で上がるレベル。長旅ほど鍛えられる。</summary>
        public int LevelGain
        {
            get
            {
                int gain = (int)(Hours * 1.5);
                return gain < 1 ? 1 : gain;
            }
        }
    }

    public static class ExpeditionDatabase
    {
        public static readonly List<ExpeditionDef> All = new List<ExpeditionDef>
        {
            new ExpeditionDef("風なぎの丘", "巣のすぐ裏手。散歩のついでに金貨を拾ってくる。", 10.0 / 60.0, 2.2),
            new ExpeditionDef("霧笛の谷", "霧の底に古い隊商の落とし物が眠っている。", 1.0, 2.6),
            new ExpeditionDef("火口の縁", "熱に灼かれた岩肌に、溶け残った宝が張りついている。", 4.0, 3.2),
            new ExpeditionDef("天空回廊", "雲の上に伸びる石の道。半日かけて端まで往復する。", 12.0, 4.0)
        };

        public static int Count { get { return All.Count; } }

        public static ExpeditionDef Get(int index)
        {
            if (index < 0 || index >= All.Count) return All[0];
            return All[index];
        }
    }
}

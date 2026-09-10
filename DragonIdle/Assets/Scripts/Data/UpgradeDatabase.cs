using System.Collections.Generic;

namespace DragonIdle
{
    public enum UpgradeId
    {
        Treasury = 0,   // 宝物庫: 全生産量アップ
        Nest = 1,       // 巣穴拡張: 育てられる数を増やす
        Training = 2,   // 訓練場: 育成コストを下げる
        Altar = 3,      // 竜の祭壇: 孵化の当たりを引きやすくする
        Hourglass = 4,  // 時の砂時計: 放置報酬の上限時間を伸ばす
        Flute = 5,      // 竜笛: なでたときの報酬を増やす
        Hoard = 6       // 寝床の宝: 放置効率そのものを上げる
    }

    /// <summary>買うたびにレベルが1上がる施設。効果は種類ごとに固定式で伸びる。</summary>
    public class UpgradeDef
    {
        public readonly UpgradeId Id;
        public readonly string Name;
        public readonly string Description;
        public readonly double BaseCost;
        public readonly double CostGrowth;
        public readonly int MaxLevel;

        public UpgradeDef(UpgradeId id, string name, string description, double baseCost, double costGrowth, int maxLevel)
        {
            Id = id;
            Name = name;
            Description = description;
            BaseCost = baseCost;
            CostGrowth = costGrowth;
            MaxLevel = maxLevel;
        }

        public double CostAt(int level)
        {
            double cost = BaseCost;
            for (int i = 0; i < level; i++) cost *= CostGrowth;
            return System.Math.Floor(cost);
        }
    }

    public static class UpgradeDatabase
    {
        public static readonly List<UpgradeDef> All = new List<UpgradeDef>
        {
            new UpgradeDef(UpgradeId.Treasury, "宝物庫",
                "積み上げた金貨が竜の自慢になる。全ドラゴンの生産量 +8%（レベルごと）", 500, 1.34, 200),
            new UpgradeDef(UpgradeId.Nest, "巣穴拡張",
                "岩壁を掘り広げて寝床を増やす。育てられるドラゴン +1", 1200, 2.15, 21),
            new UpgradeDef(UpgradeId.Training, "訓練場",
                "先輩竜が稽古をつけてくれる。育成コスト -3%（累乗）", 2500, 1.55, 40),
            new UpgradeDef(UpgradeId.Altar, "竜の祭壇",
                "供物を焚いて良い卵を呼ぶ。孵化のレア度が上がりやすくなる", 8000, 1.85, 25),
            new UpgradeDef(UpgradeId.Hourglass, "時の砂時計",
                "留守のあいだも砂が落ち続ける。放置報酬の上限 +1時間", 6000, 1.75, 20),
            new UpgradeDef(UpgradeId.Flute, "竜笛",
                "ひと吹きで竜が寄ってくる。「なでる」の報酬 +50%（レベルごと）", 1500, 1.65, 30),
            new UpgradeDef(UpgradeId.Hoard, "寝床の宝",
                "宝の上で眠ると竜はよく働く。放置報酬の効率 +4%（レベルごと）", 15000, 1.9, 10)
        };

        public static int Count { get { return All.Count; } }

        public static UpgradeDef Get(UpgradeId id) { return All[(int)id]; }
    }
}

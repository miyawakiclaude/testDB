using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonIdle
{
    /// <summary>
    /// 称号。条件を満たすと永久に生産量が上がり、転生しても消えない。
    /// すべて「ある値が目標に届いたか」という形なので、進捗バーも同じ式から出せる。
    /// </summary>
    public class Achievement
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Requirement;
        public readonly double Bonus;

        readonly Func<GameManager, double> _value;
        readonly double _target;

        public Achievement(string id, string name, string requirement, double bonus,
            Func<GameManager, double> value, double target)
        {
            Id = id;
            Name = name;
            Requirement = requirement;
            Bonus = bonus;
            _value = value;
            _target = target;
        }

        public bool IsMet(GameManager game) { return _value(game) >= _target; }

        public float Progress(GameManager game)
        {
            if (_target <= 0) return 1f;
            return Mathf.Clamp01((float)(_value(game) / _target));
        }

        public string ProgressText(GameManager game)
        {
            double current = Math.Min(_value(game), _target);
            return NumberFormat.Gold(current) + " / " + NumberFormat.Gold(_target);
        }
    }

    public static class AchievementDatabase
    {
        public static readonly List<Achievement> All = new List<Achievement>
        {
            new Achievement("gold_1", "小金持ち", "累計1万ゴールド", 0.02,
                g => g.Data.allTimeGold, 10000),
            new Achievement("gold_2", "宝の山", "累計100万ゴールド", 0.03,
                g => g.Data.allTimeGold, 1000000),
            new Achievement("gold_3", "竜の財宝", "累計1億ゴールド", 0.05,
                g => g.Data.allTimeGold, 100000000.0),
            new Achievement("gold_4", "黄金の谷", "累計1兆ゴールド", 0.10,
                g => g.Data.allTimeGold, 1e12),

            new Achievement("egg_1", "はじめての孵化", "卵を5個かえす", 0.02,
                g => g.Data.eggsAllTime, 5),
            new Achievement("egg_2", "巣の主", "卵を25個かえす", 0.03,
                g => g.Data.eggsAllTime, 25),
            new Achievement("egg_3", "孵化場", "卵を100個かえす", 0.06,
                g => g.Data.eggsAllTime, 100),

            new Achievement("rare_1", "掘り出しもの", "レア以上を手に入れる", 0.02,
                g => g.Data.bestRarity, (int)Rarity.Rare),
            new Achievement("rare_2", "目利き", "エピック以上を手に入れる", 0.04,
                g => g.Data.bestRarity, (int)Rarity.Epic),
            new Achievement("rare_3", "伝説の目撃者", "レジェンド以上を手に入れる", 0.07,
                g => g.Data.bestRarity, (int)Rarity.Legendary),
            new Achievement("rare_4", "神話に触れる", "神話のドラゴンを手に入れる", 0.12,
                g => g.Data.bestRarity, (int)Rarity.Mythic),

            new Achievement("level_1", "覚醒", "どれかをLv.25まで育てる", 0.03,
                g => g.Data.bestLevel, 25),
            new Achievement("level_2", "熟練の育て手", "どれかをLv.50まで育てる", 0.05,
                g => g.Data.bestLevel, 50),
            new Achievement("level_3", "竜と生きる", "どれかをLv.100まで育てる", 0.10,
                g => g.Data.bestLevel, 100),

            new Achievement("codex_1", "観察日記", "10種を図鑑に載せる", 0.03,
                g => g.Data.discovered.Count, 10),
            new Achievement("codex_2", "竜類学者", "20種を図鑑に載せる", 0.06,
                g => g.Data.discovered.Count, 20),
            new Achievement("codex_3", "図鑑完成", "35種すべてを図鑑に載せる", 0.15,
                g => g.Data.discovered.Count, SpeciesDatabase.TotalCount),

            new Achievement("synergy", "七属の巣", "7属性を同時に巣にそろえる", 0.08,
                g => g.DistinctElements, 7),

            new Achievement("rebirth_1", "はじめての転生", "1回転生する", 0.04,
                g => g.Data.rebirths, 1),
            new Achievement("rebirth_2", "輪をめぐる", "5回転生する", 0.07,
                g => g.Data.rebirths, 5),
            new Achievement("rebirth_3", "永劫の育て手", "20回転生する", 0.15,
                g => g.Data.rebirths, 20),

            new Achievement("soul_1", "魂を宿す", "竜魂を10集める", 0.05,
                g => g.Data.souls, 10),
            new Achievement("soul_2", "魂の器", "竜魂を100集める", 0.12,
                g => g.Data.souls, 100),

            new Achievement("pet_1", "なつかれる", "100回なでる", 0.02,
                g => g.Data.pets, 100),
            new Achievement("pet_2", "巣の人気者", "1000回なでる", 0.05,
                g => g.Data.pets, 1000),

            new Achievement("facility", "石工の腕", "どれかの施設を最大まで拡張する", 0.08,
                g => g.MaxedUpgradeCount, 1),

            new Achievement("rate", "毎秒100万", "毎秒100万ゴールドに届く", 0.10,
                g => g.GoldPerSecond, 1000000.0)
        };

        public static int Count { get { return All.Count; } }
    }
}

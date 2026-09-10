using System;
using System.Collections.Generic;

namespace DragonIdle
{
    [Serializable]
    public class DragonSave
    {
        public string speciesId;
        public int rarity;
        public int level = 1;
        public float individual = 1f; // 個体値: 同じ種族でも少しだけ強さが違う
    }

    /// <summary>JsonUtility でそのまま保存できる形。ここにある値だけが引き継がれる。</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;

        public double gold;
        public double lifetimeGold;      // 今回の転生してからの累計。転生ボーナスの計算元
        public double allTimeGold;       // 全期間の累計。記録タブ用
        public int souls;                // 竜魂: 転生で増える永続ボーナス
        public int rebirths;
        public int eggsHatched;      // 今回の周回の孵化数。卵の値段はこれで上がる
        public int eggsAllTime;      // 全期間の孵化数。記録タブ用
        public int pets;
        public int bestRarity;       // これまでに手に入れた最高レアリティ
        public int bestLevel = 1;    // これまでに到達した最高レベル
        public double playSeconds;
        public long lastSaveUnix;

        public List<DragonSave> dragons = new List<DragonSave>();
        public List<int> upgradeLevels = new List<int>();
        public List<string> discovered = new List<string>();
        public List<string> achievements = new List<string>();

        public void EnsureShape()
        {
            if (dragons == null) dragons = new List<DragonSave>();
            if (upgradeLevels == null) upgradeLevels = new List<int>();
            if (discovered == null) discovered = new List<string>();
            if (achievements == null) achievements = new List<string>();
            while (upgradeLevels.Count < UpgradeDatabase.Count) upgradeLevels.Add(0);
        }
    }
}

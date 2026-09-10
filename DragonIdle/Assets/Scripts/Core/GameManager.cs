using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonIdle
{
    /// <summary>留守のあいだに貯まったぶんの明細。復帰時に一度だけ表示する。</summary>
    public struct OfflineReport
    {
        public double Seconds;
        public double Gold;
        public bool HitCap;
        public bool HasValue;
    }

    /// <summary>
    /// ゲームの状態と計算をすべて持つ。UI はここを読むだけで、数値の決定はしない。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public SaveData Data { get; private set; }

        /// <summary>ドラゴンの増減や転生でリストの作り直しが要るときに増える。</summary>
        public int StructureVersion { get; private set; }

        public OfflineReport PendingOffline;

        public event Action<string> OnToast;

        const int BaseNestCapacity = 3;
        const double BaseEggCost = 100.0;
        const double EggCostGrowth = 1.55;
        const double RebirthThreshold = 1000000.0;

        float _saveTimer;
        float _achievementTimer;
        double _achievementMultiplier = 1.0;
        readonly HashSet<string> _unlocked = new HashSet<string>();
        readonly System.Random _rng = new System.Random();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Data = SaveSystem.Load();
            if (Data == null) Data = CreateNewGame();
            Data.EnsureShape();
            RebuildAchievementCache();
            ApplyOfflineProgress();
        }

        SaveData CreateNewGame()
        {
            SaveData data = new SaveData();
            data.EnsureShape();
            data.gold = 60;
            data.lastSaveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            DragonSave starter = new DragonSave();
            starter.speciesId = "fire1";
            starter.rarity = (int)Rarity.Common;
            starter.level = 1;
            starter.individual = 1f;
            data.dragons.Add(starter);
            data.discovered.Add("fire1");
            return data;
        }

        void ApplyOfflineProgress()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            double elapsed = now - Data.lastSaveUnix;
            if (Data.lastSaveUnix <= 0 || elapsed < 60) return;

            double capSeconds = OfflineCapHours * 3600.0;
            bool hitCap = elapsed > capSeconds;
            double counted = Math.Min(elapsed, capSeconds);
            double earned = GoldPerSecond * counted * OfflineEfficiency;
            if (earned <= 0) return;

            AddGold(earned);
            PendingOffline = new OfflineReport
            {
                Seconds = elapsed,
                Gold = earned,
                HitCap = hitCap,
                HasValue = true
            };
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Data.playSeconds += dt;
            AddGold(GoldPerSecond * dt);

            _achievementTimer += dt;
            if (_achievementTimer >= 0.5f)
            {
                _achievementTimer = 0f;
                CheckAchievements();
            }

            _saveTimer += dt;
            if (_saveTimer >= 15f)
            {
                _saveTimer = 0f;
                SaveSystem.Save(Data);
            }
        }

        void OnApplicationPause(bool paused) { if (paused) SaveSystem.Save(Data); }
        void OnApplicationFocus(bool focused) { if (!focused) SaveSystem.Save(Data); }
        void OnApplicationQuit() { SaveSystem.Save(Data); }

        void AddGold(double amount)
        {
            if (amount <= 0) return;
            Data.gold += amount;
            Data.lifetimeGold += amount;
            Data.allTimeGold += amount;
        }

        // ---------- 倍率まわり ----------

        public int UpgradeLevel(UpgradeId id) { return Data.upgradeLevels[(int)id]; }

        public int NestCapacity { get { return BaseNestCapacity + UpgradeLevel(UpgradeId.Nest); } }

        public double TreasuryMultiplier { get { return 1.0 + 0.08 * UpgradeLevel(UpgradeId.Treasury); } }

        public double SoulMultiplier { get { return 1.0 + 0.10 * Data.souls; } }

        /// <summary>巣にそろっている属性の種類ぶんだけ全体が伸びる。</summary>
        public int DistinctElements
        {
            get
            {
                bool[] seen = new bool[Elements.Count];
                int count = 0;
                for (int i = 0; i < Data.dragons.Count; i++)
                {
                    int e = (int)SpeciesDatabase.ById(Data.dragons[i].speciesId).Element;
                    if (!seen[e]) { seen[e] = true; count++; }
                }
                return count;
            }
        }

        public double SynergyMultiplier { get { return 1.0 + 0.05 * DistinctElements; } }

        public double AchievementMultiplier { get { return _achievementMultiplier; } }

        public double GlobalMultiplier
        {
            get { return TreasuryMultiplier * SoulMultiplier * SynergyMultiplier * _achievementMultiplier; }
        }

        /// <summary>図鑑に載っている中で最も高いティア。称号の判定に使う。</summary>
        public int HighestDiscoveredTier
        {
            get
            {
                int best = 0;
                for (int i = 0; i < Data.discovered.Count; i++)
                {
                    int tier = SpeciesDatabase.ById(Data.discovered[i]).Tier;
                    if (tier > best) best = tier;
                }
                return best;
            }
        }

        public int MaxedUpgradeCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < UpgradeDatabase.Count; i++)
                {
                    if (UpgradeMaxed((UpgradeId)i)) count++;
                }
                return count;
            }
        }

        // ---------- 称号 ----------

        public bool IsUnlocked(string achievementId) { return _unlocked.Contains(achievementId); }

        public int UnlockedAchievementCount { get { return _unlocked.Count; } }

        void RebuildAchievementCache()
        {
            _unlocked.Clear();
            double bonus = 0;
            for (int i = 0; i < AchievementDatabase.All.Count; i++)
            {
                Achievement achievement = AchievementDatabase.All[i];
                if (!Data.achievements.Contains(achievement.Id)) continue;
                _unlocked.Add(achievement.Id);
                bonus += achievement.Bonus;
            }
            _achievementMultiplier = 1.0 + bonus;
        }

        /// <summary>条件を満たした称号を拾い上げる。0.5秒ごとに呼ばれる。</summary>
        void CheckAchievements()
        {
            bool changed = false;
            for (int i = 0; i < AchievementDatabase.All.Count; i++)
            {
                Achievement achievement = AchievementDatabase.All[i];
                if (_unlocked.Contains(achievement.Id)) continue;
                if (!achievement.IsMet(this)) continue;

                Data.achievements.Add(achievement.Id);
                _unlocked.Add(achievement.Id);
                changed = true;
                Sfx.Play(SfxId.Achievement);
                Toast("称号「" + achievement.Name + "」を得た（生産 "
                      + NumberFormat.Percent(achievement.Bonus) + "）");
            }
            if (changed) RebuildAchievementCache();
        }

        public double OfflineCapHours { get { return 4.0 + UpgradeLevel(UpgradeId.Hourglass); } }

        public double OfflineEfficiency
        {
            get { return Math.Min(1.0, 0.5 + 0.04 * UpgradeLevel(UpgradeId.Hoard)); }
        }

        // ---------- ドラゴン ----------

        /// <summary>倍率を掛ける前の、そのドラゴン単体の生産量。</summary>
        public double BaseProduction(DragonSave d)
        {
            DragonSpecies s = SpeciesDatabase.ById(d.speciesId);
            double levelFactor = 1.0 + 0.15 * (d.level - 1);
            double awakening = Math.Pow(2.0, d.level / 25); // 25レベルごとに覚醒して2倍
            return s.BaseRate * Rarities.Multiplier((Rarity)d.rarity) * levelFactor * awakening * d.individual;
        }

        /// <summary>画面に出る、倍率込みの毎秒生産量。</summary>
        public double Production(DragonSave d) { return BaseProduction(d) * GlobalMultiplier; }

        public double GoldPerSecond
        {
            get
            {
                double sum = 0;
                for (int i = 0; i < Data.dragons.Count; i++) sum += BaseProduction(Data.dragons[i]);
                return sum * GlobalMultiplier;
            }
        }

        public double LevelUpCost(DragonSave d)
        {
            DragonSpecies s = SpeciesDatabase.ById(d.speciesId);
            double cost = 12.0 * s.BaseRate * Rarities.Multiplier((Rarity)d.rarity)
                          * Math.Pow(1.13, d.level - 1)
                          * Math.Pow(0.97, UpgradeLevel(UpgradeId.Training));
            return Math.Max(1.0, Math.Floor(cost));
        }

        public bool LevelUp(DragonSave d)
        {
            if (!LevelUpCore(d)) return false;
            Sfx.Play(SfxId.LevelUp);
            return true;
        }

        bool LevelUpCore(DragonSave d)
        {
            double cost = LevelUpCost(d);
            if (Data.gold < cost) return false;
            Data.gold -= cost;
            d.level++;
            if (d.level > Data.bestLevel) Data.bestLevel = d.level;
            if (d.level % 25 == 0)
            {
                Toast(SpeciesDatabase.ById(d.speciesId).Name + " が覚醒した！ 生産量が2倍");
            }
            return true;
        }

        /// <summary>まとめて上げる。押しっぱなしにしなくてよくなる。音は1回だけ鳴らす。</summary>
        public int LevelUpMany(DragonSave d, int max)
        {
            int done = 0;
            while (done < max && LevelUpCore(d)) done++;
            if (done > 0) Sfx.Play(SfxId.LevelUp);
            return done;
        }

        // ---------- 孵化 ----------

        public double EggCost
        {
            get { return Math.Floor(BaseEggCost * Math.Pow(EggCostGrowth, Data.eggsHatched)); }
        }

        public bool NestIsFull { get { return Data.dragons.Count >= NestCapacity; } }

        public bool CanHatch { get { return !NestIsFull && Data.gold >= EggCost; } }

        /// <summary>祭壇と竜魂で上振れしやすくなる。</summary>
        public double LuckFactor
        {
            get { return 1.0 + 0.12 * UpgradeLevel(UpgradeId.Altar) + 0.02 * Data.souls; }
        }

        Rarity RollRarity()
        {
            double[] weights = { 60.0, 25.0, 10.0, 4.0, 1.0 };
            double luck = LuckFactor;
            double total = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] *= Math.Pow(luck, i);
                total += weights[i];
            }
            double roll = _rng.NextDouble() * total;
            for (int i = 0; i < weights.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0) return (Rarity)i;
            }
            return Rarity.Common;
        }

        public DragonSave HatchEgg()
        {
            if (NestIsFull)
            {
                Toast("巣がいっぱい。巣穴を広げるか、ドラゴンを見送ろう");
                return null;
            }
            double cost = EggCost;
            if (Data.gold < cost)
            {
                Toast("ゴールドが足りない");
                return null;
            }

            Data.gold -= cost;
            Data.eggsHatched++;
            Data.eggsAllTime++;

            Rarity rarity = RollRarity();
            if ((int)rarity > Data.bestRarity) Data.bestRarity = (int)rarity;
            int minTier, maxTier;
            Rarities.TierRange(rarity, out minTier, out maxTier);
            List<DragonSpecies> pool = SpeciesDatabase.InTierRange(minTier, maxTier);
            DragonSpecies species = pool[_rng.Next(pool.Count)];

            DragonSave d = new DragonSave();
            d.speciesId = species.Id;
            d.rarity = (int)rarity;
            d.level = 1;
            d.individual = 0.9f + (float)_rng.NextDouble() * 0.25f;
            Data.dragons.Add(d);

            bool isNew = !Data.discovered.Contains(species.Id);
            if (isNew) Data.discovered.Add(species.Id);

            StructureVersion++;
            Sfx.Play(SfxId.Hatch);
            Toast((isNew ? "新種発見！ " : "") + Rarities.Name(rarity) + "の " + species.Name + " が生まれた");
            return d;
        }

        /// <summary>巣を空けるために見送る。育てたぶんは少しだけ戻る。</summary>
        public bool Release(DragonSave d)
        {
            if (Data.dragons.Count <= 1)
            {
                Toast("最後の1匹は見送れない");
                return false;
            }
            double refund = BaseProduction(d) * 30.0;
            Data.dragons.Remove(d);
            Data.gold += refund;
            StructureVersion++;
            Toast(SpeciesDatabase.ById(d.speciesId).Name + " を空へ還した（" + NumberFormat.Gold(refund) + " ゴールド）");
            return true;
        }

        // ---------- 進化 ----------

        /// <summary>同じ種族のもう1匹。進化はこの相手を取り込む形で行う。</summary>
        public DragonSave FindEvolutionPartner(DragonSave d)
        {
            DragonSpecies species = SpeciesDatabase.ById(d.speciesId);
            if (species.Tier >= 5) return null;
            for (int i = 0; i < Data.dragons.Count; i++)
            {
                DragonSave other = Data.dragons[i];
                if (!ReferenceEquals(other, d) && other.speciesId == d.speciesId) return other;
            }
            return null;
        }

        public DragonSpecies EvolutionTarget(DragonSave d)
        {
            DragonSpecies species = SpeciesDatabase.ById(d.speciesId);
            if (species.Tier >= 5) return null;
            return SpeciesDatabase.Find(species.Element, species.Tier + 1);
        }

        public bool CanEvolve(DragonSave d)
        {
            return FindEvolutionPartner(d) != null && EvolutionTarget(d) != null;
        }

        public double EvolveCost(DragonSave d)
        {
            DragonSpecies next = EvolutionTarget(d);
            if (next == null) return 0;
            return Math.Floor(180.0 * next.BaseRate * Rarities.Multiplier((Rarity)d.rarity));
        }

        /// <summary>
        /// 同じ種族を2匹あわせて、同属性のひとつ上の種族にする。
        /// レアリティも1段上がり、レベルと個体値は良いほうを引き継ぐ。
        /// </summary>
        public bool Evolve(DragonSave d)
        {
            DragonSave partner = FindEvolutionPartner(d);
            DragonSpecies next = EvolutionTarget(d);
            if (partner == null || next == null) return false;

            double cost = EvolveCost(d);
            if (Data.gold < cost)
            {
                Toast("進化にはゴールドが足りない");
                return false;
            }

            string before = SpeciesDatabase.ById(d.speciesId).Name;
            Data.gold -= cost;

            int rarity = Mathf.Min(Mathf.Max(d.rarity, partner.rarity) + 1, Rarities.Count - 1);
            int level = Mathf.Max(d.level, partner.level);
            float individual = Mathf.Min(Mathf.Max(d.individual, partner.individual) + 0.05f, 1.35f);

            Data.dragons.Remove(partner);
            d.speciesId = next.Id;
            d.rarity = rarity;
            d.level = level;
            d.individual = individual;

            if (rarity > Data.bestRarity) Data.bestRarity = rarity;
            if (!Data.discovered.Contains(next.Id)) Data.discovered.Add(next.Id);
            Data.evolutions++;

            StructureVersion++;
            Sfx.Play(SfxId.Evolve);
            Toast(before + " が " + next.Name + " に進化した（" + Rarities.Name((Rarity)rarity) + "）");
            return true;
        }

        // ---------- なでる ----------

        public double PetReward
        {
            get
            {
                double perSecond = GoldPerSecond;
                double multiplier = 2.0 * (1.0 + 0.5 * UpgradeLevel(UpgradeId.Flute));
                return Math.Max(1.0, perSecond * multiplier);
            }
        }

        public double Pet()
        {
            double reward = PetReward;
            AddGold(reward);
            Data.pets++;
            Sfx.Play(SfxId.Pet);
            return reward;
        }

        // ---------- 施設 ----------

        public double UpgradeCost(UpgradeId id)
        {
            return UpgradeDatabase.Get(id).CostAt(UpgradeLevel(id));
        }

        public bool UpgradeMaxed(UpgradeId id)
        {
            return UpgradeLevel(id) >= UpgradeDatabase.Get(id).MaxLevel;
        }

        public bool BuyUpgrade(UpgradeId id)
        {
            if (UpgradeMaxed(id)) return false;
            double cost = UpgradeCost(id);
            if (Data.gold < cost) return false;
            Data.gold -= cost;
            Data.upgradeLevels[(int)id]++;
            Sfx.Play(SfxId.Buy);
            if (id == UpgradeId.Nest) StructureVersion++;
            return true;
        }

        // ---------- 転生 ----------

        public int SoulsIfRebirthNow
        {
            get
            {
                if (Data.lifetimeGold < RebirthThreshold) return 0;
                int total = (int)Math.Floor(Math.Pow(Data.lifetimeGold / RebirthThreshold, 0.45));
                return Math.Max(0, total);
            }
        }

        public int SoulGain { get { return Math.Max(0, SoulsIfRebirthNow - Data.souls); } }

        public bool CanRebirth { get { return SoulGain > 0; } }

        public double RebirthProgress
        {
            get { return Math.Min(1.0, Data.lifetimeGold / RebirthThreshold); }
        }

        public double GoldNeededForNextSoul
        {
            get
            {
                int target = Math.Max(Data.souls + 1, SoulsIfRebirthNow + 1);
                return RebirthThreshold * Math.Pow(target, 1.0 / 0.45);
            }
        }

        public bool Rebirth()
        {
            if (!CanRebirth) return false;

            int gained = SoulGain;
            Data.souls += gained;
            Data.rebirths++;
            Data.gold = 60;
            Data.lifetimeGold = 0;
            Data.eggsHatched = 0;
            Data.dragons.Clear();
            for (int i = 0; i < Data.upgradeLevels.Count; i++) Data.upgradeLevels[i] = 0;

            DragonSave starter = new DragonSave();
            starter.speciesId = "fire1";
            starter.rarity = (int)Rarity.Common;
            starter.level = 1;
            starter.individual = 1f;
            Data.dragons.Add(starter);

            StructureVersion++;
            SaveSystem.Save(Data);
            Sfx.Play(SfxId.Rebirth);
            Toast("転生した。竜魂を " + gained + " 得た（生産 " + NumberFormat.Percent(0.10 * Data.souls) + "）");
            return true;
        }

        public void ResetEverything()
        {
            SaveSystem.Delete();
            Data = CreateNewGame();
            RebuildAchievementCache();
            StructureVersion++;
            Toast("最初からやり直す");
        }

        public void Toast(string message)
        {
            if (OnToast != null) OnToast(message);
        }
    }
}

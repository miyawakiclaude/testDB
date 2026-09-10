using System;
using System.Collections.Generic;
using System.Reflection;
using DragonIdle;

/// <summary>
/// ゲームのロジックを実際に一通り動かして、落ちたり数が合わなくなったりしないかを見る。
/// 型チェックでは出ない、実行時の取りこぼしを拾うためのもの。
/// </summary>
public static class SmokeTest
{
    static int _passed;
    static int _failed;
    static GameManager _game;
    static MethodInfo _update;

    public static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Sfx.Muted = true;

        NewGame();

        TestStart();
        TestHatchAndNest();
        TestUpgrades();
        TestLevelling();
        TestEvolution();
        TestExpedition();
        TestRelease();
        TestAchievements();
        TestMigration();
        TestRebirth();
        TestReset();

        Console.WriteLine();
        Console.WriteLine(_failed == 0
            ? "通しテスト OK（" + _passed + "件）"
            : "失敗 " + _failed + "件 / 成功 " + _passed + "件");
        return _failed == 0 ? 0 : 1;
    }

    // ---------- 足場 ----------

    static void NewGame()
    {
        _game = new GameManager();
        typeof(GameManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_game, null);
        _update = typeof(GameManager).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    static SaveData Data { get { return _game.Data; } }

    static void Tick(float seconds, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            UnityEngine.Time.__Advance(seconds);
            _update.Invoke(_game, null);
        }
    }

    static void Check(string label, bool condition, string detail = "")
    {
        if (condition) { _passed++; return; }
        _failed++;
        Console.WriteLine("  失敗: " + label + (detail.Length > 0 ? "  (" + detail + ")" : ""));
    }

    static void Section(string name) { Console.WriteLine(name); }

    // ---------- 個々の確認 ----------

    static void TestStart()
    {
        Section("初期状態");
        Check("ドラゴンが1匹いる", Data.dragons.Count == 1, "実際 " + Data.dragons.Count);
        Check("最初の1匹に番号が振られている", Data.dragons[0].uid != 0);
        Check("所持金がある", Data.gold > 0);
        Check("毎秒の生産がある", _game.GoldPerSecond > 0);
        Check("図鑑に1種載っている", Data.discovered.Count == 1);
        Check("探索枠が4つある", Data.expeditions.Count == SaveData.ExpeditionSlots);
        Check("施設の枠がそろっている", Data.upgradeLevels.Count == UpgradeDatabase.Count);
    }

    static void TestHatchAndNest()
    {
        Section("孵化と巣の広さ");
        Data.gold = 1e12;

        int before = Data.dragons.Count;
        DragonSave hatched = _game.HatchEgg();
        Check("孵化できる", hatched != null);
        Check("巣が1匹増える", Data.dragons.Count == before + 1);
        Check("新しい個体に番号が振られる", hatched != null && hatched.uid != 0);
        Check("番号が重複しない", UniqueUids());

        int guard = 0;
        while (!_game.NestIsFull && guard++ < 100) _game.HatchEgg();
        Check("巣がいっぱいになる", _game.NestIsFull);
        Check("上限を超えて増えない", Data.dragons.Count == _game.NestCapacity,
            Data.dragons.Count + "/" + _game.NestCapacity);
        Check("いっぱいなら孵化できない", _game.HatchEgg() == null);
    }

    static void TestUpgrades()
    {
        Section("施設");
        Data.gold = 1e18;
        for (int i = 0; i < UpgradeDatabase.Count; i++)
        {
            UpgradeId id = (UpgradeId)i;
            int before = _game.UpgradeLevel(id);
            bool bought = _game.BuyUpgrade(id);
            Check(UpgradeDatabase.Get(id).Name + " を買える", bought);
            Check(UpgradeDatabase.Get(id).Name + " のレベルが上がる", _game.UpgradeLevel(id) == before + 1);
        }

        double richMultiplier = _game.GlobalMultiplier;
        Check("宝物庫が全体倍率に効く", richMultiplier > 1.0, "×" + richMultiplier.ToString("0.000"));

        Data.gold = 0;
        Check("所持金が足りなければ買えない", !_game.BuyUpgrade(UpgradeId.Treasury));

        // 上限まで買い切っても壊れないこと
        Data.gold = 1e30;
        int loops = 0;
        while (!_game.UpgradeMaxed(UpgradeId.Hoard) && loops++ < 200) _game.BuyUpgrade(UpgradeId.Hoard);
        Check("上限に達する", _game.UpgradeMaxed(UpgradeId.Hoard));
        Check("上限を超えて買えない", !_game.BuyUpgrade(UpgradeId.Hoard));
        Check("放置効率が1を超えない", _game.OfflineEfficiency <= 1.0);
    }

    static void TestLevelling()
    {
        Section("育成");
        Data.gold = 1e18;
        DragonSave d = Data.dragons[0];
        int before = d.level;
        Check("レベルが上がる", _game.LevelUp(d) && d.level == before + 1);

        double productionBefore = _game.Production(d);
        int raised = _game.LevelUpMany(d, 10);
        Check("まとめて10上がる", raised == 10, "実際 " + raised);
        Check("生産量が増える", _game.Production(d) > productionBefore);
        Check("最高レベルが記録される", Data.bestLevel >= d.level);

        Data.gold = 0;
        Check("所持金が足りなければ上がらない", !_game.LevelUp(d));
        Check("足りなければ0回", _game.LevelUpMany(d, 10) == 0);
    }

    static void TestEvolution()
    {
        Section("進化");
        Data.gold = 1e18;

        // 同じ種族を2匹そろえる
        DragonSave a = Data.dragons[0];
        DragonSave b = Data.dragons[1];
        b.speciesId = a.speciesId;
        b.rarity = a.rarity;

        DragonSpecies before = SpeciesDatabase.ById(a.speciesId);
        Check("進化先がある", _game.EvolutionTarget(a) != null);
        Check("進化できる", _game.CanEvolve(a));

        int countBefore = Data.dragons.Count;
        bool evolved = _game.Evolve(a);
        Check("進化が通る", evolved);
        Check("相方が消えて1匹減る", Data.dragons.Count == countBefore - 1);
        Check("ティアが1つ上がる", SpeciesDatabase.ById(a.speciesId).Tier == before.Tier + 1);
        Check("図鑑に載る", Data.discovered.Contains(a.speciesId));
        Check("進化回数が増える", Data.evolutions == 1);

        // 神話は孵化でしか出ない
        DragonSave c = Data.dragons[0];
        DragonSave e = Data.dragons[1];
        e.speciesId = c.speciesId;
        c.rarity = (int)Rarity.Legendary;
        e.rarity = (int)Rarity.Legendary;
        Check("進化ではレジェンドが上限", _game.EvolvedRarity(c) == (int)Rarity.Legendary);

        // ティア5は進化先を持たない
        DragonSave top = Data.dragons[0];
        top.speciesId = "fire5";
        Check("頂点には進化先がない", _game.EvolutionTarget(top) == null);
        Check("頂点は進化できない", !_game.CanEvolve(top));
    }

    static void TestExpedition()
    {
        Section("探索");
        DragonSave traveller = Data.dragons[0];
        double rateBefore = _game.GoldPerSecond;

        Check("出発前は留守ではない", !_game.IsAway(traveller));
        Check("送り出せる", _game.Dispatch(0, traveller));
        Check("留守になる", _game.IsAway(traveller));
        Check("留守のぶん毎秒が減る", _game.GoldPerSecond < rateBefore);
        Check("同じ枠に重ねて送れない", !_game.Dispatch(0, Data.dragons[1]));
        Check("留守の子は放てない", !_game.Release(traveller));
        Check("留守の子は進化できない", !_game.CanEvolve(traveller));
        Check("送り出せる数が減る", _game.AvailableDragonCount == Data.dragons.Count - 1);

        Check("進み具合が0以上1以下", _game.ExpeditionProgress(0) >= 0 && _game.ExpeditionProgress(0) <= 1);

        // 出発時刻を過去にずらして、帰り着いた状態にする
        ExpeditionSave slot = _game.Slot(0);
        slot.startUnix -= (long)ExpeditionDatabase.Get(0).Seconds + 10;
        Check("帰り着いている", _game.ExpeditionProgress(0) >= 1.0);
        Check("残り時間が0", _game.ExpeditionRemainingSeconds(0) <= 0);

        double goldBefore = Data.gold;
        int levelBefore = traveller.level;
        Check("迎えられる", _game.Collect(0));
        Check("ゴールドが増える", Data.gold > goldBefore);
        Check("レベルが上がって帰る", traveller.level > levelBefore);
        Check("枠が空く", _game.Slot(0).dragonUid == 0);
        Check("巣に戻っている", !_game.IsAway(traveller));
        Check("探索回数が増える", Data.expeditionsDone == 1);
        Check("空の枠は迎えられない", !_game.Collect(0));
    }

    static void TestRelease()
    {
        Section("放つ");
        int before = Data.dragons.Count;
        Check("放てる", _game.Release(Data.dragons[Data.dragons.Count - 1]));
        Check("1匹減る", Data.dragons.Count == before - 1);

        while (Data.dragons.Count > 1) _game.Release(Data.dragons[Data.dragons.Count - 1]);
        Check("最後の1匹は残る", Data.dragons.Count == 1);
        Check("最後の1匹は放てない", !_game.Release(Data.dragons[0]));
    }

    static void TestAchievements()
    {
        Section("称号");
        Data.allTimeGold = 1e13;
        Data.eggsAllTime = 200;
        Data.pets = 2000;
        Tick(0.6f, 4);

        Check("条件を満たすと解放される", _game.UnlockedAchievementCount > 0,
            _game.UnlockedAchievementCount + "件");
        Check("解放ぶんが倍率に乗る", _game.AchievementMultiplier > 1.0,
            "×" + _game.AchievementMultiplier.ToString("0.000"));

        int count = _game.UnlockedAchievementCount;
        Tick(0.6f, 4);
        Check("二重に数えない", _game.UnlockedAchievementCount == count);
        Check("記録と一致する", Data.achievements.Count == count);
    }

    static void TestMigration()
    {
        Section("古いセーブの読み直し");
        SaveData old = new SaveData();
        old.dragons.Add(new DragonSave());
        old.dragons.Add(new DragonSave());
        old.dragons[0].speciesId = "fire1";
        old.dragons[1].speciesId = "water1";
        old.upgradeLevels.Clear();
        old.expeditions.Clear();

        old.EnsureShape();
        Check("施設の枠が補われる", old.upgradeLevels.Count == UpgradeDatabase.Count);
        Check("探索の枠が補われる", old.expeditions.Count == SaveData.ExpeditionSlots);
        Check("番号のない個体に番号が振られる", old.dragons[0].uid != 0 && old.dragons[1].uid != 0);
        Check("振られた番号が重複しない", old.dragons[0].uid != old.dragons[1].uid);

        old.EnsureShape();
        Check("二度呼んでも増えない", old.expeditions.Count == SaveData.ExpeditionSlots);
    }

    static void TestRebirth()
    {
        Section("転生");
        Data.gold = 1e12;
        Data.lifetimeGold = 1e12;
        Data.dragons.Add(new DragonSave());
        Data.dragons[Data.dragons.Count - 1].uid = Data.nextUid++;
        Data.dragons[Data.dragons.Count - 1].speciesId = "wind2";
        _game.Dispatch(1, Data.dragons[Data.dragons.Count - 1]);

        int soulsBefore = Data.souls;
        int expected = _game.SoulsIfRebirthNow;
        Check("竜魂が見込める", expected > 0, expected + "個");
        Check("転生できる", _game.CanRebirth);

        int discoveredBefore = Data.discovered.Count;
        int achievementsBefore = Data.achievements.Count;
        Check("転生が通る", _game.Rebirth());

        Check("竜魂が増える", Data.souls > soulsBefore, Data.souls + "個");
        Check("巣が1匹に戻る", Data.dragons.Count == 1);
        Check("施設が戻る", _game.UpgradeLevel(UpgradeId.Treasury) == 0);
        Check("探索が畳まれる", _game.Slot(1).dragonUid == 0);
        Check("周回の累計が0に戻る", Data.lifetimeGold == 0);
        Check("図鑑は引き継ぐ", Data.discovered.Count == discoveredBefore);
        Check("称号は引き継ぐ", Data.achievements.Count == achievementsBefore);
        Check("竜魂が倍率に乗る", _game.SoulMultiplier > 1.0, "×" + _game.SoulMultiplier.ToString("0.00"));
        Check("転生回数が増える", Data.rebirths == 1);
        Check("残った1匹に番号がある", Data.dragons[0].uid != 0);
    }

    static void TestReset()
    {
        Section("やり直し");
        _game.ResetEverything();
        Check("竜魂も消える", Data.souls == 0);
        Check("ドラゴンが1匹に戻る", Data.dragons.Count == 1);
        Check("称号も消える", _game.UnlockedAchievementCount == 0);
        Check("探索枠は保たれる", Data.expeditions.Count == SaveData.ExpeditionSlots);
        Check("倍率が初期に戻る", Math.Abs(_game.GlobalMultiplier - 1.05) < 0.001,
            "×" + _game.GlobalMultiplier.ToString("0.000"));
    }

    static bool UniqueUids()
    {
        HashSet<int> seen = new HashSet<int>();
        for (int i = 0; i < Data.dragons.Count; i++)
        {
            if (!seen.Add(Data.dragons[i].uid)) return false;
        }
        return true;
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using DragonIdle;

/// <summary>
/// バランス確認用のヘッドレス実行。Unity を起動せずに、ゲームのロジックそのものを
/// 1秒刻みで回して、時間ごとの伸びを表にする。UI も描画も通らない。
/// </summary>
public static class Simulate
{
    static GameManager _game;
    static SaveData Data { get { return _game.Data; } }

    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        double days = args.Length > 0 ? double.Parse(args[0]) : 7.0;

        Sfx.Muted = true;                 // スタブに音源が無いので鳴らさない
        _game = new GameManager();
        typeof(GameManager)
            .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_game, null);

        double[] marks = { 60, 300, 900, 3600, 4 * 3600, 12 * 3600, 86400, 3 * 86400, 7 * 86400, 14 * 86400, 30 * 86400 };
        int nextMark = 0;
        double total = days * 86400;

        Header();
        for (double t = 1; t <= total; t += 1)
        {
            Data.gold += _game.GoldPerSecond;
            Data.lifetimeGold += _game.GoldPerSecond;
            Data.allTimeGold += _game.GoldPerSecond;
            Spend();

            while (nextMark < marks.Length && t >= marks[nextMark])
            {
                Report(marks[nextMark]);
                nextMark++;
            }
        }
        Console.WriteLine();
        Summary();
    }

    /// <summary>
    /// 遊ぶ人の代わりになる単純な方針。安い順に手を打つだけで、最適化はしていない。
    /// これで詰まらずに伸びるなら、実際に遊んでも詰まらないはず。
    /// </summary>
    static void Spend()
    {
        for (int guard = 0; guard < 40; guard++)
        {
            if (BuyBestUpgrade()) continue;
            if (TryHatch()) continue;
            if (TryLevel()) continue;
            if (TryEvolve()) continue;
            return;
        }
    }

    static bool TryHatch()
    {
        if (_game.NestIsFull) return false;
        // 手持ちに余裕があるときだけ卵を買う
        if (Data.gold < _game.EggCost * 1.6) return false;
        return _game.HatchEgg() != null;
    }

    static bool TryLevel()
    {
        DragonSave best = null;
        double bestCost = double.MaxValue;
        for (int i = 0; i < Data.dragons.Count; i++)
        {
            double cost = _game.LevelUpCost(Data.dragons[i]);
            if (cost < bestCost) { bestCost = cost; best = Data.dragons[i]; }
        }
        if (best == null) return false;
        // 所持金の3割までなら育てる
        if (bestCost > Data.gold * 0.3) return false;
        return _game.LevelUp(best);
    }

    static bool TryEvolve()
    {
        for (int i = 0; i < Data.dragons.Count; i++)
        {
            DragonSave d = Data.dragons[i];
            if (!_game.CanEvolve(d)) continue;
            if (_game.EvolveCost(d) > Data.gold * 0.5) continue;
            return _game.Evolve(d);
        }
        return false;
    }

    static bool BuyBestUpgrade()
    {
        for (int i = 0; i < UpgradeDatabase.Count; i++)
        {
            UpgradeId id = (UpgradeId)i;
            if (_game.UpgradeMaxed(id)) continue;
            double cost = _game.UpgradeCost(id);
            // 巣が埋まっているなら拡張を優先、それ以外は所持金の2割まで
            double budget = (id == UpgradeId.Nest && _game.NestIsFull) ? Data.gold : Data.gold * 0.2;
            if (cost > budget) continue;
            if (_game.BuyUpgrade(id)) return true;
        }
        return false;
    }

    static void Header()
    {
        Console.WriteLine("経過        毎秒        累計        巣   最高Lv 図鑑 進化 転生見込  倍率");
        Console.WriteLine(new string('-', 78));
    }

    static void Report(double seconds)
    {
        int bestLevel = 0;
        for (int i = 0; i < Data.dragons.Count; i++) bestLevel = Math.Max(bestLevel, Data.dragons[i].level);

        Console.WriteLine("{0,-10} {1,-11} {2,-11} {3,-4} {4,-6} {5,-4} {6,-4} {7,-9} {8}",
            NumberFormat.Duration(seconds),
            NumberFormat.Rate(_game.GoldPerSecond),
            NumberFormat.Gold(Data.allTimeGold),
            Data.dragons.Count + "/" + _game.NestCapacity,
            bestLevel,
            Data.discovered.Count,
            Data.evolutions,
            _game.SoulsIfRebirthNow,
            "x" + _game.GlobalMultiplier.ToString("0.00"));
    }

    static void Summary()
    {
        Console.WriteLine("称号 " + _game.UnlockedAchievementCount + "/" + AchievementDatabase.Count
                          + "　図鑑 " + Data.discovered.Count + "/" + SpeciesDatabase.TotalCount
                          + "　孵化 " + Data.eggsAllTime + "　進化 " + Data.evolutions);
        Console.Write("施設: ");
        for (int i = 0; i < UpgradeDatabase.Count; i++)
        {
            Console.Write(UpgradeDatabase.All[i].Name + " Lv" + _game.UpgradeLevel((UpgradeId)i) + "  ");
        }
        Console.WriteLine();
    }
}

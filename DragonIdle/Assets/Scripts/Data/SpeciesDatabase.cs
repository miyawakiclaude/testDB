using System.Collections.Generic;

namespace DragonIdle
{
    /// <summary>図鑑1件ぶんの種族データ。ティアが上がるほど基礎生産量が跳ね上がる。</summary>
    public class DragonSpecies
    {
        public readonly string Id;
        public readonly string Name;
        public readonly Element Element;
        public readonly int Tier;
        public readonly string Flavor;

        public DragonSpecies(string id, string name, Element element, int tier, string flavor)
        {
            Id = id;
            Name = name;
            Element = element;
            Tier = tier;
            Flavor = flavor;
        }

        /// <summary>レベル1・コモン換算の毎秒生産量。</summary>
        public double BaseRate
        {
            get
            {
                switch (Tier)
                {
                    case 1: return 1.0;
                    case 2: return 8.0;
                    case 3: return 60.0;
                    case 4: return 420.0;
                    default: return 3000.0;
                }
            }
        }
    }

    /// <summary>全35種族。属性ごとにティア1〜5が1種ずつ並ぶ。</summary>
    public static class SpeciesDatabase
    {
        static readonly List<DragonSpecies> All = new List<DragonSpecies>
        {
            new DragonSpecies("fire1", "フレアリング", Element.Fire, 1, "手のひらで眠る火種。寝言のたびに小さく火を噴く。"),
            new DragonSpecies("fire2", "エンバードレイク", Element.Fire, 2, "鱗の隙間から炭火の匂いがする若竜。"),
            new DragonSpecies("fire3", "サラマンドラ", Element.Fire, 3, "溶岩の川を寝床にする。冷めた岩は嫌う。"),
            new DragonSpecies("fire4", "イグニスワイバーン", Element.Fire, 4, "羽ばたきひとつで空気が陽炎に歪む。"),
            new DragonSpecies("fire5", "焔帝ヴォルカニス", Element.Fire, 5, "眠りにつくと山がひとつ火口になるという。"),

            new DragonSpecies("water1", "アクアリング", Element.Water, 1, "水たまりから離れられない、しずく色の仔竜。"),
            new DragonSpecies("water2", "ミストドレイク", Element.Water, 2, "朝霧に紛れて巣に帰る。鱗はいつも湿っている。"),
            new DragonSpecies("water3", "リヴァイア", Element.Water, 3, "淡水も海水も好む。歌うと波が凪ぐ。"),
            new DragonSpecies("water4", "タイダルサーペント", Element.Water, 4, "とぐろの直径が入り江ひとつぶん。"),
            new DragonSpecies("water5", "蒼海帝アビスレイン", Element.Water, 5, "深海の底で千年ぶんの雨を蓄えている。"),

            new DragonSpecies("wind1", "ブリーズリング", Element.Wind, 1, "羽根より軽い。窓を開けると勝手に出ていく。"),
            new DragonSpecies("wind2", "ゲイルドレイク", Element.Wind, 2, "追い風を見つける嗅覚が異常に鋭い。"),
            new DragonSpecies("wind3", "シルフィード", Element.Wind, 3, "雲の切れ端を編んで巣をつくる。"),
            new DragonSpecies("wind4", "テンペストワイバーン", Element.Wind, 4, "通り道が、そのまま台風の進路になる。"),
            new DragonSpecies("wind5", "天翔帝ゼファリオン", Element.Wind, 5, "地に降りた記録が三度しかない。"),

            new DragonSpecies("earth1", "ペブルリング", Element.Earth, 1, "小石を集めて枕にする。取り上げると拗ねる。"),
            new DragonSpecies("earth2", "ストーンドレイク", Element.Earth, 2, "岩壁に擬態する。数えるときは触って確かめる。"),
            new DragonSpecies("earth3", "グランドゴーレム竜", Element.Earth, 3, "背の苔で樹齢がわかる。"),
            new DragonSpecies("earth4", "テラサウルス", Element.Earth, 4, "一歩ごとに地層がひとつ増えるといわれる。"),
            new DragonSpecies("earth5", "大地帝ガイアノート", Element.Earth, 5, "背にひとつ、名もない大陸を載せている。"),

            new DragonSpecies("thunder1", "スパークリング", Element.Thunder, 1, "撫でると静電気で毛が逆立つ。"),
            new DragonSpecies("thunder2", "ボルトドレイク", Element.Thunder, 2, "尾の先が避雷針の役目を果たす。"),
            new DragonSpecies("thunder3", "サンダーバード竜", Element.Thunder, 3, "雷雲の縁を巣にする渡り竜。"),
            new DragonSpecies("thunder4", "プラズマワイバーン", Element.Thunder, 4, "鳴き声より先に閃光が届く。"),
            new DragonSpecies("thunder5", "雷帝ライゼクス", Element.Thunder, 5, "怒ると空が一晩じゅう白い。"),

            new DragonSpecies("light1", "グリムリング", Element.Light, 1, "夜だけほのかに光る。読書灯にちょうどいい。"),
            new DragonSpecies("light2", "ルミナドレイク", Element.Light, 2, "陽だまりを見つける天才。"),
            new DragonSpecies("light3", "セラフィナ", Element.Light, 3, "六枚の翼を持つ。影を落とさない。"),
            new DragonSpecies("light4", "ホーリーワイバーン", Element.Light, 4, "通ったあとの空気が澄む。"),
            new DragonSpecies("light5", "聖光帝ソレイユ", Element.Light, 5, "この竜のいる谷には夜が来ない。"),

            new DragonSpecies("dark1", "シェイドリング", Element.Dark, 1, "家具の影から影へ移動する。呼ぶと返事はする。"),
            new DragonSpecies("dark2", "ダスクドレイク", Element.Dark, 2, "日没の十分間だけ姿がはっきり見える。"),
            new DragonSpecies("dark3", "ノクターナ", Element.Dark, 3, "月のない夜にだけ卵を産むという。"),
            new DragonSpecies("dark4", "アビスワイバーン", Element.Dark, 4, "瞳を覗くと自分の足音が聞こえる。"),
            new DragonSpecies("dark5", "冥王帝ネフィリム", Element.Dark, 5, "名を三度呼ぶと巣の灯りが全部消える。")
        };

        static Dictionary<string, DragonSpecies> _byId;

        public static IReadOnlyList<DragonSpecies> Species { get { return All; } }

        public static int TotalCount { get { return All.Count; } }

        public static DragonSpecies ById(string id)
        {
            if (_byId == null)
            {
                _byId = new Dictionary<string, DragonSpecies>();
                for (int i = 0; i < All.Count; i++) _byId[All[i].Id] = All[i];
            }
            DragonSpecies s;
            return _byId.TryGetValue(id, out s) ? s : All[0];
        }

        /// <summary>属性とティアから1種を引く。進化先を求めるのに使う。</summary>
        public static DragonSpecies Find(Element element, int tier)
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Element == element && All[i].Tier == tier) return All[i];
            }
            return null;
        }

        /// <summary>指定ティア範囲の種族を集める。孵化の抽選で使う。</summary>
        public static List<DragonSpecies> InTierRange(int minTier, int maxTier)
        {
            List<DragonSpecies> result = new List<DragonSpecies>();
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Tier >= minTier && All[i].Tier <= maxTier) result.Add(All[i]);
            }
            return result;
        }
    }
}

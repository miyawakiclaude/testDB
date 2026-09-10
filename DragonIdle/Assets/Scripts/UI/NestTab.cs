using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>巣のタブ。卵を孵し、育てているドラゴンを一覧で世話する。</summary>
    public class NestTab : TabView
    {
        class Card
        {
            public DragonSave Data;
            public GameObject Root;
            public Text Name;
            public Text Meta;
            public Text Production;
            public Text LevelUpLabel;
            public Button LevelUpButton;
            public Button BulkButton;
            public Text BulkLabel;
            public Button EvolveButton;
            public Text EvolveLabel;
            public Button ReleaseButton;
            public Text ReleaseLabel;
            public Text Hint;
        }

        const float CardHeight = 220f;

        Transform _listContent;
        NestScene _scene;
        Button _hatchButton;
        Text _hatchLabel;
        Text _hatchHint;
        readonly List<Card> _cards = new List<Card>();

        public override string Title { get { return "巣"; } }

        public override void Build(Transform parent)
        {
            Root = UIFactory.Node("NestTab", parent);

            BuildScene();

            // 孵化パネル
            Image hatchPanel = UIFactory.Panel("HatchPanel", Root.transform, UIStyle.BgPanel2, UIStyle.Round16);
            RectTransform hatchRect = UIFactory.Rect(hatchPanel.gameObject);
            hatchRect.anchorMin = new Vector2(0f, 1f);
            hatchRect.anchorMax = new Vector2(1f, 1f);
            hatchRect.pivot = new Vector2(0.5f, 1f);
            hatchRect.offsetMin = new Vector2(24, 0);
            hatchRect.offsetMax = new Vector2(-24, 0);
            hatchRect.sizeDelta = new Vector2(hatchRect.sizeDelta.x, 196);
            hatchRect.anchoredPosition = new Vector2(0, -SceneBottom - 16f);

            Text title = UIFactory.Label("Title", hatchPanel.transform, "たまごを孵す", 36, UIStyle.Text,
                TextAnchor.UpperLeft, FontStyle.Bold);
            RectTransform titleRect = UIFactory.Rect(title.gameObject);
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.sizeDelta = new Vector2(520, 46);
            titleRect.anchoredPosition = new Vector2(32, -30);

            _hatchHint = UIFactory.Label("Hint", hatchPanel.transform, "", 25, UIStyle.TextDim, TextAnchor.UpperLeft);
            RectTransform hintRect = UIFactory.Rect(_hatchHint.gameObject);
            hintRect.anchorMin = new Vector2(0f, 1f);
            hintRect.anchorMax = new Vector2(0f, 1f);
            hintRect.pivot = new Vector2(0f, 1f);
            hintRect.sizeDelta = new Vector2(600, 90);
            hintRect.anchoredPosition = new Vector2(32, -84);

            _hatchButton = UIFactory.Button("Hatch", hatchPanel.transform, "", 30, UIStyle.Gold, UIStyle.TextOnGold);
            RectTransform buttonRect = UIFactory.Rect(_hatchButton.gameObject);
            buttonRect.anchorMin = new Vector2(1f, 0.5f);
            buttonRect.anchorMax = new Vector2(1f, 0.5f);
            buttonRect.pivot = new Vector2(1f, 0.5f);
            buttonRect.sizeDelta = new Vector2(300, 124);
            buttonRect.anchoredPosition = new Vector2(-28, 0);
            _hatchLabel = _hatchButton.GetComponentInChildren<Text>();
            _hatchButton.onClick.AddListener(delegate { Game.HatchEgg(); });

            // 一覧。なでるボタンに隠れないよう下に広めの余白をとる。
            ScrollRect scrollRect;
            _listContent = UIFactory.ScrollList("DragonList", Root.transform, 18,
                new RectOffset(24, 24, 8, 300), out scrollRect);
            RectTransform listRect = UIFactory.Rect(scrollRect.gameObject);
            UIFactory.Stretch(listRect, 0, 0, 0, SceneBottom + 16f + 196f + 16f);

            BuildCards();
        }

        const float SceneTop = 20f;
        const float SceneHeight = 432f;
        const float SceneBottom = SceneTop + SceneHeight;

        /// <summary>巣の情景。育てているドラゴンが実際に並んで見える場所。</summary>
        void BuildScene()
        {
            // 角丸にすると中の床やドラゴンが角からはみ出すので、下地は四角のまま扱う。
            Image frame = UIFactory.Panel("NestScene", Root.transform, Color.white, UIStyle.CaveGradient);
            frame.type = Image.Type.Simple;
            RectTransform frameRect = UIFactory.Rect(frame.gameObject);
            frameRect.anchorMin = new Vector2(0f, 1f);
            frameRect.anchorMax = new Vector2(1f, 1f);
            frameRect.pivot = new Vector2(0.5f, 1f);
            frameRect.offsetMin = new Vector2(24, 0);
            frameRect.offsetMax = new Vector2(-24, 0);
            frameRect.sizeDelta = new Vector2(frameRect.sizeDelta.x, SceneHeight);
            frameRect.anchoredPosition = new Vector2(0, -SceneTop);

            Image floor = UIFactory.Panel("Floor", frame.transform, new Color32(0x3A, 0x30, 0x52, 0xFF));
            floor.raycastTarget = false;
            RectTransform floorRect = UIFactory.Rect(floor.gameObject);
            floorRect.anchorMin = new Vector2(0f, 0f);
            floorRect.anchorMax = new Vector2(1f, 0f);
            floorRect.pivot = new Vector2(0.5f, 0f);
            floorRect.offsetMin = new Vector2(0, 0);
            floorRect.offsetMax = new Vector2(0, 0);
            floorRect.sizeDelta = new Vector2(floorRect.sizeDelta.x, 96);
            floorRect.anchoredPosition = Vector2.zero;

            Text hint = UIFactory.Label("Hint", frame.transform, "ドラゴンをなでるとゴールドがもらえる",
                24, new Color(1f, 1f, 1f, 0.55f), TextAnchor.LowerCenter);
            RectTransform hintRect = UIFactory.Rect(hint.gameObject);
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(1f, 0f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.offsetMin = new Vector2(16, 0);
            hintRect.offsetMax = new Vector2(-16, 0);
            hintRect.sizeDelta = new Vector2(hintRect.sizeDelta.x, 36);
            hintRect.anchoredPosition = new Vector2(0, 16);

            _scene = frame.gameObject.AddComponent<NestScene>();
            _scene.Attach(hint);
            _scene.Rebuild();
        }

        public override void Rebuild()
        {
            if (_scene != null) _scene.Rebuild();
            BuildCards();
        }

        void BuildCards()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].Root != null) UnityEngine.Object.Destroy(_cards[i].Root);
            }
            _cards.Clear();

            List<DragonSave> dragons = Game.Data.dragons;
            for (int i = 0; i < dragons.Count; i++) _cards.Add(BuildCard(dragons[i]));
        }

        Card BuildCard(DragonSave data)
        {
            DragonSpecies species = SpeciesDatabase.ById(data.speciesId);
            Color elementColor = Elements.Tint(species.Element);

            Card card = new Card();
            card.Data = data;

            Image root = UIFactory.Panel("Dragon_" + species.Id, _listContent, UIStyle.BgPanel2, UIStyle.Round16);
            UIFactory.Sizing(root.gameObject, CardHeight, -1, -1, CardHeight);
            card.Root = root.gameObject;

            // 属性色の帯
            Image stripe = UIFactory.Panel("Stripe", root.transform, elementColor, UIStyle.Round8);
            RectTransform stripeRect = UIFactory.Rect(stripe.gameObject);
            stripeRect.anchorMin = new Vector2(0f, 0f);
            stripeRect.anchorMax = new Vector2(0f, 1f);
            stripeRect.pivot = new Vector2(0f, 0.5f);
            stripeRect.sizeDelta = new Vector2(16, -24);
            stripeRect.anchoredPosition = new Vector2(10, 0);

            // 肖像: 種族ごとの姿を実行時に描いたもの
            Color plateColor = elementColor;
            plateColor.a = 0.14f;
            Image plate = UIFactory.Panel("Plate", root.transform, plateColor, UIStyle.Round16);
            RectTransform plateRect = UIFactory.Rect(plate.gameObject);
            plateRect.anchorMin = new Vector2(0f, 1f);
            plateRect.anchorMax = new Vector2(0f, 1f);
            plateRect.pivot = new Vector2(0f, 1f);
            plateRect.sizeDelta = new Vector2(148, 148);
            plateRect.anchoredPosition = new Vector2(36, -22);

            Image portrait = UIFactory.Panel("Portrait", plate.transform, Color.white, null);
            portrait.sprite = DragonArt.For(species);
            portrait.type = Image.Type.Simple;
            portrait.preserveAspect = true;
            UIFactory.Stretch(UIFactory.Rect(portrait.gameObject), 6, 6, 6, 6);

            card.Name = UIFactory.Label("Name", root.transform, species.Name, 34, UIStyle.Text,
                TextAnchor.UpperLeft, FontStyle.Bold);
            Place(card.Name.gameObject, 200, -28, 500, 44);

            card.Meta = UIFactory.Label("Meta", root.transform, "", 25, UIStyle.TextDim, TextAnchor.UpperLeft);
            Place(card.Meta.gameObject, 200, -76, 500, 36);

            card.Production = UIFactory.Label("Production", root.transform, "", 29, UIStyle.Gold,
                TextAnchor.UpperLeft, FontStyle.Bold);
            Place(card.Production.gameObject, 200, -122, 460, 40);

            card.LevelUpButton = UIFactory.Button("LevelUp", root.transform, "", 27, UIStyle.BgPanel3, UIStyle.Text);
            RectTransform levelRect = UIFactory.Rect(card.LevelUpButton.gameObject);
            levelRect.anchorMin = new Vector2(1f, 1f);
            levelRect.anchorMax = new Vector2(1f, 1f);
            levelRect.pivot = new Vector2(1f, 1f);
            levelRect.sizeDelta = new Vector2(288, 92);
            levelRect.anchoredPosition = new Vector2(-24, -24);
            card.LevelUpLabel = card.LevelUpButton.GetComponentInChildren<Text>();
            DragonSave captured = data;
            card.LevelUpButton.onClick.AddListener(delegate { Game.LevelUp(captured); });

            card.BulkButton = SmallButton(root.transform, "LevelUp10", "×10", -24, UIStyle.TextDim);
            card.BulkLabel = card.BulkButton.GetComponentInChildren<Text>();
            card.BulkButton.onClick.AddListener(delegate { Game.LevelUpMany(captured, 10); });

            card.EvolveButton = SmallButton(root.transform, "Evolve", "進化", -132, UIStyle.Soul);
            card.EvolveLabel = card.EvolveButton.GetComponentInChildren<Text>();
            card.EvolveButton.onClick.AddListener(delegate { Game.Evolve(captured); });

            card.ReleaseButton = SmallButton(root.transform, "Release", "放つ", -240, UIStyle.Danger);
            card.ReleaseLabel = card.ReleaseButton.GetComponentInChildren<Text>();
            card.ReleaseButton.onClick.AddListener(delegate { Game.Release(captured); });

            card.Hint = UIFactory.Label("Hint", root.transform, "", 22, UIStyle.Soul, TextAnchor.UpperLeft);
            Place(card.Hint.gameObject, 200, -166, 460, 34);

            return card;
        }

        /// <summary>カード右下に並ぶ小さなボタン。3つが等間隔で収まる幅にしてある。</summary>
        static Button SmallButton(Transform parent, string name, string caption, float x, Color foreground)
        {
            Button button = UIFactory.Button(name, parent, caption, 24, UIStyle.BgPanel3, foreground);
            RectTransform rect = UIFactory.Rect(button.gameObject);
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(100, 74);
            rect.anchoredPosition = new Vector2(x, -128);
            return button;
        }

        static void Place(GameObject go, float x, float y, float width, float height)
        {
            RectTransform rect = UIFactory.Rect(go);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, y);
        }

        public override void Refresh()
        {
            double eggCost = Game.EggCost;
            bool full = Game.NestIsFull;
            bool affordable = Game.Data.gold >= eggCost;

            _hatchLabel.text = full ? "巣がいっぱい" : "孵す\n" + NumberFormat.Gold(eggCost);
            _hatchButton.interactable = !full && affordable;
            _hatchLabel.color = _hatchButton.interactable ? UIStyle.TextOnGold : new Color(0.1f, 0.08f, 0.02f, 0.6f);

            _hatchHint.text = "レア度上昇 " + NumberFormat.Percent(Game.LuckFactor - 1.0)
                            + "　空き " + (Game.NestCapacity - Game.Data.dragons.Count) + "\n"
                            + "<color=#6E668C>孵すたびに次の卵は高くなる</color>";

            for (int i = 0; i < _cards.Count; i++) RefreshCard(_cards[i]);
        }

        void RefreshCard(Card card)
        {
            DragonSave data = card.Data;
            DragonSpecies species = SpeciesDatabase.ById(data.speciesId);
            Rarity rarity = (Rarity)data.rarity;
            Color rarityColor = Rarities.Tint(rarity);
            bool away = Game.IsAway(data);

            card.Meta.text = "<color=#" + ColorUtility.ToHtmlStringRGB(rarityColor) + ">"
                           + Rarities.Name(rarity) + "</color>　" + Elements.Name(species.Element)
                           + "　Lv." + data.level + "　個体 " + data.individual.ToString("0.00");

            card.Production.text = away
                ? "<color=#B388FF>探索中</color>　巣では稼いでいない"
                : NumberFormat.Rate(Game.Production(data)) + " ゴールド/秒";

            double cost = Game.LevelUpCost(data);
            bool canAfford = Game.Data.gold >= cost && !away;
            card.LevelUpLabel.text = "育てる　" + NumberFormat.Gold(cost);
            card.LevelUpButton.interactable = canAfford;
            card.LevelUpLabel.color = canAfford ? UIStyle.Gold : UIStyle.TextFaint;
            card.BulkLabel.color = canAfford ? UIStyle.TextDim : UIStyle.TextFaint;
            card.BulkButton.interactable = canAfford;

            bool releasable = Game.Data.dragons.Count > 1 && !away;
            card.ReleaseButton.interactable = releasable;
            card.ReleaseLabel.color = releasable ? UIStyle.Danger : UIStyle.TextFaint;

            DragonSpecies next = Game.EvolutionTarget(data);
            bool canEvolve = Game.CanEvolve(data);
            double evolveCost = canEvolve ? Game.EvolveCost(data) : 0;
            bool affordEvolve = canEvolve && Game.Data.gold >= evolveCost;

            card.EvolveButton.interactable = affordEvolve;
            card.EvolveLabel.color = affordEvolve ? UIStyle.Soul : UIStyle.TextFaint;

            if (away)
            {
                card.Hint.text = "<color=#6E668C>帰ってくるまで育てられない</color>";
            }
            else if (canEvolve)
            {
                card.Hint.text = "進化 → " + next.Name + "　" + NumberFormat.Gold(evolveCost);
                card.Hint.color = affordEvolve ? UIStyle.Soul : UIStyle.TextFaint;
            }
            else if (next == null)
            {
                card.Hint.text = "<color=#6E668C>この系譜の頂点</color>";
            }
            else
            {
                card.Hint.text = "<color=#6E668C>同じ種族がもう1匹いれば " + next.Name + " へ進化</color>";
            }
        }
    }
}

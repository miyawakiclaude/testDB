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
            public Button ReleaseButton;
        }

        const float CardHeight = 220f;

        Transform _listContent;
        Button _hatchButton;
        Text _hatchLabel;
        Text _hatchHint;
        readonly List<Card> _cards = new List<Card>();

        public override string Title { get { return "巣"; } }

        public override void Build(Transform parent)
        {
            Root = UIFactory.Node("NestTab", parent);

            // 孵化パネル
            Image hatchPanel = UIFactory.Panel("HatchPanel", Root.transform, UIStyle.BgPanel2, UIStyle.Round16);
            RectTransform hatchRect = UIFactory.Rect(hatchPanel.gameObject);
            hatchRect.anchorMin = new Vector2(0f, 1f);
            hatchRect.anchorMax = new Vector2(1f, 1f);
            hatchRect.pivot = new Vector2(0.5f, 1f);
            hatchRect.offsetMin = new Vector2(24, 0);
            hatchRect.offsetMax = new Vector2(-24, 0);
            hatchRect.sizeDelta = new Vector2(hatchRect.sizeDelta.x, 196);
            hatchRect.anchoredPosition = new Vector2(0, -24);

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
            UIFactory.Stretch(listRect, 0, 0, 0, 236);

            BuildCards();
        }

        public override void Rebuild() { BuildCards(); }

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
            Place(card.Production.gameObject, 200, -124, 500, 40);

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

            card.BulkButton = UIFactory.Button("LevelUp10", root.transform, "×10", 25, UIStyle.BgPanel3, UIStyle.TextDim);
            RectTransform bulkRect = UIFactory.Rect(card.BulkButton.gameObject);
            bulkRect.anchorMin = new Vector2(1f, 1f);
            bulkRect.anchorMax = new Vector2(1f, 1f);
            bulkRect.pivot = new Vector2(1f, 1f);
            bulkRect.sizeDelta = new Vector2(138, 74);
            bulkRect.anchoredPosition = new Vector2(-24, -128);
            card.BulkLabel = card.BulkButton.GetComponentInChildren<Text>();
            card.BulkButton.onClick.AddListener(delegate { Game.LevelUpMany(captured, 10); });

            card.ReleaseButton = UIFactory.Button("Release", root.transform, "見送る", 25, UIStyle.BgPanel3, UIStyle.Danger);
            RectTransform releaseRect = UIFactory.Rect(card.ReleaseButton.gameObject);
            releaseRect.anchorMin = new Vector2(1f, 1f);
            releaseRect.anchorMax = new Vector2(1f, 1f);
            releaseRect.pivot = new Vector2(1f, 1f);
            releaseRect.sizeDelta = new Vector2(138, 74);
            releaseRect.anchoredPosition = new Vector2(-174, -128);
            card.ReleaseButton.onClick.AddListener(delegate { Game.Release(captured); });

            return card;
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

            card.Meta.text = "<color=#" + ColorUtility.ToHtmlStringRGB(rarityColor) + ">"
                           + Rarities.Name(rarity) + "</color>　" + Elements.Name(species.Element)
                           + "　Lv." + data.level + "　個体 " + data.individual.ToString("0.00");

            card.Production.text = NumberFormat.Rate(Game.Production(data)) + " ゴールド/秒";

            double cost = Game.LevelUpCost(data);
            bool canAfford = Game.Data.gold >= cost;
            card.LevelUpLabel.text = "育てる　" + NumberFormat.Gold(cost);
            card.LevelUpButton.interactable = canAfford;
            card.LevelUpLabel.color = canAfford ? UIStyle.Gold : UIStyle.TextFaint;
            card.BulkLabel.color = canAfford ? UIStyle.TextDim : UIStyle.TextFaint;
            card.BulkButton.interactable = canAfford;
            card.ReleaseButton.interactable = Game.Data.dragons.Count > 1;
        }
    }
}

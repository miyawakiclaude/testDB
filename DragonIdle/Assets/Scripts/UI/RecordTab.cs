using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>記録のタブ。これまでの歩みと、出会った種族の図鑑。</summary>
    public class RecordTab : TabView
    {
        readonly List<Text> _statValues = new List<Text>();
        readonly List<Text> _speciesRows = new List<Text>();
        Text _discoveryHeader;

        public override string Title { get { return "記録"; } }

        public override void Build(Transform parent)
        {
            Root = UIFactory.Node("RecordTab", parent);

            ScrollRect scrollRect;
            Transform content = UIFactory.ScrollList("RecordContent", Root.transform, 20,
                new RectOffset(24, 24, 24, 300), out scrollRect);
            UIFactory.Stretch(UIFactory.Rect(scrollRect.gameObject));

            Image statCard = UIFactory.Panel("Stats", content, UIStyle.BgPanel2, UIStyle.Round16);
            UIFactory.Sizing(statCard.gameObject, 400, -1, -1, 400);
            UIFactory.VerticalLayout(statCard.gameObject, 4, new RectOffset(32, 32, 28, 28));

            string[] labels = { "累計ゴールド", "遊んだ時間", "孵した卵", "なでた回数", "転生した回数", "毎秒の生産量" };
            for (int i = 0; i < labels.Length; i++)
            {
                _statValues.Add(BuildStatRow(statCard.transform, labels[i]));
            }

            _discoveryHeader = UIFactory.Label("DiscoveryHeader", content, "図鑑", 34, UIStyle.Text,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Sizing(_discoveryHeader.gameObject, 56, -1, -1, 56);

            // 属性ごとに1枚のカードにまとめる。ティア順に並ぶので成長の道筋が見える。
            for (int e = 0; e < Elements.Count; e++)
            {
                Element element = (Element)e;
                Image card = UIFactory.Panel("Element_" + element, content, UIStyle.BgPanel2, UIStyle.Round16);
                UIFactory.Sizing(card.gameObject, 292, -1, -1, 292);
                UIFactory.VerticalLayout(card.gameObject, 8, new RectOffset(28, 28, 22, 22));

                Text header = UIFactory.Label("Header", card.transform, Elements.Name(element) + " の系譜", 28,
                    Elements.Tint(element), TextAnchor.MiddleLeft, FontStyle.Bold);
                UIFactory.Sizing(header.gameObject, 38);

                for (int tier = 1; tier <= 5; tier++)
                {
                    Text row = UIFactory.Label("Tier" + tier, card.transform, "", 24, UIStyle.TextFaint,
                        TextAnchor.MiddleLeft);
                    UIFactory.Sizing(row.gameObject, 32);
                    _speciesRows.Add(row);
                }
            }

            Button reset = UIFactory.Button("Reset", content, "最初からやり直す", 27, UIStyle.BgPanel, UIStyle.Danger);
            UIFactory.Sizing(reset.gameObject, 100, -1, -1, 100);
            reset.onClick.AddListener(OnResetClicked);
        }

        Text BuildStatRow(Transform parent, string label)
        {
            GameObject row = UIFactory.Node("Stat_" + label, parent);
            UIFactory.Sizing(row, 52, -1, -1, 52);

            Text caption = UIFactory.Label("Caption", row.transform, label, 26, UIStyle.TextDim, TextAnchor.MiddleLeft);
            RectTransform captionRect = UIFactory.Stretch(UIFactory.Rect(caption.gameObject));
            captionRect.anchorMax = new Vector2(0.5f, 1f);

            Text value = UIFactory.Label("Value", row.transform, "", 28, UIStyle.Text, TextAnchor.MiddleRight, FontStyle.Bold);
            RectTransform valueRect = UIFactory.Stretch(UIFactory.Rect(value.gameObject));
            valueRect.anchorMin = new Vector2(0.4f, 0f);
            return value;
        }

        int _resetTaps;

        void OnResetClicked()
        {
            _resetTaps++;
            if (_resetTaps >= 2)
            {
                _resetTaps = 0;
                Game.ResetEverything();
                return;
            }
            Game.Toast("本当に消す場合はもう一度押す");
        }

        public override void Refresh()
        {
            SaveData data = Game.Data;

            _statValues[0].text = NumberFormat.Gold(data.allTimeGold);
            _statValues[1].text = NumberFormat.Duration(data.playSeconds);
            _statValues[2].text = data.eggsAllTime + " 個";
            _statValues[3].text = data.pets + " 回";
            _statValues[4].text = data.rebirths + " 回";
            _statValues[5].text = NumberFormat.Rate(Game.GoldPerSecond);

            _discoveryHeader.text = "図鑑　<color=#A79FC0><size=26>"
                                  + data.discovered.Count + " / " + SpeciesDatabase.TotalCount + "</size></color>";

            int index = 0;
            for (int e = 0; e < Elements.Count; e++)
            {
                Element element = (Element)e;
                for (int tier = 1; tier <= 5; tier++)
                {
                    DragonSpecies species = Find(element, tier);
                    Text row = _speciesRows[index++];
                    if (species == null) { row.text = ""; continue; }

                    if (data.discovered.Contains(species.Id))
                    {
                        row.text = "★" + tier + "　" + species.Name
                                 + "　<color=#6E668C><size=21>" + species.Flavor + "</size></color>";
                        row.color = UIStyle.Text;
                    }
                    else
                    {
                        row.text = "☆" + tier + "　??????";
                        row.color = UIStyle.TextFaint;
                    }
                }
            }
        }

        static DragonSpecies Find(Element element, int tier)
        {
            IReadOnlyList<DragonSpecies> all = SpeciesDatabase.Species;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Element == element && all[i].Tier == tier) return all[i];
            }
            return null;
        }
    }
}

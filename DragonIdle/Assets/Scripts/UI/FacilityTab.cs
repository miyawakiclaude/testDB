using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>施設のタブ。巣そのものを強くして、全ドラゴンにまとめて効かせる。</summary>
    public class FacilityTab : TabView
    {
        class Row
        {
            public UpgradeId Id;
            public Text Name;
            public Text Description;
            public Text Effect;
            public Button Buy;
            public Text BuyLabel;
        }

        readonly List<Row> _rows = new List<Row>();

        public override string Title { get { return "施設"; } }

        public override void Build(Transform parent)
        {
            Root = UIFactory.Node("FacilityTab", parent);

            ScrollRect scrollRect;
            Transform content = UIFactory.ScrollList("FacilityList", Root.transform, 18,
                new RectOffset(24, 24, 24, 300), out scrollRect);
            UIFactory.Stretch(UIFactory.Rect(scrollRect.gameObject));

            for (int i = 0; i < UpgradeDatabase.All.Count; i++)
            {
                _rows.Add(BuildRow(content, UpgradeDatabase.All[i]));
            }
        }

        Row BuildRow(Transform parent, UpgradeDef def)
        {
            Row row = new Row();
            row.Id = def.Id;

            Image card = UIFactory.Panel("Facility_" + def.Id, parent, UIStyle.BgPanel2, UIStyle.Round16);
            UIFactory.Sizing(card.gameObject, 214, -1, -1, 214);

            row.Name = UIFactory.Label("Name", card.transform, def.Name, 34, UIStyle.Text,
                TextAnchor.UpperLeft, FontStyle.Bold);
            Place(row.Name.gameObject, 32, -26, 520, 44);

            row.Description = UIFactory.Label("Description", card.transform, def.Description, 24, UIStyle.TextDim,
                TextAnchor.UpperLeft);
            Place(row.Description.gameObject, 32, -74, 620, 76);

            row.Effect = UIFactory.Label("Effect", card.transform, "", 26, UIStyle.Positive,
                TextAnchor.UpperLeft, FontStyle.Bold);
            Place(row.Effect.gameObject, 32, -156, 620, 38);

            row.Buy = UIFactory.Button("Buy", card.transform, "", 27, UIStyle.BgPanel3, UIStyle.Gold);
            RectTransform buyRect = UIFactory.Rect(row.Buy.gameObject);
            buyRect.anchorMin = new Vector2(1f, 0.5f);
            buyRect.anchorMax = new Vector2(1f, 0.5f);
            buyRect.pivot = new Vector2(1f, 0.5f);
            buyRect.sizeDelta = new Vector2(272, 116);
            buyRect.anchoredPosition = new Vector2(-26, 0);
            row.BuyLabel = row.Buy.GetComponentInChildren<Text>();

            UpgradeId captured = def.Id;
            row.Buy.onClick.AddListener(delegate { Game.BuyUpgrade(captured); });
            return row;
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
            for (int i = 0; i < _rows.Count; i++)
            {
                Row row = _rows[i];
                UpgradeDef def = UpgradeDatabase.Get(row.Id);
                int level = Game.UpgradeLevel(row.Id);

                row.Name.text = def.Name + "  <color=#A79FC0><size=26>Lv." + level + "</size></color>";
                row.Effect.text = EffectText(row.Id, level);

                if (Game.UpgradeMaxed(row.Id))
                {
                    row.BuyLabel.text = "最大";
                    row.BuyLabel.color = UIStyle.TextFaint;
                    row.Buy.interactable = false;
                    continue;
                }

                double cost = Game.UpgradeCost(row.Id);
                bool canAfford = Game.Data.gold >= cost;
                row.BuyLabel.text = "拡張\n" + NumberFormat.Gold(cost);
                row.BuyLabel.color = canAfford ? UIStyle.Gold : UIStyle.TextFaint;
                row.Buy.interactable = canAfford;
            }
        }

        /// <summary>いま何が効いているかを、次に上げたときの差分つきで見せる。</summary>
        string EffectText(UpgradeId id, int level)
        {
            switch (id)
            {
                case UpgradeId.Treasury:
                    return "現在 " + NumberFormat.Percent(0.08 * level) + " → 次 " + NumberFormat.Percent(0.08 * (level + 1));
                case UpgradeId.Nest:
                    return "巣の広さ " + Game.NestCapacity + " → " + (Game.NestCapacity + 1);
                case UpgradeId.Training:
                    return "育成コスト " + NumberFormat.Percent(Mathf.Pow(0.97f, level) - 1f)
                         + " → " + NumberFormat.Percent(Mathf.Pow(0.97f, level + 1) - 1f);
                case UpgradeId.Altar:
                    return "レア度上昇 " + NumberFormat.Percent(Game.LuckFactor - 1.0)
                         + " → " + NumberFormat.Percent(Game.LuckFactor - 1.0 + 0.12);
                case UpgradeId.Hourglass:
                    return "放置上限 " + Game.OfflineCapHours.ToString("0") + "時間 → "
                         + (Game.OfflineCapHours + 1).ToString("0") + "時間";
                case UpgradeId.Flute:
                    return "なでる報酬 ×" + (2.0 * (1.0 + 0.5 * level)).ToString("0.#")
                         + " → ×" + (2.0 * (1.0 + 0.5 * (level + 1))).ToString("0.#") + "（毎秒あたり）";
                default:
                    return "放置効率 " + (Game.OfflineEfficiency * 100).ToString("0") + "％ → "
                         + (Mathf.Min(1f, (float)Game.OfflineEfficiency + 0.04f) * 100f).ToString("0") + "％";
            }
        }
    }
}

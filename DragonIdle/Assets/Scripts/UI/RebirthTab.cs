using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>
    /// 転生のタブ。巣をいちど手放すかわりに竜魂を得て、次の周回が丸ごと速くなる。
    /// </summary>
    public class RebirthTab : TabView
    {
        Text _soulValue;
        Text _bonusText;
        Text _gainText;
        Text _progressText;
        Image _progressFill;
        Button _rebirthButton;
        Text _rebirthLabel;
        Text _warningText;

        public override string Title { get { return "転生"; } }

        public override void Build(Transform parent)
        {
            Root = UIFactory.Node("RebirthTab", parent);

            ScrollRect scrollRect;
            Transform content = UIFactory.ScrollList("RebirthContent", Root.transform, 22,
                new RectOffset(24, 24, 24, 300), out scrollRect);
            UIFactory.Stretch(UIFactory.Rect(scrollRect.gameObject));

            // 現在の竜魂
            Image soulCard = UIFactory.Panel("SoulCard", content, UIStyle.BgPanel2, UIStyle.Round28);
            UIFactory.Sizing(soulCard.gameObject, 260, -1, -1, 260);
            UIFactory.VerticalLayout(soulCard.gameObject, 6, new RectOffset(36, 36, 34, 30));

            Text caption = UIFactory.Label("Caption", soulCard.transform, "いま宿している竜魂", 27, UIStyle.TextDim,
                TextAnchor.MiddleCenter);
            UIFactory.Sizing(caption.gameObject, 36);

            _soulValue = UIFactory.Label("Value", soulCard.transform, "0", 84, UIStyle.Soul,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Sizing(_soulValue.gameObject, 100);

            _bonusText = UIFactory.Label("Bonus", soulCard.transform, "", 28, UIStyle.Positive, TextAnchor.MiddleCenter);
            UIFactory.Sizing(_bonusText.gameObject, 40);

            // 次の転生
            Image nextCard = UIFactory.Panel("NextCard", content, UIStyle.BgPanel2, UIStyle.Round28);
            UIFactory.Sizing(nextCard.gameObject, 500, -1, -1, 500);
            UIFactory.VerticalLayout(nextCard.gameObject, 16, new RectOffset(36, 36, 32, 32));

            Text nextTitle = UIFactory.Label("Title", nextCard.transform, "次の転生", 34, UIStyle.Text,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Sizing(nextTitle.gameObject, 44);

            _gainText = UIFactory.Label("Gain", nextCard.transform, "", 30, UIStyle.Soul, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Sizing(_gainText.gameObject, 44);

            _progressText = UIFactory.Label("Progress", nextCard.transform, "", 25, UIStyle.TextDim, TextAnchor.MiddleLeft);
            UIFactory.Sizing(_progressText.gameObject, 36);

            Image track = UIFactory.Panel("Track", nextCard.transform, UIStyle.BgPanel3, UIStyle.Round8);
            UIFactory.Sizing(track.gameObject, 22, -1, -1, 22);
            _progressFill = UIFactory.Panel("Fill", track.transform, UIStyle.Soul, UIStyle.Round8);
            RectTransform fillRect = UIFactory.Rect(_progressFill.gameObject);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            _rebirthButton = UIFactory.Button("Rebirth", nextCard.transform, "", 34, UIStyle.Soul, UIStyle.TextOnGold);
            UIFactory.Sizing(_rebirthButton.gameObject, 116, -1, -1, 116);
            _rebirthLabel = _rebirthButton.GetComponentInChildren<Text>();
            _rebirthButton.onClick.AddListener(delegate { Game.Rebirth(); });

            _warningText = UIFactory.Label("Warning", nextCard.transform, "", 23, UIStyle.TextFaint, TextAnchor.UpperLeft);
            UIFactory.Sizing(_warningText.gameObject, 60);

            // 仕組みの説明
            Image helpCard = UIFactory.Panel("HelpCard", content, UIStyle.BgPanel, UIStyle.Round16);
            UIFactory.Sizing(helpCard.gameObject, 300, -1, -1, 300);
            UIFactory.VerticalLayout(helpCard.gameObject, 12, new RectOffset(32, 32, 28, 28));

            Text helpTitle = UIFactory.Label("Title", helpCard.transform, "転生で引き継がれるもの", 29, UIStyle.Text,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFactory.Sizing(helpTitle.gameObject, 40);

            Text helpBody = UIFactory.Label("Body", helpCard.transform,
                "<color=#6BD98F>引き継ぐ</color>　竜魂・図鑑・記録\n" +
                "<color=#E86A6A>手放す</color>　ゴールド・ドラゴン・施設\n\n" +
                "竜魂は1つにつき全生産量 +10%、孵化のレア度 +2%。\n" +
                "累計ゴールドが伸びるほど、一度の転生で得られる数も増える。",
                25, UIStyle.TextDim, TextAnchor.UpperLeft);
            LayoutElement bodyElement = UIFactory.Sizing(helpBody.gameObject, 190);
            bodyElement.flexibleHeight = 1;
        }

        public override void Refresh()
        {
            SaveData data = Game.Data;

            _soulValue.text = data.souls.ToString();
            _bonusText.text = "全生産量 " + NumberFormat.Percent(Game.SoulMultiplier - 1.0)
                            + "　レア度 " + NumberFormat.Percent(0.02 * data.souls);

            int gain = Game.SoulGain;
            _gainText.text = gain > 0 ? "竜魂を " + gain + " 得られる" : "まだ竜魂は得られない";
            _gainText.color = gain > 0 ? UIStyle.Soul : UIStyle.TextFaint;

            double progress;
            if (gain > 0)
            {
                progress = 1.0;
                _progressText.text = "この周回の累計 " + NumberFormat.Gold(data.lifetimeGold) + " ゴールド";
            }
            else
            {
                double needed = Game.GoldNeededForNextSoul;
                progress = Mathf.Clamp01((float)(data.lifetimeGold / needed));
                _progressText.text = "次の竜魂まで 累計 " + NumberFormat.Gold(needed) + " ゴールド"
                                   + "（いま " + NumberFormat.Gold(data.lifetimeGold) + "）";
            }

            RectTransform fillRect = UIFactory.Rect(_progressFill.gameObject);
            fillRect.anchorMax = new Vector2(Mathf.Max(0.012f, (float)progress), 1f);

            _rebirthButton.interactable = gain > 0;
            _rebirthLabel.text = gain > 0 ? "転生する" : "条件を満たしていない";
            _rebirthLabel.color = gain > 0 ? UIStyle.TextOnGold : new Color(0.1f, 0.08f, 0.02f, 0.55f);

            _warningText.text = data.rebirths > 0
                ? "これまでの転生 " + data.rebirths + " 回"
                : "初回は累計100万ゴールドから。焦らず巣を育てていい。";
        }
    }
}

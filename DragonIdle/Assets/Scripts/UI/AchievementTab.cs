using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>
    /// 称号のタブ。達成すると永久に生産量が上がるので、次に何を目指すかの一覧でもある。
    /// </summary>
    public class AchievementTab : TabView
    {
        class Row
        {
            public Achievement Source;
            public Image Medal;
            public Text MedalMark;
            public Text Name;
            public Text Requirement;
            public Text Bonus;
            public Image ProgressFill;
            public Text ProgressText;
        }

        readonly List<Row> _rows = new List<Row>();
        Text _summaryCount;
        Text _summaryBonus;

        public override string Title { get { return "称号"; } }

        public override void Build(Transform parent)
        {
            Root = UIFactory.Node("AchievementTab", parent);

            Image summary = UIFactory.Panel("Summary", Root.transform, UIStyle.BgPanel2, UIStyle.Round16);
            RectTransform summaryRect = UIFactory.Rect(summary.gameObject);
            summaryRect.anchorMin = new Vector2(0f, 1f);
            summaryRect.anchorMax = new Vector2(1f, 1f);
            summaryRect.pivot = new Vector2(0.5f, 1f);
            summaryRect.offsetMin = new Vector2(24, 0);
            summaryRect.offsetMax = new Vector2(-24, 0);
            summaryRect.sizeDelta = new Vector2(summaryRect.sizeDelta.x, 148);
            summaryRect.anchoredPosition = new Vector2(0, -24);

            _summaryCount = UIFactory.Label("Count", summary.transform, "", 40, UIStyle.Text,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            RectTransform countRect = UIFactory.Stretch(UIFactory.Rect(_summaryCount.gameObject), 32, 0, 0, 0);
            countRect.anchorMax = new Vector2(0.55f, 1f);

            _summaryBonus = UIFactory.Label("Bonus", summary.transform, "", 30, UIStyle.Positive,
                TextAnchor.MiddleRight, FontStyle.Bold);
            RectTransform bonusRect = UIFactory.Stretch(UIFactory.Rect(_summaryBonus.gameObject), 0, 0, 32, 0);
            bonusRect.anchorMin = new Vector2(0.5f, 0f);

            ScrollRect scrollRect;
            Transform content = UIFactory.ScrollList("AchievementList", Root.transform, 16,
                new RectOffset(24, 24, 8, 300), out scrollRect);
            UIFactory.Stretch(UIFactory.Rect(scrollRect.gameObject), 0, 0, 0, 188);

            for (int i = 0; i < AchievementDatabase.All.Count; i++)
            {
                _rows.Add(BuildRow(content, AchievementDatabase.All[i]));
            }
        }

        Row BuildRow(Transform parent, Achievement achievement)
        {
            Row row = new Row();
            row.Source = achievement;

            Image card = UIFactory.Panel("Achievement_" + achievement.Id, parent, UIStyle.BgPanel2, UIStyle.Round16);
            UIFactory.Sizing(card.gameObject, 152, -1, -1, 152);

            row.Medal = UIFactory.Panel("Medal", card.transform, UIStyle.Disabled, UIStyle.Circle);
            row.Medal.type = Image.Type.Simple;
            RectTransform medalRect = UIFactory.Rect(row.Medal.gameObject);
            medalRect.anchorMin = new Vector2(0f, 0.5f);
            medalRect.anchorMax = new Vector2(0f, 0.5f);
            medalRect.pivot = new Vector2(0f, 0.5f);
            medalRect.sizeDelta = new Vector2(88, 88);
            medalRect.anchoredPosition = new Vector2(28, 0);

            row.MedalMark = UIFactory.Label("Mark", row.Medal.transform, "竜", 36, UIStyle.TextFaint,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Stretch(UIFactory.Rect(row.MedalMark.gameObject));

            row.Name = UIFactory.Label("Name", card.transform, achievement.Name, 31, UIStyle.Text,
                TextAnchor.UpperLeft, FontStyle.Bold);
            Place(row.Name.gameObject, 142, -22, 600, 42);

            row.Requirement = UIFactory.Label("Requirement", card.transform, achievement.Requirement, 24,
                UIStyle.TextDim, TextAnchor.UpperLeft);
            Place(row.Requirement.gameObject, 142, -64, 600, 34);

            Image track = UIFactory.Panel("Track", card.transform, UIStyle.BgPanel3, UIStyle.Round8);
            Place(track.gameObject, 142, -106, 600, 18);
            row.ProgressFill = UIFactory.Panel("Fill", track.transform, UIStyle.Gold, UIStyle.Round8);
            RectTransform fillRect = UIFactory.Rect(row.ProgressFill.gameObject);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            row.ProgressText = UIFactory.Label("ProgressText", card.transform, "", 21, UIStyle.TextFaint,
                TextAnchor.UpperLeft);
            Place(row.ProgressText.gameObject, 142, -126, 600, 28);

            row.Bonus = UIFactory.Label("Bonus", card.transform, "", 30, UIStyle.TextFaint,
                TextAnchor.MiddleRight, FontStyle.Bold);
            RectTransform bonusRect = UIFactory.Rect(row.Bonus.gameObject);
            bonusRect.anchorMin = new Vector2(1f, 0.5f);
            bonusRect.anchorMax = new Vector2(1f, 0.5f);
            bonusRect.pivot = new Vector2(1f, 0.5f);
            bonusRect.sizeDelta = new Vector2(170, 48);
            bonusRect.anchoredPosition = new Vector2(-28, 0);

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
            _summaryCount.text = "称号　" + Game.UnlockedAchievementCount + " / " + AchievementDatabase.Count;
            _summaryBonus.text = "生産量 " + NumberFormat.Percent(Game.AchievementMultiplier - 1.0);

            for (int i = 0; i < _rows.Count; i++)
            {
                Row row = _rows[i];
                bool unlocked = Game.IsUnlocked(row.Source.Id);

                row.Medal.color = unlocked ? UIStyle.Gold : UIStyle.Disabled;
                row.MedalMark.text = unlocked ? "竜" : "?";
                row.MedalMark.color = unlocked ? UIStyle.TextOnGold : UIStyle.TextFaint;
                row.Name.color = unlocked ? UIStyle.Text : UIStyle.TextDim;
                row.Bonus.text = NumberFormat.Percent(row.Source.Bonus);
                row.Bonus.color = unlocked ? UIStyle.Positive : UIStyle.TextFaint;

                float progress = unlocked ? 1f : row.Source.Progress(Game);
                RectTransform fillRect = UIFactory.Rect(row.ProgressFill.gameObject);
                fillRect.anchorMax = new Vector2(Mathf.Max(0.012f, progress), 1f);
                row.ProgressFill.color = unlocked ? UIStyle.Positive : UIStyle.Gold;

                row.ProgressText.text = unlocked ? "達成ずみ" : row.Source.ProgressText(Game);
            }
        }
    }
}

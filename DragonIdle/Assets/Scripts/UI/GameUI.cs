using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>
    /// 画面全体の組み立てと更新。ヘッダー・タブ・なでるボタン・通知をここが持つ。
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        GameManager _game;

        Text _goldText;
        Text _rateText;
        Text _soulChip;
        Text _nestChip;
        Text _synergyChip;

        readonly List<TabView> _tabs = new List<TabView>();
        readonly List<Button> _tabButtons = new List<Button>();
        readonly List<Image> _tabIndicators = new List<Image>();
        readonly List<Text> _tabLabels = new List<Text>();
        int _activeTab;

        RectTransform _contentArea;
        RectTransform _floatLayer;
        Text _toastText;
        Image _toastPanel;
        float _toastTimer;

        GameObject _offlinePanel;
        Text _offlineBody;

        int _seenStructureVersion = -1;
        float _refreshTimer;

        void Start()
        {
            _game = GameManager.Instance;
            _game.OnToast += ShowToast;

            Build();

            _seenStructureVersion = _game.StructureVersion;
            RefreshAll();

            if (_game.PendingOffline.HasValue) ShowOfflineReport(_game.PendingOffline);
        }

        void OnDestroy()
        {
            if (_game != null) _game.OnToast -= ShowToast;
        }

        // ---------- 組み立て ----------

        void Build()
        {
            Canvas canvas = GetComponent<Canvas>();
            RectTransform root = UIFactory.Rect(canvas.gameObject);

            Image background = UIFactory.Panel("Background", root, UIStyle.BgDeep);
            UIFactory.Stretch(UIFactory.Rect(background.gameObject));

            GameObject columnGo = UIFactory.Node("Column", root);
            UIFactory.Stretch(UIFactory.Rect(columnGo));
            VerticalLayoutGroup column = UIFactory.VerticalLayout(columnGo, 0);
            column.childForceExpandHeight = false;

            BuildHeader(columnGo.transform);
            BuildContentArea(columnGo.transform);
            BuildTabBar(columnGo.transform);

            BuildFloatingLayer(root);
        }

        void BuildHeader(Transform parent)
        {
            Image header = UIFactory.Panel("Header", parent, UIStyle.BgPanel);
            UIFactory.Sizing(header.gameObject, 268, -1, -1, 268);
            UIFactory.VerticalLayout(header.gameObject, 14, new RectOffset(36, 36, 28, 24));

            // 1段目: ゴールドと毎秒の生産量
            GameObject topRow = UIFactory.Node("GoldRow", header.transform);
            UIFactory.Sizing(topRow, 104, -1, -1, 104);
            UIFactory.HorizontalLayout(topRow, 16, null, true, true);

            Image coin = UIFactory.Panel("Coin", topRow.transform, UIStyle.Gold, UIStyle.Circle);
            coin.type = Image.Type.Simple;
            UIFactory.Sizing(coin.gameObject, 56, 56);
            Text coinMark = UIFactory.Label("Mark", coin.transform, "竜", 30, UIStyle.TextOnGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Stretch(UIFactory.Rect(coinMark.gameObject));

            GameObject goldColumn = UIFactory.Node("GoldColumn", topRow.transform);
            UIFactory.Sizing(goldColumn, 104, -1, 1);
            UIFactory.VerticalLayout(goldColumn, 0, null);
            _goldText = UIFactory.Label("Gold", goldColumn.transform, "0", 62, UIStyle.Gold, TextAnchor.LowerLeft, FontStyle.Bold);
            UIFactory.Sizing(_goldText.gameObject, 68);
            _rateText = UIFactory.Label("Rate", goldColumn.transform, "0/秒", 28, UIStyle.Positive, TextAnchor.UpperLeft);
            UIFactory.Sizing(_rateText.gameObject, 34);

            // 2段目: 状態チップ
            GameObject chipRow = UIFactory.Node("ChipRow", header.transform);
            UIFactory.Sizing(chipRow, 62, -1, -1, 62);
            UIFactory.HorizontalLayout(chipRow, 12, null, true, true);

            _soulChip = BuildChip(chipRow.transform, "竜魂 0", UIStyle.Soul);
            _nestChip = BuildChip(chipRow.transform, "巣 1/3", UIStyle.TextDim);
            _synergyChip = BuildChip(chipRow.transform, "属性 1種", UIStyle.Positive);
        }

        Text BuildChip(Transform parent, string caption, Color color)
        {
            Image chip = UIFactory.Panel("Chip", parent, UIStyle.BgPanel2, UIStyle.Round16);
            UIFactory.Sizing(chip.gameObject, 56, 240);
            Text label = UIFactory.Label("Label", chip.transform, caption, 26, color, TextAnchor.MiddleCenter);
            UIFactory.Stretch(UIFactory.Rect(label.gameObject), 16, 0, 16, 0);
            return label;
        }

        void BuildContentArea(Transform parent)
        {
            GameObject area = UIFactory.Node("Content", parent);
            LayoutElement element = UIFactory.Sizing(area, -1);
            element.flexibleHeight = 1;
            _contentArea = UIFactory.Rect(area);

            _tabs.Add(new NestTab());
            _tabs.Add(new FacilityTab());
            _tabs.Add(new AchievementTab());
            _tabs.Add(new RebirthTab());
            _tabs.Add(new RecordTab());

            for (int i = 0; i < _tabs.Count; i++)
            {
                _tabs[i].Build(area.transform);
                UIFactory.Stretch(UIFactory.Rect(_tabs[i].Root));
                _tabs[i].SetVisible(i == 0);
            }
        }

        void BuildTabBar(Transform parent)
        {
            Image bar = UIFactory.Panel("TabBar", parent, UIStyle.BgPanel);
            UIFactory.Sizing(bar.gameObject, 156, -1, -1, 156);
            HorizontalLayoutGroup layout = UIFactory.HorizontalLayout(bar.gameObject, 0, new RectOffset(0, 0, 0, 0));
            layout.childForceExpandWidth = true;

            for (int i = 0; i < _tabs.Count; i++)
            {
                int index = i;
                GameObject cell = UIFactory.Node("Tab" + i, bar.transform);
                UIFactory.Sizing(cell, 156, -1, 1);
                Image cellImage = cell.AddComponent<Image>();
                cellImage.color = new Color(0, 0, 0, 0);
                Button button = cell.AddComponent<Button>();
                button.targetGraphic = cellImage;
                button.onClick.AddListener(delegate { SelectTab(index); });

                Image indicator = UIFactory.Panel("Indicator", cell.transform, UIStyle.Gold);
                RectTransform indicatorRect = UIFactory.Rect(indicator.gameObject);
                indicatorRect.anchorMin = new Vector2(0.5f, 1f);
                indicatorRect.anchorMax = new Vector2(0.5f, 1f);
                indicatorRect.pivot = new Vector2(0.5f, 1f);
                indicatorRect.sizeDelta = new Vector2(72, 6);
                indicatorRect.anchoredPosition = Vector2.zero;

                Text label = UIFactory.Label("Label", cell.transform, _tabs[i].Title, 28,
                    UIStyle.TextFaint, TextAnchor.MiddleCenter, FontStyle.Bold);
                UIFactory.Stretch(UIFactory.Rect(label.gameObject), 0, 0, 0, 10);

                _tabButtons.Add(button);
                _tabIndicators.Add(indicator);
                _tabLabels.Add(label);
            }

            SelectTab(0);
        }

        void BuildFloatingLayer(Transform root)
        {
            GameObject layerGo = UIFactory.Node("FloatingLayer", root);
            _floatLayer = UIFactory.Stretch(UIFactory.Rect(layerGo));

            // なでるボタン: タブバーのすぐ上、右寄せ
            Image petImage = UIFactory.Panel("PetButton", layerGo.transform, UIStyle.Gold, UIStyle.Circle);
            petImage.type = Image.Type.Simple;
            RectTransform petRect = UIFactory.Rect(petImage.gameObject);
            petRect.anchorMin = new Vector2(1f, 0f);
            petRect.anchorMax = new Vector2(1f, 0f);
            petRect.pivot = new Vector2(1f, 0f);
            petRect.sizeDelta = new Vector2(212, 212);
            petRect.anchoredPosition = new Vector2(-36, 190);

            Button petButton = petImage.gameObject.AddComponent<Button>();
            petButton.targetGraphic = petImage;
            petButton.onClick.AddListener(OnPet);
            Text petLabel = UIFactory.Label("Label", petImage.transform, "なでる", 34, UIStyle.TextOnGold,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Stretch(UIFactory.Rect(petLabel.gameObject));

            // トースト
            _toastPanel = UIFactory.Panel("Toast", layerGo.transform, UIStyle.BgPanel3, UIStyle.Round16);
            RectTransform toastRect = UIFactory.Rect(_toastPanel.gameObject);
            toastRect.anchorMin = new Vector2(0.5f, 1f);
            toastRect.anchorMax = new Vector2(0.5f, 1f);
            toastRect.pivot = new Vector2(0.5f, 1f);
            toastRect.sizeDelta = new Vector2(940, 92);
            toastRect.anchoredPosition = new Vector2(0, -288);
            _toastText = UIFactory.Label("Label", _toastPanel.transform, "", 28, UIStyle.Text, TextAnchor.MiddleCenter);
            UIFactory.Stretch(UIFactory.Rect(_toastText.gameObject), 24, 0, 24, 0);
            _toastPanel.gameObject.SetActive(false);

            BuildOfflinePanel(layerGo.transform);
        }

        void BuildOfflinePanel(Transform parent)
        {
            _offlinePanel = UIFactory.Node("OfflinePanel", parent);
            UIFactory.Stretch(UIFactory.Rect(_offlinePanel));
            Image dim = _offlinePanel.AddComponent<Image>();
            dim.color = new Color(0.02f, 0.01f, 0.05f, 0.86f);

            Image card = UIFactory.Panel("Card", _offlinePanel.transform, UIStyle.BgPanel, UIStyle.Round28);
            RectTransform cardRect = UIFactory.Rect(card.gameObject);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(880, 560);
            cardRect.anchoredPosition = Vector2.zero;
            UIFactory.VerticalLayout(card.gameObject, 22, new RectOffset(48, 48, 48, 44));

            Text title = UIFactory.Label("Title", card.transform, "おかえりなさい", 46, UIStyle.Text,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.Sizing(title.gameObject, 60);

            _offlineBody = UIFactory.Label("Body", card.transform, "", 30, UIStyle.TextDim, TextAnchor.UpperCenter);
            LayoutElement bodyElement = UIFactory.Sizing(_offlineBody.gameObject, 260);
            bodyElement.flexibleHeight = 1;

            Button close = UIFactory.Button("Close", card.transform, "受け取る", 34, UIStyle.Gold, UIStyle.TextOnGold, UIStyle.Round16);
            UIFactory.Sizing(close.gameObject, 104);
            close.onClick.AddListener(delegate { _offlinePanel.SetActive(false); });

            _offlinePanel.SetActive(false);
        }

        // ---------- 操作 ----------

        void SelectTab(int index)
        {
            _activeTab = index;
            for (int i = 0; i < _tabs.Count; i++)
            {
                bool active = i == index;
                _tabs[i].SetVisible(active);
                _tabIndicators[i].gameObject.SetActive(active);
                _tabLabels[i].color = active ? UIStyle.Gold : UIStyle.TextFaint;
            }
            _tabs[index].Refresh();
        }

        void OnPet()
        {
            double reward = _game.Pet();
            SpawnFloatingGold(reward);
        }

        void SpawnFloatingGold(double amount)
        {
            Text text = UIFactory.Label("Floating", _floatLayer, "+" + NumberFormat.Gold(amount), 40,
                UIStyle.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform rect = UIFactory.Rect(text.gameObject);
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420, 60);
            rect.anchoredPosition = new Vector2(-142 + Random.Range(-40f, 40f), 420);
            StartCoroutine(FloatAndFade(rect, text));
        }

        IEnumerator FloatAndFade(RectTransform rect, Text text)
        {
            float duration = 0.9f;
            float elapsed = 0f;
            Vector2 start = rect.anchoredPosition;
            Color color = text.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = start + new Vector2(0, 150f * t);
                color.a = 1f - t * t;
                text.color = color;
                yield return null;
            }
            Destroy(rect.gameObject);
        }

        void ShowToast(string message)
        {
            _toastText.text = message;
            _toastPanel.gameObject.SetActive(true);
            _toastTimer = 3.2f;
        }

        void ShowOfflineReport(OfflineReport report)
        {
            string body = "留守のあいだ、ドラゴンたちは " + NumberFormat.Duration(report.Seconds) + " 働いた。\n\n"
                        + "<color=#F2C14E><size=52>" + NumberFormat.Gold(report.Gold) + "</size></color>\nゴールドを受け取った。\n\n"
                        + "<size=24>放置効率 " + (_game.OfflineEfficiency * 100).ToString("0") + "％／上限 "
                        + _game.OfflineCapHours.ToString("0") + "時間</size>";
            if (report.HitCap)
            {
                body += "\n<size=24><color=#E86A6A>上限に達していた。時の砂時計で伸ばせる。</color></size>";
            }
            _offlineBody.text = body;
            _offlinePanel.SetActive(true);
            _game.PendingOffline = new OfflineReport();
        }

        // ---------- 更新 ----------

        void Update()
        {
            if (_game == null) return;

            _goldText.text = NumberFormat.Gold(_game.Data.gold);
            _rateText.text = NumberFormat.Rate(_game.GoldPerSecond) + " ゴールド/秒";

            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                if (_toastTimer <= 0f) _toastPanel.gameObject.SetActive(false);
            }

            if (_seenStructureVersion != _game.StructureVersion)
            {
                _seenStructureVersion = _game.StructureVersion;
                for (int i = 0; i < _tabs.Count; i++) _tabs[i].Rebuild();
                RefreshAll();
                return;
            }

            _refreshTimer += Time.deltaTime;
            if (_refreshTimer >= 0.1f)
            {
                _refreshTimer = 0f;
                RefreshAll();
            }
        }

        void RefreshAll()
        {
            _soulChip.text = "竜魂 " + _game.Data.souls;
            _nestChip.text = "巣 " + _game.Data.dragons.Count + "/" + _game.NestCapacity;
            _synergyChip.text = "属性 " + _game.DistinctElements + "種 " + NumberFormat.Percent(_game.SynergyMultiplier - 1.0);
            _tabs[_activeTab].Refresh();
        }
    }
}

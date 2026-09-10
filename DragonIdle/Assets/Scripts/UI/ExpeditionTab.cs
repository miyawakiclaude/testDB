using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>
    /// 探索のタブ。留守にするあいだ巣では稼がないかわりに、帰りに大きく持ち帰る。
    /// 行き先は枠ごとに決まっているので、選ぶのは「誰を送るか」だけ。
    /// </summary>
    public class ExpeditionTab : TabView
    {
        class Slot
        {
            public int Index;
            public Image Plate;
            public Image Portrait;
            public Text Title;
            public Text Detail;
            public Text Status;
            public Image ProgressFill;
            public Button Action;
            public Text ActionLabel;
        }

        readonly List<Slot> _slots = new List<Slot>();

        GameObject _picker;
        Transform _pickerContent;
        Text _pickerTitle;
        int _pickerSlot = -1;

        public override string Title { get { return "探索"; } }

        public override void Build(Transform parent)
        {
            Root = UIFactory.Node("ExpeditionTab", parent);

            ScrollRect scrollRect;
            Transform content = UIFactory.ScrollList("ExpeditionList", Root.transform, 20,
                new RectOffset(24, 24, 24, 300), out scrollRect);
            UIFactory.Stretch(UIFactory.Rect(scrollRect.gameObject));

            Text intro = UIFactory.Label("Intro", content,
                "送り出したドラゴンは巣で稼がなくなるが、帰りにまとめて持ち帰り、旅のぶんだけ強くなる。",
                24, UIStyle.TextDim, TextAnchor.UpperLeft);
            UIFactory.Sizing(intro.gameObject, 72, -1, -1, 72);

            for (int i = 0; i < SaveData.ExpeditionSlots && i < ExpeditionDatabase.Count; i++)
            {
                _slots.Add(BuildSlot(content, i));
            }

            BuildPicker();
        }

        Slot BuildSlot(Transform parent, int index)
        {
            ExpeditionDef def = ExpeditionDatabase.Get(index);

            Slot slot = new Slot();
            slot.Index = index;

            Image card = UIFactory.Panel("Slot" + index, parent, UIStyle.BgPanel2, UIStyle.Round16);
            UIFactory.Sizing(card.gameObject, 250, -1, -1, 250);

            slot.Title = UIFactory.Label("Title", card.transform, def.Name, 33, UIStyle.Text,
                TextAnchor.UpperLeft, FontStyle.Bold);
            Place(slot.Title.gameObject, 32, -24, 520, 42);

            slot.Detail = UIFactory.Label("Detail", card.transform,
                def.Description + "\n所要 " + NumberFormat.Duration(def.Seconds)
                + "　実入り ×" + def.Multiplier.ToString("0.#") + "　Lv +" + def.LevelGain,
                23, UIStyle.TextDim, TextAnchor.UpperLeft);
            Place(slot.Detail.gameObject, 32, -66, 600, 72);

            Color plateColor = UIStyle.BgPanel3;
            slot.Plate = UIFactory.Panel("Plate", card.transform, plateColor, UIStyle.Round8);
            Place(slot.Plate.gameObject, 32, -146, 88, 88);

            slot.Portrait = UIFactory.Panel("Portrait", slot.Plate.transform, Color.white, null);
            slot.Portrait.preserveAspect = true;
            slot.Portrait.raycastTarget = false;
            UIFactory.Stretch(UIFactory.Rect(slot.Portrait.gameObject), 4, 4, 4, 4);

            slot.Status = UIFactory.Label("Status", card.transform, "", 25, UIStyle.TextDim,
                TextAnchor.UpperLeft);
            Place(slot.Status.gameObject, 132, -150, 480, 70);

            Image track = UIFactory.Panel("Track", card.transform, UIStyle.BgPanel3, UIStyle.Round8);
            Place(track.gameObject, 32, -224, 580, 16);
            slot.ProgressFill = UIFactory.Panel("Fill", track.transform, UIStyle.Soul, UIStyle.Round8);
            RectTransform fillRect = UIFactory.Rect(slot.ProgressFill.gameObject);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            slot.Action = UIFactory.Button("Action", card.transform, "", 27, UIStyle.BgPanel3, UIStyle.Gold);
            RectTransform actionRect = UIFactory.Rect(slot.Action.gameObject);
            actionRect.anchorMin = new Vector2(1f, 0.5f);
            actionRect.anchorMax = new Vector2(1f, 0.5f);
            actionRect.pivot = new Vector2(1f, 0.5f);
            actionRect.sizeDelta = new Vector2(272, 120);
            actionRect.anchoredPosition = new Vector2(-26, 0);
            slot.ActionLabel = slot.Action.GetComponentInChildren<Text>();

            int captured = index;
            slot.Action.onClick.AddListener(delegate { OnAction(captured); });
            return slot;
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

        // ---------- 送る相手を選ぶ ----------

        void BuildPicker()
        {
            _picker = UIFactory.Node("Picker", Root.transform);
            UIFactory.Stretch(UIFactory.Rect(_picker));
            Image dim = _picker.AddComponent<Image>();
            dim.color = new Color(0.02f, 0.01f, 0.05f, 0.88f);
            Button dismiss = _picker.AddComponent<Button>();
            dismiss.targetGraphic = dim;
            dismiss.transition = Selectable.Transition.None;
            dismiss.onClick.AddListener(ClosePicker);

            Image panel = UIFactory.Panel("Panel", _picker.transform, UIStyle.BgPanel, UIStyle.Round28);
            UIFactory.Stretch(UIFactory.Rect(panel.gameObject), 40, 60, 40, 60);

            _pickerTitle = UIFactory.Label("Title", panel.transform, "", 34, UIStyle.Text,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform titleRect = UIFactory.Rect(_pickerTitle.gameObject);
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(24, 0);
            titleRect.offsetMax = new Vector2(-24, 0);
            titleRect.sizeDelta = new Vector2(titleRect.sizeDelta.x, 64);
            titleRect.anchoredPosition = new Vector2(0, -24);

            ScrollRect scrollRect;
            _pickerContent = UIFactory.ScrollList("PickerList", panel.transform, 12,
                new RectOffset(20, 20, 8, 20), out scrollRect);
            UIFactory.Stretch(UIFactory.Rect(scrollRect.gameObject), 8, 104, 8, 96);

            Button close = UIFactory.Button("Close", panel.transform, "やめる", 28,
                UIStyle.BgPanel3, UIStyle.TextDim);
            RectTransform closeRect = UIFactory.Rect(close.gameObject);
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot = new Vector2(0.5f, 0f);
            closeRect.sizeDelta = new Vector2(320, 84);
            closeRect.anchoredPosition = new Vector2(0, 24);
            close.onClick.AddListener(ClosePicker);

            _picker.SetActive(false);
        }

        void OpenPicker(int slotIndex)
        {
            _pickerSlot = slotIndex;
            ExpeditionDef def = ExpeditionDatabase.Get(slotIndex);
            _pickerTitle.text = def.Name + " へ送る";

            for (int i = _pickerContent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(_pickerContent.GetChild(i).gameObject);
            }

            List<DragonSave> available = Game.AvailableDragons();
            if (available.Count == 0)
            {
                Text empty = UIFactory.Label("Empty", _pickerContent,
                    "送り出せるドラゴンがいない。", 26, UIStyle.TextFaint, TextAnchor.MiddleCenter);
                UIFactory.Sizing(empty.gameObject, 80, -1, -1, 80);
            }
            for (int i = 0; i < available.Count; i++) BuildPickerRow(available[i], slotIndex);

            _picker.SetActive(true);
            _picker.transform.SetAsLastSibling();
        }

        void BuildPickerRow(DragonSave dragon, int slotIndex)
        {
            DragonSpecies species = SpeciesDatabase.ById(dragon.speciesId);

            Image row = UIFactory.Panel("Pick_" + dragon.uid, _pickerContent, UIStyle.BgPanel2, UIStyle.Round16);
            UIFactory.Sizing(row.gameObject, 128, -1, -1, 128);

            Button button = row.gameObject.AddComponent<Button>();
            button.targetGraphic = row;

            Color plateColor = Elements.Tint(species.Element);
            plateColor.a = 0.14f;
            Image plate = UIFactory.Panel("Plate", row.transform, plateColor, UIStyle.Round8);
            Place(plate.gameObject, 20, -16, 96, 96);

            Image portrait = UIFactory.Panel("Portrait", plate.transform, Color.white, null);
            portrait.sprite = DragonArt.For(species);
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            UIFactory.Stretch(UIFactory.Rect(portrait.gameObject), 4, 4, 4, 4);

            Text name = UIFactory.Label("Name", row.transform,
                species.Name + "　<color=#A79FC0><size=22>Lv." + dragon.level + "</size></color>",
                28, UIStyle.Text, TextAnchor.UpperLeft, FontStyle.Bold);
            Place(name.gameObject, 132, -22, 460, 38);

            Text reward = UIFactory.Label("Reward", row.transform,
                "持ち帰り " + NumberFormat.Gold(Game.ExpeditionFullReward(slotIndex, dragon)) + " ゴールド",
                23, UIStyle.Gold, TextAnchor.UpperLeft);
            Place(reward.gameObject, 132, -64, 460, 34);

            DragonSave captured = dragon;
            int capturedSlot = slotIndex;
            button.onClick.AddListener(delegate
            {
                Game.Dispatch(capturedSlot, captured);
                ClosePicker();
            });
        }

        void ClosePicker()
        {
            _pickerSlot = -1;
            if (_picker != null) _picker.SetActive(false);
        }

        void OnAction(int slotIndex)
        {
            ExpeditionSave save = Game.Slot(slotIndex);
            if (save == null) return;
            if (save.dragonUid == 0) OpenPicker(slotIndex);
            else Game.Collect(slotIndex);
        }

        public override void Rebuild()
        {
            if (_pickerSlot >= 0) OpenPicker(_pickerSlot);
        }

        public override void Refresh()
        {
            for (int i = 0; i < _slots.Count; i++) RefreshSlot(_slots[i]);
        }

        void RefreshSlot(Slot slot)
        {
            ExpeditionSave save = Game.Slot(slot.Index);
            DragonSave dragon = Game.DragonOnExpedition(slot.Index);

            if (save == null || save.dragonUid == 0 || dragon == null)
            {
                slot.Portrait.enabled = false;
                slot.Status.text = "空いている。誰かを送り出せる。";
                slot.Status.color = UIStyle.TextFaint;
                slot.ProgressFill.enabled = false;
                slot.ActionLabel.text = "送り出す";
                slot.ActionLabel.color = UIStyle.Gold;
                slot.Action.interactable = Game.AvailableDragonCount > 0;
                return;
            }

            DragonSpecies species = SpeciesDatabase.ById(dragon.speciesId);
            if (slot.Portrait.sprite == null || !slot.Portrait.enabled)
            {
                slot.Portrait.enabled = true;
            }
            slot.Portrait.sprite = DragonArt.For(species);

            double progress = Game.ExpeditionProgress(slot.Index);
            double remaining = Game.ExpeditionRemainingSeconds(slot.Index);
            bool done = progress >= 1.0;

            slot.ProgressFill.enabled = true;
            RectTransform fillRect = UIFactory.Rect(slot.ProgressFill.gameObject);
            fillRect.anchorMax = new Vector2(Mathf.Max(0.012f, (float)progress), 1f);
            slot.ProgressFill.color = done ? UIStyle.Positive : UIStyle.Soul;

            slot.Status.text = species.Name + " が " + (done ? "帰り着いた" : "旅の途中")
                             + "\n<color=#F2C14E>" + NumberFormat.Gold(Game.ExpeditionReward(slot.Index))
                             + " ゴールド</color>"
                             + (done ? "" : "　残り " + NumberFormat.Duration(remaining));
            slot.Status.color = UIStyle.TextDim;

            slot.ActionLabel.text = done ? "迎える" : "呼び戻す";
            slot.ActionLabel.color = done ? UIStyle.Positive : UIStyle.TextDim;
            slot.Action.interactable = true;
        }
    }
}

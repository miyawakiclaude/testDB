using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>
    /// 卵が揺れて割れ、生まれたドラゴンが現れるまでの短い演出。
    /// 孵化はこのゲームの一番おいしい瞬間なので、一覧に1行増えるだけでは終わらせない。
    /// </summary>
    public class HatchReveal : MonoBehaviour
    {
        GameObject _root;
        Image _dim;
        Image _egg;
        Image _burst;
        Image _portrait;
        Image _flash;
        Text _rarityText;
        Text _nameText;
        Text _flavorText;
        Text _tapHint;
        Text _newBadge;
        Button _dismiss;

        RectTransform _eggRect;
        RectTransform _burstRect;
        RectTransform _portraitRect;

        Coroutine _running;
        bool _built;

        public bool IsPlaying { get { return _root != null && _root.activeSelf; } }

        public void Build(Transform parent)
        {
            _root = UIFactory.Node("HatchReveal", parent);
            UIFactory.Stretch(UIFactory.Rect(_root));

            _dim = _root.AddComponent<Image>();
            _dim.color = new Color(0.02f, 0.01f, 0.05f, 0.9f);

            _dismiss = _root.AddComponent<Button>();
            _dismiss.targetGraphic = _dim;
            _dismiss.transition = Selectable.Transition.None;
            _dismiss.onClick.AddListener(Dismiss);

            _burst = UIFactory.Panel("Burst", _root.transform, new Color(1f, 1f, 1f, 0.25f), null);
            _burst.sprite = UIStyle.Burst;
            _burst.raycastTarget = false;
            _burstRect = UIFactory.Rect(_burst.gameObject);
            Center(_burstRect, new Vector2(900, 900), 120f);

            _portrait = UIFactory.Panel("Portrait", _root.transform, Color.white, null);
            _portrait.preserveAspect = true;
            _portrait.raycastTarget = false;
            _portraitRect = UIFactory.Rect(_portrait.gameObject);
            Center(_portraitRect, new Vector2(460, 460), 150f);

            _egg = UIFactory.Panel("Egg", _root.transform, Color.white, null);
            _egg.sprite = UIStyle.Egg;
            _egg.preserveAspect = true;
            _egg.raycastTarget = false;
            _eggRect = UIFactory.Rect(_egg.gameObject);
            Center(_eggRect, new Vector2(300, 375), 150f);

            _newBadge = UIFactory.Label("NewBadge", _root.transform, "新種発見", 30, UIStyle.Positive,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            Center(UIFactory.Rect(_newBadge.gameObject), new Vector2(700, 44), -108f);

            _rarityText = UIFactory.Label("Rarity", _root.transform, "", 34, UIStyle.Gold,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            Center(UIFactory.Rect(_rarityText.gameObject), new Vector2(700, 48), -160f);

            _nameText = UIFactory.Label("Name", _root.transform, "", 54, UIStyle.Text,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            Center(UIFactory.Rect(_nameText.gameObject), new Vector2(940, 74), -232f);

            _flavorText = UIFactory.Label("Flavor", _root.transform, "", 26, UIStyle.TextDim,
                TextAnchor.UpperCenter);
            Center(UIFactory.Rect(_flavorText.gameObject), new Vector2(820, 90), -320f);

            _tapHint = UIFactory.Label("TapHint", _root.transform, "画面をタップして閉じる", 24,
                UIStyle.TextFaint, TextAnchor.MiddleCenter);
            Center(UIFactory.Rect(_tapHint.gameObject), new Vector2(700, 40), -430f);

            // 割れた瞬間の白い閃光。いちばん手前に置く。
            _flash = UIFactory.Panel("Flash", _root.transform, new Color(1f, 1f, 1f, 0f), null);
            _flash.raycastTarget = false;
            UIFactory.Stretch(UIFactory.Rect(_flash.gameObject));

            _root.SetActive(false);
            _built = true;
        }

        static void Center(RectTransform rect, Vector2 size, float y)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0, y);
        }

        public void Show(DragonSave dragon, bool isNewSpecies)
        {
            if (!_built || dragon == null) return;

            DragonSpecies species = SpeciesDatabase.ById(dragon.speciesId);
            Rarity rarity = (Rarity)dragon.rarity;
            Color rarityColor = Rarities.Tint(rarity);

            _portrait.sprite = DragonArt.For(species);
            _burst.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0f);
            _egg.color = Color.Lerp(Color.white, Elements.Tint(species.Element), 0.30f);

            _rarityText.text = Rarities.Name(rarity) + "　" + Elements.Name(species.Element)
                             + "　★" + species.Tier;
            _rarityText.color = rarityColor;
            _nameText.text = species.Name;
            _flavorText.text = species.Flavor;

            _newBadge.gameObject.SetActive(isNewSpecies);

            _root.SetActive(true);
            _root.transform.SetAsLastSibling();

            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(PlaySequence(rarity, isNewSpecies));
        }

        IEnumerator PlaySequence(Rarity rarity, bool isNewSpecies)
        {
            // レアなほど、割れるまでの溜めを長くする。
            float shakeTime = 0.7f + (int)rarity * 0.18f;

            SetVisible(_rarityText, false);
            SetVisible(_nameText, false);
            SetVisible(_flavorText, false);
            SetVisible(_tapHint, false);
            SetVisible(_newBadge, false);
            _portraitRect.localScale = Vector3.zero;
            _eggRect.localScale = Vector3.one;
            _egg.enabled = true;
            _flash.color = new Color(1f, 1f, 1f, 0f);

            float elapsed = 0f;
            while (elapsed < shakeTime)
            {
                elapsed += Time.deltaTime;
                float intensity = Mathf.Clamp01(elapsed / shakeTime);
                float speed = 14f + intensity * 26f;
                float angle = Mathf.Sin(elapsed * speed) * (3f + intensity * 9f);
                _eggRect.localRotation = Quaternion.Euler(0f, 0f, angle);
                _eggRect.anchoredPosition = new Vector2(Mathf.Sin(elapsed * speed * 1.7f) * intensity * 8f, 150f);
                float pulse = 1f + Mathf.Sin(elapsed * 9f) * 0.02f;
                _eggRect.localScale = new Vector3(pulse, pulse, 1f);
                yield return null;
            }

            // 割れる: 卵が消え、閃光が走り、光芒が開く
            _egg.enabled = false;
            _flash.color = new Color(1f, 1f, 1f, 0.85f);
            _burst.color = new Color(_burst.color.r, _burst.color.g, _burst.color.b, 0.42f);

            SetVisible(_rarityText, true);
            SetVisible(_nameText, true);
            SetVisible(_flavorText, true);
            SetVisible(_tapHint, true);
            SetVisible(_newBadge, isNewSpecies);

            elapsed = 0f;
            const float grow = 0.55f;
            while (elapsed < grow)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / grow);
                // 行き過ぎてから戻る、はずむような立ち上がり
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.18f - (1f - t) * (1f - t);
                _portraitRect.localScale = new Vector3(scale, scale, 1f);
                _flash.color = new Color(1f, 1f, 1f, 0.85f * (1f - t));
                yield return null;
            }
            _portraitRect.localScale = Vector3.one;
            _flash.color = new Color(1f, 1f, 1f, 0f);
            _running = null;
        }

        void Update()
        {
            if (!IsPlaying) return;
            // 光芒はゆっくり回り続ける
            _burstRect.localRotation = Quaternion.Euler(0f, 0f, Time.time * 12f);
        }

        static void SetVisible(Text text, bool visible)
        {
            if (text != null) text.enabled = visible;
        }

        public void Dismiss()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }
            if (_root != null) _root.SetActive(false);
        }
    }
}

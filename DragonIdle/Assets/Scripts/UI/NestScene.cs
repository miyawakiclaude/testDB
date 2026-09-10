using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>
    /// 巣そのものを描く場所。育てているドラゴンが並んで息をしていて、触るとゴールドがもらえる。
    /// 一覧の数字だけでは出ない手触りを担当する。
    /// 奥の列から先に並べるので、手前のドラゴンが自然に前へ重なる。
    /// </summary>
    public class NestScene : MonoBehaviour
    {
        class Actor
        {
            public RectTransform Rect;
            public GameObject Shadow;
            public Vector2 Home;
            public float Size;
            public float Phase;
            public float Squash;
        }

        readonly List<Actor> _actors = new List<Actor>();
        RectTransform _self;
        Text _hint;

        static GameManager Game { get { return GameManager.Instance; } }

        void Awake()
        {
            _self = UIFactory.Rect(gameObject);
        }

        /// <summary>「なでてみよう」の案内。何度かなでたら自然に消える。</summary>
        public void Attach(Text hint) { _hint = hint; }

        public void Rebuild()
        {
            for (int i = 0; i < _actors.Count; i++)
            {
                if (_actors[i].Rect != null) Destroy(_actors[i].Rect.gameObject);
                if (_actors[i].Shadow != null) Destroy(_actors[i].Shadow);
            }
            _actors.Clear();

            if (Game == null) return;
            List<DragonSave> dragons = Game.Data.dragons;
            int count = dragons.Count;
            if (count == 0) return;

            int rows = count <= 4 ? 1 : (count <= 12 ? 2 : 3);
            int perRow = Mathf.CeilToInt(count / (float)rows);
            float width = UIStyle.ReferenceResolution.x - 88f;
            float baseSize = Mathf.Clamp(width / perRow * 0.92f, 58f, 172f);

            int index = 0;
            for (int rowFromBack = 0; rowFromBack < rows && index < count; rowFromBack++)
            {
                int inRow = Mathf.Min(perRow, count - index);
                int rowFromFront = rows - 1 - rowFromBack;
                float depth = Mathf.Pow(0.84f, rowFromFront);
                float y = 92f + rowFromFront * (baseSize * 0.44f + 20f);

                for (int column = 0; column < inRow; column++)
                {
                    float x = (column + 0.5f - inRow * 0.5f) * (width / inRow);
                    _actors.Add(BuildActor(dragons[index], new Vector2(x, y), baseSize * depth, depth, index));
                    index++;
                }
            }
        }

        Actor BuildActor(DragonSave data, Vector2 home, float size, float depth, int index)
        {
            DragonSpecies species = SpeciesDatabase.ById(data.speciesId);

            Image shadow = UIFactory.Panel("Shadow", transform, new Color(0f, 0f, 0f, 0.30f * depth), UIStyle.Circle);
            shadow.type = Image.Type.Simple;
            shadow.raycastTarget = false;
            Anchor(UIFactory.Rect(shadow.gameObject),
                new Vector2(home.x, home.y - size * 0.05f), new Vector2(size * 0.58f, size * 0.16f));

            Image portrait = UIFactory.Panel("Actor_" + species.Id, transform, Color.white, null);
            portrait.sprite = DragonArt.For(species);
            portrait.type = Image.Type.Simple;
            portrait.preserveAspect = true;
            portrait.color = Color.Lerp(new Color(0.58f, 0.55f, 0.70f), Color.white, depth);

            RectTransform rect = UIFactory.Rect(portrait.gameObject);
            Anchor(rect, home, new Vector2(size, size));

            Actor actor = new Actor();
            actor.Rect = rect;
            actor.Shadow = shadow.gameObject;
            actor.Home = home;
            actor.Size = size;
            actor.Phase = index * 0.9f;

            Button button = portrait.gameObject.AddComponent<Button>();
            button.targetGraphic = portrait;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.06f;
            button.colors = colors;
            button.onClick.AddListener(delegate { Touch(actor); });

            return actor;
        }

        static void Anchor(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        void Touch(Actor actor)
        {
            double reward = Game.Pet();
            actor.Squash = 1f;

            // 画面中央基準の座標へ直してから、ドラゴンの頭の上に数字を出す。
            float centerY = actor.Home.y - _self.rect.height * 0.5f + actor.Size * 0.95f;
            FloatingText.Spawn(this, _self, new Vector2(actor.Home.x, centerY),
                "+" + NumberFormat.Gold(reward), UIStyle.Gold, 34);
        }

        void Update()
        {
            if (_hint != null && Game != null)
            {
                bool showHint = Game.Data.pets < 5;
                if (_hint.enabled != showHint) _hint.enabled = showHint;
            }

            float time = Time.time;
            for (int i = 0; i < _actors.Count; i++)
            {
                Actor actor = _actors[i];
                if (actor.Rect == null) continue;

                float breath = Mathf.Sin(time * 1.7f + actor.Phase);
                actor.Rect.anchoredPosition = actor.Home + new Vector2(0f, breath * actor.Size * 0.035f);
                actor.Rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(time * 1.15f + actor.Phase) * 2.4f);

                if (actor.Squash > 0f)
                {
                    actor.Squash = Mathf.Max(0f, actor.Squash - Time.deltaTime * 4.5f);
                    float s = actor.Squash * actor.Squash;
                    actor.Rect.localScale = new Vector3(1f + 0.16f * s, 1f - 0.12f * s, 1f);
                }
                else if (actor.Rect.localScale != Vector3.one)
                {
                    actor.Rect.localScale = Vector3.one;
                }
            }
        }
    }
}

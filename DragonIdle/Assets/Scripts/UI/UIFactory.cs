using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>
    /// uGUI の階層をコードから組み立てるための小さな道具箱。
    /// シーンにプレハブを置かずに済むので、プロジェクトを開いてすぐ遊べる。
    /// </summary>
    public static class UIFactory
    {
        public static RectTransform Rect(GameObject go)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            return rt;
        }

        public static GameObject Node(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        /// <summary>親いっぱいに広がる矩形にする。</summary>
        public static RectTransform Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static Image Panel(string name, Transform parent, Color color, Sprite sprite = null)
        {
            GameObject go = Node(name, parent);
            UnityEngine.UI.Image image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = UnityEngine.UI.Image.Type.Sliced;
            }
            return image;
        }

        public static UnityEngine.UI.Text Label(string name, Transform parent, string content, int size,
            Color color, TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            GameObject go = Node(name, parent);
            UnityEngine.UI.Text text = go.AddComponent<UnityEngine.UI.Text>();
            text.font = UIStyle.MainFont;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = anchor;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.supportRichText = true;
            return text;
        }

        /// <summary>押せる要素。ラベルは中央に置かれ、押下で少し沈む。</summary>
        public static UnityEngine.UI.Button Button(string name, Transform parent, string caption, int fontSize,
            Color background, Color foreground, Sprite sprite = null)
        {
            UnityEngine.UI.Image image = Panel(name, parent, background, sprite != null ? sprite : UIStyle.Round16);
            UnityEngine.UI.Button button = image.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            UnityEngine.UI.Text label = Label("Label", image.transform, caption, fontSize, foreground, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(Rect(label.gameObject), 12, 4, 12, 4);
            return button;
        }

        public static VerticalLayoutGroup VerticalLayout(GameObject go, int spacing,
            RectOffset padding = null, bool controlWidth = true, bool controlHeight = true)
        {
            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding != null ? padding : new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = controlWidth;
            layout.childControlHeight = controlHeight;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
            return layout;
        }

        public static HorizontalLayoutGroup HorizontalLayout(GameObject go, int spacing,
            RectOffset padding = null, bool controlWidth = true, bool controlHeight = true)
        {
            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding != null ? padding : new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = controlWidth;
            layout.childControlHeight = controlHeight;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleLeft;
            return layout;
        }

        public static LayoutElement Sizing(GameObject go, float preferredHeight = -1, float preferredWidth = -1,
            float flexibleWidth = -1, float minHeight = -1)
        {
            LayoutElement element = go.GetComponent<LayoutElement>();
            if (element == null) element = go.AddComponent<LayoutElement>();
            if (preferredHeight >= 0) element.preferredHeight = preferredHeight;
            if (preferredWidth >= 0) element.preferredWidth = preferredWidth;
            if (flexibleWidth >= 0) element.flexibleWidth = flexibleWidth;
            if (minHeight >= 0) element.minHeight = minHeight;
            return element;
        }

        /// <summary>
        /// 縦スクロールする一覧をつくる。Viewport にマスク、Content に縦レイアウトと
        /// ContentSizeFitter を付けるのが uGUI の決まった形。
        /// </summary>
        public static RectTransform ScrollList(string name, Transform parent, int spacing, RectOffset padding,
            out ScrollRect scrollRect)
        {
            GameObject root = Node(name, parent);
            scrollRect = root.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.08f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.12f;
            scrollRect.scrollSensitivity = 40f;

            GameObject viewportGo = Node("Viewport", root.transform);
            RectTransform viewport = Stretch(Rect(viewportGo));
            viewport.pivot = new Vector2(0.5f, 1f);
            UnityEngine.UI.Image viewportImage = viewportGo.AddComponent<UnityEngine.UI.Image>();
            viewportImage.color = new Color(1, 1, 1, 0.004f); // マスクに必要な、ほぼ透明な下地
            RectMask2D mask = viewportGo.AddComponent<RectMask2D>();
            mask.softness = new Vector2Int(0, 12);

            GameObject contentGo = Node("Content", viewportGo.transform);
            RectTransform content = Rect(contentGo);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = new Vector2(0, 0);
            content.offsetMax = new Vector2(0, 0);

            VerticalLayout(contentGo, spacing, padding);
            ContentSizeFitter fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            return content;
        }

        /// <summary>細い区切り線。カードの中の情報を段に分けるのに使う。</summary>
        public static UnityEngine.UI.Image Divider(Transform parent, Color color, float height = 2f)
        {
            UnityEngine.UI.Image image = Panel("Divider", parent, color);
            Sizing(image.gameObject, height, -1, -1, height);
            return image;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>ふわりと浮かんで消える数字。なでたときの手応えを出すためのもの。</summary>
    public static class FloatingText
    {
        public static void Spawn(MonoBehaviour host, RectTransform layer, Vector2 anchoredPosition,
            string message, Color color, int fontSize = 40)
        {
            Text text = UIFactory.Label("Floating", layer, message, fontSize, color,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            RectTransform rect = UIFactory.Rect(text.gameObject);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(420, 60);
            rect.anchoredPosition = anchoredPosition;
            host.StartCoroutine(Rise(rect, text));
        }

        static IEnumerator Rise(RectTransform rect, Text text)
        {
            const float duration = 0.9f;
            float elapsed = 0f;
            Vector2 start = rect.anchoredPosition;
            float drift = Random.Range(-26f, 26f);
            Color color = text.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = start + new Vector2(drift * t, 150f * t);
                color.a = 1f - t * t;
                text.color = color;
                yield return null;
            }
            Object.Destroy(rect.gameObject);
        }
    }
}

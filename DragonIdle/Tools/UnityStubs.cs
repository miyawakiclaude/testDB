// 型チェック専用の最小スタブ。プロジェクトには含めない。
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public enum HideFlags { None, HideAndDontSave }
    public enum TextureFormat { RGBA32 }
    public enum TextureWrapMode { Clamp, Repeat }
    public enum FilterMode { Point, Bilinear, Trilinear }
    public enum SpriteMeshType { FullRect, Tight }
    public enum CameraClearFlags { Skybox, SolidColor, Depth, Nothing }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }
    public enum TextAnchor
    {
        UpperLeft, UpperCenter, UpperRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        LowerLeft, LowerCenter, LowerRight
    }
    public enum RuntimeInitializeLoadType { AfterSceneLoad, BeforeSceneLoad, BeforeSplashScreen, SubsystemRegistration }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute() { }
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) { }
    }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return new Vector2(0, 0); } }
        public static Vector2 one { get { return new Vector2(1, 1); } }
        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y); } }
        public static float Dot(Vector2 a, Vector2 b) { return a.x * b.x + a.y * b.y; }
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) { return new Vector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t); }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator *(Vector2 a, float b) { return new Vector2(a.x * b, a.y * b); }
        public static Vector2 operator *(float b, Vector2 a) { return new Vector2(a.x * b, a.y * b); }
        public static Vector2 operator /(Vector2 a, float b) { return new Vector2(a.x / b, a.y / b); }
        public static bool operator ==(Vector2 a, Vector2 b) { return a.x == b.x && a.y == b.y; }
        public static bool operator !=(Vector2 a, Vector2 b) { return !(a == b); }
        public override bool Equals(object o) { return false; }
        public override int GetHashCode() { return 0; }
        public static implicit operator Vector3(Vector2 v) { return new Vector3(v.x, v.y, 0); }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(0, 0, 0); } }
        public static Vector3 one { get { return new Vector3(1, 1, 1); } }
        public static bool operator ==(Vector3 a, Vector3 b) { return a.x == b.x && a.y == b.y && a.z == b.z; }
        public static bool operator !=(Vector3 a, Vector3 b) { return !(a == b); }
        public override bool Equals(object o) { return false; }
        public override int GetHashCode() { return 0; }
    }

    public struct Vector4
    {
        public float x, y, z, w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }

    public struct Vector2Int
    {
        public int x, y;
        public Vector2Int(int x, int y) { this.x = x; this.y = y; }
    }

    public struct Quaternion
    {
        public static Quaternion identity { get { return new Quaternion(); } }
        public static Quaternion Euler(float x, float y, float z) { return new Quaternion(); }
    }

    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; this.a = 1f; }
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white { get { return new Color(1, 1, 1, 1); } }
        public static Color black { get { return new Color(0, 0, 0, 1); } }
        public static Color clear { get { return new Color(0, 0, 0, 0); } }
        public static Color Lerp(Color a, Color b, float t) { return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t); }
        public static Color operator *(Color a, float b) { return a; }
        public static bool operator ==(Color a, Color b) { return a.r == b.r; }
        public static bool operator !=(Color a, Color b) { return !(a == b); }
        public override bool Equals(object o) { return false; }
        public override int GetHashCode() { return 0; }
        public static implicit operator Color32(Color c) { return new Color32(0, 0, 0, 0); }
    }

    public struct Color32
    {
        public byte r, g, b, a;
        public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static implicit operator Color(Color32 c) { return new Color(0, 0, 0, 0); }
    }

    public static class ColorUtility
    {
        public static string ToHtmlStringRGB(Color color) { return ""; }
    }

    public static class Mathf
    {
        public const float PI = 3.14159265f;
        public const float Deg2Rad = 0.0174532924f;
        public static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }
        public static float Clamp(float v, float a, float b) { return v < a ? a : (v > b ? b : v); }
        public static int Clamp(int v, int a, int b) { return v < a ? a : (v > b ? b : v); }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static int Min(int a, int b) { return a < b ? a : b; }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static int Max(int a, int b) { return a > b ? a : b; }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * Clamp01(t); }
        public static float InverseLerp(float a, float b, float v) { return a == b ? 0f : Clamp01((v - a) / (b - a)); }
        public static float Pow(float a, float b) { return (float)Math.Pow(a, b); }
        public static float Sqrt(float a) { return (float)Math.Sqrt(a); }
        public static float Sin(float a) { return (float)Math.Sin(a); }
        public static float Cos(float a) { return (float)Math.Cos(a); }
        public static float Atan2(float y, float x) { return (float)Math.Atan2(y, x); }
        public static float Abs(float a) { return Math.Abs(a); }
        public static float Exp(float a) { return (float)Math.Exp(a); }
        public static int RoundToInt(float a) { return (int)Math.Round(a); }
        public static int CeilToInt(float a) { return (int)Math.Ceiling(a); }
        public static int FloorToInt(float a) { return (int)Math.Floor(a); }
    }

    public static class Time
    {
        public static float deltaTime { get { return 0; } }
        public static float time { get { return 0; } }
    }

    public static class Random
    {
        public static float value { get { return 0; } }
        public static float Range(float a, float b) { return a; }
        public static int Range(int a, int b) { return a; }
    }

    public static class Debug
    {
        public static void Log(object o) { }
        public static void LogWarning(object o) { }
        public static void LogError(object o) { }
    }

    public class Object
    {
        public string name;
        public HideFlags hideFlags;
        public static void Destroy(Object o) { }
        public static void DontDestroyOnLoad(Object o) { }
        public static bool operator ==(Object a, Object b) { return ReferenceEquals(a, b); }
        public static bool operator !=(Object a, Object b) { return !ReferenceEquals(a, b); }
        public override bool Equals(object o) { return base.Equals(o); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public static implicit operator bool(Object o) { return !ReferenceEquals(o, null); }
    }

    public class Component : Object
    {
        public GameObject gameObject { get { return null; } }
        public Transform transform { get { return null; } }
        public T GetComponent<T>() where T : Component { return null; }
        public T GetComponentInChildren<T>() where T : Component { return null; }
    }

    public class Behaviour : Component { public bool enabled; }

    public class Transform : Component
    {
        public Transform parent { get { return null; } }
        public Vector3 localScale { get; set; }
        public Quaternion localRotation { get; set; }
        public void SetParent(Transform parent, bool worldPositionStays) { }
        public int childCount { get { return 0; } }
        public Transform GetChild(int index) { return null; }
        public void SetSiblingIndex(int index) { }
        public void SetAsLastSibling() { }
        public void SetAsFirstSibling() { }
    }

    public class RectTransform : Transform
    {
        public Vector2 anchorMin { get; set; }
        public Vector2 anchorMax { get; set; }
        public Vector2 pivot { get; set; }
        public Vector2 sizeDelta { get; set; }
        public Vector2 anchoredPosition { get; set; }
        public Vector2 offsetMin { get; set; }
        public Vector2 offsetMax { get; set; }
        public Rect rect { get { return new Rect(); } }
    }

    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string name) { }
        public GameObject(string name, params Type[] components) { }
        public string tag;
        public Transform transform { get { return null; } }
        public T AddComponent<T>() where T : Component { return null; }
        public T GetComponent<T>() where T : Component { return null; }
        public T GetComponentInChildren<T>() where T : Component { return null; }
        public void SetActive(bool value) { }
        public bool activeSelf { get { return true; } }
    }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine) { return null; }
        public void StopCoroutine(Coroutine routine) { }
        public void StopAllCoroutines() { }
    }

    public class Coroutine { }

    public class Texture : Object { public TextureWrapMode wrapMode; public FilterMode filterMode; }

    public class Texture2D : Texture
    {
        public Texture2D(int width, int height) { }
        public Texture2D(int width, int height, TextureFormat format, bool mipChain) { }
        public void SetPixels32(Color32[] colors) { }
        public void Apply() { }
    }

    public class Sprite : Object
    {
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot) { return null; }
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit,
            uint extrude, SpriteMeshType meshType) { return null; }
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit,
            uint extrude, SpriteMeshType meshType, Vector4 border) { return null; }
    }

    public class Font : Object
    {
        public static string[] GetOSInstalledFontNames() { return new string[0]; }
        public static Font CreateDynamicFontFromOSFont(string fontname, int size) { return null; }
        public static Font CreateDynamicFontFromOSFont(string[] fontnames, int size) { return null; }
    }

    public static class Resources
    {
        public static T GetBuiltinResource<T>(string path) where T : Object { return null; }
    }

    public static class PlayerPrefs
    {
        static readonly Dictionary<string, string> Strings = new Dictionary<string, string>();
        static readonly Dictionary<string, int> Ints = new Dictionary<string, int>();
        public static string GetString(string key, string def)
        {
            string value;
            return Strings.TryGetValue(key, out value) ? value : def;
        }
        public static void SetString(string key, string value) { Strings[key] = value; }
        public static int GetInt(string key, int def)
        {
            int value;
            return Ints.TryGetValue(key, out value) ? value : def;
        }
        public static void SetInt(string key, int value) { Ints[key] = value; }
        public static void Save() { }
        public static void DeleteKey(string key) { Strings.Remove(key); Ints.Remove(key); }
    }

    public static class JsonUtility
    {
        public static string ToJson(object obj) { return ""; }
        public static T FromJson<T>(string json) { return default(T); }
    }

    public class Camera : Behaviour
    {
        public static Camera main { get { return null; } }
        public CameraClearFlags clearFlags { get; set; }
        public Color backgroundColor { get; set; }
        public bool orthographic { get; set; }
    }

    public class AudioClip : Object
    {
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream) { return null; }
        public bool SetData(float[] data, int offsetSamples) { return true; }
    }

    public class AudioSource : Behaviour
    {
        public bool playOnAwake { get; set; }
        public float spatialBlend { get; set; }
        public float volume { get; set; }
        public float pitch { get; set; }
        public void PlayOneShot(AudioClip clip) { }
    }

    public class AudioListener : Behaviour { }

    public class Canvas : Behaviour
    {
        public RenderMode renderMode { get; set; }
        public bool pixelPerfect { get; set; }
    }

    public class RectOffset
    {
        public RectOffset() { }
        public RectOffset(int left, int right, int top, int bottom) { }
    }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public class UnityEvent
    {
        public void AddListener(UnityAction call) { }
        public void RemoveListener(UnityAction call) { }
    }
}

namespace UnityEngine.UI
{
    using UnityEngine.Events;

    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }

    public class Graphic : Behaviour
    {
        public Color color { get; set; }
        public bool raycastTarget { get; set; }
    }

    public class MaskableGraphic : Graphic { }

    public class Image : MaskableGraphic
    {
        public enum Type { Simple, Sliced, Tiled, Filled }
        public Sprite sprite { get; set; }
        public Type type { get; set; }
        public bool preserveAspect { get; set; }
    }

    public class Text : MaskableGraphic
    {
        public string text { get; set; }
        public Font font { get; set; }
        public int fontSize { get; set; }
        public TextAnchor alignment { get; set; }
        public FontStyle fontStyle { get; set; }
        public HorizontalWrapMode horizontalOverflow { get; set; }
        public VerticalWrapMode verticalOverflow { get; set; }
        public bool supportRichText { get; set; }
    }

    public struct ColorBlock
    {
        public Color normalColor, highlightedColor, pressedColor, selectedColor, disabledColor;
        public float colorMultiplier, fadeDuration;
    }

    public class Selectable : Behaviour
    {
        public enum Transition { None, ColorTint, SpriteSwap, Animation }
        public Transition transition { get; set; }
        public Graphic targetGraphic { get; set; }
        public bool interactable { get; set; }
        public ColorBlock colors { get; set; }
    }

    public class Button : Selectable
    {
        public class ButtonClickedEvent : UnityEvent { }
        public ButtonClickedEvent onClick { get { return null; } }
    }

    public class LayoutGroup : Behaviour
    {
        public RectOffset padding { get; set; }
        public TextAnchor childAlignment { get; set; }
    }

    public class HorizontalOrVerticalLayoutGroup : LayoutGroup
    {
        public float spacing { get; set; }
        public bool childControlWidth { get; set; }
        public bool childControlHeight { get; set; }
        public bool childForceExpandWidth { get; set; }
        public bool childForceExpandHeight { get; set; }
    }

    public class VerticalLayoutGroup : HorizontalOrVerticalLayoutGroup { }
    public class HorizontalLayoutGroup : HorizontalOrVerticalLayoutGroup { }

    public class LayoutElement : Behaviour
    {
        public float preferredWidth { get; set; }
        public float preferredHeight { get; set; }
        public float flexibleWidth { get; set; }
        public float flexibleHeight { get; set; }
        public float minWidth { get; set; }
        public float minHeight { get; set; }
    }

    public class ContentSizeFitter : Behaviour
    {
        public enum FitMode { Unconstrained, MinSize, PreferredSize }
        public FitMode horizontalFit { get; set; }
        public FitMode verticalFit { get; set; }
    }

    public class ScrollRect : Behaviour
    {
        public enum MovementType { Unrestricted, Elastic, Clamped }
        public bool horizontal { get; set; }
        public bool vertical { get; set; }
        public MovementType movementType { get; set; }
        public float elasticity { get; set; }
        public bool inertia { get; set; }
        public float decelerationRate { get; set; }
        public float scrollSensitivity { get; set; }
        public RectTransform viewport { get; set; }
        public RectTransform content { get; set; }
    }

    public class RectMask2D : Behaviour { public Vector2Int softness { get; set; } }
    public class Mask : Behaviour { public bool showMaskGraphic { get; set; } }

    public class CanvasScaler : Behaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public enum ScreenMatchMode { MatchWidthOrHeight, Expand, Shrink }
        public ScaleMode uiScaleMode { get; set; }
        public Vector2 referenceResolution { get; set; }
        public ScreenMatchMode screenMatchMode { get; set; }
        public float matchWidthOrHeight { get; set; }
    }

    public class GraphicRaycaster : Behaviour { }
}

namespace UnityEngine.EventSystems
{
    public class EventSystem : Behaviour { public static EventSystem current { get { return null; } } }
    public class StandaloneInputModule : Behaviour { }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { public string name; }
}

namespace UnityEditor
{
    using UnityEngine;
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MenuItemAttribute : Attribute
    {
        public MenuItemAttribute(string path) { }
        public MenuItemAttribute(string path, bool validate) { }
        public MenuItemAttribute(string path, bool validate, int priority) { }
    }
    public static class AssetDatabase
    {
        public static bool IsValidFolder(string path) { return false; }
        public static string CreateFolder(string parent, string name) { return ""; }
        public static void SaveAssets() { }
    }
    public static class EditorUtility
    {
        public static bool DisplayDialog(string title, string message, string ok, string cancel) { return false; }
    }
    public class EditorBuildSettingsScene
    {
        public EditorBuildSettingsScene(string path, bool enabled) { }
    }
    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes { get; set; }
    }
}

namespace UnityEditor.SceneManagement
{
    using UnityEngine.SceneManagement;
    public enum NewSceneSetup { EmptyScene, DefaultGameObjects }
    public enum NewSceneMode { Single, Additive }
    public static class EditorSceneManager
    {
        public static Scene NewScene(NewSceneSetup setup, NewSceneMode mode) { return new Scene(); }
        public static bool SaveScene(Scene scene, string path) { return true; }
        public static bool SaveCurrentModifiedScenesIfUserWantsTo() { return true; }
    }
}

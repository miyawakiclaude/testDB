using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonIdle
{
    /// <summary>
    /// 空のシーンで再生を押すだけでゲームが立ち上がるようにする。
    /// シーンにプレハブを置く必要がないので、プロジェクトを開いた直後から遊べる。
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            if (GameManager.Instance != null) return;

            EnsureCamera();
            EnsureEventSystem();

            GameObject managerGo = new GameObject("GameManager");
            managerGo.AddComponent<GameManager>();

            CreateCanvas();
        }

        static void EnsureCamera()
        {
            if (Camera.main != null)
            {
                // 既存のカメラに聞き手がなければ足す。無いと効果音が鳴らない。
                if (Camera.main.GetComponent<AudioListener>() == null)
                {
                    Camera.main.gameObject.AddComponent<AudioListener>();
                }
                return;
            }

            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = UIStyle.BgDeep;
            camera.orthographic = true;
            cameraGo.AddComponent<AudioListener>();
        }

        static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            GameObject eventGo = new GameObject("EventSystem");
            eventGo.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            eventGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventGo.AddComponent<StandaloneInputModule>();
#endif
        }

        static void CreateCanvas()
        {
            GameObject canvasGo = new GameObject("GameCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UIStyle.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<GameUI>();
            Object.DontDestroyOnLoad(canvasGo);
        }
    }
}

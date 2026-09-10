using UnityEngine;

namespace DragonIdle
{
    /// <summary>
    /// 見た目の決め事をまとめた場所。色・フォント・角丸スプライトはすべてここから取る。
    /// 竜の巣穴を想定した暗い背景に、金貨の金と竜魂の紫を効かせる。
    /// </summary>
    public static class UIStyle
    {
        public static readonly Color BgDeep = new Color32(0x12, 0x10, 0x1B, 0xFF);
        public static readonly Color BgPanel = new Color32(0x1E, 0x1B, 0x2E, 0xFF);
        public static readonly Color BgPanel2 = new Color32(0x27, 0x23, 0x38, 0xFF);
        public static readonly Color BgPanel3 = new Color32(0x33, 0x2E, 0x47, 0xFF);
        public static readonly Color Line = new Color32(0x3D, 0x37, 0x55, 0xFF);

        public static readonly Color Gold = new Color32(0xF2, 0xC1, 0x4E, 0xFF);
        public static readonly Color GoldDim = new Color32(0x8A, 0x6E, 0x2C, 0xFF);
        public static readonly Color Soul = new Color32(0xB3, 0x88, 0xFF, 0xFF);
        public static readonly Color Positive = new Color32(0x6B, 0xD9, 0x8F, 0xFF);
        public static readonly Color Danger = new Color32(0xE8, 0x6A, 0x6A, 0xFF);

        public static readonly Color Text = new Color32(0xED, 0xE9, 0xF5, 0xFF);
        public static readonly Color TextDim = new Color32(0xA7, 0x9F, 0xC0, 0xFF);
        public static readonly Color TextFaint = new Color32(0x6E, 0x66, 0x8C, 0xFF);
        public static readonly Color TextOnGold = new Color32(0x1A, 0x14, 0x05, 0xFF);
        public static readonly Color Disabled = new Color32(0x2C, 0x28, 0x3E, 0xFF);

        /// <summary>設計上の基準解像度。縦持ちのスマホを想定している。</summary>
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        static UnityEngine.Font _mainFont;

        /// <summary>
        /// 日本語を出せるフォントが見つかったか。見つからないまま組み込みフォントに落ちた場合、
        /// 日本語はすべて豆腐になるので、画面にその旨を出して原因をわかるようにする。
        /// </summary>
        public static bool JapaneseFontFound { get; private set; }

        /// <summary>実際に使っているフォント名。見つからなければ空。</summary>
        public static string ResolvedFontName { get; private set; }

        /// <summary>
        /// 日本語が出せる OS フォントを探す。見つからなければ組み込みフォントに落とす
        /// （その場合、日本語は豆腐になるので README の手順でフォントを入れる）。
        /// </summary>
        public static UnityEngine.Font MainFont
        {
            get
            {
                if (_mainFont != null) return _mainFont;

                string[] preferred =
                {
                    "Yu Gothic UI", "Yu Gothic", "YuGothic", "Meiryo", "MS Gothic", "MS PGothic",
                    "Hiragino Sans", "Hiragino Kaku Gothic ProN", "Hiragino Kaku Gothic Pro",
                    "Noto Sans CJK JP", "Noto Sans JP", "Source Han Sans", "IPAexGothic",
                    "IPAGothic", "TakaoGothic", "VL Gothic", "Arial Unicode MS"
                };

                string[] installed;
                try { installed = UnityEngine.Font.GetOSInstalledFontNames(); }
                catch { installed = new string[0]; }

                string match = FindFont(installed, preferred);
                if (match == null) match = FindByKeyword(installed);

                if (match != null)
                {
                    _mainFont = UnityEngine.Font.CreateDynamicFontFromOSFont(match, 40);
                    if (_mainFont != null)
                    {
                        JapaneseFontFound = true;
                        ResolvedFontName = match;
                        return _mainFont;
                    }
                }

                JapaneseFontFound = false;
                ResolvedFontName = "";
                Debug.LogWarning(
                    "日本語を出せるフォントが見つかりませんでした。文字が豆腐になります。" +
                    "Assets/Fonts/ の手順に従って日本語フォントを入れ、UIStyle.MainFont が" +
                    "それを返すようにしてください。");
                _mainFont = BuiltinFallback();
                if (_mainFont == null) _mainFont = UnityEngine.Font.CreateDynamicFontFromOSFont("Arial", 40);
                return _mainFont;
            }
        }

        static string FindFont(string[] installed, string[] preferred)
        {
            for (int p = 0; p < preferred.Length; p++)
            {
                for (int i = 0; i < installed.Length; i++)
                {
                    if (string.Equals(installed[i], preferred[p], System.StringComparison.OrdinalIgnoreCase))
                        return installed[i];
                }
            }
            // 完全一致がなければ前方一致（"Yu Gothic Bold" のような派生名を拾う）
            for (int p = 0; p < preferred.Length; p++)
            {
                for (int i = 0; i < installed.Length; i++)
                {
                    if (installed[i].StartsWith(preferred[p], System.StringComparison.OrdinalIgnoreCase))
                        return installed[i];
                }
            }
            return null;
        }

        static string FindByKeyword(string[] installed)
        {
            string[] keywords = { "Gothic", "ゴシック", "Mincho", "明朝", "PingFang", "Hiragino", "Noto Sans CJK" };
            for (int k = 0; k < keywords.Length; k++)
            {
                for (int i = 0; i < installed.Length; i++)
                {
                    if (installed[i].IndexOf(keywords[k], System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return installed[i];
                }
            }
            return null;
        }

        static UnityEngine.Font BuiltinFallback()
        {
            string[] names = { "LegacyRuntime.ttf", "Arial.ttf" };
            for (int i = 0; i < names.Length; i++)
            {
                try
                {
                    UnityEngine.Font f = Resources.GetBuiltinResource<UnityEngine.Font>(names[i]);
                    if (f != null) return f;
                }
                catch { /* 次の候補へ */ }
            }
            return null;
        }

        // ---------- 角丸スプライト ----------

        static Sprite _round8, _round16, _round28, _circle;

        public static Sprite Round8 { get { if (_round8 == null) _round8 = BuildRounded(8); return _round8; } }
        public static Sprite Round16 { get { if (_round16 == null) _round16 = BuildRounded(16); return _round16; } }
        public static Sprite Round28 { get { if (_round28 == null) _round28 = BuildRounded(28); return _round28; } }

        public static Sprite Circle
        {
            get
            {
                if (_circle == null)
                {
                    int size = 128;
                    Texture2D tex = NewTexture(size, size);
                    float r = size * 0.5f;
                    Color32[] pixels = new Color32[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = x + 0.5f - r;
                            float dy = y + 0.5f - r;
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            float a = Mathf.Clamp01(r - d);
                            pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                        }
                    }
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    _circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                        100f, 0, SpriteMeshType.FullRect);
                }
                return _circle;
            }
        }

        static Sprite _egg;
        static Sprite _burst;

        /// <summary>孵化演出に使う卵。上がやや細い実際の卵の形で、斑点を焼き込んである。</summary>
        public static Sprite Egg
        {
            get
            {
                if (_egg != null) return _egg;

                const int width = 128, height = 160;
                Texture2D tex = NewTexture(width, height);
                Color32[] pixels = new Color32[width * height];
                System.Random rng = new System.Random(20260910);

                // 斑点の位置を先に決めておく
                const int speckCount = 26;
                float[] sx = new float[speckCount], sy = new float[speckCount], sr = new float[speckCount];
                for (int i = 0; i < speckCount; i++)
                {
                    sx[i] = (float)rng.NextDouble() * 2f - 1f;
                    sy[i] = (float)rng.NextDouble() * 2f - 1f;
                    sr[i] = 0.05f + (float)rng.NextDouble() * 0.06f;
                }

                for (int y = 0; y < height; y++)
                {
                    float v = (y + 0.5f) / height * 2f - 1f;
                    for (int x = 0; x < width; x++)
                    {
                        float u = (x + 0.5f) / width * 2f - 1f;

                        // 上へ行くほど細くなる楕円
                        float halfWidth = Mathf.Sqrt(Mathf.Max(0f, 1f - v * v)) * (1f - 0.22f * v);
                        float alpha = Mathf.Clamp01((halfWidth - Mathf.Abs(u)) * width * 0.5f);
                        if (alpha <= 0f) { pixels[y * width + x] = new Color32(0, 0, 0, 0); continue; }

                        // 左上からの当たり光
                        float shade = Mathf.Clamp01(0.62f + 0.38f * (0.5f - u * 0.55f + v * 0.35f));
                        Color color = Color.Lerp(new Color(0.80f, 0.75f, 0.66f), new Color(1f, 0.98f, 0.93f), shade);

                        for (int i = 0; i < speckCount; i++)
                        {
                            float d = Mathf.Sqrt((u - sx[i]) * (u - sx[i]) + (v - sy[i]) * (v - sy[i]));
                            if (d < sr[i]) color = Color.Lerp(color, new Color(0.62f, 0.55f, 0.47f), 0.55f);
                        }

                        color.a = alpha;
                        pixels[y * width + x] = color;
                    }
                }

                tex.SetPixels32(pixels);
                tex.Apply();
                _egg = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f),
                    100f, 0, SpriteMeshType.FullRect);
                return _egg;
            }
        }

        /// <summary>孵化の瞬間に背後で回る光芒。中心ほど濃く、外へ向かって消える。</summary>
        public static Sprite Burst
        {
            get
            {
                if (_burst != null) return _burst;

                const int size = 256;
                const int rays = 16;
                Texture2D tex = NewTexture(size, size);
                Color32[] pixels = new Color32[size * size];
                float center = size * 0.5f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x + 0.5f - center;
                        float dy = y + 0.5f - center;
                        float radius = Mathf.Sqrt(dx * dx + dy * dy) / center;
                        if (radius > 1f) { pixels[y * size + x] = new Color32(0, 0, 0, 0); continue; }

                        float angle = Mathf.Atan2(dy, dx);
                        float wedge = Mathf.Sin(angle * rays) * 0.5f + 0.5f;
                        float sharpness = Mathf.Pow(wedge, 2.2f);
                        float falloff = Mathf.Clamp01(1f - radius) * Mathf.Clamp01(radius * 6f);
                        float alpha = sharpness * falloff;
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(alpha) * 255f));
                    }
                }

                tex.SetPixels32(pixels);
                tex.Apply();
                _burst = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                    100f, 0, SpriteMeshType.FullRect);
                return _burst;
            }
        }

        static Sprite _caveGradient;

        /// <summary>巣の情景に敷く縦グラデーション。上ほど暗く、下の床に向かって明るむ。</summary>
        public static Sprite CaveGradient
        {
            get
            {
                if (_caveGradient == null)
                {
                    const int height = 128;
                    Texture2D tex = NewTexture(4, height);
                    Color top = new Color32(0x0C, 0x0A, 0x14, 0xFF);
                    Color bottom = new Color32(0x2A, 0x22, 0x3E, 0xFF);
                    Color32[] pixels = new Color32[4 * height];
                    for (int y = 0; y < height; y++)
                    {
                        float t = y / (height - 1f);
                        Color color = Color.Lerp(top, bottom, Mathf.Pow(t, 1.6f));
                        for (int x = 0; x < 4; x++) pixels[y * 4 + x] = color;
                    }
                    tex.SetPixels32(pixels);
                    tex.Apply();
                    _caveGradient = Sprite.Create(tex, new Rect(0, 0, 4, height),
                        new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                }
                return _caveGradient;
            }
        }

        /// <summary>9スライス用の角丸矩形。どんな大きさに引き伸ばしても角の丸みが崩れない。</summary>
        static Sprite BuildRounded(int radius)
        {
            int size = radius * 2 + 4;
            Texture2D tex = NewTexture(size, size);
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float cx = Mathf.Clamp(px, radius, size - radius);
                    float cy = Mathf.Clamp(py, radius, size - radius);
                    float d = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                    float a = Mathf.Clamp01(radius - d + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
        }

        static Texture2D NewTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }
    }
}

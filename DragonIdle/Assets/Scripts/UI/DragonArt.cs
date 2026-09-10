using System.Collections.Generic;
using UnityEngine;

namespace DragonIdle
{
    /// <summary>
    /// 種族ごとのドラゴン像を実行時に描く。画像アセットを持たずに35種を描き分けるため、
    /// 体を「太みのある線分」の集まりとして距離場で塗り、翼は骨と膜に分けて張る。
    /// ティアが上がるほど首が伸び、翼が広がり、角と背びれが増える。
    /// </summary>
    public static class DragonArt
    {
        const int Size = 192;
        const float OutlineWidth = 0.011f;
        const float Margin = 0.045f;

        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite For(DragonSpecies species)
        {
            Sprite cached;
            if (Cache.TryGetValue(species.Id, out cached)) return cached;

            Texture2D texture = Paint(species);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            Cache[species.Id] = sprite;
            return sprite;
        }

        /// <summary>太みが端から端へ変化する線分。体はこれの集まりでできている。</summary>
        struct Limb
        {
            public Vector2 A, B;
            public float RadiusA, RadiusB;
        }

        static Limb Bone(float ax, float ay, float bx, float by, float ra, float rb)
        {
            Limb limb;
            limb.A = new Vector2(ax, ay);
            limb.B = new Vector2(bx, by);
            limb.RadiusA = ra;
            limb.RadiusB = rb;
            return limb;
        }

        static Texture2D Paint(DragonSpecies species)
        {
            // 同じ種族なら毎回同じ姿になるよう、種族IDから乱数の種をつくる。
            int seed = 0;
            for (int i = 0; i < species.Id.Length; i++) seed = seed * 131 + species.Id[i];
            System.Random rng = new System.Random(seed);

            float t = (species.Tier - 1) / 4f;
            float headScale = Mathf.Lerp(1.30f, 0.88f, t);   // 幼いほど頭が大きい
            float neckLength = Mathf.Lerp(0.05f, 0.21f, t);
            float wingSpan = Mathf.Lerp(0.26f, 0.52f, t);
            float bodyRadius = Mathf.Lerp(0.140f, 0.175f, t);
            float tailLength = Mathf.Lerp(0.90f, 1.15f, t);
            int hornCount = Mathf.Clamp(species.Tier - 1, 0, 4);
            bool hasSpikes = species.Tier >= 3;

            float jitter = (float)rng.NextDouble() * 2f - 1f;
            float wingLift = 1f + jitter * 0.06f;
            float snoutLength = 0.105f + (float)rng.NextDouble() * 0.035f;
            float tailCurl = jitter * 0.04f;

            float bcx = 0.44f, bcy = 0.40f;
            float nbx = bcx + 0.055f, nby = bcy + bodyRadius * 0.72f;
            float hcx = nbx + 0.10f, hcy = nby + neckLength;

            // 尾: 付け根から一度下がり、先で跳ね上がる曲線を折れ線で近似する。
            List<Limb> body = new List<Limb>();
            Vector2 previous = TailPoint(0f, bcx, bcy, tailLength, tailCurl);
            float previousRadius = bodyRadius * 0.62f;
            for (int i = 1; i <= 4; i++)
            {
                float u = i / 4f;
                Vector2 point = TailPoint(u, bcx, bcy, tailLength, tailCurl);
                float radius = Mathf.Lerp(bodyRadius * 0.62f, 0.010f, Mathf.Pow(u, 0.85f));
                body.Add(Bone(previous.x, previous.y, point.x, point.y, previousRadius, radius));
                previous = point;
                previousRadius = radius;
            }

            body.Add(Bone(bcx - 0.03f, bcy - 0.05f, bcx - 0.075f, 0.185f, 0.062f, 0.034f));   // 後脚
            body.Add(Bone(bcx - 0.075f, 0.185f, bcx + 0.015f, 0.150f, 0.034f, 0.027f));
            body.Add(Bone(bcx + 0.075f, bcy - 0.05f, bcx + 0.095f, 0.180f, 0.046f, 0.028f));  // 前脚
            body.Add(Bone(bcx + 0.095f, 0.180f, bcx + 0.165f, 0.150f, 0.028f, 0.022f));
            body.Add(Bone(bcx - 0.065f, bcy, bcx + 0.055f, bcy + 0.025f, bodyRadius, bodyRadius * 0.86f));
            body.Add(Bone(nbx, nby, hcx, hcy, 0.072f, 0.055f));                                // 首
            body.Add(Bone(hcx, hcy, hcx + 0.025f, hcy + 0.004f, 0.078f * headScale, 0.074f * headScale));
            body.Add(Bone(hcx + 0.035f, hcy - 0.010f, hcx + snoutLength + 0.02f, hcy - 0.034f,
                0.050f * headScale, 0.028f * headScale));                                      // 鼻先

            // 角は象牙色、背びれは体色を落とした色で塗り分ける。
            List<Limb> horns = new List<Limb>();
            for (int i = 0; i < hornCount; i++)
            {
                float spread = hornCount == 1 ? 0f : (i / (hornCount - 1f)) - 0.5f;
                float bx = hcx - 0.052f + spread * 0.042f;
                float by = hcy + 0.042f * headScale;
                horns.Add(Bone(bx, by, bx - 0.062f - Mathf.Abs(spread) * 0.022f,
                    by + 0.062f + t * 0.022f - Mathf.Abs(spread) * 0.024f, 0.026f, 0.007f));
            }

            List<Limb> spikes = new List<Limb>();
            if (hasSpikes)
            {
                for (int i = 0; i < 5; i++)
                {
                    float u = i / 4f;
                    float x = Mathf.Lerp(bcx - 0.075f, nbx, u);
                    float y = Mathf.Lerp(bcy + bodyRadius * 0.96f, nby + 0.030f, u);
                    spikes.Add(Bone(x, y, x - 0.018f, y + 0.062f + u * 0.024f, 0.026f, 0.005f));
                }
            }

            // 翼: 肩から扇状に骨を伸ばし、骨の間に膜を張る。
            Vector2 shoulder = new Vector2(bcx + 0.005f, bcy + bodyRadius * 0.66f);
            float[] angles = { 104f, 138f, 170f, 199f };
            float[] reach = { 1.00f, 0.99f, 0.88f, 0.64f };
            Vector2[] tips = new Vector2[4];
            List<Limb> fingers = new List<Limb>();
            for (int i = 0; i < 4; i++)
            {
                float radians = angles[i] * Mathf.Deg2Rad;
                float length = wingSpan * reach[i] * (i < 2 ? wingLift : 1f);
                tips[i] = shoulder + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * length;
                fingers.Add(Bone(shoulder.x, shoulder.y, tips[i].x, tips[i].y, i == 0 ? 0.020f : 0.014f, 0.005f));
            }

            // 種族ごとに大きさがばらつかないよう、全体を枠に合わせて拡大縮小する。
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            Bounds2(body, ref minX, ref minY, ref maxX, ref maxY);
            Bounds2(horns, ref minX, ref minY, ref maxX, ref maxY);
            Bounds2(spikes, ref minX, ref minY, ref maxX, ref maxY);
            Bounds2(fingers, ref minX, ref minY, ref maxX, ref maxY);
            for (int i = 0; i < tips.Length; i++)
            {
                minX = Mathf.Min(minX, tips[i].x); maxX = Mathf.Max(maxX, tips[i].x);
                minY = Mathf.Min(minY, tips[i].y); maxY = Mathf.Max(maxY, tips[i].y);
            }

            float spanX = Mathf.Max(maxX - minX, 1e-6f);
            float spanY = Mathf.Max(maxY - minY, 1e-6f);
            float scale = Mathf.Min((1f - 2f * Margin) / spanX, (1f - 2f * Margin) / spanY);
            Vector2 offset = new Vector2(0.5f - (minX + maxX) * 0.5f * scale, 0.5f - (minY + maxY) * 0.5f * scale);

            Fit(body, scale, offset);
            Fit(horns, scale, offset);
            Fit(spikes, scale, offset);
            Fit(fingers, scale, offset);
            for (int i = 0; i < tips.Length; i++) tips[i] = tips[i] * scale + offset;
            shoulder = shoulder * scale + offset;

            Vector2 belly = new Vector2(bcx + 0.015f, bcy - 0.050f) * scale + offset;
            Vector2 bellyRadius = new Vector2(0.100f, 0.070f) * scale;
            Vector2 eye = new Vector2(hcx + 0.034f * headScale, hcy + 0.020f * headScale) * scale + offset;
            float eyeRadius = 0.029f * headScale * scale;
            float bodyCenterY = bcy * scale + offset.y;

            // 影は黒ではなく洞窟の暗がりへ落とす。淡い属性でも濁らず、巣の背景ともなじむ。
            Color shadow = new Color(0.078f, 0.063f, 0.141f);
            Color elementColor = Elements.Tint(species.Element);
            Color bodyTop = Color.Lerp(elementColor, Color.white, 0.16f);
            Color bodyBottom = Color.Lerp(elementColor, shadow, 0.50f);
            Color bellyColor = Color.Lerp(elementColor, new Color(1f, 0.97f, 0.88f), 0.60f);
            Color wingColor = Color.Lerp(elementColor, shadow, 0.62f);
            Color wingInner = Color.Lerp(elementColor, shadow, 0.34f);
            Color boneColor = Color.Lerp(elementColor, new Color(1f, 0.96f, 0.86f), 0.72f);
            Color spikeColor = Color.Lerp(elementColor, shadow, 0.34f);
            Color outline = Color.Lerp(elementColor, shadow, 0.86f);

            Color32[] pixels = new Color32[Size * Size];

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    Vector2 p = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size);
                    float alpha = 0f;
                    Color color = Color.clear;

                    // 奥から手前へ: 膜 → 翼の骨 → 背びれ → 角 → 体
                    Layer(Membrane(p, shoulder, tips), wingColor, outline, ref alpha, ref color);
                    Layer(Nearest(p, fingers), wingInner, outline, ref alpha, ref color);
                    if (spikes.Count > 0) Layer(Nearest(p, spikes), spikeColor, outline, ref alpha, ref color);
                    if (horns.Count > 0) Layer(Nearest(p, horns), boneColor, outline, ref alpha, ref color);

                    float bodyD = Nearest(p, body);
                    float coverage = Edge(bodyD);
                    if (coverage > 0f)
                    {
                        float vertical = Mathf.InverseLerp(bodyCenterY - 0.24f * scale, bodyCenterY + 0.34f * scale, p.y);
                        Color skin = Color.Lerp(bodyBottom, bodyTop, vertical);
                        float bellyMask = 1f - Mathf.Clamp01(Ellipse(p, belly, bellyRadius) * 22f / scale);
                        skin = Color.Lerp(skin, bellyColor, bellyMask * 0.82f);
                        float shade = bodyD > -OutlineWidth ? Mathf.Clamp01((bodyD + OutlineWidth) / OutlineWidth) : 0f;
                        skin = Color.Lerp(skin, outline, shade);
                        color = Color.Lerp(color, skin, coverage);
                        alpha = Mathf.Max(alpha, coverage);
                    }

                    if (alpha <= 0.01f)
                    {
                        pixels[y * Size + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }
                    color.a = alpha;
                    pixels[y * Size + x] = color;
                }
            }

            PaintEye(pixels, eye, eyeRadius, species.Tier);

            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        static Vector2 TailPoint(float u, float bcx, float bcy, float tailLength, float tailCurl)
        {
            float x = bcx - 0.06f - 0.40f * tailLength * u;
            float y = bcy - 0.02f - 0.10f * Mathf.Sin(u * 2.2f) + 0.30f * u * u + tailCurl * u;
            return new Vector2(x, y);
        }

        static void Bounds2(List<Limb> limbs, ref float minX, ref float minY, ref float maxX, ref float maxY)
        {
            for (int i = 0; i < limbs.Count; i++)
            {
                Limb limb = limbs[i];
                minX = Mathf.Min(minX, Mathf.Min(limb.A.x - limb.RadiusA, limb.B.x - limb.RadiusB));
                maxX = Mathf.Max(maxX, Mathf.Max(limb.A.x + limb.RadiusA, limb.B.x + limb.RadiusB));
                minY = Mathf.Min(minY, Mathf.Min(limb.A.y - limb.RadiusA, limb.B.y - limb.RadiusB));
                maxY = Mathf.Max(maxY, Mathf.Max(limb.A.y + limb.RadiusA, limb.B.y + limb.RadiusB));
            }
        }

        static void Fit(List<Limb> limbs, float scale, Vector2 offset)
        {
            for (int i = 0; i < limbs.Count; i++)
            {
                Limb limb = limbs[i];
                limb.A = limb.A * scale + offset;
                limb.B = limb.B * scale + offset;
                limb.RadiusA *= scale;
                limb.RadiusB *= scale;
                limbs[i] = limb;
            }
        }

        // ---------- 塗りの道具 ----------

        /// <summary>縁を1画素ぶんだけぼかして、輪郭のギザつきを消す。</summary>
        static float Edge(float distance)
        {
            return Mathf.Clamp01(0.5f - distance * Size);
        }

        /// <summary>形の内側は指定色、縁の内側は輪郭色。手前の層ほど後から重ねる。</summary>
        static void Layer(float distance, Color baseColor, Color outline, ref float alpha, ref Color color)
        {
            float coverage = Edge(distance);
            if (coverage <= 0f) return;
            float shade = distance > -OutlineWidth ? Mathf.Clamp01((distance + OutlineWidth) / OutlineWidth) : 0f;
            color = Color.Lerp(color, Color.Lerp(baseColor, outline, shade), coverage);
            alpha = Mathf.Max(alpha, coverage);
        }

        static float Nearest(Vector2 p, List<Limb> limbs)
        {
            float best = float.MaxValue;
            for (int i = 0; i < limbs.Count; i++) best = Mathf.Min(best, Distance(p, limbs[i]));
            return best;
        }

        static float Distance(Vector2 p, Limb limb)
        {
            Vector2 ab = limb.B - limb.A;
            float lengthSquared = Vector2.Dot(ab, ab);
            float h = lengthSquared <= 1e-8f ? 0f : Mathf.Clamp01(Vector2.Dot(p - limb.A, ab) / lengthSquared);
            float radius = Mathf.Lerp(limb.RadiusA, limb.RadiusB, h);
            return (p - (limb.A + ab * h)).magnitude - radius;
        }

        static float Ellipse(Vector2 p, Vector2 center, Vector2 radius)
        {
            Vector2 q = new Vector2((p.x - center.x) / radius.x, (p.y - center.y) / radius.y);
            return (q.magnitude - 1f) * Mathf.Min(radius.x, radius.y);
        }

        /// <summary>骨の間に張った膜。後縁を丸くえぐって竜の翼らしくする。</summary>
        static float Membrane(Vector2 p, Vector2 shoulder, Vector2[] tips)
        {
            float d = float.MaxValue;
            for (int i = 0; i < tips.Length - 1; i++)
            {
                d = Mathf.Min(d, Triangle(p, shoulder, tips[i], tips[i + 1]));
            }
            for (int i = 0; i < tips.Length - 1; i++)
            {
                Vector2 mid = (tips[i] + tips[i + 1]) * 0.5f;
                Vector2 v = mid - shoulder;
                float length = v.magnitude;
                if (length <= 1e-6f) continue;
                float chord = (tips[i + 1] - tips[i]).magnitude;
                Vector2 center = shoulder + v / length * (length + chord * 0.44f);
                d = Mathf.Max(d, -((p - center).magnitude - chord * 0.60f));
            }
            return d;
        }

        static float Triangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            // 頂点の並び順に関わらず内側が負になるよう、向きを揃えてから半平面の最大をとる。
            float area = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            if (area < 0f) { Vector2 swap = b; b = c; c = swap; }
            float d = HalfPlane(p, a, b);
            d = Mathf.Max(d, HalfPlane(p, b, c));
            d = Mathf.Max(d, HalfPlane(p, c, a));
            return d;
        }

        static float HalfPlane(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 e = b - a;
            Vector2 n = new Vector2(e.y, -e.x);
            float length = n.magnitude;
            if (length <= 1e-8f) return 0f;
            return Vector2.Dot(p - a, n / length);
        }

        static void PaintEye(Color32[] pixels, Vector2 center, float radius, int tier)
        {
            Color sclera = tier >= 4 ? new Color(1f, 0.94f, 0.62f) : new Color(0.98f, 0.97f, 0.95f);
            Color pupil = new Color(0.08f, 0.06f, 0.12f);

            int cx = Mathf.RoundToInt(center.x * Size);
            int cy = Mathf.RoundToInt(center.y * Size);
            int r = Mathf.CeilToInt(radius * Size) + 2;

            for (int y = cy - r; y <= cy + r; y++)
            {
                if (y < 0 || y >= Size) continue;
                for (int x = cx - r; x <= cx + r; x++)
                {
                    if (x < 0 || x >= Size) continue;
                    int index = y * Size + x;
                    if (pixels[index].a < 200) continue; // 顔の上にだけ描く

                    Vector2 p = new Vector2((x + 0.5f) / Size, (y + 0.5f) / Size);
                    float outer = Mathf.Clamp01((radius - (p - center).magnitude) * Size);
                    if (outer <= 0f) continue;

                    float inner = Mathf.Clamp01(
                        (radius * 0.50f - (p - center - new Vector2(radius * 0.20f, 0f)).magnitude) * Size);
                    Color32 current = pixels[index];
                    Color blended = Color.Lerp(new Color(current.r / 255f, current.g / 255f, current.b / 255f),
                        Color.Lerp(sclera, pupil, inner), outer);
                    blended.a = 1f;
                    pixels[index] = blended;
                }
            }
        }
    }
}

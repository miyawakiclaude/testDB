using System.Collections.Generic;
using UnityEngine;

namespace DragonIdle
{
    public enum SfxId
    {
        Pet,
        Hatch,
        LevelUp,
        Buy,
        Evolve,
        Achievement,
        Rebirth
    }

    /// <summary>
    /// 効果音を実行時に合成する。音源ファイルを持たずに済むので、プロジェクトを開いて
    /// すぐ鳴る。倍音を少し足した減衰音を、五音音階の上に並べているだけ。
    /// </summary>
    public static class Sfx
    {
        struct Note
        {
            public float Start;
            public float Frequency;
            public float Duration;
            public float Decay;
            public float Amplitude;
        }

        const int SampleRate = 44100;
        const string MuteKey = "dragon_idle_muted";

        static readonly Dictionary<SfxId, AudioClip> Clips = new Dictionary<SfxId, AudioClip>();
        static AudioSource _source;
        static int _muted = -1;

        public static bool Muted
        {
            get
            {
                if (_muted < 0) _muted = PlayerPrefs.GetInt(MuteKey, 0);
                return _muted != 0;
            }
            set
            {
                _muted = value ? 1 : 0;
                PlayerPrefs.SetInt(MuteKey, _muted);
                PlayerPrefs.Save();
            }
        }

        static AudioSource Source
        {
            get
            {
                if (_source == null)
                {
                    GameObject go = new GameObject("Sfx");
                    Object.DontDestroyOnLoad(go);
                    _source = go.AddComponent<AudioSource>();
                    _source.playOnAwake = false;
                    _source.spatialBlend = 0f;
                    _source.volume = 0.5f;
                }
                return _source;
            }
        }

        public static void Play(SfxId id)
        {
            if (Muted) return;

            AudioClip clip;
            if (!Clips.TryGetValue(id, out clip))
            {
                clip = Build(id);
                Clips[id] = clip;
            }
            if (clip == null) return;

            // なでる音だけは高さを散らして、連打しても単調にならないようにする。
            Source.pitch = id == SfxId.Pet ? Random.Range(0.94f, 1.12f) : 1f;
            Source.PlayOneShot(clip);
        }

        // 五音音階。どう重ねても濁らないので、雑に組み合わせても成立する。
        const float C5 = 523.25f, D5 = 587.33f, E5 = 659.25f, G5 = 783.99f, A5 = 880.00f;
        const float C6 = 1046.50f, E6 = 1318.51f, G6 = 1567.98f;
        const float C3 = 130.81f, G3 = 196.00f, C4 = 261.63f;

        static AudioClip Build(SfxId id)
        {
            switch (id)
            {
                case SfxId.Pet:
                    return Render("sfx_pet", 0.24f, new[]
                    {
                        Tone(0f, A5, 0.22f, 22f, 0.55f),
                        Tone(0f, E6, 0.16f, 30f, 0.18f)
                    });

                case SfxId.Hatch:
                    return Render("sfx_hatch", 0.85f, new[]
                    {
                        Tone(0.00f, C5, 0.45f, 7f, 0.45f),
                        Tone(0.09f, E5, 0.45f, 7f, 0.45f),
                        Tone(0.18f, G5, 0.45f, 7f, 0.45f),
                        Tone(0.28f, C6, 0.55f, 5f, 0.55f)
                    });

                case SfxId.LevelUp:
                    return Render("sfx_levelup", 0.30f, new[]
                    {
                        Tone(0.00f, D5, 0.18f, 24f, 0.45f),
                        Tone(0.07f, A5, 0.22f, 18f, 0.45f)
                    });

                case SfxId.Buy:
                    return Render("sfx_buy", 0.18f, new[]
                    {
                        Tone(0f, C4, 0.14f, 34f, 0.5f),
                        Tone(0f, C5, 0.10f, 44f, 0.2f)
                    });

                case SfxId.Evolve:
                    return Render("sfx_evolve", 0.95f, new[]
                    {
                        Tone(0.00f, C5, 0.16f, 20f, 0.36f),
                        Tone(0.07f, E5, 0.16f, 20f, 0.36f),
                        Tone(0.14f, G5, 0.16f, 20f, 0.36f),
                        Tone(0.21f, C6, 0.16f, 20f, 0.36f),
                        Tone(0.28f, E6, 0.18f, 16f, 0.40f),
                        Tone(0.36f, G6, 0.60f, 5f, 0.50f)
                    });

                case SfxId.Achievement:
                    return Render("sfx_achievement", 1.10f, new[]
                    {
                        Tone(0.00f, C5, 0.80f, 4f, 0.32f),
                        Tone(0.00f, E5, 0.80f, 4f, 0.28f),
                        Tone(0.00f, G5, 0.80f, 4f, 0.26f),
                        Tone(0.14f, C6, 0.85f, 3.4f, 0.40f)
                    });

                default:
                    return Render("sfx_rebirth", 1.80f, new[]
                    {
                        Tone(0.00f, C3, 1.70f, 1.5f, 0.55f),
                        Tone(0.02f, G3, 1.60f, 1.7f, 0.35f),
                        Tone(0.30f, C5, 1.20f, 2.4f, 0.28f),
                        Tone(0.30f, G5, 1.20f, 2.6f, 0.20f)
                    });
            }
        }

        static Note Tone(float start, float frequency, float duration, float decay, float amplitude)
        {
            Note note;
            note.Start = start;
            note.Frequency = frequency;
            note.Duration = duration;
            note.Decay = decay;
            note.Amplitude = amplitude;
            return note;
        }

        static AudioClip Render(string name, float length, Note[] notes)
        {
            int total = Mathf.CeilToInt(length * SampleRate);
            if (total <= 0) return null;
            float[] data = new float[total];

            for (int n = 0; n < notes.Length; n++)
            {
                Note note = notes[n];
                int start = Mathf.Clamp(Mathf.RoundToInt(note.Start * SampleRate), 0, total - 1);
                int count = Mathf.Min(Mathf.RoundToInt(note.Duration * SampleRate), total - start);
                float step = 2f * Mathf.PI * note.Frequency / SampleRate;

                for (int i = 0; i < count; i++)
                {
                    float t = i / (float)SampleRate;
                    // 立ち上がりを丸めて、耳につくプチッという音を防ぐ。
                    float envelope = Mathf.Exp(-t * note.Decay) * (1f - Mathf.Exp(-t * 500f));
                    float phase = step * i;
                    float wave = Mathf.Sin(phase) * 0.72f
                               + Mathf.Sin(phase * 2f) * 0.20f
                               + Mathf.Sin(phase * 3f) * 0.08f;
                    data[start + i] += wave * envelope * note.Amplitude;
                }
            }

            // 重ねたぶん振り切れることがあるので、最大値で正規化しておく。
            float peak = 0f;
            for (int i = 0; i < total; i++) peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            if (peak > 0.0001f)
            {
                float gain = 0.85f / peak;
                for (int i = 0; i < total; i++) data[i] *= gain;
            }

            AudioClip clip = AudioClip.Create(name, total, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

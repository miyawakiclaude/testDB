using System;
using UnityEngine;

namespace DragonIdle
{
    /// <summary>
    /// PlayerPrefs に JSON を1本置くだけの保存。エディタでも実機でも同じ挙動になる。
    /// </summary>
    public static class SaveSystem
    {
        const string Key = "dragon_idle_save_v1";

        public static SaveData Load()
        {
            string json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return null;
                data.EnsureShape();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning("セーブデータを読めませんでした。新しく始めます: " + e.Message);
                return null;
            }
        }

        public static void Save(SaveData data)
        {
            if (data == null) return;
            data.lastSaveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static void Delete()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}

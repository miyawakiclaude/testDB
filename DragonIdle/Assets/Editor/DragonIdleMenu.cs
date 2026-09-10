using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DragonIdle.EditorTools
{
    /// <summary>
    /// ビルド用のシーンを用意したり、セーブを消したりするための小さなメニュー。
    /// エディタで遊ぶだけなら、どのシーンでも再生ボタンで起動する。
    /// </summary>
    public static class DragonIdleMenu
    {
        const string ScenePath = "Assets/Scenes/Nest.unity";

        [MenuItem("Tools/ドラゴン育成/起動シーンを作成", false, 1)]
        public static void CreateBootScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettingsScene[] scenes = { new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = scenes;

            AssetDatabase.SaveAssets();
            Debug.Log("起動シーンを作成しました: " + ScenePath + "（ビルド設定にも登録済み）");
        }

        [MenuItem("Tools/ドラゴン育成/セーブデータを消す", false, 20)]
        public static void ClearSave()
        {
            if (!EditorUtility.DisplayDialog("セーブデータを消す",
                    "育てたドラゴン・ゴールド・竜魂がすべて消えます。よろしいですか？", "消す", "やめる"))
            {
                return;
            }
            SaveSystem.Delete();
            Debug.Log("セーブデータを消しました。次の再生から最初のドラゴンで始まります。");
        }
    }
}

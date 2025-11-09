using UnityEngine;
using UnityEditor;

namespace Project.Editor
{
    /// <summary>
    /// AssetDatabaseを更新してUnityにファイルを認識させる
    /// </summary>
    public class AssetRefresher
    {
        [MenuItem("Tools/Refresh Assets")]
        public static void RefreshAssets()
        {
            // AssetDatabaseをリフレッシュ
            AssetDatabase.Refresh();
            
            // メタファイルを再生成
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            
            Project.Debug.Debug.Log("Assets refreshed. Check if Player.prefab is now visible in Unity.");
        }
        
        [MenuItem("Tools/Reimport Player Prefab")]
        public static void ReimportPlayerPrefab()
        {
            string prefabPath = "Assets/Prefabs/Player/Player.prefab";
            
            if (System.IO.File.Exists(prefabPath))
            {
                AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
                Project.Debug.Debug.Log($"Player prefab reimported: {prefabPath}");
                
                // Projectウィンドウで選択
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Project.Debug.Debug.LogError($"Player prefab not found at: {prefabPath}");
            }
        }
    }
}

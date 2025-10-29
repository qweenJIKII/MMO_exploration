// Editorユーティリティ: URP設定とWindowsビルド設定の初期化
// 注意: Unity Editorでのみ有効
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Project.Editor.Setup
{
    public static class ProjectSetup
    {
        private const string UrpDir = "Assets/Settings/URP";
        private const string UrpAssetPath = UrpDir + "/URP_RenderPipelineAsset.asset";
        private const string UrpRendererPath = UrpDir + "/URP_ForwardRenderer.asset";

        [MenuItem("Tools/Setup/Configure URP")]
        public static void ConfigureUrp()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }
            if (!AssetDatabase.IsValidFolder(UrpDir))
            {
                AssetDatabase.CreateFolder("Assets/Settings", "URP");
            }

            // Renderer作成
            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, UrpRendererPath);

            // URPアセット作成
            var urpAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            // Rendererを割り当て
            typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(urpAsset, new ScriptableObject[] { rendererData });

            AssetDatabase.CreateAsset(urpAsset, UrpAssetPath);
            AssetDatabase.SaveAssets();

            // GraphicsSettingsへ適用
            GraphicsSettings.defaultRenderPipeline = urpAsset;

            // QualitySettings全レベルに適用
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
                QualitySettings.renderPipeline = urpAsset;
            }
            Debug.Log("[Setup] URP configured: " + UrpAssetPath);
        }

        [MenuItem("Tools/Setup/Configure Build (Windows)")]
        public static void ConfigureBuildWindows()
        {
            // シーンをBuild Settingsへ追加
            var scenePath = "Assets/Scenes/SampleScene.unity";
            var scenes = EditorBuildSettings.scenes;
            bool exists = false;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) { exists = true; break; }
            }
            if (!exists && File.Exists(scenePath))
            {
                var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
                {
                    new EditorBuildSettingsScene(scenePath, true)
                };
                EditorBuildSettings.scenes = list.ToArray();
                Debug.Log("[Setup] Added to Build Settings: " + scenePath);
            }

            // Windowsプラットフォームへ切替（必要な場合）
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }

            // 基本のPlayer設定（最小）
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            QualitySettings.vSyncCount = 0; // VSync OFF（FPS検証向け）

            Debug.Log("[Setup] Build configured for Windows");
        }
    }
}
#endif

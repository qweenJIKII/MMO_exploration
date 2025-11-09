using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Project.Editor
{
    /// <summary>
    /// テスト用シーンをセットアップするエディタスクリプト
    /// </summary>
    public class TestSceneSetup
    {
        [MenuItem("Tools/Setup Test Scene")]
        public static void SetupTestScene()
        {
            // 現在のシーンをクリア
            if (SceneManager.GetActiveScene().isDirty)
            {
                if (!EditorUtility.DisplayDialog("Setup Test Scene", 
                    "Current scene has unsaved changes. Continue?", "Yes", "No"))
                {
                    return;
                }
            }
            
            // 新しいシーンを作成
            Scene scene = SceneManager.CreateScene("PlayerTestScene");
            
            // 地面を作成
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10, 1, 10);
            
            // ライティングをセットアップ
            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            
            // Main Cameraをセットアップ
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.transform.position = new Vector3(0, 2, -5);
            cameraGO.transform.rotation = Quaternion.identity;
            
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            
            // Cinemachine Brainを追加
            var cinemachineBrain = cameraGO.AddComponent<Unity.Cinemachine.CinemachineBrain>();
            // m_DefaultBlendは3.xでは直接アクセスできないため、デフォルト設定を使用
            cinemachineBrain.ShowCameraFrustum = true;
            
            // Player Prefabをロードして配置
            string playerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
            
            if (playerPrefab != null)
            {
                GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
                player.name = "Player";
                player.transform.position = new Vector3(0, 2, 0);
                
                // PlayerDataManagerをシーンに追加
                GameObject playerDataManagerGO = new GameObject("PlayerDataManager");
                playerDataManagerGO.AddComponent<Project.Core.Player.PlayerDataManager>();
                
                Project.Debug.Debug.Log("Test scene setup complete!");
                Project.Debug.Debug.Log("Player prefab instantiated and configured.");
            }
            else
            {
                Project.Debug.Debug.LogWarning("Player prefab not found at: " + playerPrefabPath);
                Project.Debug.Debug.Log("Please create the Player prefab first using Tools > Create Player Prefab");
            }
            
            // シーンを保存
            string scenePath = "Assets/Scenes/PlayerTestScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Project.Debug.Debug.Log("Test scene saved at: " + scenePath);
        }
        
        [MenuItem("Tools/Create Basic Environment")]
        public static void CreateBasicEnvironment()
        {
            // 簡単なテスト環境を作成
            GameObject environment = new GameObject("TestEnvironment");
            
            // 床
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(environment.transform);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(20, 0.1f, 20);
            
            // 壁
            CreateWall(environment, "WallNorth", new Vector3(0, 2.5f, 10), new Vector3(20, 5, 0.5f));
            CreateWall(environment, "WallSouth", new Vector3(0, 2.5f, -10), new Vector3(20, 5, 0.5f));
            CreateWall(environment, "WallEast", new Vector3(10, 2.5f, 0), new Vector3(0.5f, 5, 20));
            CreateWall(environment, "WallWest", new Vector3(-10, 2.5f, 0), new Vector3(0.5f, 5, 20));
            
            // 障害物
            for (int i = 0; i < 5; i++)
            {
                GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = "Obstacle" + i;
                obstacle.transform.SetParent(environment.transform);
                obstacle.transform.position = new Vector3(
                    Random.Range(-8f, 8f),
                    0.5f,
                    Random.Range(-8f, 8f)
                );
                obstacle.transform.localScale = new Vector3(1, 1, 1);
            }
            
            // マテリアルを設定
            Material groundMaterial = new Material(Shader.Find("Standard"));
            groundMaterial.color = Color.gray;
            
            Material wallMaterial = new Material(Shader.Find("Standard"));
            wallMaterial.color = Color.white;
            
            foreach (Transform child in environment.transform)
            {
                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (child.name.Contains("Wall"))
                    {
                        renderer.material = wallMaterial;
                    }
                    else
                    {
                        renderer.material = groundMaterial;
                    }
                }
            }
            
            Project.Debug.Debug.Log("Basic test environment created!");
        }
        
        private static void CreateWall(GameObject parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform);
            wall.transform.position = position;
            wall.transform.localScale = scale;
        }
    }
}

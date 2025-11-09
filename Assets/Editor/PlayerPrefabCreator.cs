using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Project.Editor
{
    /// <summary>
    /// Player Prefabを作成するエディタスクリプト
    /// </summary>
    public class PlayerPrefabCreator
    {
        [MenuItem("Tools/Create Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            // PlayerのルートGameObjectを作成
            GameObject playerRoot = new GameObject("Player");
            
            // CharacterControllerを追加
            CharacterController characterController = playerRoot.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 1f, 0);
            
            // PlayerControllerを追加
            Project.Core.Player.PlayerController playerController = 
                playerRoot.AddComponent<Project.Core.Player.PlayerController>();
            
            // CameraControllerを追加
            Project.Core.Player.CameraController cameraController = 
                playerRoot.AddComponent<Project.Core.Player.CameraController>();
            
            // CameraTargetを作成
            GameObject cameraTarget = new GameObject("CameraTarget");
            cameraTarget.transform.SetParent(playerRoot.transform);
            cameraTarget.transform.localPosition = new Vector3(0, 1.5f, 0); // 頭上あたり
            
            // 基本的なメッシュを追加（Capsule）
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "PlayerMesh";
            capsule.transform.SetParent(playerRoot.transform);
            capsule.transform.localPosition = new Vector3(0, 1f, 0);
            capsule.transform.localScale = new Vector3(1f, 1f, 1f);
            
            // Colliderを削除（CharacterControllerを使用するため）
            Collider capsuleCollider = capsule.GetComponent<Collider>();
            if (capsuleCollider != null)
            {
                Object.DestroyImmediate(capsuleCollider);
            }
            
            // Cinemachine Cameraを別途作成
            GameObject cmCamera = new GameObject("CM_PlayerCamera");
            cmCamera.transform.position = new Vector3(0, 1.5f, -5f);
            
            CinemachineCamera virtualCamera = cmCamera.AddComponent<CinemachineCamera>();
            CinemachineThirdPersonFollow thirdPersonFollow = cmCamera.AddComponent<CinemachineThirdPersonFollow>();
            
            // ThirdPersonFollowの設定
            thirdPersonFollow.CameraDistance = 5f;
            thirdPersonFollow.ShoulderOffset = new Vector3(0.5f, 0, -0.5f);
            thirdPersonFollow.VerticalArmLength = 0.2f;
            
            // CameraControllerの設定
            var cameraControllerSerialized = new SerializedObject(cameraController);
            cameraControllerSerialized.FindProperty("virtualCamera").objectReferenceValue = virtualCamera;
            cameraControllerSerialized.FindProperty("cameraTarget").objectReferenceValue = cameraTarget.transform;
            cameraControllerSerialized.ApplyModifiedProperties();
            
            // Input Actionsの設定（PlayerController）
            var playerControllerSerialized = new SerializedObject(playerController);
            
            // InputActionsアセットを検索
            string[] guids = AssetDatabase.FindAssets("InputActions t:InputActionAsset");
            InputActionAsset inputActions = null;
            
            if (guids.Length > 0)
            {
                // 最初の有効なアセットを読み込み
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                    
                    if (inputActions != null)
                    {
                        Project.Debug.Debug.Log($"Found InputActionAsset at: {path}");
                        break;
                    }
                }
                
                if (inputActions != null)
                {
                    // InputActionReferencesを設定
                    playerControllerSerialized.FindProperty("moveAction").objectReferenceValue = 
                        GetInputActionReference(inputActions, "Player/Move");
                    playerControllerSerialized.FindProperty("jumpAction").objectReferenceValue = 
                        GetInputActionReference(inputActions, "Player/Jump");
                    playerControllerSerialized.FindProperty("sprintAction").objectReferenceValue = 
                        GetInputActionReference(inputActions, "Player/Sprint");
                    
                    cameraControllerSerialized.FindProperty("lookAction").objectReferenceValue = 
                        GetInputActionReference(inputActions, "Camera/Look");
                    cameraControllerSerialized.FindProperty("zoomAction").objectReferenceValue = 
                        GetInputActionReference(inputActions, "Camera/Zoom");
                }
                else
                {
                    Project.Debug.Debug.LogWarning("InputActionAsset could not be loaded. Input actions will not be set.");
                }
            }
            else
            {
                Project.Debug.Debug.LogWarning("No InputActions asset found. Input actions will not be set.");
                Project.Debug.Debug.Log("Make sure you have an InputActionAsset file in your project (e.g., InputActions.inputactions)");
            }
            
            playerControllerSerialized.ApplyModifiedProperties();
            cameraControllerSerialized.ApplyModifiedProperties();
            
            // タグを設定
            playerRoot.tag = "Player";
            
            // レイヤーを設定
            playerRoot.layer = LayerMask.NameToLayer("Default");
            
            // Prefabとして保存
            string prefabPath = "Assets/Prefabs/Player/Player.prefab";
            PrefabUtility.SaveAsPrefabAsset(playerRoot, prefabPath);
            
            // シーンから一時オブジェクトを削除
            Object.DestroyImmediate(playerRoot);
            Object.DestroyImmediate(cmCamera);
            
            Project.Debug.Debug.Log($"Player Prefab created at: {prefabPath}");
            Project.Debug.Debug.Log("CM_PlayerCamera was created in scene. Configure it as needed.");
        }
        
        private static InputActionReference GetInputActionReference(InputActionAsset asset, string actionPath)
        {
            if (asset == null)
            {
                Project.Debug.Debug.LogError($"InputActionAsset is null for action path: {actionPath}");
                return null;
            }
            
            string[] paths = actionPath.Split('/');
            if (paths.Length != 2) 
            {
                Project.Debug.Debug.LogError($"Invalid action path format: {actionPath}. Expected format: 'Map/Action'");
                return null;
            }
            
            var map = asset.FindActionMap(paths[0]);
            if (map == null) 
            {
                Project.Debug.Debug.LogError($"Action map not found: {paths[0]} in asset: {asset.name}");
                return null;
            }
            
            var action = map.FindAction(paths[1]);
            if (action == null) 
            {
                Project.Debug.Debug.LogError($"Action not found: {paths[1]} in map: {paths[0]}");
                return null;
            }
            
            return InputActionReference.Create(action);
        }
    }
}

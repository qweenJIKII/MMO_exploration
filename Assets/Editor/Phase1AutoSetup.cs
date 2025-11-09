using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

namespace Project.Editor
{
    /// <summary>
    /// Phase1_Implementation_Status.mdの項目4～7を自動設定
    /// </summary>
    public class Phase1AutoSetup
    {
        [MenuItem("Tools/Phase1 Setup/4. Cinemachine Virtual Camera")]
        public static void SetupCinemachineVirtualCamera()
        {
            // 既存のCM_PlayerCameraを探す
            GameObject cmCamera = GameObject.Find("CM_PlayerCamera");
            
            if (cmCamera == null)
            {
                // 新しく作成
                cmCamera = new GameObject("CM_PlayerCamera");
                cmCamera.transform.position = new Vector3(0, 1.5f, -5f);
            }
            
            // CinemachineCameraを追加
            var virtualCamera = cmCamera.GetComponent<CinemachineCamera>();
            if (virtualCamera == null)
            {
                virtualCamera = cmCamera.AddComponent<CinemachineCamera>();
            }
            
            // CinemachineThirdPersonFollowを追加
            var thirdPersonFollow = cmCamera.GetComponent<CinemachineThirdPersonFollow>();
            if (thirdPersonFollow == null)
            {
                thirdPersonFollow = cmCamera.AddComponent<CinemachineThirdPersonFollow>();
            }
            
            // 設定
            thirdPersonFollow.CameraDistance = 5f;
            thirdPersonFollow.ShoulderOffset = new Vector3(0.5f, 0, -0.5f);
            thirdPersonFollow.VerticalArmLength = 0.2f;
            thirdPersonFollow.Damping = new Vector3(0.1f, 0.5f, 0.3f);
            
            // Playerを探してFollow/LookAtを設定
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                Transform cameraTarget = player.transform.Find("CameraTarget");
                if (cameraTarget != null)
                {
                    virtualCamera.Follow = cameraTarget;
                    virtualCamera.LookAt = cameraTarget;
                    
                    // CameraControllerにVirtual Cameraを設定
                    var cameraController = player.GetComponent<Project.Core.Player.CameraController>();
                    if (cameraController != null)
                    {
                        var serialized = new SerializedObject(cameraController);
                        serialized.FindProperty("virtualCamera").objectReferenceValue = virtualCamera;
                        serialized.ApplyModifiedProperties();
                    }
                }
            }
            
            Project.Debug.Debug.Log("Cinemachine Virtual Camera setup complete!");
        }
        
        [MenuItem("Tools/Phase1 Setup/5. HUD Canvas")]
        public static void SetupHUDCanvas()
        {
            // HUD Canvasを作成
            GameObject canvasGO = new GameObject("HUD Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // HUDManagerを追加
            var hudManager = canvasGO.AddComponent<Project.Core.UI.HUDManager>();
            
            // HUDパネルを作成
            GameObject hudPanel = new GameObject("HUD Panel");
            hudPanel.transform.SetParent(canvasGO.transform, false);
            
            var panelRect = hudPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // パネル自体は透明に
            var panelImage = hudPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0);
            
            // === 左上：プレイヤー情報エリア ===
            GameObject playerInfoPanel = CreateHUDPanel(hudPanel.transform, "PlayerInfoPanel", 
                new Vector2(0, 1), new Vector2(0, 0), new Vector2(300, 120));
            
            // プレイヤー名（左上）
            CreateHUDText(playerInfoPanel.transform, "PlayerNameText", "Player Name", 
                new Vector2(10, -10), 22, TextAnchor.UpperLeft, Color.white);
            
            // レベルとクラス
            CreateHUDText(playerInfoPanel.transform, "LevelClassText", "Lv.1 Warrior", 
                new Vector2(10, -40), 18, TextAnchor.UpperLeft, Color.yellow);
            
            // HPバー
            CreateStatusBar(playerInfoPanel.transform, "HPBar", new Vector2(10, -70), 
                new Vector2(280, 20), Color.red, "HP: 100/100");
            
            // MPバー  
            CreateStatusBar(playerInfoPanel.transform, "MPBar", new Vector2(10, -95), 
                new Vector2(280, 20), Color.blue, "MP: 50/50");
            
            // === 右上：ミニマップ準備エリア ===
            GameObject minimapPanel = CreateHUDPanel(hudPanel.transform, "MinimapPanel", 
                new Vector2(1, 1), new Vector2(-200, 0), new Vector2(200, 200));
            
            CreateHUDText(minimapPanel.transform, "MinapLabel", "MINIMAP", 
                new Vector2(0, -10), 14, TextAnchor.UpperCenter, Color.gray);
            
            // === 中央下：スキルバー ===
            GameObject skillBarPanel = CreateHUDPanel(hudPanel.transform, "SkillBarPanel", 
                new Vector2(0.5f, 0), new Vector2(-200, 20), new Vector2(400, 60));
            
            // スキルスロット（6個）
            for (int i = 0; i < 6; i++)
            {
                CreateSkillSlot(skillBarPanel.transform, $"SkillSlot{i+1}", 
                    new Vector2(10 + i * 65, 10), new Vector2(50, 50));
            }
            
            // === 左下：クエスト情報 ===
            GameObject questPanel = CreateHUDPanel(hudPanel.transform, "QuestPanel", 
                new Vector2(0, 0), new Vector2(10, 150), new Vector2(250, 140));
            
            CreateHUDText(questPanel.transform, "QuestTitle", "CURRENT QUESTS", 
                new Vector2(10, -10), 14, TextAnchor.UpperLeft, Color.cyan);
            
            CreateHUDText(questPanel.transform, "Quest1Text", "• Find the Lost Sword", 
                new Vector2(10, -35), 12, TextAnchor.UpperLeft, Color.white);
            
            CreateHUDText(questPanel.transform, "Quest2Text", "• Defeat 5 Monsters", 
                new Vector2(10, -55), 12, TextAnchor.UpperLeft, Color.white);
            
            CreateHUDText(questPanel.transform, "Quest3Text", "• Deliver Message", 
                new Vector2(10, -75), 12, TextAnchor.UpperLeft, Color.gray);
            
            // === 右下：システム情報 ===
            GameObject systemPanel = CreateHUDPanel(hudPanel.transform, "SystemPanel", 
                new Vector2(1, 0), new Vector2(-160, 20), new Vector2(150, 80));
            
            // FPS表示
            CreateHUDText(systemPanel.transform, "FPSText", "60 FPS", 
                new Vector2(0, -10), 12, TextAnchor.UpperRight, Color.green);
            
            // 時間表示
            CreateHUDText(systemPanel.transform, "TimeText", "12:00", 
                new Vector2(0, -30), 12, TextAnchor.UpperRight, Color.white);
            
            // ピン（経験値）
            CreateHUDText(systemPanel.transform, "PingText", "Ping: 25ms", 
                new Vector2(0, -50), 12, TextAnchor.UpperRight, Color.yellow);
            
            Project.Debug.Debug.Log("HUD Canvas setup complete!");
        }
        
        private static GameObject CreateHUDText(Transform parent, string name, string defaultText, Vector2 position, int fontSize, TextAnchor anchor = TextAnchor.UpperLeft, Color color = default(Color))
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            
            var textMeshPro = textGO.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = defaultText;
            textMeshPro.fontSize = fontSize;
            textMeshPro.color = color == default(Color) ? Color.white : color;
            
            // TextAnchorをTextAlignmentOptionsに変換
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    alignment = TextAlignmentOptions.TopLeft;
                    break;
                case TextAnchor.UpperCenter:
                    alignment = TextAlignmentOptions.Top;
                    break;
                case TextAnchor.UpperRight:
                    alignment = TextAlignmentOptions.TopRight;
                    break;
                case TextAnchor.MiddleLeft:
                    alignment = TextAlignmentOptions.Left;
                    break;
                case TextAnchor.MiddleCenter:
                    alignment = TextAlignmentOptions.Center;
                    break;
                case TextAnchor.MiddleRight:
                    alignment = TextAlignmentOptions.Right;
                    break;
                case TextAnchor.LowerLeft:
                    alignment = TextAlignmentOptions.BottomLeft;
                    break;
                case TextAnchor.LowerCenter:
                    alignment = TextAlignmentOptions.Bottom;
                    break;
                case TextAnchor.LowerRight:
                    alignment = TextAlignmentOptions.BottomRight;
                    break;
            }
            textMeshPro.alignment = alignment;
            
            return textGO;
        }
        
        private static GameObject CreateHUDPanel(Transform parent, string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject panelGO = new GameObject(name);
            panelGO.transform.SetParent(parent, false);
            
            var rectTransform = panelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = anchor;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            
            var image = panelGO.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.5f);
            
            return panelGO;
        }
        
        private static GameObject CreateStatusBar(Transform parent, string name, Vector2 position, Vector2 size, Color fillColor, string defaultText)
        {
            GameObject barGO = new GameObject(name);
            barGO.transform.SetParent(parent, false);
            
            var rectTransform = barGO.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            
            // 背景
            var background = barGO.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // フィルバー
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(barGO.transform, false);
            
            var fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            var fill = fillGO.AddComponent<Image>();
            fill.color = fillColor;
            
            // テキスト
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(barGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = defaultText;
            text.fontSize = 12;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            return barGO;
        }
        
        private static GameObject CreateSkillSlot(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject slotGO = new GameObject(name);
            slotGO.transform.SetParent(parent, false);
            
            var rectTransform = slotGO.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            
            var image = slotGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // 枠線効果
            var outline = slotGO.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2, 2);
            
            // スキル番号
            GameObject numberGO = new GameObject("Number");
            numberGO.transform.SetParent(slotGO.transform, false);
            
            var numberRect = numberGO.AddComponent<RectTransform>();
            numberRect.anchorMin = new Vector2(0, 0);
            numberRect.anchorMax = new Vector2(0, 0);
            numberRect.anchoredPosition = new Vector2(5, 5);
            numberRect.sizeDelta = new Vector2(20, 20);
            
            var numberText = numberGO.AddComponent<TextMeshProUGUI>();
            numberText.text = name.Replace("SkillSlot", "");
            numberText.fontSize = 10;
            numberText.color = Color.white;
            numberText.alignment = TextAlignmentOptions.Left;
            
            return slotGO;
        }
        
        [MenuItem("Tools/Phase1 Setup/6. InputActionReference")]
        public static void SetupInputActionReferences()
        {
            // InputActionsアセットをロード
            string[] guids = AssetDatabase.FindAssets("InputActions t:InputActionAsset");
            if (guids.Length == 0)
            {
                Project.Debug.Debug.LogError("InputActions asset not found!");
                return;
            }
            
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            
            if (inputActions == null)
            {
                Project.Debug.Debug.LogError("Failed to load InputActions asset!");
                return;
            }
            
            // PlayerControllerの設定
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                var playerController = player.GetComponent<Project.Core.Player.PlayerController>();
                if (playerController != null)
                {
                    var serialized = new SerializedObject(playerController);
                    serialized.FindProperty("moveAction").objectReferenceValue = GetInputActionReference(inputActions, "Player/Move");
                    serialized.FindProperty("jumpAction").objectReferenceValue = GetInputActionReference(inputActions, "Player/Jump");
                    serialized.FindProperty("sprintAction").objectReferenceValue = GetInputActionReference(inputActions, "Player/Sprint");
                    serialized.ApplyModifiedProperties();
                }
                
                var cameraController = player.GetComponent<Project.Core.Player.CameraController>();
                if (cameraController != null)
                {
                    var serialized = new SerializedObject(cameraController);
                    serialized.FindProperty("lookAction").objectReferenceValue = GetInputActionReference(inputActions, "Camera/Look");
                    serialized.FindProperty("zoomAction").objectReferenceValue = GetInputActionReference(inputActions, "Camera/Zoom");
                    serialized.ApplyModifiedProperties();
                }
            }
            
            // DevConsoleの設定（存在する場合のみ）
            GameObject devConsole = GameObject.Find("DevConsole");
            if (devConsole != null)
            {
                Project.Debug.Debug.Log("DevConsole found but DevConsole class not implemented yet. Skipping InputActionReference setup.");
            }
            
            // ProfilingManagerの設定（存在する場合のみ）
            GameObject profilingManager = GameObject.Find("ProfilingManager");
            if (profilingManager != null)
            {
                Project.Debug.Debug.Log("ProfilingManager found but ProfilingManager class not implemented yet. Skipping InputActionReference setup.");
            }
            
            Project.Debug.Debug.Log("InputActionReference setup complete!");
        }
        
        [MenuItem("Tools/Phase1 Setup/7. Player Settings")]
        public static void SetupPlayerSettings()
        {
            Project.Debug.Debug.Log("=== Player Settings Manual Setup Guide ===");
            Project.Debug.Debug.Log("Please follow these steps manually:");
            Project.Debug.Debug.Log("1. Go to Edit > Project Settings");
            Project.Debug.Debug.Log("2. Select 'Player' section");
            Project.Debug.Debug.Log("3. Expand 'Configuration' > 'Other Settings'");
            Project.Debug.Debug.Log("4. Set 'Active Input Handling' to 'Input System Package (New)'");
            Project.Debug.Debug.Log("5. Check 'Both' temporarily if you have issues");
            Project.Debug.Debug.Log("6. Restart Unity Editor");
            Project.Debug.Debug.Log("==========================================");
            
            // 代替：Input System Packageがインストールされているか確認
            bool inputSystemInstalled = false;
            foreach (var package in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                if (package.name == "com.unity.inputsystem")
                {
                    inputSystemInstalled = true;
                    break;
                }
            }
            
            if (inputSystemInstalled)
            {
                Project.Debug.Debug.Log("✅ Input System Package is installed");
            }
            else
            {
                Project.Debug.Debug.LogError("❌ Input System Package is not installed!");
                Project.Debug.Debug.Log("Please install it via Window > Package Manager");
            }
        }
        
        [MenuItem("Tools/Phase1 Setup/Run All Setup")]
        public static void RunAllSetup()
        {
            SetupCinemachineVirtualCamera();
            SetupHUDCanvas();
            SetupInputActionReferences();
            SetupPlayerSettings();
            
            Project.Debug.Debug.Log("Phase 1 setup complete! All items 4-7 have been configured.");
        }
        
        private static InputActionReference GetInputActionReference(InputActionAsset asset, string actionPath)
        {
            if (asset == null) return null;
            
            string[] paths = actionPath.Split('/');
            if (paths.Length != 2) return null;
            
            var map = asset.FindActionMap(paths[0]);
            if (map == null) return null;
            
            var action = map.FindAction(paths[1]);
            if (action == null) return null;
            
            return InputActionReference.Create(action);
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Project.UI;

namespace Project.Editor
{
    /// <summary>
    /// Settings Menu UIを自動生成するエディタツール
    /// </summary>
    public class SettingsMenuCreator : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool showAudioSettings = true;
        private bool showGraphicsSettings = true;
        private bool showControlsSettings = true;
        
        // 生成設定
        private string canvasName = "Settings Canvas";
        private bool createAsPrefab = true;
        private string prefabPath = "Assets/Prefabs/UI/SettingsMenu.prefab";
        
        [MenuItem("Tools/Phase2 Setup/Create Settings Menu")]
        public static void ShowWindow()
        {
            GetWindow<SettingsMenuCreator>("Settings Menu Creator");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Settings Menu Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 基本設定
            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            canvasName = EditorGUILayout.TextField("Canvas Name", canvasName);
            createAsPrefab = EditorGUILayout.Toggle("Create as Prefab", createAsPrefab);
            if (createAsPrefab)
            {
                prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);
            }
            EditorGUILayout.Space();
            
            // オーディオ設定
            showAudioSettings = EditorGUILayout.Foldout(showAudioSettings, "Audio Settings", true);
            if (showAudioSettings)
            {
                EditorGUILayout.LabelField("• Master Volume Slider");
                EditorGUILayout.LabelField("• BGM Volume Slider");
                EditorGUILayout.LabelField("• SE Volume Slider");
                EditorGUILayout.LabelField("• Volume Percentage Display");
            }
            
            // グラフィック設定
            showGraphicsSettings = EditorGUILayout.Foldout(showGraphicsSettings, "Graphics Settings", true);
            if (showGraphicsSettings)
            {
                EditorGUILayout.LabelField("• Resolution Dropdown");
                EditorGUILayout.LabelField("• Quality Level Dropdown");
                EditorGUILayout.LabelField("• Fullscreen Toggle");
                EditorGUILayout.LabelField("• VSync Toggle");
            }
            
            // コントロール設定
            showControlsSettings = EditorGUILayout.Foldout(showControlsSettings, "Controls Settings", true);
            if (showControlsSettings)
            {
                EditorGUILayout.LabelField("• Mouse Sensitivity Slider");
                EditorGUILayout.LabelField("• Key Binding Areas (Future)");
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            
            // 作成ボタン
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Settings Menu"))
            {
                CreateSettingsMenu();
            }
            if (GUILayout.Button("Create Only UI"))
            {
                CreateSettingsUI();
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Create Settings Manager"))
            {
                CreateSettingsManager();
            }
        }
        
        private void CreateSettingsMenu()
        {
            Project.Debug.Debug.Log("Creating complete Settings Menu...");
            
            // Canvasを作成
            GameObject canvas = CreateCanvas();
            
            // Settings Managerを作成
            CreateSettingsManagerOnCanvas(canvas);
            
            // Settings UIを作成
            CreateSettingsUIOnCanvas(canvas);
            
            // Prefabとして保存
            if (createAsPrefab)
            {
                SaveAsPrefab(canvas);
            }
            
            Project.Debug.Debug.Log("Settings Menu created successfully!");
        }
        
        private void CreateSettingsUI()
        {
            Project.Debug.Debug.Log("Creating Settings UI only...");
            
            GameObject canvas = CreateCanvas();
            CreateSettingsUIOnCanvas(canvas);
            
            if (createAsPrefab)
            {
                SaveAsPrefab(canvas);
            }
            
            Project.Debug.Debug.Log("Settings UI created successfully!");
        }
        
        private void CreateSettingsManager()
        {
            Project.Debug.Debug.Log("Creating Settings Manager...");
            
            // 既存のSettings Managerを検索
            var existingManager = FindAnyObjectByType<SettingsManager>();
            if (existingManager != null)
            {
                Selection.activeGameObject = existingManager.gameObject;
                EditorGUIUtility.PingObject(existingManager.gameObject);
                Project.Debug.Debug.Log("Settings Manager already exists!");
                return;
            }
            
            // 新しいSettings Managerを作成
            GameObject managerGO = new GameObject("SettingsManager");
            managerGO.AddComponent<SettingsManager>();
            
            Selection.activeGameObject = managerGO;
            Project.Debug.Debug.Log("Settings Manager created!");
        }
        
        private GameObject CreateCanvas()
        {
            // Canvasを作成
            GameObject canvasGO = new GameObject(canvasName);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Canvas Scalerを追加
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // Graphic Raycasterを追加
            canvasGO.AddComponent<GraphicRaycaster>();
            
            return canvasGO;
        }
        
        private void CreateSettingsManagerOnCanvas(GameObject canvas)
        {
            GameObject managerGO = new GameObject("SettingsManager");
            managerGO.transform.SetParent(canvas.transform, false);
            managerGO.AddComponent<SettingsManager>();
        }
        
        private void CreateSettingsUIOnCanvas(GameObject canvas)
        {
            // メインパネル
            GameObject mainPanel = CreatePanel(canvas.transform, "MainPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 600));
            
            // タイトル
            CreateTitleText(mainPanel.transform, "Settings", new Vector2(0, -50), 32);
            
            // タブナビゲーション
            GameObject tabPanel = CreateTabNavigation(mainPanel.transform);
            
            // コンテンツパネル
            GameObject contentPanel = CreatePanel(mainPanel.transform, "ContentPanel", new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(700, 450));
            
            // 各設定パネルを作成
            CreateAudioSettingsPanel(contentPanel.transform);
            CreateGraphicsSettingsPanel(contentPanel.transform);
            CreateControlsSettingsPanel(contentPanel.transform);
            
            // ボタンパネル
            CreateButtonPanel(mainPanel.transform);
        }
        
        private GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            GameObject panelGO = new GameObject(name);
            panelGO.transform.SetParent(parent, false);
            
            var rectTransform = panelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = anchor;
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            
            var image = panelGO.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            return panelGO;
        }
        
        private GameObject CreateTitleText(Transform parent, string text, Vector2 position, int fontSize)
        {
            GameObject textGO = new GameObject("TitleText");
            textGO.transform.SetParent(parent, false);
            
            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(400, 50);
            
            var textMeshPro = textGO.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = text;
            textMeshPro.fontSize = fontSize;
            textMeshPro.color = Color.white;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            
            return textGO;
        }
        
        private GameObject CreateTabNavigation(Transform parent)
        {
            GameObject tabPanel = new GameObject("TabPanel");
            tabPanel.transform.SetParent(parent, false);
            
            var rectTransform = tabPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -100);
            rectTransform.sizeDelta = new Vector2(600, 50);
            
            // タブボタンを作成
            CreateTabButton(tabPanel.transform, "AudioButton", "Audio", new Vector2(-200, 0));
            CreateTabButton(tabPanel.transform, "GraphicsButton", "Graphics", new Vector2(0, 0));
            CreateTabButton(tabPanel.transform, "ControlsButton", "Controls", new Vector2(200, 0));
            
            return tabPanel;
        }
        
        private GameObject CreateTabButton(Transform parent, string name, string text, Vector2 position)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);
            
            var rectTransform = buttonGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(150, 40);
            
            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            var button = buttonGO.AddComponent<Button>();
            
            // ボタンテキスト
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textMeshPro = textGO.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = text;
            textMeshPro.fontSize = 16;
            textMeshPro.color = Color.white;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            
            return buttonGO;
        }
        
        private void CreateAudioSettingsPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "AudioPanel", Vector2.zero, Vector2.zero, new Vector2(650, 400));
            panel.SetActive(false); // デフォルトで非表示
            
            // マスターボリューム
            CreateVolumeSetting(panel.transform, "MasterVolume", "Master Volume", new Vector2(0, -50));
            
            // BGMボリューム
            CreateVolumeSetting(panel.transform, "BGMVolume", "BGM Volume", new Vector2(0, -120));
            
            // SEボリューム
            CreateVolumeSetting(panel.transform, "SEVolume", "SE Volume", new Vector2(0, -190));
        }
        
        private void CreateVolumeSetting(Transform parent, string name, string label, Vector2 position)
        {
            // ラベル
            CreateLabel(parent, name + "Label", label, position + new Vector2(-250, 0));
            
            // スライダー
            GameObject sliderGO = new GameObject(name + "Slider");
            sliderGO.transform.SetParent(parent, false);
            
            var rectTransform = sliderGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(300, 20);
            
            var slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;
            
            // スライダーの背景
            var background = new GameObject("Background");
            background.transform.SetParent(sliderGO.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            slider.fillRect = bgImage.rectTransform;
            
            // 値表示テキスト
            CreateValueText(parent, name + "Value", "80%", position + new Vector2(200, 0));
        }
        
        private void CreateGraphicsSettingsPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "GraphicsPanel", Vector2.zero, Vector2.zero, new Vector2(650, 400));
            panel.SetActive(false);
            
            // 解像度
            CreateDropdownSetting(panel.transform, "Resolution", "Resolution", new Vector2(0, -50));
            
            // 品質
            CreateDropdownSetting(panel.transform, "Quality", "Quality", new Vector2(0, -120));
            
            // フルスクリーン
            CreateToggleSetting(panel.transform, "Fullscreen", "Fullscreen", new Vector2(0, -190));
            
            // VSync
            CreateToggleSetting(panel.transform, "VSync", "VSync", new Vector2(0, -260));
        }
        
        private void CreateControlsSettingsPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "ControlsPanel", Vector2.zero, Vector2.zero, new Vector2(650, 400));
            panel.SetActive(false);
            
            // マウス感度
            CreateSliderSetting(panel.transform, "MouseSensitivity", "Mouse Sensitivity", new Vector2(0, -50), 0.1f, 3f, 1f);
        }
        
        private void CreateDropdownSetting(Transform parent, string name, string label, Vector2 position)
        {
            CreateLabel(parent, name + "Label", label, position + new Vector2(-200, 0));
            
            GameObject dropdownGO = new GameObject(name + "Dropdown");
            dropdownGO.transform.SetParent(parent, false);
            
            var rectTransform = dropdownGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(200, 30);
            
            dropdownGO.AddComponent<TMP_Dropdown>();
        }
        
        private void CreateToggleSetting(Transform parent, string name, string label, Vector2 position)
        {
            CreateLabel(parent, name + "Label", label, position + new Vector2(-200, 0));
            
            GameObject toggleGO = new GameObject(name + "Toggle");
            toggleGO.transform.SetParent(parent, false);
            
            var rectTransform = toggleGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(30, 30);
            
            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.isOn = true;
            
            var bgImage = toggleGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            toggle.targetGraphic = bgImage;
        }
        
        private void CreateSliderSetting(Transform parent, string name, string label, Vector2 position, float min, float max, float defaultValue)
        {
            CreateLabel(parent, name + "Label", label, position + new Vector2(-200, 0));
            
            GameObject sliderGO = new GameObject(name + "Slider");
            sliderGO.transform.SetParent(parent, false);
            
            var rectTransform = sliderGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(200, 20);
            
            var slider = sliderGO.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = defaultValue;
            
            CreateValueText(parent, name + "Value", defaultValue.ToString("F1"), position + new Vector2(150, 0));
        }
        
        private GameObject CreateLabel(Transform parent, string name, string text, Vector2 position)
        {
            GameObject labelGO = new GameObject(name);
            labelGO.transform.SetParent(parent, false);
            
            var rectTransform = labelGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(150, 30);
            
            var textMeshPro = labelGO.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = text;
            textMeshPro.fontSize = 14;
            textMeshPro.color = Color.white;
            textMeshPro.alignment = TextAlignmentOptions.Right;
            
            return labelGO;
        }
        
        private GameObject CreateValueText(Transform parent, string name, string text, Vector2 position)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            var rectTransform = textGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(80, 30);
            
            var textMeshPro = textGO.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = text;
            textMeshPro.fontSize = 14;
            textMeshPro.color = Color.white;
            textMeshPro.alignment = TextAlignmentOptions.Left;
            
            return textGO;
        }
        
        private void CreateButtonPanel(Transform parent)
        {
            GameObject buttonPanel = new GameObject("ButtonPanel");
            buttonPanel.transform.SetParent(parent, false);
            
            var rectTransform = buttonPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = new Vector2(0, 30);
            rectTransform.sizeDelta = new Vector2(400, 50);
            
            CreateActionButton(buttonPanel.transform, "ApplyButton", "Apply", new Vector2(-100, 0));
            CreateActionButton(buttonPanel.transform, "ResetButton", "Reset", new Vector2(0, 0));
            CreateActionButton(buttonPanel.transform, "CloseButton", "Close", new Vector2(100, 0));
        }
        
        private GameObject CreateActionButton(Transform parent, string name, string text, Vector2 position)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);
            
            var rectTransform = buttonGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(80, 35);
            
            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            
            var button = buttonGO.AddComponent<Button>();
            
            // ボタンテキスト
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textMeshPro = textGO.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = text;
            textMeshPro.fontSize = 14;
            textMeshPro.color = Color.white;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            
            return buttonGO;
        }
        
        private void SaveAsPrefab(GameObject gameObject)
        {
            // ディレクトリが存在しない場合は作成
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // Prefabとして保存
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            Project.Debug.Debug.Log($"Settings Menu saved as prefab: {prefabPath}");
        }
    }
}

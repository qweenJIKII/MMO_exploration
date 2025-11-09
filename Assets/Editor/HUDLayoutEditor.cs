using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Project.Editor
{
    /// <summary>
    /// HUDの各要素を視覚的に配置・調整できるエディタツール
    /// </summary>
    public class HUDLayoutEditor : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool showPlayerInfo = true;
        private bool showMinimap = true;
        private bool showSkillBar = true;
        private bool showQuestPanel = true;
        private bool showSystemPanel = true;
        
        // プレイヤー情報
        private Vector2 playerInfoAnchor = Vector2.zero;
        private Vector2 playerInfoPosition = Vector2.zero;
        private Vector2 playerInfoSize = new Vector2(300, 120);
        
        // ミニマップ
        private Vector2 minimapAnchor = new Vector2(1, 1);
        private Vector2 minimapPosition = new Vector2(-200, 0);
        private Vector2 minimapSize = new Vector2(200, 200);
        
        // スキルバー
        private Vector2 skillBarAnchor = new Vector2(0.5f, 0);
        private Vector2 skillBarPosition = new Vector2(-200, 20);
        private Vector2 skillBarSize = new Vector2(400, 60);
        
        // クエストパネル
        private Vector2 questAnchor = Vector2.zero;
        private Vector2 questPosition = new Vector2(10, 150);
        private Vector2 questSize = new Vector2(250, 140);
        
        // システムパネル
        private Vector2 systemAnchor = new Vector2(1, 0);
        private Vector2 systemPosition = new Vector2(-160, 20);
        private Vector2 systemSize = new Vector2(150, 80);
        
        private GameObject hudCanvas;
        private GameObject[] hudPanels = new GameObject[5];
        
        [MenuItem("Tools/HUD Layout Editor")]
        public static void ShowWindow()
        {
            GetWindow<HUDLayoutEditor>("HUD Layout Editor");
        }
        
        private void OnEnable()
        {
            FindHUDCanvas();
        }
        
        private void FindHUDCanvas()
        {
            // GothicHUD_Canvasも探す
            hudCanvas = GameObject.Find("HUD Canvas");
            if (hudCanvas == null)
            {
                hudCanvas = GameObject.Find("GothicHUD_Canvas");
            }
            
            if (hudCanvas != null)
            {
                var hudPanel = hudCanvas.transform.Find("HUD Panel");
                if (hudPanel != null)
                {
                    hudPanels[0] = hudPanel.Find("PlayerInfoPanel")?.gameObject;
                    hudPanels[1] = hudPanel.Find("MinimapPanel")?.gameObject;
                    hudPanels[2] = hudPanel.Find("SkillBarPanel")?.gameObject;
                    hudPanels[3] = hudPanel.Find("QuestPanel")?.gameObject;
                    hudPanels[4] = hudPanel.Find("SystemPanel")?.gameObject;
                    
                    // 見つからないパネルがあれば警告（GothicHUD_Canvasの場合は警告しない）
                    if (hudCanvas.name == "HUD Canvas")
                    {
                        for (int i = 0; i < hudPanels.Length; i++)
                        {
                            if (hudPanels[i] == null)
                            {
                                Project.Debug.Debug.LogWarning($"HUD Panel {i} not found in HUD Canvas!");
                            }
                        }
                    }
                }
                else if (hudCanvas.name == "HUD Canvas")
                {
                    Project.Debug.Debug.LogWarning("HUD Panel not found in HUD Canvas!");
                }
            }
            // GothicHUD_Canvasを使用している場合は警告を出さない
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("HUD Layout Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (hudCanvas == null)
            {
                EditorGUILayout.HelpBox("HUD Canvas not found! Please create HUD first.", MessageType.Warning);
                if (GUILayout.Button("Create HUD Canvas"))
                {
                    Phase1AutoSetup.SetupHUDCanvas();
                    FindHUDCanvas();
                }
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // プレイヤー情報
            showPlayerInfo = EditorGUILayout.Foldout(showPlayerInfo, "Player Info Panel", true);
            if (showPlayerInfo)
            {
                DrawPanelSettings("Player Info", ref playerInfoAnchor, ref playerInfoPosition, ref playerInfoSize, 0);
            }
            
            // ミニマップ
            showMinimap = EditorGUILayout.Foldout(showMinimap, "Minimap Panel", true);
            if (showMinimap)
            {
                DrawPanelSettings("Minimap", ref minimapAnchor, ref minimapPosition, ref minimapSize, 1);
            }
            
            // スキルバー
            showSkillBar = EditorGUILayout.Foldout(showSkillBar, "Skill Bar Panel", true);
            if (showSkillBar)
            {
                DrawPanelSettings("Skill Bar", ref skillBarAnchor, ref skillBarPosition, ref skillBarSize, 2);
            }
            
            // クエストパネル
            showQuestPanel = EditorGUILayout.Foldout(showQuestPanel, "Quest Panel", true);
            if (showQuestPanel)
            {
                DrawPanelSettings("Quest", ref questAnchor, ref questPosition, ref questSize, 3);
            }
            
            // システムパネル
            showSystemPanel = EditorGUILayout.Foldout(showSystemPanel, "System Panel", true);
            if (showSystemPanel)
            {
                DrawPanelSettings("System", ref systemAnchor, ref systemPosition, ref systemSize, 4);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Changes"))
            {
                ApplyLayoutChanges();
            }
            if (GUILayout.Button("Reset to Default"))
            {
                ResetToDefault();
            }
            if (GUILayout.Button("Restore Missing Panels"))
            {
                RestoreMissingPanels();
            }
            if (GUILayout.Button("Force Recreate HUD"))
            {
                ForceRecreateHUD();
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Save as Preset"))
            {
                SaveLayoutPreset();
            }
            
            if (GUILayout.Button("Load Preset"))
            {
                LoadLayoutPreset();
            }
        }
        
        private void DrawPanelSettings(string panelName, ref Vector2 anchor, ref Vector2 position, ref Vector2 size, int panelIndex)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField($"{panelName} Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anchor:", GUILayout.Width(50));
            anchor = EditorGUILayout.Vector2Field("", anchor);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position:", GUILayout.Width(50));
            position = EditorGUILayout.Vector2Field("", position);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Size:", GUILayout.Width(50));
            size = EditorGUILayout.Vector2Field("", size);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Update Panel"))
            {
                UpdatePanel(panelIndex, anchor, position, size);
            }
            if (GUILayout.Button("Select Panel"))
            {
                SelectPanel(panelIndex);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        private void UpdatePanel(int panelIndex, Vector2 anchor, Vector2 position, Vector2 size)
        {
            if (hudPanels[panelIndex] != null)
            {
                var rectTransform = hudPanels[panelIndex].GetComponent<RectTransform>();
                
                // Anchorを安全に設定（anchorMinとanchorMaxを同じにして固定）
                Vector2 safeAnchor = new Vector2(
                    Mathf.Clamp01(anchor.x), 
                    Mathf.Clamp01(anchor.y)
                );
                rectTransform.anchorMin = safeAnchor;
                rectTransform.anchorMax = safeAnchor;
                
                // PivotをAnchorに合わせる（重要：ズレを防ぐ）
                rectTransform.pivot = safeAnchor;
                
                // Positionを設定（Anchor基準）
                rectTransform.anchoredPosition = position;
                
                // Sizeを設定
                rectTransform.sizeDelta = new Vector2(
                    Mathf.Max(50, size.x), // 最小サイズを保証
                    Mathf.Max(20, size.y)
                );
                
                // パネルが画面内にあることを確認
                ClampPanelToScreen(rectTransform);
                
                // 子要素の位置も再調整
                AdjustChildElements(rectTransform);
                
                EditorUtility.SetDirty(hudPanels[panelIndex]);
                Project.Debug.Debug.Log($"Updated {hudPanels[panelIndex].name} layout - Anchor: {safeAnchor}, Position: {position}, Size: {size}");
            }
            else
            {
                Project.Debug.Debug.LogWarning($"Panel {panelIndex} not found!");
            }
        }
        
        private void AdjustChildElements(RectTransform panelRect)
        {
            // 子要素の位置を調整して背景とズレないようにする
            Vector2 panelAnchor = panelRect.anchorMin;
            
            foreach (Transform child in panelRect)
            {
                var childRect = child.GetComponent<RectTransform>();
                if (childRect != null)
                {
                    // パネルのanchor位置に基づいて子要素のanchorを設定
                    Vector2 childAnchor, childPivot;
                    
                    if (panelAnchor.x >= 0.9f && panelAnchor.y >= 0.9f)
                    {
                        // 右上配置のパネル
                        childAnchor = new Vector2(1, 1); // 右上基準
                        childPivot = new Vector2(1, 1);
                    }
                    else if (panelAnchor.x >= 0.9f)
                    {
                        // 右配置のパネル
                        childAnchor = new Vector2(1, 1); // 右上基準
                        childPivot = new Vector2(1, 1);
                    }
                    else if (panelAnchor.y <= 0.1f)
                    {
                        // 下配置のパネル
                        childAnchor = new Vector2(0, 0); // 左下基準
                        childPivot = new Vector2(0, 0);
                    }
                    else
                    {
                        // その他（左上基準）
                        childAnchor = new Vector2(0, 1);
                        childPivot = new Vector2(0, 1);
                    }
                    
                    childRect.anchorMin = childAnchor;
                    childRect.anchorMax = childAnchor;
                    childRect.pivot = childPivot;
                    
                    Project.Debug.Debug.Log($"Adjusted {child.name} - Anchor: {childAnchor}, Pivot: {childPivot}");
                }
            }
        }
        
        private void ClampPanelToScreen(RectTransform rectTransform)
        {
            // 画面内にパネルをクランプ
            Vector2 anchoredPos = rectTransform.anchoredPosition;
            Vector2 size = rectTransform.sizeDelta;
            Vector2 anchor = rectTransform.anchorMin;
            
            // 画面サイズを取得（Canvasのサイズ）
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                Vector2 canvasSize = canvasRect.sizeDelta;
                
                // Anchor位置に基づいてクランプ
                if (anchor.x == 0) // 左
                    anchoredPos.x = Mathf.Max(0, anchoredPos.x);
                else if (anchor.x == 1) // 右
                    anchoredPos.x = Mathf.Min(0, anchoredPos.x);
                else // 中央
                    anchoredPos.x = Mathf.Clamp(anchoredPos.x, -canvasSize.x/2 + size.x/2, canvasSize.x/2 - size.x/2);
                
                if (anchor.y == 0) // 下
                    anchoredPos.y = Mathf.Max(0, anchoredPos.y);
                else if (anchor.y == 1) // 上
                    anchoredPos.y = Mathf.Min(0, anchoredPos.y);
                else // 中央
                    anchoredPos.y = Mathf.Clamp(anchoredPos.y, -canvasSize.y/2 + size.y/2, canvasSize.y/2 - size.y/2);
                
                rectTransform.anchoredPosition = anchoredPos;
            }
        }
        
        private void SelectPanel(int panelIndex)
        {
            if (hudPanels[panelIndex] != null)
            {
                Selection.activeGameObject = hudPanels[panelIndex];
                EditorGUIUtility.PingObject(hudPanels[panelIndex]);
            }
        }
        
        private void ApplyLayoutChanges()
        {
            UpdatePanel(0, playerInfoAnchor, playerInfoPosition, playerInfoSize);
            UpdatePanel(1, minimapAnchor, minimapPosition, minimapSize);
            UpdatePanel(2, skillBarAnchor, skillBarPosition, skillBarSize);
            UpdatePanel(3, questAnchor, questPosition, questSize);
            UpdatePanel(4, systemAnchor, systemPosition, systemSize);
            
            Project.Debug.Debug.Log("All HUD layout changes applied!");
        }
        
        private void ResetToDefault()
        {
            // デフォルト値にリセット
            playerInfoAnchor = Vector2.zero;
            playerInfoPosition = Vector2.zero;
            playerInfoSize = new Vector2(300, 120);
            
            minimapAnchor = new Vector2(1, 1);
            minimapPosition = new Vector2(-200, 0);
            minimapSize = new Vector2(200, 200);
            
            skillBarAnchor = new Vector2(0.5f, 0);
            skillBarPosition = new Vector2(-200, 20);
            skillBarSize = new Vector2(400, 60);
            
            questAnchor = Vector2.zero;
            questPosition = new Vector2(10, 150);
            questSize = new Vector2(250, 140);
            
            systemAnchor = new Vector2(1, 0);
            systemPosition = new Vector2(-160, 20);
            systemSize = new Vector2(150, 80);
            
            ApplyLayoutChanges();
        }
        
        private void RestoreMissingPanels()
        {
            if (hudCanvas == null)
            {
                Project.Debug.Debug.LogError("HUD Canvas not found!");
                return;
            }
            
            var hudPanel = hudCanvas.transform.Find("HUD Panel");
            if (hudPanel == null)
            {
                Project.Debug.Debug.LogError("HUD Panel not found!");
                return;
            }
            
            // 各パネルを復元
            string[] panelNames = { "PlayerInfoPanel", "MinimapPanel", "SkillBarPanel", "QuestPanel", "SystemPanel" };
            
            for (int i = 0; i < panelNames.Length; i++)
            {
                if (hudPanels[i] == null)
                {
                    Project.Debug.Debug.Log($"Restoring missing panel: {panelNames[i]}");
                    
                    // 新しいパネルを作成
                    GameObject newPanel = new GameObject(panelNames[i]);
                    newPanel.transform.SetParent(hudPanel, false);
                    newPanel.SetActive(true); // アクティブに設定
                    
                    var rectTransform = newPanel.AddComponent<RectTransform>();
                    var image = newPanel.AddComponent<Image>();
                    image.color = new Color(0, 0, 0, 0.5f);
                    
                    // デフォルト位置に設定
                    switch (i)
                    {
                        case 0: // PlayerInfoPanel
                            rectTransform.anchorMin = Vector2.zero;
                            rectTransform.anchorMax = Vector2.zero;
                            rectTransform.pivot = Vector2.zero;
                            rectTransform.anchoredPosition = Vector2.zero;
                            rectTransform.sizeDelta = new Vector2(300, 120);
                            break;
                        case 1: // MinimapPanel
                            rectTransform.anchorMin = new Vector2(1, 1);
                            rectTransform.anchorMax = new Vector2(1, 1);
                            rectTransform.pivot = new Vector2(1, 1);
                            rectTransform.anchoredPosition = new Vector2(-200, 0);
                            rectTransform.sizeDelta = new Vector2(200, 200);
                            break;
                        case 2: // SkillBarPanel
                            rectTransform.anchorMin = new Vector2(0.5f, 0);
                            rectTransform.anchorMax = new Vector2(0.5f, 0);
                            rectTransform.pivot = new Vector2(0.5f, 0);
                            rectTransform.anchoredPosition = new Vector2(-200, 20);
                            rectTransform.sizeDelta = new Vector2(400, 60);
                            break;
                        case 3: // QuestPanel
                            rectTransform.anchorMin = Vector2.zero;
                            rectTransform.anchorMax = Vector2.zero;
                            rectTransform.pivot = Vector2.zero;
                            rectTransform.anchoredPosition = new Vector2(10, 150);
                            rectTransform.sizeDelta = new Vector2(250, 140);
                            break;
                        case 4: // SystemPanel
                            rectTransform.anchorMin = new Vector2(1, 0);
                            rectTransform.anchorMax = new Vector2(1, 0);
                            rectTransform.pivot = new Vector2(1, 0);
                            rectTransform.anchoredPosition = new Vector2(-160, 20);
                            rectTransform.sizeDelta = new Vector2(150, 80);
                            break;
                    }
                    
                    // 基本的なUI要素を追加（空のパネルでも表示されるように）
                    AddBasicUIElements(newPanel, i);
                    
                    hudPanels[i] = newPanel;
                    EditorUtility.SetDirty(newPanel);
                }
                else
                {
                    // 既存のパネルもアクティブに設定
                    hudPanels[i].SetActive(true);
                }
            }
            
            // HUD PanelとHUD Canvasもアクティブに設定
            hudPanel.gameObject.SetActive(true);
            hudCanvas.SetActive(true);
            
            // Canvasの設定を確認
            var canvas = hudCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                Project.Debug.Debug.Log($"Canvas render mode: {canvas.renderMode}, Pixel Perfect: {canvas.pixelPerfect}");
            }
            
            // Canvas Scalerも確認
            var canvasScaler = hudCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                Project.Debug.Debug.Log($"Canvas Scaler UI Scale Mode: {canvasScaler.uiScaleMode}");
            }
            
            FindHUDCanvas(); // 再検索
            Project.Debug.Debug.Log("Missing panels restored and activated!");
        }
        
        private void ForceRecreateHUD()
        {
            Project.Debug.Debug.Log("Force recreating entire HUD...");
            
            // 既存のHUD Canvasを削除
            if (hudCanvas != null)
            {
                DestroyImmediate(hudCanvas);
            }
            
            // 新しいHUDを作成
            Phase1AutoSetup.SetupHUDCanvas();
            
            // 再検索
            FindHUDCanvas();
            
            if (hudCanvas != null)
            {
                Project.Debug.Debug.Log("HUD force recreated successfully!");
                
                // すべてのパネルをアクティブに設定
                for (int i = 0; i < hudPanels.Length; i++)
                {
                    if (hudPanels[i] != null)
                    {
                        hudPanels[i].SetActive(true);
                    }
                }
                
                // Canvasもアクティブに設定
                hudCanvas.SetActive(true);
                
                var canvas = hudCanvas.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.enabled = true;
                }
            }
            else
            {
                Project.Debug.Debug.LogError("Failed to recreate HUD!");
            }
        }
        
        private void AddBasicUIElements(GameObject panel, int panelType)
        {
            // 各パネルに基本的なUI要素を追加して表示を保証
            switch (panelType)
            {
                case 0: // PlayerInfoPanel
                    CreateHUDText(panel.transform, "PlayerNameText", "Player Name", 
                        new Vector2(10, -10), 22, TextAnchor.UpperLeft, Color.white);
                    CreateHUDText(panel.transform, "LevelClassText", "Lv.1 Warrior", 
                        new Vector2(10, -40), 18, TextAnchor.UpperLeft, Color.yellow);
                    break;
                    
                case 1: // MinimapPanel
                    CreateHUDText(panel.transform, "MinapLabel", "MINIMAP", 
                        new Vector2(0, -10), 14, TextAnchor.UpperCenter, Color.gray);
                    break;
                    
                case 2: // SkillBarPanel
                    for (int i = 0; i < 6; i++)
                    {
                        CreateSkillSlot(panel.transform, $"SkillSlot{i+1}", 
                            new Vector2(10 + i * 65, 10), new Vector2(50, 50));
                    }
                    break;
                    
                case 3: // QuestPanel
                    CreateHUDText(panel.transform, "QuestTitle", "CURRENT QUESTS", 
                        new Vector2(10, -10), 14, TextAnchor.UpperLeft, Color.cyan);
                    break;
                    
                case 4: // SystemPanel
                    CreateHUDText(panel.transform, "FPSText", "60 FPS", 
                        new Vector2(0, -10), 12, TextAnchor.UpperRight, Color.green);
                    CreateHUDText(panel.transform, "TimeText", "12:00", 
                        new Vector2(0, -30), 12, TextAnchor.UpperRight, Color.white);
                    CreateHUDText(panel.transform, "PingText", "Ping: 25ms", 
                        new Vector2(0, -50), 12, TextAnchor.UpperRight, Color.yellow);
                    break;
            }
        }
        
        private static GameObject CreateHUDText(Transform parent, string name, string defaultText, Vector2 position, int fontSize, TextAnchor anchor = TextAnchor.UpperLeft, Color color = default(Color))
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            var rectTransform = textGO.AddComponent<RectTransform>();
            
            // 親パネルのanchorに合わせて子要素のanchorを設定
            var parentRect = parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                Vector2 parentAnchor = parentRect.anchorMin;
                Vector2 childAnchor, childPivot;
                
                if (parentAnchor.x >= 0.9f && parentAnchor.y >= 0.9f)
                {
                    // 右上配置のパネル
                    childAnchor = new Vector2(1, 1); // 右上基準
                    childPivot = new Vector2(1, 1);
                }
                else if (parentAnchor.x >= 0.9f)
                {
                    // 右配置のパネル
                    childAnchor = new Vector2(1, 1); // 右上基準
                    childPivot = new Vector2(1, 1);
                }
                else if (parentAnchor.y <= 0.1f)
                {
                    // 下配置のパネル
                    childAnchor = new Vector2(0, 0); // 左下基準
                    childPivot = new Vector2(0, 0);
                }
                else
                {
                    // その他（左上基準）
                    childAnchor = new Vector2(0, 1);
                    childPivot = new Vector2(0, 1);
                }
                
                rectTransform.anchorMin = childAnchor;
                rectTransform.anchorMax = childAnchor;
                rectTransform.pivot = childPivot;
                
                Project.Debug.Debug.Log($"Created {name} - Parent Anchor: {parentAnchor}, Child Anchor: {childAnchor}");
            }
            else
            {
                // デフォルト設定
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.zero;
                rectTransform.pivot = Vector2.zero;
            }
            
            // Positionを設定
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
        
        private static GameObject CreateSkillSlot(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject slotGO = new GameObject(name);
            slotGO.transform.SetParent(parent, false);
            
            var rectTransform = slotGO.AddComponent<RectTransform>();
            
            // 親パネルのanchorに合わせて子要素のanchorを設定
            var parentRect = parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                // 親のanchor位置に基づいて子のanchorを設定
                rectTransform.anchorMin = new Vector2(0, 1); // 左上基準
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1); // 左上をpivotに
            }
            else
            {
                // デフォルト設定
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.zero;
                rectTransform.pivot = Vector2.zero;
            }
            
            // Positionを設定（左上基準の座標系）
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
            numberRect.anchorMin = new Vector2(0, 1); // 左上基準
            numberRect.anchorMax = new Vector2(0, 1);
            numberRect.pivot = new Vector2(0, 1);
            numberRect.anchoredPosition = new Vector2(5, -5); // 左上基準で調整
            numberRect.sizeDelta = new Vector2(20, 20);
            
            var numberText = numberGO.AddComponent<TextMeshProUGUI>();
            numberText.text = name.Replace("SkillSlot", "");
            numberText.fontSize = 10;
            numberText.color = Color.white;
            numberText.alignment = TextAlignmentOptions.Left;
            
            return slotGO;
        }
        
        private void SaveLayoutPreset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save HUD Layout Preset",
                "HUDLayoutPreset.asset",
                "asset",
                "Choose where to save the HUD layout preset"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                HUDLayoutPreset preset = ScriptableObject.CreateInstance<HUDLayoutPreset>();
                preset.SaveLayout(
                    playerInfoAnchor, playerInfoPosition, playerInfoSize,
                    minimapAnchor, minimapPosition, minimapSize,
                    skillBarAnchor, skillBarPosition, skillBarSize,
                    questAnchor, questPosition, questSize,
                    systemAnchor, systemPosition, systemSize
                );
                
                AssetDatabase.CreateAsset(preset, path);
                AssetDatabase.SaveAssets();
                Project.Debug.Debug.Log($"HUD layout preset saved to: {path}");
            }
        }
        
        private void LoadLayoutPreset()
        {
            string path = EditorUtility.OpenFilePanel(
                "Load HUD Layout Preset",
                "Assets",
                "asset"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Replace(Application.dataPath, "Assets");
                HUDLayoutPreset preset = AssetDatabase.LoadAssetAtPath<HUDLayoutPreset>(path);
                
                if (preset != null)
                {
                    preset.LoadLayout(
                        out playerInfoAnchor, out playerInfoPosition, out playerInfoSize,
                        out minimapAnchor, out minimapPosition, out minimapSize,
                        out skillBarAnchor, out skillBarPosition, out skillBarSize,
                        out questAnchor, out questPosition, out questSize,
                        out systemAnchor, out systemPosition, out systemSize
                    );
                    
                    ApplyLayoutChanges();
                    Project.Debug.Debug.Log($"HUD layout preset loaded from: {path}");
                }
            }
        }
    }
    
    /// <summary>
    /// HUDレイアウトプリセットデータ
    /// </summary>
    public class HUDLayoutPreset : ScriptableObject
    {
        [System.Serializable]
        public class PanelLayout
        {
            public Vector2 anchor;
            public Vector2 position;
            public Vector2 size;
        }
        
        public PanelLayout playerInfo;
        public PanelLayout minimap;
        public PanelLayout skillBar;
        public PanelLayout quest;
        public PanelLayout system;
        
        public void SaveLayout(
            Vector2 playerInfoAnchor, Vector2 playerInfoPos, Vector2 playerInfoSize,
            Vector2 minimapAnchor, Vector2 minimapPos, Vector2 minimapSize,
            Vector2 skillBarAnchor, Vector2 skillBarPos, Vector2 skillBarSize,
            Vector2 questAnchor, Vector2 questPos, Vector2 questSize,
            Vector2 systemAnchor, Vector2 systemPos, Vector2 systemSize)
        {
            playerInfo = new PanelLayout { anchor = playerInfoAnchor, position = playerInfoPos, size = playerInfoSize };
            minimap = new PanelLayout { anchor = minimapAnchor, position = minimapPos, size = minimapSize };
            skillBar = new PanelLayout { anchor = skillBarAnchor, position = skillBarPos, size = skillBarSize };
            quest = new PanelLayout { anchor = questAnchor, position = questPos, size = questSize };
            system = new PanelLayout { anchor = systemAnchor, position = systemPos, size = systemSize };
        }
        
        public void LoadLayout(
            out Vector2 playerInfoAnchor, out Vector2 playerInfoPos, out Vector2 playerInfoSize,
            out Vector2 minimapAnchor, out Vector2 minimapPos, out Vector2 minimapSize,
            out Vector2 skillBarAnchor, out Vector2 skillBarPos, out Vector2 skillBarSize,
            out Vector2 questAnchor, out Vector2 questPos, out Vector2 questSize,
            out Vector2 systemAnchor, out Vector2 systemPos, out Vector2 systemSize)
        {
            playerInfoAnchor = playerInfo.anchor;
            playerInfoPos = playerInfo.position;
            playerInfoSize = playerInfo.size;
            
            minimapAnchor = minimap.anchor;
            minimapPos = minimap.position;
            minimapSize = minimap.size;
            
            skillBarAnchor = skillBar.anchor;
            skillBarPos = skillBar.position;
            skillBarSize = skillBar.size;
            
            questAnchor = quest.anchor;
            questPos = quest.position;
            questSize = quest.size;
            
            systemAnchor = system.anchor;
            systemPos = system.position;
            systemSize = system.size;
        }
    }
}

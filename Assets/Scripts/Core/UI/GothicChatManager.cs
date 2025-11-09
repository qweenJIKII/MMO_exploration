// GothicChatManager: チャットメッセージ管理
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Project.Core.UI
{
    /// <summary>
    /// チャットメッセージの表示と管理
    /// </summary>
    public class GothicChatManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform messageContainer;
        [SerializeField] private TMP_InputField inputField;

        [Header("Settings")]
        [SerializeField] private int maxMessages = 50;
        [SerializeField] private GameObject messageTextPrefab;

        private List<GameObject> messageObjects = new List<GameObject>();

        private void Start()
        {
            Debug.Log($"[GothicChatManager] Start called. InputField: {inputField != null}, ScrollRect: {scrollRect != null}, Container: {messageContainer != null}");
            
            // InputFieldのEnterキーでメッセージ送信
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(OnSubmitMessage);
                inputField.onValueChanged.AddListener(OnInputValueChanged);
                
                // InputFieldを有効化
                inputField.interactable = true;
                inputField.readOnly = false;
                
                // InputFieldのTextAreaとTextコンポーネントを確認
                if (inputField.textComponent != null)
                {
                    inputField.textComponent.raycastTarget = false;
                    Debug.Log($"[GothicChatManager] Text component: {inputField.textComponent.name}");
                }
                else
                {
                    Debug.LogError("[GothicChatManager] Text component is NULL!");
                }
                
                // TextViewportを確認
                if (inputField.textViewport != null)
                {
                    Debug.Log($"[GothicChatManager] Text viewport: {inputField.textViewport.name}");
                }
                else
                {
                    Debug.LogError("[GothicChatManager] Text viewport is NULL!");
                }
                
                // Placeholderを確認
                if (inputField.placeholder != null)
                {
                    inputField.placeholder.raycastTarget = false;
                }
                
                // InputFieldの背景Imageを確認
                Image bgImage = inputField.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.raycastTarget = true;
                    Debug.Log($"[GothicChatManager] Background Image raycastTarget: {bgImage.raycastTarget}");
                }
                
                // InputFieldのRectTransformを確認
                RectTransform rectTransform = inputField.GetComponent<RectTransform>();
                Debug.Log($"[GothicChatManager] InputField size: {rectTransform.rect.size}");
                
                // InputFieldのタイプを確認
                Debug.Log($"[GothicChatManager] InputField type: {inputField.contentType}, LineType: {inputField.lineType}");
                
                Debug.Log($"[GothicChatManager] InputField configured. Interactable: {inputField.interactable}, ReadOnly: {inputField.readOnly}");
            }
            else
            {
                Debug.LogError("[GothicChatManager] InputField is NULL!");
            }

            // テスト用のウェルカムメッセージ
            AddMessage("System", "Welcome to the game!", Color.yellow);
            AddMessage("System", "Press Enter to start typing...", Color.cyan);
        }
        
        private void OnInputValueChanged(string value)
        {
            Debug.Log($"[GothicChatManager] Input value changed: {value}");
        }
        
        private void Update()
        {
            // Enterキーでチャット入力をアクティブ化（New Input System）
            if (Keyboard.current != null)
            {
                if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                {
                    if (inputField != null && !inputField.isFocused)
                    {
                        FocusInputField();
                    }
                }
            }
        }
        
        /// <summary>
        /// InputFieldをクリックしてフォーカス
        /// </summary>
        public void FocusInputField()
        {
            if (inputField != null)
            {
                // 複数回試行してアクティブ化
                StartCoroutine(ActivateInputFieldCoroutine());
            }
        }
        
        private System.Collections.IEnumerator ActivateInputFieldCoroutine()
        {
            if (inputField == null) yield break;
            
            // フレームを待つ
            yield return null;
            
            // InputFieldをアクティブ化
            inputField.ActivateInputField();
            inputField.Select();
            
            Debug.Log($"[GothicChatManager] InputField activated. IsFocused: {inputField.isFocused}, Interactable: {inputField.interactable}");
            
            // もう一度試行
            yield return new WaitForSeconds(0.1f);
            
            if (!inputField.isFocused)
            {
                inputField.ActivateInputField();
                inputField.Select();
                Debug.Log($"[GothicChatManager] InputField re-activated. IsFocused: {inputField.isFocused}");
            }
        }
        
        /// <summary>
        /// デバッグ用：強制的にInputFieldをアクティブ化
        /// </summary>
        [ContextMenu("Test: Focus Input Field")]
        private void TestFocusInputField()
        {
            FocusInputField();
        }

        /// <summary>
        /// メッセージを追加
        /// </summary>
        public void AddMessage(string sender, string message, Color color)
        {
            if (messageContainer == null) return;

            // メッセージテキストを作成
            GameObject messageObj = new GameObject($"Message_{messageObjects.Count}");
            messageObj.transform.SetParent(messageContainer, false);

            RectTransform rect = messageObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(0, 20);

            TextMeshProUGUI text = messageObj.AddComponent<TextMeshProUGUI>();
            text.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>[{sender}]</color> {message}";
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.textWrappingMode = TMPro.TextWrappingModes.Normal;

            // ContentSizeFitterを追加して自動サイズ調整
            ContentSizeFitter fitter = messageObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            messageObjects.Add(messageObj);

            // 最大メッセージ数を超えたら古いものを削除
            if (messageObjects.Count > maxMessages)
            {
                GameObject oldMessage = messageObjects[0];
                messageObjects.RemoveAt(0);
                Destroy(oldMessage);
            }

            // スクロールを最下部に
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// メッセージ送信
        /// </summary>
        private void OnSubmitMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            // プレイヤーのメッセージとして追加
            AddMessage("Player", message, Color.cyan);

            // InputFieldをクリア
            inputField.text = "";
            inputField.ActivateInputField();
        }

        /// <summary>
        /// システムメッセージを追加
        /// </summary>
        public void AddSystemMessage(string message)
        {
            AddMessage("System", message, Color.yellow);
        }

        #region Debug Context Menu
#if UNITY_EDITOR
        [ContextMenu("Test: Add Chat Message")]
        private void TestAddMessage()
        {
            AddMessage("Test", "This is a test message!", Color.green);
        }
#endif
        #endregion
    }
}

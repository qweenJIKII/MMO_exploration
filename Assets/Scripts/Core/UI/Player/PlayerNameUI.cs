// PlayerNameUI: プレイヤー名設定UI
using UnityEngine;
using UnityEngine.UIElements;
using Project.Core.Player;
using System.Threading.Tasks;

namespace Project.Core.UI.Player
{
    /// <summary>
    /// プレイヤー名設定UI
    /// Phase 1実装: 初回ログイン時の名前入力、名前変更機能
    /// </summary>
    public class PlayerNameUI : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Settings")]
        [SerializeField] private int minNameLength = 3;
        [SerializeField] private int maxNameLength = 16;

        private VisualElement root;
        private TextField nameInputField;
        private Button confirmButton;
        private Button cancelButton;
        private Label errorLabel;
        private VisualElement namePanel;

        private System.Action<string> onNameConfirmed;
        private bool isWaitingForInput = false;

        private void Awake()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[PlayerNameUI] UIDocument reference is missing!");
                return;
            }

            InitializeUI();
        }

        private void InitializeUI()
        {
            root = uiDocument.rootVisualElement;

            // UI要素を取得
            namePanel = root.Q("player-name-panel");
            nameInputField = root.Q<TextField>("name-input");
            confirmButton = root.Q<Button>("confirm-button");
            cancelButton = root.Q<Button>("cancel-button");
            errorLabel = root.Q<Label>("error-label");

            // イベント登録
            if (confirmButton != null)
            {
                confirmButton.clicked += OnConfirmClicked;
            }

            if (cancelButton != null)
            {
                cancelButton.clicked += OnCancelClicked;
            }

            if (nameInputField != null)
            {
                nameInputField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            }

            // 初期状態は非表示
            Hide();
        }

        /// <summary>
        /// 名前入力UIを表示
        /// </summary>
        public Task<string> ShowAsync(string currentName = "")
        {
            var tcs = new TaskCompletionSource<string>();

            Show(currentName, (name) =>
            {
                tcs.SetResult(name);
            });

            return tcs.Task;
        }

        /// <summary>
        /// 名前入力UIを表示（コールバック版）
        /// </summary>
        public void Show(string currentName = "", System.Action<string> onConfirmed = null)
        {
            if (namePanel == null) return;

            namePanel.style.display = DisplayStyle.Flex;
            isWaitingForInput = true;
            onNameConfirmed = onConfirmed;

            // 現在の名前を設定
            if (nameInputField != null)
            {
                nameInputField.value = currentName;
                nameInputField.Focus();
            }

            // エラーメッセージをクリア
            if (errorLabel != null)
            {
                errorLabel.text = "";
                errorLabel.style.display = DisplayStyle.None;
            }

            Debug.Log("[PlayerNameUI] Name input UI shown");
        }

        /// <summary>
        /// 名前入力UIを非表示
        /// </summary>
        public void Hide()
        {
            if (namePanel != null)
            {
                namePanel.style.display = DisplayStyle.None;
            }
            isWaitingForInput = false;
        }

        private void OnConfirmClicked()
        {
            if (!isWaitingForInput) return;

            string playerName = nameInputField?.value ?? "";
            
            if (ValidateName(playerName, out string errorMessage))
            {
                // 名前が有効
                Hide();
                onNameConfirmed?.Invoke(playerName);
                Debug.Log($"[PlayerNameUI] Name confirmed: {playerName}");
            }
            else
            {
                // エラー表示
                ShowError(errorMessage);
            }
        }

        private void OnCancelClicked()
        {
            if (!isWaitingForInput) return;

            Hide();
            onNameConfirmed?.Invoke(null);
            Debug.Log("[PlayerNameUI] Name input cancelled");
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                OnConfirmClicked();
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                OnCancelClicked();
                evt.StopPropagation();
            }
        }

        /// <summary>
        /// 名前のバリデーション
        /// </summary>
        private bool ValidateName(string name, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "名前を入力してください";
                return false;
            }

            if (name.Length < minNameLength)
            {
                errorMessage = $"名前は{minNameLength}文字以上で入力してください";
                return false;
            }

            if (name.Length > maxNameLength)
            {
                errorMessage = $"名前は{maxNameLength}文字以内で入力してください";
                return false;
            }

            // 使用禁止文字チェック
            if (ContainsInvalidCharacters(name))
            {
                errorMessage = "使用できない文字が含まれています";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 使用禁止文字が含まれているかチェック
        /// </summary>
        private bool ContainsInvalidCharacters(string name)
        {
            // 基本的な禁止文字（拡張可能）
            char[] invalidChars = { '<', '>', '/', '\\', '|', ':', '*', '?', '"' };
            
            foreach (char c in invalidChars)
            {
                if (name.Contains(c))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// エラーメッセージを表示
        /// </summary>
        private void ShowError(string message)
        {
            if (errorLabel != null)
            {
                errorLabel.text = message;
                errorLabel.style.display = DisplayStyle.Flex;
            }
        }

        /// <summary>
        /// 名前の重複チェック（Cloud Code経由）
        /// </summary>
        public async Task<bool> CheckNameAvailability(string name)
        {
            // TODO: Cloud Code関数を呼び出して名前の重複チェック
            // 現在は常にtrueを返す（Phase 4で実装予定）
            await Task.Delay(100); // ネットワーク遅延をシミュレート
            
            Debug.Log($"[PlayerNameUI] Checking name availability: {name}");
            return true;
        }

#if UNITY_EDITOR
        [ContextMenu("Test: Show Name Input")]
        private void TestShowNameInput()
        {
            Show("TestPlayer", (name) =>
            {
                Debug.Log($"Name confirmed: {name}");
            });
        }
#endif
    }
}

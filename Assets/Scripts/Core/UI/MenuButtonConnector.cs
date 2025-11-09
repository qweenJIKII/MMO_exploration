// MenuButtonConnector: ポーズメニューのボタンを自動接続
using UnityEngine;
using UnityEngine.UI;

namespace Project.Core.UI
{
    /// <summary>
    /// ポーズメニューのボタンをMenuManagerに自動接続
    /// Startで実行されるため、手動設定不要
    /// </summary>
    [RequireComponent(typeof(MenuManager))]
    public class MenuButtonConnector : MonoBehaviour
    {
        private MenuManager menuManager;

        private void Awake()
        {
            menuManager = GetComponent<MenuManager>();
            ConnectButtons();
        }

        private void ConnectButtons()
        {
            // システムメニューのボタン（必須）
            ConnectButton("PauseMenu/MenuPanel/Button_Settings", menuManager.OpenSettingsMenu, required: true);
            
            // オプションのボタン（警告を出さない）
            ConnectButton("PauseMenu/MenuPanel/Button_Help", menuManager.OpenHelp, required: false);
            ConnectButton("PauseMenu/MenuPanel/Button_Logout", menuManager.ShowQuitConfirmation, required: false);

            // 設定メニューのボタン（必須）
            ConnectButton("SettingsMenu/Panel/Button_Back", menuManager.CloseSettingsMenu, required: true);

            // ログアウト確認のボタン（必須）
            ConnectButton("ConfirmQuitPanel/Dialog/Button_Yes", menuManager.QuitGame, required: true);
            ConnectButton("ConfirmQuitPanel/Dialog/Button_No", menuManager.CancelQuit, required: true);

            Debug.Log("[MenuButtonConnector] ボタン接続完了");
        }

        private void ConnectButton(string path, UnityEngine.Events.UnityAction action, bool required = true)
        {
            Transform buttonTransform = transform.Find(path);
            if (buttonTransform != null)
            {
                Button button = buttonTransform.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(action);
                    Debug.Log($"[MenuButtonConnector] 接続成功: {path}");
                }
                else
                {
                    Debug.LogWarning($"[MenuButtonConnector] Buttonコンポーネントが見つかりません: {path}");
                }
            }
            else
            {
                // requiredがtrueの場合のみ警告を出す
                if (required)
                {
                    Debug.LogWarning($"[MenuButtonConnector] ボタンが見つかりません: {path}");
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test: Reconnect All Buttons")]
        private void TestReconnect()
        {
            menuManager = GetComponent<MenuManager>();
            ConnectButtons();
        }
#endif
    }
}

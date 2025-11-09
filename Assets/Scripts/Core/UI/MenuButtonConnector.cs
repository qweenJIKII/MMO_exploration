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
            // システムメニューのボタン
            ConnectButton("PauseMenu/MenuPanel/Button_Settings", menuManager.OpenSettingsMenu);
            ConnectButton("PauseMenu/MenuPanel/Button_Help", menuManager.OpenHelp);
            ConnectButton("PauseMenu/MenuPanel/Button_Logout", menuManager.ShowQuitConfirmation);

            // 設定メニューのボタン
            ConnectButton("SettingsMenu/Panel/Button_Back", menuManager.CloseSettingsMenu);

            // ログアウト確認のボタン
            ConnectButton("ConfirmQuitPanel/Dialog/Button_Yes", menuManager.QuitGame);
            ConnectButton("ConfirmQuitPanel/Dialog/Button_No", menuManager.CancelQuit);

            Debug.Log("[MenuButtonConnector] すべてのボタンを接続しました（システムメニュー）");
        }

        private void ConnectButton(string path, UnityEngine.Events.UnityAction action)
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
                Debug.LogWarning($"[MenuButtonConnector] ボタンが見つかりません: {path}");
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

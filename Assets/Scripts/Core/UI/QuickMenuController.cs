// QuickMenuController: 既存のQuickMenuに機能を追加
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Project.Core.UI
{
    /// <summary>
    /// 既存のQuickMenu（チャット上のボタン群）に機能を接続
    /// ホットキー対応: C, I, Q, M
    /// </summary>
    public class QuickMenuController : MonoBehaviour
    {
        [Header("Quick Menu Buttons")]
        [SerializeField] private Button characterButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private Button mailButton;
        [SerializeField] private Button optionsButton;

        [Header("Panels (Phase 3で実装予定)")]
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject questPanel;
        [SerializeField] private GameObject mapPanel;

        [Header("Quick Options Menu")]
        [SerializeField] private GameObject quickOptionsPanel;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Button closeOptionsButton;

        private Keyboard keyboard;
        private GameObject currentOpenPanel;

        private void Start()
        {
            keyboard = Keyboard.current;

            // ボタンにイベントを接続
            if (characterButton != null)
            {
                characterButton.onClick.AddListener(ToggleCharacter);
                Debug.Log("[QuickMenuController] Character button connected");
            }
            
            if (inventoryButton != null)
            {
                inventoryButton.onClick.AddListener(ToggleInventory);
                Debug.Log("[QuickMenuController] Inventory button connected");
            }
            
            if (questButton != null)
            {
                questButton.onClick.AddListener(ToggleQuest);
                Debug.Log("[QuickMenuController] Quest button connected");
            }
            
            if (mapButton != null)
            {
                mapButton.onClick.AddListener(ToggleMap);
                Debug.Log("[QuickMenuController] Map button connected");
            }

            if (mailButton != null)
            {
                mailButton.onClick.AddListener(ToggleMail);
                Debug.Log("[QuickMenuController] Mail button connected");
            }
            else
            {
                Debug.LogWarning("[QuickMenuController] Mail button is not assigned!");
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(ToggleQuickOptions);
                Debug.Log("[QuickMenuController] Options button connected");
            }
            else
            {
                Debug.LogWarning("[QuickMenuController] Options button is not assigned!");
            }

            // クイックオプションメニューのボタン接続
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OpenSettings);
            }
            
            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(Logout);
            }
            
            if (closeOptionsButton != null)
            {
                closeOptionsButton.onClick.AddListener(() => CloseQuickOptions());
            }

            // 初期状態: クイックオプションメニューを非表示
            if (quickOptionsPanel != null)
            {
                quickOptionsPanel.SetActive(false);
            }

            Debug.Log("[QuickMenuController] 初期化完了");
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (keyboard == null)
            {
                keyboard = Keyboard.current;
                if (keyboard == null) return;
            }

            // ホットキー
            if (keyboard.cKey.wasPressedThisFrame)
                ToggleCharacter();
            else if (keyboard.iKey.wasPressedThisFrame)
                ToggleInventory();
            else if (keyboard.qKey.wasPressedThisFrame)
                ToggleQuest();
            else if (keyboard.mKey.wasPressedThisFrame)
                ToggleMap();
        }

        public void ToggleCharacter()
        {
            Debug.Log("[QuickMenuController] Character (C) - Phase 3で実装予定");
            // TODO: Phase 3で実装
        }

        public void ToggleInventory()
        {
            Debug.Log("[QuickMenuController] Inventory (I) - Phase 3で実装予定");
            // TODO: Phase 3で実装
        }

        public void ToggleQuest()
        {
            Debug.Log("[QuickMenuController] Quest (Q) - Phase 4で実装予定");
            // TODO: Phase 4で実装
        }

        public void ToggleMap()
        {
            Debug.Log("[QuickMenuController] Map (M) - Phase 4で実装予定");
            // TODO: Phase 4で実装
        }

        public void ToggleMail()
        {
            Debug.Log("[QuickMenuController] Mail - Phase 4で実装予定");
            // TODO: Phase 4で実装（メールボックス機能）
        }

        #region Quick Options Menu
        /// <summary>
        /// クイックオプションメニューを開閉
        /// </summary>
        public void ToggleQuickOptions()
        {
            if (quickOptionsPanel != null)
            {
                bool isActive = quickOptionsPanel.activeSelf;
                quickOptionsPanel.SetActive(!isActive);
                
                if (!isActive)
                {
                    Debug.Log("[QuickMenuController] クイックオプションメニューを開きました");
                    // カーソルを表示
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Debug.Log("[QuickMenuController] クイックオプションメニューを閉じました");
                    // 他のメニューが開いていない場合のみカーソルを非表示
                    MenuManager menuManager = FindFirstObjectByType<MenuManager>();
                    if (menuManager != null && !menuManager.IsAnyMenuOpen())
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[QuickMenuController] Quick Options Panel is not assigned!");
            }
        }

        /// <summary>
        /// クイックオプションメニューを閉じる
        /// </summary>
        /// <param name="keepCursor">カーソルの表示状態を維持するか（他のメニューに遷移する場合はtrue）</param>
        public void CloseQuickOptions(bool keepCursor = false)
        {
            if (quickOptionsPanel != null)
            {
                quickOptionsPanel.SetActive(false);
                Debug.Log("[QuickMenuController] クイックオプションメニューを閉じました");
                
                // keepCursorがtrueの場合はカーソルを維持
                if (keepCursor)
                {
                    Debug.Log("[QuickMenuController] カーソルを維持します（他のメニューに遷移）");
                    return;
                }
                
                // 他のメニューが開いていない場合のみカーソルを非表示
                MenuManager menuManager = FindFirstObjectByType<MenuManager>();
                if (menuManager != null && !menuManager.IsAnyMenuOpen())
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        /// <summary>
        /// 設定メニューを開く
        /// </summary>
        public void OpenSettings()
        {
            Debug.Log("[QuickMenuController] Settings - Opening settings menu");
            
            // クイックオプションメニューを閉じる（カーソルは維持）
            CloseQuickOptions(keepCursor: true);
            
            // MenuManagerの設定メニューを開く
            MenuManager menuManager = FindFirstObjectByType<MenuManager>();
            if (menuManager != null)
            {
                menuManager.OpenSettingsMenu();
            }
            else
            {
                Debug.LogWarning("[QuickMenuController] MenuManager not found!");
            }
        }

        /// <summary>
        /// ログアウト（タイトル画面に戻る）
        /// </summary>
        public async void Logout()
        {
            Debug.Log("[QuickMenuController] Logout - Returning to title");
            
            // クイックオプションメニューを閉じる（ログアウト処理中はカーソル維持）
            CloseQuickOptions(keepCursor: true);
            
            // MenuManagerのログアウト処理を実行
            MenuManager menuManager = FindFirstObjectByType<MenuManager>();
            if (menuManager != null)
            {
                menuManager.ReturnToTitle();
            }
            else
            {
                Debug.LogWarning("[QuickMenuController] MenuManager not found! Executing direct logout.");
                
                // MenuManagerが見つからない場合は直接セーブ&ログアウト
                var saveManager = Save.SaveManager.Instance;
                if (saveManager != null)
                {
                    Debug.Log("[QuickMenuController] ログアウト前にセーブを実行");
                    await saveManager.SaveGame();
                }
                
                // タイトルシーンに遷移
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
        #endregion
    }
}

// MenuManager: メニュー管理
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Project.Core.UI
{
    /// <summary>
    /// ポーズメニュー・設定メニューの管理
    /// ESCキーでポーズメニューを開閉
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject settingsMenuPanel;
        [SerializeField] private GameObject confirmQuitPanel;

        [Header("Settings")]
        [SerializeField] private bool showCursorWhenMenuOpen = true;

        private bool isMenuOpen = false;
        private bool isInSettingsMenu = false;
        private bool isCursorVisible = false; // カーソル表示状態
        
        // Input System
        private Keyboard keyboard;

        private void Start()
        {
            // 初期状態: すべてのメニューを非表示
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            
            if (settingsMenuPanel != null)
                settingsMenuPanel.SetActive(false);
            
            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(false);

            // Input Systemの初期化
            keyboard = Keyboard.current;

            // 初期状態: カーソル非表示
            SetCursorVisibility(false);

            Debug.Log("[MenuManager] 初期化完了");
        }

        private void Update()
        {
            // エディターモードでは動作しない（シーン編集の邪魔にならない）
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            // キーボードが利用可能か確認
            if (keyboard == null)
            {
                keyboard = Keyboard.current;
                if (keyboard == null) return;
            }

            // ESCキーでポーズメニューを開閉
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (isInSettingsMenu)
                {
                    // 設定メニューから戻る
                    CloseSettingsMenu();
                }
                else if (confirmQuitPanel != null && confirmQuitPanel.activeSelf)
                {
                    // 終了確認から戻る
                    CancelQuit();
                }
                else
                {
                    // ポーズメニューを開閉
                    TogglePauseMenu();
                }
            }

            // 左CTRLキーでカーソル表示切り替え
            if (keyboard.leftCtrlKey.wasPressedThisFrame)
            {
                // メニューが開いていない時のみカーソル切り替え可能
                if (!isMenuOpen)
                {
                    ToggleCursor();
                }
            }
        }

        #region Pause Menu
        /// <summary>
        /// メニューを開閉
        /// </summary>
        public void TogglePauseMenu()
        {
            if (isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        /// <summary>
        /// メニューを開く（MMO: ゲームは継続）
        /// </summary>
        public void OpenMenu()
        {
            isMenuOpen = true;

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            // カーソルを表示（MMOではゲームは継続）
            if (showCursorWhenMenuOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            Debug.Log("[MenuManager] メニューを開きました（ゲームは継続）");
        }

        /// <summary>
        /// メニューを閉じる
        /// </summary>
        public void CloseMenu()
        {
            isMenuOpen = false;

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            // メニューを閉じたらカーソルを非表示に
            isCursorVisible = false;
            SetCursorVisibility(false);

            Debug.Log("[MenuManager] メニューを閉じました（カーソル非表示）");
        }

        // 後方互換性のため残す
        public void PauseGame() => OpenMenu();
        public void ResumeGame() => CloseMenu();

        /// <summary>
        /// カーソル表示を切り替え（左CTRL）
        /// </summary>
        private void ToggleCursor()
        {
            isCursorVisible = !isCursorVisible;
            SetCursorVisibility(isCursorVisible);
            Debug.Log($"[MenuManager] カーソル表示切り替え: {(isCursorVisible ? "表示" : "非表示")}");
        }

        /// <summary>
        /// カーソルの表示状態を設定
        /// </summary>
        private void SetCursorVisibility(bool visible)
        {
            if (visible)
            {
                // カーソルを表示（位置は保持）
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // カーソルを非表示（中央にロックしない）
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined; // 画面内に制限するが中央にロックしない
            }
        }
        #endregion

        #region Settings Menu
        /// <summary>
        /// 設定メニューを開く
        /// </summary>
        public void OpenSettingsMenu()
        {
            isInSettingsMenu = true;

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (settingsMenuPanel != null)
                settingsMenuPanel.SetActive(true);

            // カーソルを表示
            if (showCursorWhenMenuOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            Debug.Log("[MenuManager] 設定メニューを開きました");
        }

        /// <summary>
        /// 設定メニューを閉じる
        /// </summary>
        public void CloseSettingsMenu()
        {
            isInSettingsMenu = false;

            if (settingsMenuPanel != null)
                settingsMenuPanel.SetActive(false);

            if (isMenuOpen && pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            Debug.Log("[MenuManager] 設定メニューを閉じました");
        }
        #endregion

        #region Quit Confirmation
        /// <summary>
        /// 終了確認ダイアログを表示
        /// </summary>
        public void ShowQuitConfirmation()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(true);

            Debug.Log("[MenuManager] 終了確認ダイアログを表示");
        }

        /// <summary>
        /// 終了確認をキャンセル
        /// </summary>
        public void CancelQuit()
        {
            if (confirmQuitPanel != null)
                confirmQuitPanel.SetActive(false);

            if (isMenuOpen && pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            Debug.Log("[MenuManager] 終了をキャンセル");
        }

        /// <summary>
        /// ゲームを終了
        /// </summary>
        public async void QuitGame()
        {
            Debug.Log("[MenuManager] ゲームを終了します");

            // 終了前にセーブ
            var saveManager = Save.SaveManager.Instance;
            if (saveManager != null)
            {
                Debug.Log("[MenuManager] 終了前にセーブを実行");
                await saveManager.SaveGame();
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region Public Utility Methods
        /// <summary>
        /// いずれかのメニューが開いているかチェック
        /// </summary>
        public bool IsAnyMenuOpen()
        {
            return isMenuOpen || isInSettingsMenu || 
                   (confirmQuitPanel != null && confirmQuitPanel.activeSelf);
        }
        #endregion

        #region System Menu Functions
        /// <summary>
        /// ヘルプ/チュートリアルを開く（将来実装）
        /// </summary>
        public void OpenHelp()
        {
            Debug.Log("[MenuManager] ヘルプを開きます（未実装）");
            // TODO: Phase 4で実装
            CloseMenu();
        }
        #endregion

        #region Scene Management
        /// <summary>
        /// タイトル画面に戻る（MMO: ログアウト処理）
        /// </summary>
        public async void ReturnToTitle()
        {
            Debug.Log("[MenuManager] タイトル画面に戻ります（ログアウト）");

            // セーブ処理
            var saveManager = Save.SaveManager.Instance;
            if (saveManager != null)
            {
                Debug.Log("[MenuManager] ログアウト前にセーブを実行");
                await saveManager.SaveGame();
            }

            // TODO: Phase 3 - サーバーへのログアウト通知
            // - サーバーにログアウトを通知
            // - オンライン状態を更新
            // - パーティ/ギルドから離脱

            // タイトルシーンをロード（存在する場合）
            if (Application.CanStreamedLevelBeLoaded("TitleScene"))
            {
                Debug.Log("[MenuManager] TitleScene にロードします");
                SceneManager.LoadScene("TitleScene");
            }
            else if (Application.CanStreamedLevelBeLoaded("Phase2_TestScene"))
            {
                Debug.Log("[MenuManager] TitleScene が見つかりません。Phase2_TestScene をロードします（開発用）");
                SceneManager.LoadScene("Phase2_TestScene");
            }
            else
            {
                Debug.LogWarning("[MenuManager] TitleScene が見つかりません。現在のシーンを再読み込みします。");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        /// <summary>
        /// シーンをリロード（MMO: 再接続）
        /// </summary>
        public void RestartScene()
        {
            Debug.Log("[MenuManager] シーンを再読み込みします（再接続）");

            // TODO: 再接続処理

            // 現在のシーンを再読み込み
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        #endregion

        #region Debug Context Menu
#if UNITY_EDITOR
        [ContextMenu("Test: Open Pause Menu")]
        private void TestOpenPauseMenu()
        {
            PauseGame();
        }

        [ContextMenu("Test: Close Pause Menu")]
        private void TestClosePauseMenu()
        {
            ResumeGame();
        }

        [ContextMenu("Test: Open Settings Menu")]
        private void TestOpenSettingsMenu()
        {
            OpenSettingsMenu();
        }
#endif
        #endregion
    }
}

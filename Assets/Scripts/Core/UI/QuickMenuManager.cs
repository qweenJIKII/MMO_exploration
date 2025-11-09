// QuickMenuManager: ゲーム内機能へのクイックアクセス
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core.UI
{
    /// <summary>
    /// ゲーム内機能へのクイックアクセス
    /// キャラクター、インベントリ、クエスト、マップなど
    /// </summary>
    public class QuickMenuManager : MonoBehaviour
    {
        [Header("Quick Menu Panels")]
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject questPanel;
        [SerializeField] private GameObject mapPanel;

        [Header("Settings")]
        [SerializeField] private bool showCursorWhenOpen = true;

        // Input System
        private Keyboard keyboard;

        // 状態管理
        private GameObject currentOpenPanel;

        private void Start()
        {
            // 初期状態: すべて非表示
            CloseAllPanels();

            // Input Systemの初期化
            keyboard = Keyboard.current;

            Debug.Log("[QuickMenuManager] 初期化完了");
        }

        private void Update()
        {
            // エディターモードでは動作しない
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            // キーボードが利用可能か確認
            if (keyboard == null)
            {
                keyboard = Keyboard.current;
                if (keyboard == null) return;
            }

            // ホットキー
            if (keyboard.cKey.wasPressedThisFrame)
            {
                ToggleCharacter();
            }
            else if (keyboard.iKey.wasPressedThisFrame)
            {
                ToggleInventory();
            }
            else if (keyboard.qKey.wasPressedThisFrame)
            {
                ToggleQuest();
            }
            else if (keyboard.mKey.wasPressedThisFrame)
            {
                ToggleMap();
            }
        }

        #region Character
        /// <summary>
        /// キャラクター画面を開閉（Cキー）
        /// </summary>
        public void ToggleCharacter()
        {
            if (characterPanel != null && characterPanel.activeSelf)
            {
                CloseCharacter();
            }
            else
            {
                OpenCharacter();
            }
        }

        public void OpenCharacter()
        {
            CloseAllPanels();

            if (characterPanel != null)
            {
                characterPanel.SetActive(true);
                currentOpenPanel = characterPanel;
                ShowCursor();
                Debug.Log("[QuickMenuManager] キャラクター画面を開きました");
            }
            else
            {
                Debug.LogWarning("[QuickMenuManager] キャラクター画面が未実装です（Phase 3で実装予定）");
            }
        }

        public void CloseCharacter()
        {
            if (characterPanel != null)
            {
                characterPanel.SetActive(false);
            }
            currentOpenPanel = null;
            HideCursor();
            Debug.Log("[QuickMenuManager] キャラクター画面を閉じました");
        }
        #endregion

        #region Inventory
        /// <summary>
        /// インベントリを開閉（Iキー）
        /// </summary>
        public void ToggleInventory()
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }

        public void OpenInventory()
        {
            CloseAllPanels();

            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
                currentOpenPanel = inventoryPanel;
                ShowCursor();
                Debug.Log("[QuickMenuManager] インベントリを開きました");
            }
            else
            {
                Debug.LogWarning("[QuickMenuManager] インベントリが未実装です（Phase 3で実装予定）");
            }
        }

        public void CloseInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            currentOpenPanel = null;
            HideCursor();
            Debug.Log("[QuickMenuManager] インベントリを閉じました");
        }
        #endregion

        #region Quest
        /// <summary>
        /// クエストログを開閉（Qキー）
        /// </summary>
        public void ToggleQuest()
        {
            if (questPanel != null && questPanel.activeSelf)
            {
                CloseQuest();
            }
            else
            {
                OpenQuest();
            }
        }

        public void OpenQuest()
        {
            CloseAllPanels();

            if (questPanel != null)
            {
                questPanel.SetActive(true);
                currentOpenPanel = questPanel;
                ShowCursor();
                Debug.Log("[QuickMenuManager] クエストログを開きました");
            }
            else
            {
                Debug.LogWarning("[QuickMenuManager] クエストログが未実装です（Phase 4で実装予定）");
            }
        }

        public void CloseQuest()
        {
            if (questPanel != null)
            {
                questPanel.SetActive(false);
            }
            currentOpenPanel = null;
            HideCursor();
            Debug.Log("[QuickMenuManager] クエストログを閉じました");
        }
        #endregion

        #region Map
        /// <summary>
        /// ワールドマップを開閉（Mキー）
        /// </summary>
        public void ToggleMap()
        {
            if (mapPanel != null && mapPanel.activeSelf)
            {
                CloseMap();
            }
            else
            {
                OpenMap();
            }
        }

        public void OpenMap()
        {
            CloseAllPanels();

            if (mapPanel != null)
            {
                mapPanel.SetActive(true);
                currentOpenPanel = mapPanel;
                ShowCursor();
                Debug.Log("[QuickMenuManager] ワールドマップを開きました");
            }
            else
            {
                Debug.LogWarning("[QuickMenuManager] ワールドマップが未実装です（Phase 4で実装予定）");
            }
        }

        public void CloseMap()
        {
            if (mapPanel != null)
            {
                mapPanel.SetActive(false);
            }
            currentOpenPanel = null;
            HideCursor();
            Debug.Log("[QuickMenuManager] ワールドマップを閉じました");
        }
        #endregion

        #region Utility
        /// <summary>
        /// すべてのパネルを閉じる
        /// </summary>
        public void CloseAllPanels()
        {
            if (characterPanel != null) characterPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (questPanel != null) questPanel.SetActive(false);
            if (mapPanel != null) mapPanel.SetActive(false);

            currentOpenPanel = null;
            HideCursor();
        }

        /// <summary>
        /// 何かパネルが開いているか
        /// </summary>
        public bool IsAnyPanelOpen()
        {
            return currentOpenPanel != null && currentOpenPanel.activeSelf;
        }

        private void ShowCursor()
        {
            if (showCursorWhenOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void HideCursor()
        {
            if (showCursorWhenOpen)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        #endregion

        #region Debug Context Menu
#if UNITY_EDITOR
        [ContextMenu("Test: Open Character")]
        private void TestOpenCharacter()
        {
            OpenCharacter();
        }

        [ContextMenu("Test: Open Inventory")]
        private void TestOpenInventory()
        {
            OpenInventory();
        }

        [ContextMenu("Test: Close All")]
        private void TestCloseAll()
        {
            CloseAllPanels();
        }
#endif
        #endregion
    }
}

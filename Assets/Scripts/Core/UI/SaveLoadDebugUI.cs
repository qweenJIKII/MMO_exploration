// SaveLoadDebugUI: セーブ/ロード機能のテスト用UI
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Project.Core.Save;
using Project.Core.Player;

namespace Project.Core.UI
{
    /// <summary>
    /// セーブ/ロード機能のテスト用デバッグUI
    /// Phase 2の動作確認用
    /// </summary>
    public class SaveLoadDebugUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button quickSaveButton;
        [SerializeField] private Button quickLoadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI saveInfoText;

        [Header("Player References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private ExperienceManager experienceManager;

        private SaveManager saveManager;
        private bool isPanelVisible = false;
        private Keyboard keyboard;

        private void Start()
        {
            saveManager = SaveManager.Instance;
            keyboard = Keyboard.current;

            // ボタンイベント接続
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveButtonClicked);

            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadButtonClicked);

            if (quickSaveButton != null)
                quickSaveButton.onClick.AddListener(OnQuickSaveClicked);

            if (quickLoadButton != null)
                quickLoadButton.onClick.AddListener(OnQuickLoadClicked);

            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnDeleteSaveClicked);

            // SaveManagerのイベント購読
            if (saveManager != null)
            {
                saveManager.OnSaveCompleted += OnSaveCompleted;
                saveManager.OnLoadCompleted += OnLoadCompleted;
            }

            // 初期状態: 非表示
            if (debugPanel != null)
            {
                debugPanel.SetActive(false);
                Debug.Log("[SaveLoadDebugUI] 初期化完了 - F5:パネル開閉, F6:クイックセーブ, F7:クイックロード, F9:セーブリセット");
            }
            else
            {
                Debug.LogWarning("[SaveLoadDebugUI] debugPanel が未設定 - F5:ステータス表示, F6:クイックセーブ, F7:クイックロード, F9:セーブリセット");
            }

            UpdateStatusText("Ready");
            UpdateSaveInfo();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            // キーボードが利用可能か確認
            if (keyboard == null)
            {
                keyboard = Keyboard.current;
                if (keyboard == null) return;
            }

            // F5キーでパネル開閉（またはステータス表示）
            if (keyboard.f5Key.wasPressedThisFrame)
            {
                Debug.Log("[SaveLoadDebugUI] F5キーが押されました");
                
                if (debugPanel != null)
                {
                    TogglePanel();
                }
                else
                {
                    // UIパネルがない場合は、コンソールにステータスを表示
                    LogCurrentStatus();
                }
            }

            // F6キーでクイックセーブ
            if (keyboard.f6Key.wasPressedThisFrame)
            {
                Debug.Log("[SaveLoadDebugUI] F6キーが押されました");
                OnQuickSaveClicked();
            }

            // F7キーでクイックロード
            if (keyboard.f7Key.wasPressedThisFrame)
            {
                Debug.Log("[SaveLoadDebugUI] F7キーが押されました");
                OnQuickLoadClicked();
            }

            // F9キーでセーブデータをリセット（新規ゲーム）
            if (keyboard.f9Key.wasPressedThisFrame)
            {
                Debug.Log("[SaveLoadDebugUI] F9キーが押されました - セーブデータをリセット");
                OnDeleteSaveClicked();
            }
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (saveManager != null)
            {
                saveManager.OnSaveCompleted -= OnSaveCompleted;
                saveManager.OnLoadCompleted -= OnLoadCompleted;
            }
        }

        #region UI Control
        private void TogglePanel()
        {
            isPanelVisible = !isPanelVisible;
            
            if (debugPanel != null)
            {
                debugPanel.SetActive(isPanelVisible);
                Debug.Log($"[SaveLoadDebugUI] パネル {(isPanelVisible ? "表示" : "非表示")}");
                
                if (isPanelVisible)
                {
                    UpdateSaveInfo();
                }
            }
            else
            {
                Debug.LogError("[SaveLoadDebugUI] debugPanel が null です！Inspectorで設定してください。");
            }
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"Status: {message}";
                statusText.color = Color.white;
            }
        }

        private void UpdateStatusTextError(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"Error: {message}";
                statusText.color = Color.red;
            }
        }

        private void UpdateStatusTextSuccess(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"Success: {message}";
                statusText.color = Color.green;
            }
        }

        private void UpdateSaveInfo()
        {
            if (saveInfoText == null) return;

            if (playerStats != null && experienceManager != null)
            {
                saveInfoText.text = $"<b>Current Player Data:</b>\n" +
                    $"HP: {playerStats.currentHealth}/{playerStats.maxHealth}\n" +
                    $"MP: {playerStats.currentMana}/{playerStats.maxMana}\n" +
                    $"Level: {experienceManager.CurrentLevel}\n" +
                    $"EXP: {experienceManager.CurrentExperience}\n" +
                    $"Position: {GetPlayerPosition()}";
            }
            else
            {
                saveInfoText.text = "Player references not set!";
            }
        }
        #endregion

        #region Button Handlers
        private async void OnSaveButtonClicked()
        {
            UpdateStatusText("Saving...");
            Debug.Log("[SaveLoadDebugUI] セーブボタンがクリックされました");

            if (saveManager != null)
            {
                bool success = await saveManager.SaveGame();
                if (success)
                {
                    UpdateStatusTextSuccess("Game saved!");
                }
                else
                {
                    UpdateStatusTextError("Save failed!");
                }
            }
            else
            {
                UpdateStatusTextError("SaveManager not found!");
            }
        }

        private async void OnLoadButtonClicked()
        {
            UpdateStatusText("Loading...");
            Debug.Log("[SaveLoadDebugUI] ロードボタンがクリックされました");

            if (saveManager != null)
            {
                SaveData data = await saveManager.LoadGame();
                if (data != null)
                {
                    UpdateStatusTextSuccess("Game loaded!");
                    UpdateSaveInfo();
                }
                else
                {
                    UpdateStatusTextError("Load failed!");
                }
            }
            else
            {
                UpdateStatusTextError("SaveManager not found!");
            }
        }

        private async void OnQuickSaveClicked()
        {
            UpdateStatusText("Quick saving...");
            Debug.Log("[SaveLoadDebugUI] クイックセーブ");

            if (saveManager != null)
            {
                await saveManager.SaveGame();
            }
        }

        private async void OnQuickLoadClicked()
        {
            UpdateStatusText("Quick loading...");
            Debug.Log("[SaveLoadDebugUI] クイックロード");

            if (saveManager != null)
            {
                SaveData data = await saveManager.LoadGame();
                if (data != null)
                {
                    UpdateStatusTextSuccess("Quick load complete!");
                    UpdateSaveInfo();
                }
                else
                {
                    UpdateStatusTextError("Quick load failed!");
                }
            }
        }

        private void OnDeleteSaveClicked()
        {
            UpdateStatusText("Deleting save...");
            Debug.Log("[SaveLoadDebugUI] セーブデータ削除");

            if (saveManager != null)
            {
                saveManager.DeleteAllSaves();
                UpdateStatusTextSuccess("Save data deleted!");
            }
        }
        #endregion

        #region Event Handlers
        private void OnSaveCompleted(SaveData saveData)
        {
            Debug.Log($"[SaveLoadDebugUI] セーブ完了: {saveData.saveDate}");
            UpdateSaveInfo();
        }

        private void OnLoadCompleted(SaveData saveData)
        {
            Debug.Log($"[SaveLoadDebugUI] ロード完了: {saveData.saveDate}");
            UpdateSaveInfo();
        }
        #endregion

        #region Utility
        private Vector3 GetPlayerPosition()
        {
            if (playerStats != null)
            {
                return playerStats.transform.position;
            }
            return Vector3.zero;
        }

        /// <summary>
        /// コンソールに現在のステータスを表示（UIパネルがない場合用）
        /// </summary>
        private void LogCurrentStatus()
        {
            Debug.Log("=== SAVE/LOAD DEBUG STATUS ===");
            
            if (playerStats != null && experienceManager != null)
            {
                Debug.Log($"HP: {playerStats.currentHealth}/{playerStats.maxHealth}");
                Debug.Log($"MP: {playerStats.currentMana}/{playerStats.maxMana}");
                Debug.Log($"Level: {experienceManager.CurrentLevel}");
                Debug.Log($"EXP: {experienceManager.CurrentExperience}");
                Debug.Log($"Position: {GetPlayerPosition()}");
            }
            else
            {
                Debug.LogWarning("Player references not set!");
            }
            
            Debug.Log("==============================");
            Debug.Log("F6: Quick Save | F7: Quick Load");
        }
        #endregion

        #region Context Menu (Editor Only)
#if UNITY_EDITOR
        [ContextMenu("Test: Toggle Panel")]
        private void TestTogglePanel()
        {
            TogglePanel();
        }

        [ContextMenu("Test: Show Panel")]
        private void TestShowPanel()
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(true);
                isPanelVisible = true;
                UpdateSaveInfo();
                Debug.Log("[SaveLoadDebugUI] パネルを強制表示");
            }
        }

        [ContextMenu("Test: Save Game")]
        private void TestSave()
        {
            OnSaveButtonClicked();
        }

        [ContextMenu("Test: Load Game")]
        private void TestLoad()
        {
            OnLoadButtonClicked();
        }

        [ContextMenu("Test: Delete Save")]
        private void TestDelete()
        {
            OnDeleteSaveClicked();
        }

        [ContextMenu("Debug: Check References")]
        private void DebugCheckReferences()
        {
            Debug.Log($"[SaveLoadDebugUI] debugPanel: {(debugPanel != null ? "OK" : "NULL")}");
            Debug.Log($"[SaveLoadDebugUI] saveButton: {(saveButton != null ? "OK" : "NULL")}");
            Debug.Log($"[SaveLoadDebugUI] loadButton: {(loadButton != null ? "OK" : "NULL")}");
            Debug.Log($"[SaveLoadDebugUI] statusText: {(statusText != null ? "OK" : "NULL")}");
            Debug.Log($"[SaveLoadDebugUI] saveInfoText: {(saveInfoText != null ? "OK" : "NULL")}");
            Debug.Log($"[SaveLoadDebugUI] playerStats: {(playerStats != null ? "OK" : "NULL")}");
            Debug.Log($"[SaveLoadDebugUI] experienceManager: {(experienceManager != null ? "OK" : "NULL")}");
            Debug.Log($"[SaveLoadDebugUI] saveManager: {(saveManager != null ? "OK" : "NULL")}");
        }
#endif
        #endregion
    }
}

// RemoteConfigDebugUI: Remote Config設定値のデバッグ表示UI
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Core.Services;

namespace Project.Core.UI
{
    /// <summary>
    /// Remote Config設定値のデバッグ表示UI
    /// F8キーで開閉
    /// </summary>
    public class RemoteConfigDebugUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private TextMeshProUGUI configText;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button closeButton;

        private RemoteConfigManager configManager;
        private UnityEngine.InputSystem.Keyboard keyboard;

        private void Start()
        {
            configManager = RemoteConfigManager.Instance;
            keyboard = UnityEngine.InputSystem.Keyboard.current;

            // ボタンイベント接続
            if (refreshButton != null)
                refreshButton.onClick.AddListener(OnRefreshClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);

            // 初期状態: パネル非表示
            if (debugPanel != null)
            {
                debugPanel.SetActive(false);
                Debug.Log("[RemoteConfigDebugUI] 初期化完了 - F8キーでパネル開閉");
            }

            // イベント登録
            if (configManager != null)
            {
                configManager.OnConfigFetched += UpdateConfigDisplay;
                configManager.OnConfigError += OnConfigError;
            }

            UpdateConfigDisplay();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (keyboard == null)
            {
                keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard == null) return;
            }

            // F8キーでパネル開閉
            if (keyboard.f8Key.wasPressedThisFrame)
            {
                Debug.Log("[RemoteConfigDebugUI] F8キーが押されました");
                TogglePanel();
            }
        }

        private void OnDestroy()
        {
            if (configManager != null)
            {
                configManager.OnConfigFetched -= UpdateConfigDisplay;
                configManager.OnConfigError -= OnConfigError;
            }
        }

        /// <summary>
        /// パネルの開閉
        /// </summary>
        private void TogglePanel()
        {
            if (debugPanel != null)
            {
                bool isActive = !debugPanel.activeSelf;
                debugPanel.SetActive(isActive);

                if (isActive)
                {
                    UpdateConfigDisplay();
                }

                Debug.Log($"[RemoteConfigDebugUI] パネル{(isActive ? "表示" : "非表示")}");
            }
        }

        /// <summary>
        /// 設定値を表示
        /// </summary>
        private void UpdateConfigDisplay()
        {
            if (configText == null || configManager == null) return;

            string display = "=== Remote Config Settings ===\n\n";
            display += $"<color=#FFD700>Experience Multiplier:</color> <b>{configManager.ExperienceMultiplier:F2}x</b>\n";
            display += $"<color=#FFD700>Drop Rate Multiplier:</color> <b>{configManager.DropRateMultiplier:F2}x</b>\n";
            display += $"<color=#FFD700>Max Level:</color> <b>{configManager.MaxLevel}</b>\n";
            display += $"<color=#FFD700>Daily Reward Gold:</color> <b>{configManager.DailyRewardGold}</b>\n\n";

            if (configManager.EventEnabled)
            {
                display += $"<color=#00FF00>🎉 Event Active!</color>\n";
                display += $"<color=#FFFFFF>{configManager.EventMessage}</color>\n";
            }
            else
            {
                display += $"<color=#808080>No active event</color>\n";
            }

            display += "\n==============================\n";
            display += "<size=12><color=#808080>Press F8 to close | Click Refresh to update</color></size>";

            configText.text = display;
        }

        /// <summary>
        /// 更新ボタンクリック
        /// </summary>
        private void OnRefreshClicked()
        {
            Debug.Log("[RemoteConfigDebugUI] 設定を更新中...");
            if (configManager != null)
            {
                configManager.RefreshConfig();
            }
        }

        /// <summary>
        /// 閉じるボタンクリック
        /// </summary>
        private void OnCloseClicked()
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 設定取得エラー
        /// </summary>
        private void OnConfigError(string error)
        {
            Debug.LogWarning($"[RemoteConfigDebugUI] Config取得エラー: {error}");
            
            if (configText != null)
            {
                configText.text = $"<color=#FF0000>Error loading config:</color>\n{error}\n\n<color=#FFFF00>Using default values</color>";
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test: Toggle Panel")]
        private void TestTogglePanel()
        {
            TogglePanel();
        }

        [ContextMenu("Test: Update Display")]
        private void TestUpdateDisplay()
        {
            UpdateConfigDisplay();
        }
#endif
    }
}

// GothicDebugMenu: デバッグ機能を提供するUIメニュー
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Core.Player;

namespace Project.Core.UI
{
    /// <summary>
    /// デバッグ機能を提供するGothicスタイルのメニュー
    /// </summary>
    public class GothicDebugMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private ExperienceManager experienceManager;
        [SerializeField] private GothicHUDManager hudManager;

        [Header("UI Elements")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button toggleButton;

        private bool isMenuOpen = false;

        private void Start()
        {
            Debug.Log("[GothicDebugMenu] Start() called");

            // 初期状態は非表示
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
                Debug.Log("[GothicDebugMenu] Menu panel hidden");
            }
            else
            {
                Debug.LogWarning("[GothicDebugMenu] Menu panel is NULL!");
            }

            // トグルボタンの参照を自動検索
            if (toggleButton == null)
            {
                GameObject toggleButtonObj = GameObject.Find("DebugToggleButton");
                if (toggleButtonObj != null)
                {
                    toggleButton = toggleButtonObj.GetComponent<Button>();
                    Debug.Log($"[GothicDebugMenu] Found toggle button: {toggleButtonObj.name}");
                }
                else
                {
                    Debug.LogError("[GothicDebugMenu] DebugToggleButton GameObject not found!");
                }
            }

            // トグルボタンのイベント設定
            if (toggleButton != null)
            {
                // ボタンの状態を確認
                Image buttonImage = toggleButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    Debug.Log($"[GothicDebugMenu] Button Image - raycastTarget: {buttonImage.raycastTarget}, enabled: {buttonImage.enabled}");
                }

                Debug.Log($"[GothicDebugMenu] Button - interactable: {toggleButton.interactable}, enabled: {toggleButton.enabled}");

                toggleButton.onClick.RemoveAllListeners(); // 重複防止
                toggleButton.onClick.AddListener(ToggleMenu);
                Debug.Log("[GothicDebugMenu] Toggle button connected!");
            }
            else
            {
                Debug.LogError("[GothicDebugMenu] Toggle button component is NULL!");
            }

            // 自動で参照を探す
            if (playerStats == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerStats = player.GetComponent<PlayerStats>();
                    experienceManager = player.GetComponent<ExperienceManager>();
                }
            }

            if (hudManager == null)
            {
                hudManager = FindFirstObjectByType<GothicHUDManager>();
            }

            // Canvasの確認
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            if (canvas != null)
            {
                Debug.Log($"[GothicDebugMenu] Canvas found: {canvas.name}");
                
                // GraphicRaycasterの確認
                UnityEngine.UI.GraphicRaycaster raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster == null)
                {
                    Debug.LogError("[GothicDebugMenu] GraphicRaycaster not found on Canvas!");
                }
                else
                {
                    Debug.Log($"[GothicDebugMenu] GraphicRaycaster found, enabled: {raycaster.enabled}");
                }
            }
            else
            {
                Debug.LogError("[GothicDebugMenu] Canvas not found!");
            }

            // EventSystemの確認
            UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogError("[GothicDebugMenu] EventSystem not found!");
            }
            else
            {
                Debug.Log($"[GothicDebugMenu] EventSystem found: {eventSystem.name}");
            }
        }

        // Update()を削除（New Input Systemと競合するため）
        // デバッグはボタンのonClickイベントで確認

        /// <summary>
        /// メニューの表示/非表示を切り替え
        /// </summary>
        public void ToggleMenu()
        {
            Debug.Log("[GothicDebugMenu] ToggleMenu() called!");
            isMenuOpen = !isMenuOpen;
            if (menuPanel != null)
            {
                menuPanel.SetActive(isMenuOpen);
                Debug.Log($"[GothicDebugMenu] Menu panel set to: {isMenuOpen}");
            }
            else
            {
                Debug.LogError("[GothicDebugMenu] Menu panel is NULL in ToggleMenu!");
            }
        }

        #region Debug Actions

        /// <summary>
        /// ダメージテスト: 10ダメージ（防御力無視）
        /// </summary>
        public void TestDamage10()
        {
            if (playerStats != null)
            {
                playerStats.ModifyHealth(-10f);
                Debug.Log("[DebugMenu] Dealt 10 damage (pure)");
            }
        }

        /// <summary>
        /// ダメージテスト: 50ダメージ（防御力無視）
        /// </summary>
        public void TestDamage50()
        {
            if (playerStats != null)
            {
                playerStats.ModifyHealth(-50f);
                Debug.Log("[DebugMenu] Dealt 50 damage (pure)");
            }
        }

        /// <summary>
        /// 回復テスト: 50回復
        /// </summary>
        public void TestHeal50()
        {
            if (playerStats != null)
            {
                playerStats.Heal(50f);
                Debug.Log("[DebugMenu] Healed 50 HP");
            }
        }

        /// <summary>
        /// 完全回復
        /// </summary>
        public void TestFullHeal()
        {
            if (playerStats != null)
            {
                playerStats.Heal(playerStats.maxHealth);
                Debug.Log("[DebugMenu] Full heal");
            }
        }

        /// <summary>
        /// マナ消費テスト: 30消費
        /// </summary>
        public void TestUseMana30()
        {
            if (playerStats != null)
            {
                playerStats.ConsumeMana(30f);
                Debug.Log("[DebugMenu] Used 30 mana");
            }
        }

        /// <summary>
        /// マナ回復テスト: 50回復
        /// </summary>
        public void TestRestoreMana50()
        {
            if (playerStats != null)
            {
                playerStats.ModifyMana(50f);
                Debug.Log("[DebugMenu] Restored 50 mana");
            }
        }

        /// <summary>
        /// 経験値付与: 50 EXP
        /// </summary>
        public void TestGrantExp50()
        {
            if (experienceManager != null)
            {
                experienceManager.GrantExperience(50);
                Debug.Log("[DebugMenu] Granted 50 EXP");
            }
        }

        /// <summary>
        /// 経験値付与: 500 EXP
        /// </summary>
        public void TestGrantExp500()
        {
            if (experienceManager != null)
            {
                experienceManager.GrantExperience(500);
                Debug.Log("[DebugMenu] Granted 500 EXP");
            }
        }

        /// <summary>
        /// レベルアップテスト
        /// </summary>
        public void TestLevelUp()
        {
            if (experienceManager != null && playerStats != null)
            {
                int required = experienceManager.GetRequiredExperience(playerStats.level);
                experienceManager.GrantExperience(required);
                Debug.Log("[DebugMenu] Level up!");
            }
        }

        /// <summary>
        /// レベル10に設定
        /// </summary>
        public void TestSetLevel10()
        {
            if (playerStats != null)
            {
                playerStats.SetLevel(10);
                Debug.Log("[DebugMenu] Set level to 10");
            }
        }

        #endregion
    }
}

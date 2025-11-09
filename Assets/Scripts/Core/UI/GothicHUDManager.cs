// GothicHUDManager: GothicUIアセットを使用したHUD管理
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Core.Player;

namespace Project.Core.UI
{
    /// <summary>
    /// GothicUIアセットを使用したHUD管理
    /// Phase 2実装: プレミアムゴシックスタイルUI
    /// </summary>
    [ExecuteAlways] // エディタモードでも実行
    public class GothicHUDManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private ExperienceManager experienceManager;

        [Header("HP/MP Ampuls")]
        [SerializeField] private Image healthAmpulFill;
        [SerializeField] private Image manaAmpulFill;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI manaText;

        // HPFlowController（Fluidシェーダー制御）
        private HPFlowController healthFlowController;
        private HPFlowController manaFlowController;

        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Experience Bar")]
        [SerializeField] private Image experienceBarFill;
        [SerializeField] private TextMeshProUGUI experienceText;

        [Header("Skill Bar")]
        [SerializeField] private Transform skillBarContainer;
        [SerializeField] private GameObject skillSlotPrefab;

        [Header("System Info")]
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI pingText;

        private float fpsUpdateInterval = 1.0f;
        private float fpsTimer = 0f;
        private int frameCount = 0;

        private void Awake()
        {
            // イベントバインドは後で行う
        }

        private void Start()
        {
            InitializeUI();
            BindEvents(); // UI初期化後にイベントをバインド
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        /// <summary>
        /// エディタモードでのUI更新
        /// </summary>
        private void UpdateEditorMode()
        {
            if (playerStats != null)
            {
                UpdateHealth(playerStats.currentHealth, playerStats.maxHealth);
                UpdateMana(playerStats.currentMana, playerStats.maxMana);
                UpdateLevel(playerStats.level);
                
                // プレイヤー名を更新
                if (playerNameText != null)
                {
                    playerNameText.text = "Hero";
                }
            }

            if (experienceManager != null && playerStats != null)
            {
                float current = experienceManager.GetCurrentExperience();
                float required = experienceManager.GetRequiredExperience(playerStats.level);
                UpdateExperience(current, required);
            }
        }

        private void BindEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += OnHealthChanged;
                playerStats.OnManaChanged += OnManaChanged;
                playerStats.OnLevelChanged += OnLevelChanged;
            }

            if (experienceManager != null)
            {
                experienceManager.OnExperienceChanged += OnExperienceChanged;
                experienceManager.OnLevelUp += OnLevelUp;
            }
        }

        private void UnbindEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= OnHealthChanged;
                playerStats.OnManaChanged -= OnManaChanged;
                playerStats.OnLevelChanged -= OnLevelChanged;
            }

            if (experienceManager != null)
            {
                experienceManager.OnExperienceChanged -= OnExperienceChanged;
                experienceManager.OnLevelUp -= OnLevelUp;
            }
        }

        private void InitializeUI()
        {
            Debug.Log($"[GothicHUDManager] InitializeUI called. healthAmpulFill: {healthAmpulFill != null}, manaAmpulFill: {manaAmpulFill != null}");
            
            // HPFlowControllerをhealthAmpulFill（Fluid Image）に追加
            if (healthAmpulFill != null)
            {
                healthFlowController = healthAmpulFill.GetComponent<HPFlowController>();
                if (healthFlowController == null)
                {
                    healthFlowController = healthAmpulFill.gameObject.AddComponent<HPFlowController>();
                    Debug.Log($"[GothicHUDManager] HPFlowController added to {healthAmpulFill.gameObject.name}");
                }
                // 初期値を設定
                healthFlowController.SetValue(1.0f);
                Debug.Log("[GothicHUDManager] Health flow controller initialized with value = 1.0");
            }
            else
            {
                Debug.LogError("[GothicHUDManager] healthAmpulFill is NULL!");
            }

            // HPFlowControllerをmanaAmpulFill（Fluid Image）に追加
            if (manaAmpulFill != null)
            {
                manaFlowController = manaAmpulFill.GetComponent<HPFlowController>();
                if (manaFlowController == null)
                {
                    manaFlowController = manaAmpulFill.gameObject.AddComponent<HPFlowController>();
                    Debug.Log($"[GothicHUDManager] HPFlowController added to {manaAmpulFill.gameObject.name}");
                }
                // 初期値を設定
                manaFlowController.SetValue(1.0f);
                Debug.Log("[GothicHUDManager] Mana flow controller initialized with value = 1.0");
            }
            else
            {
                Debug.LogError("[GothicHUDManager] manaAmpulFill is NULL!");
            }

            if (playerStats != null)
            {
                UpdateHealth(playerStats.currentHealth, playerStats.maxHealth);
                UpdateMana(playerStats.currentMana, playerStats.maxMana);
                UpdateLevel(playerStats.level);
            }
            else
            {
                Debug.LogWarning("[GothicHUDManager] PlayerStats is NULL!");
            }

            if (experienceManager != null && playerStats != null)
            {
                float current = experienceManager.GetCurrentExperience();
                float required = experienceManager.GetRequiredExperience(playerStats.level);
                UpdateExperience(current, required);
            }

            // プレイヤー名を設定（仮）
            if (playerNameText != null)
            {
                playerNameText.text = "Hero";
            }

            Debug.Log("[GothicHUDManager] UI initialized with Gothic style");
        }

        private void Update()
        {
            // エディタモードでもリアルタイム更新
            if (!Application.isPlaying)
            {
                UpdateEditorMode();
            }
            else
            {
                UpdateFPS();
            }
        }

        private void UpdateFPS()
        {
            fpsTimer += Time.deltaTime;
            frameCount++;

            if (fpsTimer >= fpsUpdateInterval)
            {
                float fps = frameCount / fpsTimer;
                
                if (fpsText != null)
                {
                    fpsText.text = $"FPS: {fps:F0}";
                    
                    // FPSに応じて色を変更
                    if (fps >= 60)
                        fpsText.color = new Color(0.5f, 1f, 0.5f); // 緑
                    else if (fps >= 30)
                        fpsText.color = new Color(1f, 1f, 0.5f); // 黄
                    else
                        fpsText.color = new Color(1f, 0.5f, 0.5f); // 赤
                }

                fpsTimer = 0f;
                frameCount = 0;
            }
        }

        #region Event Handlers

        private void OnHealthChanged(float current, float max)
        {
            UpdateHealth(current, max);
        }

        private void OnManaChanged(float current, float max)
        {
            UpdateMana(current, max);
        }

        private void OnLevelChanged(int newLevel)
        {
            UpdateLevel(newLevel);
        }

        private void OnExperienceChanged(float current, float required)
        {
            UpdateExperience(current, required);
        }

        private void OnLevelUp(int newLevel)
        {
            Debug.Log($"[GothicHUDManager] Level Up! New Level: {newLevel}");
            // レベルアップエフェクトを追加可能
        }

        #endregion

        #region UI Update Methods

        private void UpdateHealth(float current, float max)
        {
            float fillAmount = max > 0 ? current / max : 0;
            
            // HPFlowControllerを使用してFillLevelを更新
            if (healthFlowController != null)
            {
                healthFlowController.SetValue(fillAmount);
            }

            if (healthText != null)
            {
                healthText.text = $"{current:F0}/{max:F0}";
            }
        }

        private void UpdateMana(float current, float max)
        {
            float fillAmount = max > 0 ? current / max : 0;
            
            // HPFlowControllerを使用してFillLevelを更新
            if (manaFlowController != null)
            {
                manaFlowController.SetValue(fillAmount);
            }

            if (manaText != null)
            {
                manaText.text = $"{current:F0}/{max:F0}";
            }
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }
        }

        private void UpdateExperience(float current, float required)
        {
            if (experienceBarFill != null)
            {
                experienceBarFill.fillAmount = required > 0 ? current / required : 0;
            }

            if (experienceText != null)
            {
                experienceText.text = $"{current:F0}/{required:F0}";
            }
        }

        #endregion

        #region Debug Context Menu

#if UNITY_EDITOR
        [ContextMenu("Test: Damage 20")]
        private void TestDamage()
        {
            if (playerStats != null)
            {
                playerStats.TakeDamage(20f);
            }
        }

        [ContextMenu("Test: Heal 50")]
        private void TestHeal()
        {
            if (playerStats != null)
            {
                playerStats.Heal(50f);
            }
        }

        [ContextMenu("Test: Grant 100 EXP")]
        private void TestGrantExp()
        {
            if (experienceManager != null)
            {
                experienceManager.GrantExperience(100);
            }
        }

        [ContextMenu("Test: Use Mana 30")]
        private void TestUseMana()
        {
            if (playerStats != null)
            {
                playerStats.ConsumeMana(30f);
            }
        }
#endif

        #endregion

        #region Public Methods
        /// <summary>
        /// PlayerStatsを強制的に更新（セーブ/ロード時に使用）
        /// </summary>
        public void UpdatePlayerStats(PlayerStats stats)
        {
            if (stats == null) return;

            Debug.Log($"[GothicHUDManager] UpdatePlayerStats: HP={stats.currentHealth}/{stats.maxHealth}, MP={stats.currentMana}/{stats.maxMana}");

            // HP/MPを更新
            UpdateHealth(stats.currentHealth, stats.maxHealth);
            UpdateMana(stats.currentMana, stats.maxMana);
            UpdateLevel(stats.level);

            // 経験値も更新
            if (experienceManager != null)
            {
                int required = experienceManager.GetRequiredExperience(experienceManager.CurrentLevel);
                UpdateExperience(experienceManager.CurrentExperience, required);
            }
        }
        #endregion
    }
}

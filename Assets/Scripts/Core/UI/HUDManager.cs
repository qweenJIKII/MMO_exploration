// HUDマネージャー: プレイヤー情報を表示するHUD（UI Toolkit版）
using UnityEngine;
using UnityEngine.UIElements;
using Project.Core.Player;
using Project.Core.UI.Components;

namespace Project.Core.UI
{
    /// <summary>
    /// HUD（ヘッドアップディスプレイ）の管理
    /// Phase 2実装: UI Toolkit対応、ファンタジーMMOスタイル
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        [Header("References")]
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private ExperienceManager experienceManager;

        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.1f; // UI更新間隔（秒）

        private VisualElement root;
        private FantasyPlayerFrame playerFrame;
        private FantasyMiniMap miniMap;
        private FantasyQuestTracker questTracker;
        private FantasyChatWindow chatWindow;

        private float _updateTimer = 0f;

        private void Awake()
        {
            if (uiDocument == null)
            {
                Debug.LogError("[HUDManager] UIDocument reference is missing!");
                return;
            }

            InitializeComponents();
            BindEvents();
        }

        private void InitializeComponents()
        {
            root = uiDocument.rootVisualElement;

            if (root == null)
            {
                Debug.LogError("[HUDManager] UIDocument.rootVisualElement is null! Please assign a UXML file to UIDocument.");
                return;
            }

            // 各コンポーネント初期化（nullチェック付き）
            var playerFrameElement = root.Q("player-frame");
            if (playerFrameElement != null)
            {
                playerFrame = new FantasyPlayerFrame(playerFrameElement);
            }
            else
            {
                Debug.LogWarning("[HUDManager] 'player-frame' element not found in UXML");
            }

            var miniMapElement = root.Q("minimap");
            if (miniMapElement != null)
            {
                miniMap = new FantasyMiniMap(miniMapElement);
            }
            else
            {
                Debug.LogWarning("[HUDManager] 'minimap' element not found in UXML");
            }

            var questTrackerElement = root.Q("quest-tracker");
            if (questTrackerElement != null)
            {
                questTracker = new FantasyQuestTracker(questTrackerElement);
            }
            else
            {
                Debug.LogWarning("[HUDManager] 'quest-tracker' element not found in UXML");
            }

            var chatWindowElement = root.Q("chat-window");
            if (chatWindowElement != null)
            {
                chatWindow = new FantasyChatWindow(chatWindowElement);
            }
            else
            {
                Debug.LogWarning("[HUDManager] 'chat-window' element not found in UXML");
            }

            Debug.Log("[HUDManager] UI components initialized");
        }

        private void BindEvents()
        {
            // PlayerStatsイベント購読
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += OnHealthChanged;
                playerStats.OnManaChanged += OnManaChanged;
                playerStats.OnStaminaChanged += OnStaminaChanged;
                playerStats.OnLevelChanged += OnLevelChanged;
            }

            // ExperienceManagerイベント購読
            if (experienceManager != null)
            {
                experienceManager.OnExperienceChanged += OnExperienceChanged;
                experienceManager.OnLevelUp += OnLevelUp;
            }

            Debug.Log("[HUDManager] Events bound");
        }

        private void Start()
        {
            // 初期値を設定
            if (playerStats != null)
            {
                playerFrame?.UpdateHealth(playerStats.currentHealth, playerStats.maxHealth);
                playerFrame?.UpdateMana(playerStats.currentMana, playerStats.maxMana);
                playerFrame?.SetLevel(playerStats.level);
            }

            if (experienceManager != null)
            {
                int current = experienceManager.GetCurrentExperience();
                int required = experienceManager.GetRequiredExperience(experienceManager.GetCurrentLevel());
                playerFrame?.UpdateExperience(current, required);
            }

            // テストデータ
            miniMap?.SetLocation("Starting Area");
            
            // テストチャットメッセージ
            chatWindow?.AddMessage("System", "Welcome to MMO Exploration!", FantasyChatWindow.ChatChannel.System);
        }

        private void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= updateInterval)
            {
                _updateTimer = 0f;
                UpdateHUD();
            }
        }

        private void UpdateHUD()
        {
            // 定期的な更新処理（必要に応じて）
        }

        // イベントハンドラー
        private void OnHealthChanged(float current, float max)
        {
            playerFrame?.UpdateHealth(current, max);
        }

        private void OnManaChanged(float current, float max)
        {
            playerFrame?.UpdateMana(current, max);
        }

        private void OnStaminaChanged(float current, float max)
        {
            playerFrame?.UpdateStamina(current, max);
        }

        private void OnLevelChanged(int newLevel)
        {
            playerFrame?.SetLevel(newLevel);
        }

        private void OnExperienceChanged(float current, float required)
        {
            playerFrame?.UpdateExperience(current, required);
        }

        private void OnLevelUp(int newLevel)
        {
            // レベルアップエフェクト
            Debug.Log($"[HUDManager] Level Up! New Level: {newLevel}");
            chatWindow?.AddMessage("System", $"Level Up! You are now level {newLevel}!", FantasyChatWindow.ChatChannel.System);
        }

        /// <summary>
        /// HUDの表示/非表示を切り替え
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (root != null)
            {
                root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// ミニマップにアイコンを追加/更新
        /// </summary>
        public void UpdateMinimapIcon(string id, Vector2 normalizedPosition, FantasyMiniMap.IconType type)
        {
            miniMap?.UpdateIcon(id, normalizedPosition, type);
        }

        /// <summary>
        /// クエストを追加
        /// </summary>
        public void AddQuest(string questId, string title)
        {
            questTracker?.AddQuest(questId, title);
        }

        /// <summary>
        /// チャットメッセージを追加
        /// </summary>
        public void AddChatMessage(string sender, string message, FantasyChatWindow.ChatChannel channel)
        {
            chatWindow?.AddMessage(sender, message, channel);
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= OnHealthChanged;
                playerStats.OnManaChanged -= OnManaChanged;
                playerStats.OnStaminaChanged -= OnStaminaChanged;
                playerStats.OnLevelChanged -= OnLevelChanged;
            }

            if (experienceManager != null)
            {
                experienceManager.OnExperienceChanged -= OnExperienceChanged;
                experienceManager.OnLevelUp -= OnLevelUp;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test: Damage 10")]
        private void TestDamage()
        {
            playerStats?.TakeDamage(10f);
        }

        [ContextMenu("Test: Heal 50")]
        private void TestHeal()
        {
            playerStats?.Heal(50f);
        }

        [ContextMenu("Test: Grant 100 EXP")]
        private void TestGrantExp()
        {
            experienceManager?.GrantExperience(100);
        }

        [ContextMenu("Test: Add Chat Message")]
        private void TestChatMessage()
        {
            chatWindow?.AddMessage("TestUser", "Hello World!", FantasyChatWindow.ChatChannel.Party);
        }
#endif
    }
}

// AnalyticsManager: Unity Analytics統合
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;

namespace Project.Core.Services
{
    /// <summary>
    /// Analytics管理
    /// プレイヤー行動トラッキング、カスタムイベント送信
    /// Phase 2実装: 基本的なイベント収集
    /// Unity Analytics 6.1.1対応
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool debugMode = false;

        // イベント送信状態
        private bool isInitialized = false;
        private int eventsSentThisSession = 0;

        // イベント
        public event Action OnAnalyticsInitialized;
        public event Action<string> OnEventSent;
        public event Action<string> OnAnalyticsError;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[AnalyticsManager] 初期化完了");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (!enableAnalytics)
            {
                Debug.Log("[AnalyticsManager] Analytics無効");
                return;
            }

            try
            {
                // Unity Servicesが初期化されているか確認
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    Debug.LogWarning("[AnalyticsManager] Unity Servicesが初期化されていません");
                    return;
                }

                // Analytics 6.1.1では自動的に初期化される
                isInitialized = true;
                
                Debug.Log("[AnalyticsManager] Analytics初期化成功 (Unity Analytics 6.1.1)");
                OnAnalyticsInitialized?.Invoke();

                // セッション開始イベント
                RecordSessionStart();
            }
            catch (Exception e)
            {
                Debug.LogError($"[AnalyticsManager] 初期化失敗: {e.Message}");
                OnAnalyticsError?.Invoke(e.Message);
            }
        }

        private void OnApplicationQuit()
        {
            if (isInitialized && enableAnalytics)
            {
                RecordSessionEnd();
            }
        }

        #region セッションイベント

        /// <summary>
        /// セッション開始を記録
        /// </summary>
        private void RecordSessionStart()
        {
            var parameters = new Dictionary<string, object>
            {
                { "platform", Application.platform.ToString() },
                { "unity_version", Application.unityVersion },
                { "app_version", Application.version }
            };

            SendCustomEvent("session_start", parameters);
        }

        /// <summary>
        /// セッション終了を記録
        /// </summary>
        private void RecordSessionEnd()
        {
            var parameters = new Dictionary<string, object>
            {
                { "session_duration", Time.realtimeSinceStartup },
                { "events_sent", eventsSentThisSession }
            };

            SendCustomEvent("session_end", parameters);
            
            // イベント送信を確実に完了させる
            AnalyticsService.Instance.Flush();
        }

        #endregion

        #region プレイヤーイベント

        /// <summary>
        /// レベルアップを記録
        /// </summary>
        public void RecordLevelUp(int newLevel, int experience)
        {
            var parameters = new Dictionary<string, object>
            {
                { "new_level", newLevel },
                { "total_experience", experience },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("player_level_up", parameters);
        }

        /// <summary>
        /// プレイヤー死亡を記録
        /// </summary>
        public void RecordPlayerDeath(string causeOfDeath, int level)
        {
            var parameters = new Dictionary<string, object>
            {
                { "cause_of_death", causeOfDeath },
                { "player_level", level },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("player_death", parameters);
        }

        /// <summary>
        /// アイテム取得を記録
        /// </summary>
        public void RecordItemAcquired(string itemId, string itemName, int quantity, string source)
        {
            var parameters = new Dictionary<string, object>
            {
                { "item_id", itemId },
                { "item_name", itemName },
                { "quantity", quantity },
                { "source", source },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("item_acquired", parameters);
        }

        /// <summary>
        /// アイテム使用を記録
        /// </summary>
        public void RecordItemUsed(string itemId, string itemName, int quantity)
        {
            var parameters = new Dictionary<string, object>
            {
                { "item_id", itemId },
                { "item_name", itemName },
                { "quantity", quantity },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("item_used", parameters);
        }

        #endregion

        #region クエストイベント

        /// <summary>
        /// クエスト開始を記録
        /// </summary>
        public void RecordQuestStart(string questId, string questName, int playerLevel)
        {
            var parameters = new Dictionary<string, object>
            {
                { "quest_id", questId },
                { "quest_name", questName },
                { "player_level", playerLevel },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("quest_start", parameters);
        }

        /// <summary>
        /// クエスト完了を記録
        /// </summary>
        public void RecordQuestComplete(string questId, string questName, float completionTime, int playerLevel)
        {
            var parameters = new Dictionary<string, object>
            {
                { "quest_id", questId },
                { "quest_name", questName },
                { "completion_time", completionTime },
                { "player_level", playerLevel },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("quest_complete", parameters);
        }

        #endregion

        #region 経済イベント

        /// <summary>
        /// 通貨取得を記録
        /// </summary>
        public void RecordCurrencyGained(string currencyType, int amount, string source)
        {
            var parameters = new Dictionary<string, object>
            {
                { "currency_type", currencyType },
                { "amount", amount },
                { "source", source },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("currency_gained", parameters);
        }

        /// <summary>
        /// 通貨消費を記録
        /// </summary>
        public void RecordCurrencySpent(string currencyType, int amount, string itemPurchased)
        {
            var parameters = new Dictionary<string, object>
            {
                { "currency_type", currencyType },
                { "amount", amount },
                { "item_purchased", itemPurchased },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("currency_spent", parameters);
        }

        #endregion

        #region UIイベント

        /// <summary>
        /// 画面遷移を記録
        /// </summary>
        public void RecordScreenView(string screenName, string previousScreen = null)
        {
            var parameters = new Dictionary<string, object>
            {
                { "screen_name", screenName },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            if (!string.IsNullOrEmpty(previousScreen))
            {
                parameters.Add("previous_screen", previousScreen);
            }

            SendCustomEvent("screen_view", parameters);
        }

        /// <summary>
        /// ボタンクリックを記録
        /// </summary>
        public void RecordButtonClick(string buttonName, string screenName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "button_name", buttonName },
                { "screen_name", screenName },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("button_click", parameters);
        }

        #endregion

        #region カスタムイベント

        /// <summary>
        /// カスタムイベントを送信
        /// </summary>
        public void SendCustomEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!enableAnalytics || !isInitialized)
            {
                if (debugMode)
                {
                    Debug.Log($"[AnalyticsManager] Analytics無効またはイベント送信スキップ: {eventName}");
                }
                return;
            }

            try
            {
                // Unity Analytics 6.1.1のCustomEventを使用
                var customEvent = new Unity.Services.Analytics.CustomEvent(eventName);
                
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var param in parameters)
                    {
                        customEvent.Add(param.Key, param.Value);
                    }
                }

                AnalyticsService.Instance.RecordEvent(customEvent);

                eventsSentThisSession++;
                OnEventSent?.Invoke(eventName);

                if (debugMode)
                {
                    Debug.Log($"[AnalyticsManager] イベント送信: {eventName}");
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            Debug.Log($"  - {param.Key}: {param.Value}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AnalyticsManager] イベント送信失敗 ({eventName}): {e.Message}");
                OnAnalyticsError?.Invoke($"{eventName}: {e.Message}");
            }
        }

        /// <summary>
        /// イベントを即座に送信
        /// </summary>
        public void FlushEvents()
        {
            if (isInitialized && enableAnalytics)
            {
                AnalyticsService.Instance.Flush();
                Debug.Log("[AnalyticsManager] イベント送信完了");
            }
        }

        #endregion

        #region Analytics 6.1.1 標準イベント

        /// <summary>
        /// トランザクションイベントを記録（Analytics 6.1.1）
        /// </summary>
        public void RecordTransaction(string productId, decimal amount, string currency, string transactionId = null)
        {
            if (!isInitialized || !enableAnalytics)
            {
                return;
            }

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "productID", productId },
                    { "amount", (double)amount },
                    { "currency", currency }
                };

                if (!string.IsNullOrEmpty(transactionId))
                {
                    parameters.Add("transactionID", transactionId);
                }

                parameters.Add("timestamp", DateTime.UtcNow.ToString("o"));

                SendCustomEvent("transaction", parameters);

                if (debugMode)
                {
                    Debug.Log($"[AnalyticsManager] トランザクション記録: {productId} - {amount} {currency}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AnalyticsManager] トランザクション記録失敗: {e.Message}");
            }
        }

        /// <summary>
        /// プレイヤープログレスイベントを記録
        /// </summary>
        public void RecordPlayerProgress(string progressionStatus, string progressionType, string progressionName, int score = 0)
        {
            var parameters = new Dictionary<string, object>
            {
                { "progressionStatus", progressionStatus }, // "start", "complete", "fail"
                { "progressionType", progressionType },     // "level", "quest", "achievement"
                { "progressionName", progressionName },
                { "score", score },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SendCustomEvent("player_progression", parameters);
        }

        #endregion

        #region デバッグ機能

        [ContextMenu("Test: Send Test Event")]
        private void TestSendEvent()
        {
            var parameters = new Dictionary<string, object>
            {
                { "test_parameter", "test_value" },
                { "test_number", 123 }
            };
            SendCustomEvent("test_event", parameters);
        }

        [ContextMenu("Test: Record Level Up")]
        private void TestLevelUp()
        {
            RecordLevelUp(10, 5000);
        }

        [ContextMenu("Test: Record Quest Complete")]
        private void TestQuestComplete()
        {
            RecordQuestComplete("quest_001", "初めてのクエスト", 120.5f, 5);
        }

        [ContextMenu("Test: Record Item Acquired")]
        private void TestItemAcquired()
        {
            RecordItemAcquired("item_001", "体力ポーション", 5, "treasure_chest");
        }

        [ContextMenu("Test: Flush Events")]
        private void TestFlush()
        {
            FlushEvents();
        }

        [ContextMenu("Test: Record Transaction")]
        private void TestTransaction()
        {
            RecordTransaction("gold_pack_100", 9.99m, "USD", "test_transaction_001");
        }

        [ContextMenu("Test: Record Player Progress")]
        private void TestPlayerProgress()
        {
            RecordPlayerProgress("complete", "level", "tutorial", 100);
        }

        /// <summary>
        /// Analytics統計を取得
        /// </summary>
        public string GetAnalyticsStats()
        {
            return $"Analytics統計:\n" +
                   $"- 初期化状態: {(isInitialized ? "完了" : "未完了")}\n" +
                   $"- 有効状態: {(enableAnalytics ? "有効" : "無効")}\n" +
                   $"- セッション送信イベント数: {eventsSentThisSession}\n" +
                   $"- デバッグモード: {(debugMode ? "ON" : "OFF")}";
        }

        #endregion
    }
}

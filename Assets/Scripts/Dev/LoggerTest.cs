// LoggerService テスト用スクリプト
using UnityEngine;
using Project.Core.Observability;
using System;

namespace Project.Dev
{
    /// <summary>
    /// LoggerServiceの動作確認用テストスクリプト
    /// </summary>
    public class LoggerTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = false; // デフォルトは手動実行
        [SerializeField] private bool runContinuousTest = false;
        [SerializeField] private float testInterval = 5f;

        private float _nextTestTime;

        private void Start()
        {
            if (runOnStart)
            {
                RunAllTests();
            }

            _nextTestTime = Time.time + testInterval;
        }

        private void Update()
        {
            if (runContinuousTest && Time.time >= _nextTestTime)
            {
                RunPeriodicTest();
                _nextTestTime = Time.time + testInterval;
            }
        }

        /// <summary>
        /// すべてのログレベルをテスト
        /// </summary>
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            LoggerService.Debug("LoggerTest", "This is a DEBUG message - for development info");
            LoggerService.Info("LoggerTest", "This is an INFO message - general information");
            LoggerService.Warning("LoggerTest", "This is a WARNING message - something might be wrong");
            LoggerService.Error("LoggerTest", "This is an ERROR message - something went wrong");
            LoggerService.Critical("LoggerTest", "This is a CRITICAL message - severe error!");

            // カスタムイベントのテスト
            TestCustomEvents();

            // 例外ログのテスト
            TestExceptionLogging();

            UnityEngine.Debug.Log("[LoggerTest] All tests completed. Check logs at: " + Application.persistentDataPath + "/Logs/");
        }

        /// <summary>
        /// カスタムイベントをテスト
        /// </summary>
        [ContextMenu("Test Custom Events")]
        public void TestCustomEvents()
        {
            // プレイヤーレベルアップイベント
            LoggerService.LogCustomEvent("PlayerLevelUp", new
            {
                level = 10,
                exp = 1000,
                timestamp = DateTime.UtcNow
            });

            // アイテム取得イベント
            LoggerService.LogCustomEvent("ItemAcquired", new
            {
                itemId = "sword_001",
                itemName = "Legendary Sword",
                rarity = "Legendary",
                quantity = 1
            });

            // ゲームプレイイベント
            LoggerService.LogCustomEvent("GameplayMetrics", new
            {
                sessionDuration = Time.time,
                enemiesDefeated = 42,
                goldCollected = 1337,
                deathCount = 3
            });

            UnityEngine.Debug.Log("[LoggerTest] Custom events logged");
        }

        /// <summary>
        /// 例外ログをテスト
        /// </summary>
        [ContextMenu("Test Exception Logging")]
        public void TestExceptionLogging()
        {
            try
            {
                // 意図的に例外を発生させる
                throw new InvalidOperationException("This is a test exception for logging");
            }
            catch (Exception ex)
            {
                LoggerService.LogException("LoggerTest", ex);
                UnityEngine.Debug.Log("[LoggerTest] Exception logged successfully");
            }
        }

        /// <summary>
        /// 定期的なテスト（パフォーマンス確認用）
        /// </summary>
        private void RunPeriodicTest()
        {
            LoggerService.Info("LoggerTest", $"Periodic test - Time: {Time.time:F2}s, FPS: {1f / Time.deltaTime:F1}");

            LoggerService.LogCustomEvent("PeriodicMetrics", new
            {
                time = Time.time,
                fps = 1f / Time.deltaTime,
                memoryUsage = System.GC.GetTotalMemory(false) / 1024f / 1024f // MB
            });
        }

        /// <summary>
        /// ストレステスト（大量ログ）
        /// </summary>
        [ContextMenu("Run Stress Test (100 logs)")]
        public void RunStressTest()
        {
            UnityEngine.Debug.Log("[LoggerTest] Starting stress test...");

            for (int i = 0; i < 100; i++)
            {
                LoggerService.Debug("StressTest", $"Log entry {i + 1}/100");
            }

            UnityEngine.Debug.Log("[LoggerTest] Stress test completed - 100 logs written");
        }
    }
}

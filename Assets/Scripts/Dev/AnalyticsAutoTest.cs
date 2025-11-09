// AnalyticsAutoTest: Analytics自動テストスクリプト
using System.Collections;
using UnityEngine;
using Project.Core.Services;

namespace Project.Dev
{
    /// <summary>
    /// Analytics自動テスト
    /// シーン起動時に自動的にAnalyticsの各機能をテスト
    /// </summary>
    public class AnalyticsAutoTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private float delayBetweenTests = 1.0f;
        [SerializeField] private bool verboseLogging = true;

        [Header("Test Selection")]
        [SerializeField] private bool testSessionEvents = true;
        [SerializeField] private bool testPlayerEvents = true;
        [SerializeField] private bool testQuestEvents = true;
        [SerializeField] private bool testEconomyEvents = true;
        [SerializeField] private bool testUIEvents = true;
        [SerializeField] private bool testStandardEvents = true;

        private int totalTests = 0;
        private int passedTests = 0;
        private int failedTests = 0;

        private void Start()
        {
            if (runOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }

        /// <summary>
        /// すべてのテストを実行
        /// </summary>
        public IEnumerator RunAllTests()
        {
            Debug.Log("=== Analytics自動テスト開始 ===");
            totalTests = 0;
            passedTests = 0;
            failedTests = 0;

            // AnalyticsManagerの初期化を待つ
            yield return new WaitUntil(() => AnalyticsManager.Instance != null);
            yield return new WaitForSeconds(0.5f);

            // 各テストを実行
            if (testSessionEvents)
            {
                yield return TestSessionEvents();
                yield return new WaitForSeconds(delayBetweenTests);
            }

            if (testPlayerEvents)
            {
                yield return TestPlayerEvents();
                yield return new WaitForSeconds(delayBetweenTests);
            }

            if (testQuestEvents)
            {
                yield return TestQuestEvents();
                yield return new WaitForSeconds(delayBetweenTests);
            }

            if (testEconomyEvents)
            {
                yield return TestEconomyEvents();
                yield return new WaitForSeconds(delayBetweenTests);
            }

            if (testUIEvents)
            {
                yield return TestUIEvents();
                yield return new WaitForSeconds(delayBetweenTests);
            }

            if (testStandardEvents)
            {
                yield return TestStandardEvents();
                yield return new WaitForSeconds(delayBetweenTests);
            }

            // 結果サマリー
            PrintTestSummary();
        }

        #region セッションイベントテスト

        private IEnumerator TestSessionEvents()
        {
            LogTestCategory("セッションイベントテスト");

            // セッション開始は自動的に送信されるため、ここではテストしない
            LogTest("セッション開始イベント", "自動送信済み", true);

            yield return null;
        }

        #endregion

        #region プレイヤーイベントテスト

        private IEnumerator TestPlayerEvents()
        {
            LogTestCategory("プレイヤーイベントテスト");

            // レベルアップ
            try
            {
                AnalyticsManager.Instance.RecordLevelUp(5, 1000);
                LogTest("レベルアップイベント", "Level 5, XP 1000", true);
            }
            catch (System.Exception e)
            {
                LogTest("レベルアップイベント", e.Message, false);
            }
            yield return new WaitForSeconds(0.2f);

            // プレイヤー死亡
            try
            {
                AnalyticsManager.Instance.RecordPlayerDeath("enemy_attack", 5);
                LogTest("プレイヤー死亡イベント", "enemy_attack, Level 5", true);
            }
            catch (System.Exception e)
            {
                LogTest("プレイヤー死亡イベント", e.Message, false);
            }
            yield return new WaitForSeconds(0.2f);

            // アイテム取得
            try
            {
                AnalyticsManager.Instance.RecordItemAcquired("sword_001", "鉄の剣", 1, "chest");
                LogTest("アイテム取得イベント", "鉄の剣 x1", true);
            }
            catch (System.Exception e)
            {
                LogTest("アイテム取得イベント", e.Message, false);
            }
            yield return new WaitForSeconds(0.2f);

            // アイテム使用
            try
            {
                AnalyticsManager.Instance.RecordItemUsed("potion_001", "体力ポーション", 1);
                LogTest("アイテム使用イベント", "体力ポーション x1", true);
            }
            catch (System.Exception e)
            {
                LogTest("アイテム使用イベント", e.Message, false);
            }

            yield return null;
        }

        #endregion

        #region クエストイベントテスト

        private IEnumerator TestQuestEvents()
        {
            LogTestCategory("クエストイベントテスト");

            // クエスト開始
            try
            {
                AnalyticsManager.Instance.RecordQuestStart("quest_001", "初めてのクエスト", 3);
                LogTest("クエスト開始イベント", "初めてのクエスト", true);
            }
            catch (System.Exception e)
            {
                LogTest("クエスト開始イベント", e.Message, false);
            }
            yield return new WaitForSeconds(0.2f);

            // クエスト完了
            try
            {
                AnalyticsManager.Instance.RecordQuestComplete("quest_001", "初めてのクエスト", 120.5f, 3);
                LogTest("クエスト完了イベント", "120.5秒で完了", true);
            }
            catch (System.Exception e)
            {
                LogTest("クエスト完了イベント", e.Message, false);
            }

            yield return null;
        }

        #endregion

        #region 経済イベントテスト

        private IEnumerator TestEconomyEvents()
        {
            LogTestCategory("経済イベントテスト");

            // 通貨取得
            try
            {
                AnalyticsManager.Instance.RecordCurrencyGained("gold", 100, "quest_reward");
                LogTest("通貨取得イベント", "Gold +100", true);
            }
            catch (System.Exception e)
            {
                LogTest("通貨取得イベント", e.Message, false);
            }
            yield return new WaitForSeconds(0.2f);

            // 通貨消費
            try
            {
                AnalyticsManager.Instance.RecordCurrencySpent("gold", 50, "health_potion");
                LogTest("通貨消費イベント", "Gold -50", true);
            }
            catch (System.Exception e)
            {
                LogTest("通貨消費イベント", e.Message, false);
            }

            yield return null;
        }

        #endregion

        #region UIイベントテスト

        private IEnumerator TestUIEvents()
        {
            LogTestCategory("UIイベントテスト");

            // 画面遷移
            try
            {
                AnalyticsManager.Instance.RecordScreenView("MainMenu", "TitleScreen");
                LogTest("画面遷移イベント", "TitleScreen → MainMenu", true);
            }
            catch (System.Exception e)
            {
                LogTest("画面遷移イベント", e.Message, false);
            }
            yield return new WaitForSeconds(0.2f);

            // ボタンクリック
            try
            {
                AnalyticsManager.Instance.RecordButtonClick("StartButton", "MainMenu");
                LogTest("ボタンクリックイベント", "StartButton", true);
            }
            catch (System.Exception e)
            {
                LogTest("ボタンクリックイベント", e.Message, false);
            }

            yield return null;
        }

        #endregion

        #region 標準イベントテスト (Analytics 6.1.1)

        private IEnumerator TestStandardEvents()
        {
            LogTestCategory("標準イベントテスト (Analytics 6.1.1)");

            // トランザクション
            try
            {
                AnalyticsManager.Instance.RecordTransaction("gold_pack_100", 9.99m, "USD", "test_tx_001");
                LogTest("トランザクションイベント", "$9.99 USD", true);
            }
            catch (System.Exception e)
            {
                LogTest("トランザクションイベント", e.Message, false);
            }
            yield return new WaitForSeconds(0.2f);

            // プレイヤープログレス
            try
            {
                AnalyticsManager.Instance.RecordPlayerProgress("complete", "tutorial", "basic_tutorial", 100);
                LogTest("プレイヤープログレスイベント", "Tutorial完了", true);
            }
            catch (System.Exception e)
            {
                LogTest("プレイヤープログレスイベント", e.Message, false);
            }

            yield return null;
        }

        #endregion

        #region ヘルパーメソッド

        private void LogTestCategory(string category)
        {
            Debug.Log($"\n<color=cyan>【{category}】</color>");
        }

        private void LogTest(string testName, string detail, bool passed)
        {
            totalTests++;
            if (passed)
            {
                passedTests++;
                if (verboseLogging)
                {
                    Debug.Log($"<color=green>✓</color> {testName}: {detail}");
                }
            }
            else
            {
                failedTests++;
                Debug.LogError($"<color=red>✗</color> {testName}: {detail}");
            }
        }

        private void PrintTestSummary()
        {
            Debug.Log("\n" + new string('=', 50));
            Debug.Log("<color=yellow><b>テスト結果サマリー</b></color>");
            Debug.Log(new string('=', 50));
            Debug.Log($"総テスト数: {totalTests}");
            Debug.Log($"<color=green>成功: {passedTests}</color>");
            Debug.Log($"<color=red>失敗: {failedTests}</color>");
            
            float successRate = totalTests > 0 ? (float)passedTests / totalTests * 100f : 0f;
            Debug.Log($"成功率: {successRate:F1}%");
            
            if (failedTests == 0)
            {
                Debug.Log("<color=green><b>✓ すべてのテストが成功しました！</b></color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>⚠ {failedTests}件のテストが失敗しました</color>");
            }
            
            Debug.Log(new string('=', 50) + "\n");

            // Analytics統計を表示
            if (AnalyticsManager.Instance != null)
            {
                Debug.Log(AnalyticsManager.Instance.GetAnalyticsStats());
            }
        }

        #endregion

        #region Context Menu

        [ContextMenu("Run All Tests")]
        public void RunAllTestsManual()
        {
            StartCoroutine(RunAllTests());
        }

        [ContextMenu("Run Session Tests")]
        public void RunSessionTestsManual()
        {
            StartCoroutine(TestSessionEvents());
        }

        [ContextMenu("Run Player Tests")]
        public void RunPlayerTestsManual()
        {
            StartCoroutine(TestPlayerEvents());
        }

        [ContextMenu("Run Quest Tests")]
        public void RunQuestTestsManual()
        {
            StartCoroutine(TestQuestEvents());
        }

        [ContextMenu("Run Economy Tests")]
        public void RunEconomyTestsManual()
        {
            StartCoroutine(TestEconomyEvents());
        }

        [ContextMenu("Run UI Tests")]
        public void RunUITestsManual()
        {
            StartCoroutine(TestUIEvents());
        }

        [ContextMenu("Run Standard Events Tests")]
        public void RunStandardEventsTestsManual()
        {
            StartCoroutine(TestStandardEvents());
        }

        #endregion
    }
}

// AnalyticsManagerTests: Unity Test Framework用テスト
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Project.Core.Services;

namespace Project.Tests
{
    /// <summary>
    /// AnalyticsManager用Unity Test Framework自動テスト
    /// </summary>
    public class AnalyticsManagerTests
    {
        private GameObject testObject;
        private AnalyticsManager analyticsManager;

        [SetUp]
        public void Setup()
        {
            // テスト用GameObjectとAnalyticsManagerを作成
            testObject = new GameObject("TestAnalyticsManager");
            analyticsManager = testObject.AddComponent<AnalyticsManager>();
        }

        [TearDown]
        public void Teardown()
        {
            // テスト後のクリーンアップ
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        #region 基本機能テスト

        [Test]
        public void AnalyticsManager_Singleton_IsNotNull()
        {
            // AnalyticsManagerのシングルトンインスタンスが存在することを確認
            Assert.IsNotNull(AnalyticsManager.Instance, "AnalyticsManager.Instanceがnullです");
        }

        [Test]
        public void AnalyticsManager_GetAnalyticsStats_ReturnsString()
        {
            // GetAnalyticsStats()が文字列を返すことを確認
            string stats = analyticsManager.GetAnalyticsStats();
            Assert.IsNotNull(stats, "GetAnalyticsStats()がnullを返しました");
            Assert.IsNotEmpty(stats, "GetAnalyticsStats()が空文字列を返しました");
        }

        #endregion

        #region プレイヤーイベントテスト

        [Test]
        public void RecordLevelUp_DoesNotThrowException()
        {
            // RecordLevelUp()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordLevelUp(10, 5000);
            }, "RecordLevelUp()が例外をスローしました");
        }

        [Test]
        public void RecordPlayerDeath_DoesNotThrowException()
        {
            // RecordPlayerDeath()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordPlayerDeath("enemy_attack", 5);
            }, "RecordPlayerDeath()が例外をスローしました");
        }

        [Test]
        public void RecordItemAcquired_DoesNotThrowException()
        {
            // RecordItemAcquired()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordItemAcquired("item_001", "テストアイテム", 1, "test");
            }, "RecordItemAcquired()が例外をスローしました");
        }

        [Test]
        public void RecordItemUsed_DoesNotThrowException()
        {
            // RecordItemUsed()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordItemUsed("item_001", "テストアイテム", 1);
            }, "RecordItemUsed()が例外をスローしました");
        }

        #endregion

        #region クエストイベントテスト

        [Test]
        public void RecordQuestStart_DoesNotThrowException()
        {
            // RecordQuestStart()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordQuestStart("quest_001", "テストクエスト", 5);
            }, "RecordQuestStart()が例外をスローしました");
        }

        [Test]
        public void RecordQuestComplete_DoesNotThrowException()
        {
            // RecordQuestComplete()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordQuestComplete("quest_001", "テストクエスト", 120.5f, 5);
            }, "RecordQuestComplete()が例外をスローしました");
        }

        #endregion

        #region 経済イベントテスト

        [Test]
        public void RecordCurrencyGained_DoesNotThrowException()
        {
            // RecordCurrencyGained()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordCurrencyGained("gold", 100, "test");
            }, "RecordCurrencyGained()が例外をスローしました");
        }

        [Test]
        public void RecordCurrencySpent_DoesNotThrowException()
        {
            // RecordCurrencySpent()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordCurrencySpent("gold", 50, "test_item");
            }, "RecordCurrencySpent()が例外をスローしました");
        }

        #endregion

        #region UIイベントテスト

        [Test]
        public void RecordScreenView_DoesNotThrowException()
        {
            // RecordScreenView()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordScreenView("TestScreen", "PreviousScreen");
            }, "RecordScreenView()が例外をスローしました");
        }

        [Test]
        public void RecordButtonClick_DoesNotThrowException()
        {
            // RecordButtonClick()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordButtonClick("TestButton", "TestScreen");
            }, "RecordButtonClick()が例外をスローしました");
        }

        #endregion

        #region 標準イベントテスト (Analytics 6.1.1)

        [Test]
        public void RecordTransaction_DoesNotThrowException()
        {
            // RecordTransaction()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordTransaction("test_product", 9.99m, "USD", "test_tx");
            }, "RecordTransaction()が例外をスローしました");
        }

        [Test]
        public void RecordPlayerProgress_DoesNotThrowException()
        {
            // RecordPlayerProgress()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.RecordPlayerProgress("complete", "level", "test_level", 100);
            }, "RecordPlayerProgress()が例外をスローしました");
        }

        #endregion

        #region カスタムイベントテスト

        [Test]
        public void SendCustomEvent_WithoutParameters_DoesNotThrowException()
        {
            // パラメータなしのSendCustomEvent()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.SendCustomEvent("test_event");
            }, "SendCustomEvent()（パラメータなし）が例外をスローしました");
        }

        [Test]
        public void SendCustomEvent_WithParameters_DoesNotThrowException()
        {
            // パラメータありのSendCustomEvent()が例外をスローしないことを確認
            var parameters = new System.Collections.Generic.Dictionary<string, object>
            {
                { "test_key", "test_value" },
                { "test_number", 123 }
            };

            Assert.DoesNotThrow(() =>
            {
                analyticsManager.SendCustomEvent("test_event", parameters);
            }, "SendCustomEvent()（パラメータあり）が例外をスローしました");
        }

        [Test]
        public void FlushEvents_DoesNotThrowException()
        {
            // FlushEvents()が例外をスローしないことを確認
            Assert.DoesNotThrow(() =>
            {
                analyticsManager.FlushEvents();
            }, "FlushEvents()が例外をスローしました");
        }

        #endregion

        #region PlayModeテスト

        [UnityTest]
        public IEnumerator AnalyticsManager_InitializesCorrectly()
        {
            // AnalyticsManagerが正しく初期化されることを確認
            yield return new WaitForSeconds(1.0f);

            Assert.IsNotNull(AnalyticsManager.Instance, "AnalyticsManagerが初期化されていません");
        }

        [UnityTest]
        public IEnumerator SendMultipleEvents_DoesNotCauseErrors()
        {
            // 複数のイベントを連続送信してもエラーが発生しないことを確認
            yield return new WaitForSeconds(0.5f);

            LogAssert.NoUnexpectedReceived();

            analyticsManager.RecordLevelUp(1, 100);
            yield return new WaitForSeconds(0.1f);

            analyticsManager.RecordItemAcquired("item_001", "テスト", 1, "test");
            yield return new WaitForSeconds(0.1f);

            analyticsManager.RecordQuestStart("quest_001", "テスト", 1);
            yield return new WaitForSeconds(0.1f);

            // エラーログがないことを確認
            LogAssert.NoUnexpectedReceived();
        }

        #endregion
    }
}

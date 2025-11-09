// RemoteConfigManager: Unity Remote Configを使用したサーバー側設定管理
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.RemoteConfig;

namespace Project.Core.Services
{
    /// <summary>
    /// Remote Config管理
    /// サーバー側から動的に設定を変更可能
    /// Phase 2実装: 経験値倍率、ドロップ率、イベント設定
    /// </summary>
    public class RemoteConfigManager : MonoBehaviour
    {
        public static RemoteConfigManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool autoFetchOnStart = true;
        [SerializeField] private float autoFetchInterval = 300f; // 5分ごと

        // デフォルト設定値
        private struct DefaultConfig
        {
            public const float ExperienceMultiplier = 1.0f;
            public const float DropRateMultiplier = 1.0f;
            public const bool EventEnabled = false;
            public const string EventMessage = "No active event";
            public const int MaxLevel = 100;
            public const int DailyRewardGold = 100;
        }

        // 現在の設定値
        public float ExperienceMultiplier { get; private set; } = DefaultConfig.ExperienceMultiplier;
        public float DropRateMultiplier { get; private set; } = DefaultConfig.DropRateMultiplier;
        public bool EventEnabled { get; private set; } = DefaultConfig.EventEnabled;
        public string EventMessage { get; private set; } = DefaultConfig.EventMessage;
        public int MaxLevel { get; private set; } = DefaultConfig.MaxLevel;
        public int DailyRewardGold { get; private set; } = DefaultConfig.DailyRewardGold;

        // イベント
        public event Action OnConfigFetched;
        public event Action<string> OnConfigError;

        private float nextFetchTime;
        private bool isFetching = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[RemoteConfigManager] 初期化完了");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            if (autoFetchOnStart)
            {
                await FetchConfigAsync();
            }

            nextFetchTime = Time.time + autoFetchInterval;
        }

        private void Update()
        {
            // 自動更新
            if (autoFetchInterval > 0 && Time.time >= nextFetchTime)
            {
                _ = FetchConfigAsync();
                nextFetchTime = Time.time + autoFetchInterval;
            }
        }

        /// <summary>
        /// Remote Configを取得
        /// </summary>
        public async Task<bool> FetchConfigAsync()
        {
            if (isFetching)
            {
                Debug.LogWarning("[RemoteConfigManager] 既に取得中です");
                return false;
            }

            isFetching = true;

            try
            {
                Debug.Log("[RemoteConfigManager] Remote Config取得開始...");

                // Unity Gaming Servicesが初期化されているか確認
                if (!Unity.Services.Core.UnityServices.State.Equals(Unity.Services.Core.ServicesInitializationState.Initialized))
                {
                    Debug.LogWarning("[RemoteConfigManager] Unity Services未初期化 - デフォルト値を使用");
                    ApplyDefaultConfig();
                    isFetching = false;
                    return false;
                }

                // Remote Configを取得
                await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());

                // 設定値を適用
                ApplyRemoteConfig();

                Debug.Log("[RemoteConfigManager] Remote Config取得成功");
                OnConfigFetched?.Invoke();

                isFetching = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RemoteConfigManager] Remote Config取得失敗: {e.Message}");
                Debug.LogWarning("[RemoteConfigManager] デフォルト値を使用します");
                
                ApplyDefaultConfig();
                OnConfigError?.Invoke(e.Message);

                isFetching = false;
                return false;
            }
        }

        /// <summary>
        /// Remote Configの値を適用
        /// </summary>
        private void ApplyRemoteConfig()
        {
            var config = RemoteConfigService.Instance.appConfig;
            
            ExperienceMultiplier = config.GetFloat("experienceMultiplier", DefaultConfig.ExperienceMultiplier);
            DropRateMultiplier = config.GetFloat("dropRateMultiplier", DefaultConfig.DropRateMultiplier);
            EventEnabled = config.GetBool("eventEnabled", DefaultConfig.EventEnabled);
            EventMessage = config.GetString("eventMessage", DefaultConfig.EventMessage);
            MaxLevel = config.GetInt("maxLevel", DefaultConfig.MaxLevel);
            DailyRewardGold = config.GetInt("dailyRewardGold", DefaultConfig.DailyRewardGold);

            Debug.Log($"[RemoteConfigManager] 設定適用: EXP倍率={ExperienceMultiplier}, ドロップ倍率={DropRateMultiplier}, イベント={EventEnabled}");
        }

        /// <summary>
        /// デフォルト設定を適用
        /// </summary>
        private void ApplyDefaultConfig()
        {
            ExperienceMultiplier = DefaultConfig.ExperienceMultiplier;
            DropRateMultiplier = DefaultConfig.DropRateMultiplier;
            EventEnabled = DefaultConfig.EventEnabled;
            EventMessage = DefaultConfig.EventMessage;
            MaxLevel = DefaultConfig.MaxLevel;
            DailyRewardGold = DefaultConfig.DailyRewardGold;

            Debug.Log("[RemoteConfigManager] デフォルト設定を適用");
        }

        /// <summary>
        /// 手動で設定を更新
        /// </summary>
        public void RefreshConfig()
        {
            _ = FetchConfigAsync();
        }

        /// <summary>
        /// 現在の設定を取得（デバッグ用）
        /// </summary>
        public Dictionary<string, object> GetCurrentConfig()
        {
            return new Dictionary<string, object>
            {
                { "experienceMultiplier", ExperienceMultiplier },
                { "dropRateMultiplier", DropRateMultiplier },
                { "eventEnabled", EventEnabled },
                { "eventMessage", EventMessage },
                { "maxLevel", MaxLevel },
                { "dailyRewardGold", DailyRewardGold }
            };
        }

#if UNITY_EDITOR
        [ContextMenu("Test: Fetch Config")]
        private void TestFetchConfig()
        {
            _ = FetchConfigAsync();
        }

        [ContextMenu("Test: Show Current Config")]
        private void TestShowConfig()
        {
            Debug.Log("=== Current Remote Config ===");
            Debug.Log($"Experience Multiplier: {ExperienceMultiplier}");
            Debug.Log($"Drop Rate Multiplier: {DropRateMultiplier}");
            Debug.Log($"Event Enabled: {EventEnabled}");
            Debug.Log($"Event Message: {EventMessage}");
            Debug.Log($"Max Level: {MaxLevel}");
            Debug.Log($"Daily Reward Gold: {DailyRewardGold}");
            Debug.Log("==============================");
        }

        [ContextMenu("Test: Apply Default Config")]
        private void TestApplyDefault()
        {
            ApplyDefaultConfig();
            Debug.Log("[RemoteConfigManager] デフォルト設定を適用しました");
        }
#endif
    }

    /// <summary>
    /// ユーザー属性（A/Bテスト用）
    /// </summary>
    public struct UserAttributes { }

    /// <summary>
    /// アプリ属性（A/Bテスト用）
    /// </summary>
    public struct AppAttributes { }
}

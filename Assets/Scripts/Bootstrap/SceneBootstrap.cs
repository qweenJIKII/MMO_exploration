// シーンブートストラップ: 各シーンの初期化処理
using UnityEngine;
using Project.Core;
using Project.Core.Observability;
using Project.Dev;

namespace Project.Bootstrap
{
    /// <summary>
    /// シーンの初期化処理を管理
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("Unity Gaming Services")]
        [SerializeField] private bool enableUGS = true;

        [Header("Development Tools")]
        [SerializeField] private bool enableDevConsole = true;
        [SerializeField] private bool enableProfiling = true;
        [SerializeField] private bool enableMetricsSampling = true;

        private async void Awake()
        {
            LoggerService.Info("Bootstrap", $"Scene '{gameObject.scene.name}' initializing...");

            // UGS初期化
            if (enableUGS)
            {
                await InitializeUGS();
            }

            // 開発ツールの初期化
            InitializeDevelopmentTools();

            // ゲーム状態管理の初期化
            InitializeGameState();

            LoggerService.Info("Bootstrap", "Scene initialization completed.");
        }

        private void InitializeDevelopmentTools()
        {
            // DevConsole
            if (enableDevConsole && FindFirstObjectByType<DevConsole>() == null)
            {
                GameObject consoleGo = new GameObject("DevConsole");
                consoleGo.AddComponent<DevConsole>();
                DontDestroyOnLoad(consoleGo);
                LoggerService.Debug("Bootstrap", "DevConsole initialized.");
            }

            // ProfilingManager
            if (enableProfiling)
            {
                // ProfilingManagerはシングルトンなので、アクセスするだけで初期化される
                _ = ProfilingManager.Instance;
                LoggerService.Debug("Bootstrap", "ProfilingManager initialized.");
            }

            // MetricsSampler
            if (enableMetricsSampling && FindFirstObjectByType<MetricsSampler>() == null)
            {
                GameObject metricsGo = new GameObject("MetricsSampler");
                metricsGo.AddComponent<MetricsSampler>();
                DontDestroyOnLoad(metricsGo);
                LoggerService.Debug("Bootstrap", "MetricsSampler initialized.");
            }

            // DebugDrawManager
            _ = DebugDrawManager.Instance;
            LoggerService.Debug("Bootstrap", "DebugDrawManager initialized.");
        }

        private void InitializeGameState()
        {
            // GameStateManagerはシングルトンなので、アクセスするだけで初期化される
            _ = GameStateManager.Instance;
            LoggerService.Debug("Bootstrap", "GameStateManager initialized.");
        }

        private async System.Threading.Tasks.Task InitializeUGS()
        {
            try
            {
                // UGS初期化
                await Unity.Services.Core.UnityServices.InitializeAsync();
                LoggerService.Info("Bootstrap", "Unity Services initialized.");

                // 匿名サインイン
                if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
                {
                    await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
                    string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                    LoggerService.Info("Bootstrap", $"Signed in anonymously. Player ID: {playerId}");
                }
                else
                {
                    string playerId = Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;
                    LoggerService.Info("Bootstrap", $"Already signed in. Player ID: {playerId}");
                }
            }
            catch (System.Exception ex)
            {
                LoggerService.LogException("Bootstrap", ex);
                LoggerService.Error("Bootstrap", "Failed to initialize Unity Services. UGS features will be unavailable.");
            }
        }

        private void OnDestroy()
        {
            LoggerService.Info("Bootstrap", $"Scene '{gameObject.scene.name}' destroyed.");
        }
    }
}

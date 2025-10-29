// プロファイリング管理: ProfilerMarkerの統合管理
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace Project.Dev
{
    /// <summary>
    /// プロファイリングマーカーを管理し、計測結果を表示
    /// </summary>
    public class ProfilingManager : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool showUI = false; // デフォルトは非表示（F2で表示）
        [SerializeField] private int fontSize = 14;
        [SerializeField] private int windowWidth = 400;
        [SerializeField] private int windowHeight = 350;
        [SerializeField] private bool anchorToRight = true; // 右側に固定

        private static ProfilingManager _instance;
        private Dictionary<string, ProfilerMarker> _markers = new Dictionary<string, ProfilerMarker>();
        private Dictionary<string, ProfilerRecorder> _recorders = new Dictionary<string, ProfilerRecorder>();
        private Dictionary<string, ProfilingStats> _stats = new Dictionary<string, ProfilingStats>();
        private Vector2 _scrollPosition;

        public static ProfilingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ProfilingManager");
                    _instance = go.AddComponent<ProfilingManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // デフォルトマーカーを登録
            RegisterDefaultMarkers();
        }

        private void Update()
        {
            // New Input Systemを試行
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            bool f2Pressed = false;

            if (keyboard != null)
            {
                f2Pressed = keyboard.f2Key.wasPressedThisFrame;
            }
            else
            {
                // Fallback: 旧Input API
                f2Pressed = Input.GetKeyDown(KeyCode.F2);
            }

            // F2キーでトグル
            if (f2Pressed)
            {
                showUI = !showUI;
                UnityEngine.Debug.Log($"[ProfilingManager] F2 pressed. ShowUI: {showUI}");
            }

            // 統計情報を更新
            UpdateStats();
        }

        private void OnDestroy()
        {
            // すべてのRecorderを破棄
            foreach (var recorder in _recorders.Values)
            {
                recorder.Dispose();
            }
            _recorders.Clear();
        }

        private void OnGUI()
        {
            if (!showUI) return;

            // フォントサイズを設定
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label) 
            { 
                richText = true, 
                fontSize = fontSize + 2,
                fontStyle = FontStyle.Bold
            };
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize };

            // ウィンドウの位置を計算（右上に配置）
            float xPos = anchorToRight ? Screen.width - windowWidth - 10 : 10;
            float yPos = 10;

            GUILayout.BeginArea(new Rect(xPos, yPos, windowWidth, windowHeight));
            GUILayout.BeginVertical("box");

            GUILayout.Label("<b>Profiling Stats (F2 to toggle)</b>", headerStyle);
            GUILayout.Space(5);

            // スクロール可能なエリア
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(windowHeight - 60));

            foreach (var kvp in _stats)
            {
                string name = kvp.Key;
                ProfilingStats stats = kvp.Value;

                if (stats.SampleCount > 0)
                {
                    GUILayout.Label($"<b>{name}</b>:", new GUIStyle(labelStyle) { richText = true });
                    GUILayout.Label($"  Avg: {stats.AverageMs:F3}ms | Min: {stats.MinMs:F3}ms | Max: {stats.MaxMs:F3}ms", labelStyle);
                    GUILayout.Space(3);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void RegisterDefaultMarkers()
        {
            // デフォルトのプロファイリングカテゴリを登録
            RegisterMarker("Update");
            RegisterMarker("FixedUpdate");
            RegisterMarker("LateUpdate");
            RegisterMarker("Rendering");
            RegisterMarker("Physics");
            RegisterMarker("Animation");
            RegisterMarker("UI");
        }

        private void UpdateStats()
        {
            foreach (var kvp in _recorders)
            {
                string name = kvp.Key;
                ProfilerRecorder recorder = kvp.Value;

                if (!recorder.Valid || recorder.Count == 0)
                    continue;

                double lastValue = recorder.LastValue / 1e6; // ナノ秒からミリ秒に変換

                if (!_stats.ContainsKey(name))
                {
                    _stats[name] = new ProfilingStats();
                }

                ProfilingStats stats = _stats[name];
                stats.AddSample(lastValue);
                _stats[name] = stats;
            }
        }

        // ========== Public API ==========

        /// <summary>
        /// プロファイリングマーカーを登録
        /// </summary>
        public static void RegisterMarker(string name)
        {
            if (!Instance._markers.ContainsKey(name))
            {
                Instance._markers[name] = new ProfilerMarker(name);
                Instance._recorders[name] = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, name);
                Instance._stats[name] = new ProfilingStats();
            }
        }

        /// <summary>
        /// プロファイリングマーカーを取得（自動登録）
        /// </summary>
        public static ProfilerMarker GetMarker(string name)
        {
            if (!Instance._markers.ContainsKey(name))
            {
                RegisterMarker(name);
            }
            return Instance._markers[name];
        }

        /// <summary>
        /// プロファイリングスコープを開始
        /// </summary>
        public static ProfilerMarker.AutoScope BeginScope(string name)
        {
            return GetMarker(name).Auto();
        }

        /// <summary>
        /// 統計情報をリセット
        /// </summary>
        public static void ResetStats()
        {
            Instance._stats.Clear();
        }

        /// <summary>
        /// 統計情報を取得
        /// </summary>
        public static Dictionary<string, ProfilingStats> GetStats()
        {
            return new Dictionary<string, ProfilingStats>(Instance._stats);
        }

        // ========== Internal Types ==========

        public struct ProfilingStats
        {
            public double TotalMs;
            public double MinMs;
            public double MaxMs;
            public int SampleCount;

            public double AverageMs => SampleCount > 0 ? TotalMs / SampleCount : 0;

            public void AddSample(double valueMs)
            {
                if (SampleCount == 0)
                {
                    MinMs = valueMs;
                    MaxMs = valueMs;
                }
                else
                {
                    MinMs = Math.Min(MinMs, valueMs);
                    MaxMs = Math.Max(MaxMs, valueMs);
                }

                TotalMs += valueMs;
                SampleCount++;
            }
        }
    }

    /// <summary>
    /// プロファイリングスコープのヘルパークラス
    /// using (new ProfilingScope("MyCategory")) { ... } の形式で使用
    /// </summary>
    public struct ProfilingScope : IDisposable
    {
        private ProfilerMarker.AutoScope _scope;

        public ProfilingScope(string name)
        {
            _scope = ProfilingManager.BeginScope(name);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}

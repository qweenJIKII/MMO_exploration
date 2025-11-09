// プレイヤーデータマネージャー: Cloud Saveとの連携、自動保存
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;

namespace Project.Core.Player
{
    /// <summary>
    /// プレイヤーデータの管理とCloud Save連携
    /// </summary>
    public class PlayerDataManager : MonoBehaviour
    {
        private const string PLAYER_DATA_KEY = "PlayerData";
        private const float AUTO_SAVE_INTERVAL = 60f; // 60秒ごとに自動保存

        private static PlayerDataManager _instance;
        public static PlayerDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerDataManager");
                    _instance = go.AddComponent<PlayerDataManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private PlayerData _currentPlayerData;
        private float _autoSaveTimer = 0f;
        private bool _isDirty = false; // データが変更されたかどうか

        public PlayerData CurrentPlayerData => _currentPlayerData;
        public bool IsLoaded => _currentPlayerData != null;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // 自動保存タイマー
            if (_isDirty && IsLoaded)
            {
                _autoSaveTimer += Time.deltaTime;
                if (_autoSaveTimer >= AUTO_SAVE_INTERVAL)
                {
                    _autoSaveTimer = 0f;
                    _ = SavePlayerDataAsync();
                }
            }
        }

        private void OnApplicationQuit()
        {
            // アプリケーション終了時に保存
            if (_isDirty && IsLoaded)
            {
                _ = SavePlayerDataAsync();
            }
        }

        /// <summary>
        /// プレイヤーデータをCloud Saveから読み込み
        /// </summary>
        public async Task<PlayerData> LoadPlayerDataAsync()
        {
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.LogError("[PlayerDataManager] Not authenticated. Cannot load player data.");
                    return null;
                }

                string playerId = AuthenticationService.Instance.PlayerId;
                Debug.Log($"[PlayerDataManager] Loading player data for: {playerId}");

                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { PLAYER_DATA_KEY });

                if (data.TryGetValue(PLAYER_DATA_KEY, out var item))
                {
                    string json = item.Value.GetAsString();
                    _currentPlayerData = JsonUtility.FromJson<PlayerData>(json);
                    _currentPlayerData.UpdateLastLogin();
                    _isDirty = true; // ログイン時刻を更新したので保存が必要
                    Debug.Log($"[PlayerDataManager] Loaded player data: {_currentPlayerData.playerName} (Level {_currentPlayerData.level})");
                }
                else
                {
                    // データが存在しない場合はデフォルトデータを作成
                    Debug.Log("[PlayerDataManager] No existing data found. Creating default player data.");
                    _currentPlayerData = PlayerData.CreateDefault(playerId);
                    _isDirty = true;
                    await SavePlayerDataAsync(); // 初回保存
                }

                return _currentPlayerData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerDataManager] Failed to load player data: {ex.Message}");
                
                // フォールバック: ローカルデータを作成
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    return null;
                }
                
                _currentPlayerData = PlayerData.CreateDefault(AuthenticationService.Instance.PlayerId);
                _isDirty = true;
                return _currentPlayerData;
            }
        }

        /// <summary>
        /// プレイヤーデータをCloud Saveに保存
        /// </summary>
        public async Task SavePlayerDataAsync()
        {
            if (_currentPlayerData == null)
            {
                Debug.LogWarning("[PlayerDataManager] No player data to save.");
                return;
            }

            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.LogError("[PlayerDataManager] Not authenticated. Cannot save player data.");
                    return;
                }

                string json = JsonUtility.ToJson(_currentPlayerData);
                var data = new Dictionary<string, object> { { PLAYER_DATA_KEY, json } };

                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                _isDirty = false;
                Debug.Log($"[PlayerDataManager] Saved player data: {_currentPlayerData.playerName} (Level {_currentPlayerData.level})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerDataManager] Failed to save player data: {ex.Message}");
            }
        }

        /// <summary>
        /// プレイヤー名を設定
        /// </summary>
        public void SetPlayerName(string name)
        {
            if (_currentPlayerData == null)
            {
                Debug.LogWarning("[PlayerDataManager] Player data not loaded.");
                return;
            }

            _currentPlayerData.playerName = name;
            _isDirty = true;
            Debug.Log($"[PlayerDataManager] Player name set to: {name}");
        }

        /// <summary>
        /// 経験値を追加
        /// </summary>
        public void AddExperience(int amount)
        {
            if (_currentPlayerData == null)
            {
                Debug.LogWarning("[PlayerDataManager] Player data not loaded.");
                return;
            }

            _currentPlayerData.AddExperience(amount);
            _isDirty = true;
        }

        /// <summary>
        /// プレイヤーの位置と回転を更新
        /// </summary>
        public void UpdatePosition(Vector3 position, Quaternion rotation)
        {
            if (_currentPlayerData == null)
            {
                Debug.LogWarning("[PlayerDataManager] Player data not loaded.");
                return;
            }

            _currentPlayerData.position = position;
            _currentPlayerData.rotation = rotation;
            _isDirty = true;
        }

        /// <summary>
        /// 手動保存をトリガー
        /// </summary>
        public void ForceSave()
        {
            if (_isDirty && IsLoaded)
            {
                _ = SavePlayerDataAsync();
            }
        }
    }
}

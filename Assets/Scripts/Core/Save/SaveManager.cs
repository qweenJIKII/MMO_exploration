// SaveManager: セーブ/ロード管理
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace Project.Core.Save
{
    /// <summary>
    /// セーブ/ロード管理
    /// Cloud Save + ローカルバックアップ
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance => instance;

        [Header("Save Settings")]
        [SerializeField] private bool useCloudSave = true;
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5分

        [Header("Local Backup")]
        [SerializeField] private bool createLocalBackup = true;
        [SerializeField] private int maxBackupFiles = 3;

        private const string SAVE_KEY = "PlayerSaveData";
        private const string LOCAL_SAVE_FOLDER = "Saves";
        private const string SAVE_FILE_NAME = "save.json";

        private float autoSaveTimer = 0f;
        private SaveData currentSaveData;

        // イベント
        public event System.Action<SaveData> OnSaveCompleted;
        public event System.Action<SaveData> OnLoadCompleted;

        private void Awake()
        {
            // シングルトン
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[SaveManager] 初期化");
        }

        private async void Start()
        {
            // ゲーム開始時に自動ロード
            await LoadGame();
        }

        private void OnApplicationQuit()
        {
            // アプリケーション終了時に自動セーブ
            Debug.Log("[SaveManager] アプリケーション終了 - セーブ実行");
            _ = SaveGame();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // アプリがバックグラウンドに移行する時にセーブ（モバイル対応）
            if (pauseStatus)
            {
                Debug.Log("[SaveManager] アプリケーション一時停止 - セーブ実行");
                _ = SaveGame();
            }
        }

        private void Update()
        {
            // オートセーブ
            if (autoSave)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    autoSaveTimer = 0f;
                    _ = SaveGame();
                }
            }
        }

        #region Save
        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        public async Task<bool> SaveGame()
        {
            try
            {
                // セーブデータを作成
                SaveData saveData = CreateSaveData();
                currentSaveData = saveData;

                // JSON化
                string json = JsonUtility.ToJson(saveData, true);

                // ローカルに保存
                Debug.Log($"[SaveManager] セーブデータ作成: HP={saveData.currentHealth}, Level={saveData.level}");
                SaveToLocal(json);
                Debug.Log($"[SaveManager] ローカルにセーブしました");

                // Cloud Saveに保存（エラーは無視）
                if (useCloudSave)
                {
                    try
                    {
                        await SaveToCloud(json);
                    }
                    catch (System.Exception cloudError)
                    {
                        Debug.LogWarning($"[SaveManager] Cloud Save失敗（ローカル保存は成功）: {cloudError.Message}");
                    }
                }

                Debug.Log($"[SaveManager] ゲームをセーブしました: {saveData.saveDate}");
                OnSaveCompleted?.Invoke(saveData);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] セーブ失敗: {e.Message}");
                return false;
            }
        }

        private SaveData CreateSaveData()
        {
            SaveData data = new SaveData();

            // PlayerStatsから情報を取得
            var playerStats = FindFirstObjectByType<Player.PlayerStats>();
            if (playerStats != null)
            {
                data.level = playerStats.level;
                data.currentHealth = playerStats.currentHealth;
                data.maxHealth = playerStats.maxHealth;
                data.currentMana = playerStats.currentMana;
                data.maxMana = playerStats.maxMana;
                data.currentStamina = playerStats.currentStamina;
                data.maxStamina = playerStats.maxStamina;
                data.strength = playerStats.strength;
                data.defense = (int)playerStats.defensePower;
                data.magic = playerStats.intelligence;
                data.agility = playerStats.dexterity;
                data.criticalRate = playerStats.criticalRate;
            }

            // ExperienceManagerから経験値を取得
            var expManager = FindFirstObjectByType<Player.ExperienceManager>();
            if (expManager != null)
            {
                data.experience = expManager.CurrentExperience;
            }

            // プレイヤー位置を取得
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                data.position = new Vector3Data(player.transform.position);
                data.rotation = new Vector3Data(player.transform.eulerAngles);
            }

            // 現在のシーン
            data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            return data;
        }

        private void SaveToLocal(string json)
        {
            string savePath = GetSavePath();
            
            // ディレクトリ作成
            string directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // バックアップ作成
            if (createLocalBackup && File.Exists(savePath))
            {
                CreateBackup(savePath);
            }

            // 保存
            File.WriteAllText(savePath, json);
            Debug.Log($"[SaveManager] ローカルに保存: {savePath}");
        }

        private async Task SaveToCloud(string json)
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object>
            {
                { SAVE_KEY, json }
            });

            Debug.Log("[SaveManager] Cloud Saveに保存しました");
        }

        private void CreateBackup(string originalPath)
        {
            string directory = Path.GetDirectoryName(originalPath);
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(directory, $"save_backup_{timestamp}.json");

            File.Copy(originalPath, backupPath);

            // 古いバックアップを削除
            CleanupOldBackups(directory);

            Debug.Log($"[SaveManager] バックアップ作成: {backupPath}");
        }

        private void CleanupOldBackups(string directory)
        {
            var backupFiles = Directory.GetFiles(directory, "save_backup_*.json");
            if (backupFiles.Length > maxBackupFiles)
            {
                System.Array.Sort(backupFiles);
                for (int i = 0; i < backupFiles.Length - maxBackupFiles; i++)
                {
                    File.Delete(backupFiles[i]);
                    Debug.Log($"[SaveManager] 古いバックアップを削除: {backupFiles[i]}");
                }
            }
        }
        #endregion

        #region Load
        /// <summary>
        /// ゲームをロード
        /// </summary>
        public async Task<SaveData> LoadGame()
        {
            try
            {
                string json = null;

                // Cloud Saveから読み込み（エラーは無視）
                if (useCloudSave)
                {
                    try
                    {
                        json = await LoadFromCloud();
                    }
                    catch (System.Exception cloudError)
                    {
                        Debug.LogWarning($"[SaveManager] Cloud Save読み込み失敗（ローカルから読み込みます）: {cloudError.Message}");
                    }
                }

                // フォールバック: ローカルから読み込み
                if (string.IsNullOrEmpty(json))
                {
                    json = LoadFromLocal();
                }

                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("[SaveManager] セーブデータが見つかりません");
                    return null;
                }

                // JSON解析
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                currentSaveData = saveData;

                Debug.Log($"[SaveManager] ロードデータ: HP={saveData.currentHealth}, Level={saveData.level}");

                // データを適用
                ApplySaveData(saveData);

                Debug.Log($"[SaveManager] ゲームをロードしました: {saveData.saveDate}");
                OnLoadCompleted?.Invoke(saveData);
                return saveData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] ロード失敗: {e.Message}");
                return null;
            }
        }

        private async Task<string> LoadFromCloud()
        {
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { SAVE_KEY });
            
            if (data.TryGetValue(SAVE_KEY, out var saveJson))
            {
                Debug.Log("[SaveManager] Cloud Saveから読み込みました");
                return saveJson.Value.GetAsString();
            }

            return null;
        }

        private string LoadFromLocal()
        {
            string savePath = GetSavePath();
            
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                Debug.Log($"[SaveManager] ローカルから読み込み: {savePath}");
                return json;
            }

            return null;
        }

        private void ApplySaveData(SaveData data)
        {
            Debug.Log($"[SaveManager] ApplySaveData開始: HP={data.currentHealth}, Level={data.level}");

            // データ検証: 不正なデータの場合は新規ゲームとして扱う
            bool isValidData = data.level > 0 && data.currentHealth > 0;
            if (!isValidData)
            {
                Debug.LogWarning("[SaveManager] 不正なセーブデータを検出。新規ゲームとして開始します。");
                // 不正なデータは適用せず、初期状態を維持
                return;
            }

            // PlayerStatsに適用
            var playerStats = FindFirstObjectByType<Player.PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log($"[SaveManager] PlayerStats適用前: HP={playerStats.currentHealth}, Level={playerStats.level}");
                
                // 1. レベルを設定してステータスを再計算（最大値を更新）
                playerStats.level = data.level;
                playerStats.RecalculateStats();
                
                // 2. 現在値を適用（最大値を超えないようにClamp）
                playerStats.currentHealth = Mathf.Min(data.currentHealth, playerStats.maxHealth);
                playerStats.currentMana = Mathf.Min(data.currentMana, playerStats.maxMana);
                playerStats.currentStamina = Mathf.Min(data.currentStamina, playerStats.maxStamina);
                
                Debug.Log($"[SaveManager] PlayerStats適用後: HP={playerStats.currentHealth}/{playerStats.maxHealth}, Level={playerStats.level}");
                
                // UIに通知（重要！）
                var hudManager = FindFirstObjectByType<UI.GothicHUDManager>();
                if (hudManager != null)
                {
                    hudManager.UpdatePlayerStats(playerStats);
                    Debug.Log("[SaveManager] HUDを更新しました");
                }
            }
            else
            {
                Debug.LogWarning("[SaveManager] PlayerStatsが見つかりません！");
            }

            // ExperienceManagerに適用
            var expManager = FindFirstObjectByType<Player.ExperienceManager>();
            if (expManager != null)
            {
                // レベルと経験値を設定
                // expManager.SetLevelAndExperience(data.level, data.experience);
                Debug.Log("[SaveManager] ExperienceManager適用（未実装）");
            }

            // プレイヤー位置を適用（異常な座標の場合はスキップ）
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && data.position != null)
            {
                Vector3 loadedPosition = data.position.ToVector3();
                
                // 座標の妥当性チェック（Y座標が異常に低い場合はスキップ）
                if (Mathf.Abs(loadedPosition.y) < 1000f)
                {
                    player.transform.position = loadedPosition;
                    player.transform.eulerAngles = data.rotation.ToVector3();
                    Debug.Log($"[SaveManager] プレイヤー位置適用: {loadedPosition}");
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] 異常な座標を検出: {loadedPosition}。位置の適用をスキップします。");
                }
            }
            
            Debug.Log("[SaveManager] ApplySaveData完了");
        }
        #endregion

        #region Utility
        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FOLDER, SAVE_FILE_NAME);
        }

        public bool HasSaveData()
        {
            return File.Exists(GetSavePath());
        }

        public void DeleteSaveData()
        {
            string savePath = GetSavePath();
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("[SaveManager] セーブデータを削除しました");
            }
        }

        /// <summary>
        /// すべてのセーブデータを削除（ローカル + バックアップ）
        /// </summary>
        public void DeleteAllSaves()
        {
            // メインセーブファイルを削除
            DeleteSaveData();

            // バックアップファイルも削除
            string directory = Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FOLDER);
            if (Directory.Exists(directory))
            {
                var backupFiles = Directory.GetFiles(directory, "save_backup_*.json");
                foreach (var file in backupFiles)
                {
                    File.Delete(file);
                    Debug.Log($"[SaveManager] バックアップを削除: {file}");
                }
            }

            currentSaveData = null;
            Debug.Log("[SaveManager] すべてのセーブデータを削除しました");
        }
        #endregion

        #region Debug Context Menu
#if UNITY_EDITOR
        [ContextMenu("Test: Save Game")]
        private void TestSaveGame()
        {
            _ = SaveGame();
        }

        [ContextMenu("Test: Load Game")]
        private void TestLoadGame()
        {
            _ = LoadGame();
        }

        [ContextMenu("Test: Delete Save Data")]
        private void TestDeleteSaveData()
        {
            DeleteSaveData();
        }

        [ContextMenu("Test: Show Save Path")]
        private void TestShowSavePath()
        {
            Debug.Log($"[SaveManager] セーブパス: {GetSavePath()}");
        }
#endif
        #endregion
    }
}

// SettingsManager: 設定管理
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Project.Core.Settings
{
    /// <summary>
    /// グラフィック/オーディオ/入力設定の管理
    /// Cloud Saveと連携して設定を永続化
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager instance;
        public static SettingsManager Instance => instance;

        [Header("Graphics Settings")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool vSyncEnabled = true;
        [SerializeField] private int qualityLevel = 2; // 0=Low, 1=Medium, 2=High

        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = 1.0f;
        [SerializeField] private float musicVolume = 0.8f;
        [SerializeField] private float sfxVolume = 1.0f;

        [Header("Input Settings")]
        [SerializeField] private float mouseSensitivity = 1.0f;
        [SerializeField] private bool invertYAxis = false;

        [Header("Cloud Save")]
        [SerializeField] private bool useCloudSave = true;
        [SerializeField] private bool autoSaveSettings = true;

        // イベント
        public event System.Action OnSettingsChanged;

        private const string SETTINGS_KEY = "PlayerSettings";

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

            Debug.Log("[SettingsManager] 初期化");
        }

        private async void Start()
        {
            // 設定を読み込み
            await LoadSettings();

            // 設定を適用
            ApplyAllSettings();
        }

        #region Settings Application
        /// <summary>
        /// すべての設定を適用
        /// </summary>
        public void ApplyAllSettings()
        {
            ApplyGraphicsSettings();
            ApplyAudioSettings();
            ApplyInputSettings();

            Debug.Log("[SettingsManager] すべての設定を適用しました");
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// グラフィック設定を適用
        /// </summary>
        public void ApplyGraphicsSettings()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
            QualitySettings.SetQualityLevel(qualityLevel, true);

            Debug.Log($"[SettingsManager] グラフィック設定: FPS={targetFrameRate}, VSync={vSyncEnabled}, Quality={qualityLevel}");
        }

        /// <summary>
        /// オーディオ設定を適用
        /// </summary>
        public void ApplyAudioSettings()
        {
            AudioListener.volume = masterVolume;

            // AudioMixerを使用する場合はここで設定
            // audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
            // audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
            // audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);

            Debug.Log($"[SettingsManager] オーディオ設定: Master={masterVolume}, Music={musicVolume}, SFX={sfxVolume}");
        }

        /// <summary>
        /// 入力設定を適用
        /// </summary>
        public void ApplyInputSettings()
        {
            // カメラコントローラーなどに設定を反映
            // 実装は各コントローラーに依存

            Debug.Log($"[SettingsManager] 入力設定: Sensitivity={mouseSensitivity}, InvertY={invertYAxis}");
        }
        #endregion

        #region Getters/Setters
        // Graphics
        public int TargetFrameRate
        {
            get => targetFrameRate;
            set
            {
                targetFrameRate = Mathf.Clamp(value, 30, 144);
                ApplyGraphicsSettings();
                if (autoSaveSettings) _ = SaveSettings();
            }
        }

        public bool VSyncEnabled
        {
            get => vSyncEnabled;
            set
            {
                vSyncEnabled = value;
                ApplyGraphicsSettings();
                if (autoSaveSettings) _ = SaveSettings();
            }
        }

        public int QualityLevel
        {
            get => qualityLevel;
            set
            {
                qualityLevel = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
                ApplyGraphicsSettings();
                if (autoSaveSettings) _ = SaveSettings();
            }
        }

        // Audio
        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                ApplyAudioSettings();
                if (autoSaveSettings) _ = SaveSettings();
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                ApplyAudioSettings();
                if (autoSaveSettings) _ = SaveSettings();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                ApplyAudioSettings();
                if (autoSaveSettings) _ = SaveSettings();
            }
        }

        // Input
        public float MouseSensitivity
        {
            get => mouseSensitivity;
            set
            {
                mouseSensitivity = Mathf.Clamp(value, 0.1f, 5.0f);
                ApplyInputSettings();
                if (autoSaveSettings) _ = SaveSettings();
            }
        }

        public bool InvertYAxis
        {
            get => invertYAxis;
            set
            {
                invertYAxis = value;
                ApplyInputSettings();
                if (autoSaveSettings) _ = SaveSettings();
            }
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// 設定を保存
        /// </summary>
        public async Task SaveSettings()
        {
            var settingsData = new Dictionary<string, object>
            {
                { "targetFrameRate", targetFrameRate },
                { "vSyncEnabled", vSyncEnabled },
                { "qualityLevel", qualityLevel },
                { "masterVolume", masterVolume },
                { "musicVolume", musicVolume },
                { "sfxVolume", sfxVolume },
                { "mouseSensitivity", mouseSensitivity },
                { "invertYAxis", invertYAxis }
            };

            // ローカルに保存（PlayerPrefs）
            SaveToPlayerPrefs(settingsData);

            // Cloud Saveに保存
            if (useCloudSave)
            {
                try
                {
                    await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object>
                    {
                        { SETTINGS_KEY, JsonUtility.ToJson(settingsData) }
                    });

                    Debug.Log("[SettingsManager] 設定をCloud Saveに保存しました");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SettingsManager] Cloud Save失敗: {e.Message}");
                }
            }

            Debug.Log("[SettingsManager] 設定を保存しました");
        }

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public async Task LoadSettings()
        {
            // Cloud Saveから読み込み
            if (useCloudSave)
            {
                try
                {
                    var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { SETTINGS_KEY });
                    
                    if (data.TryGetValue(SETTINGS_KEY, out var settingsJson))
                    {
                        LoadFromJson(settingsJson.Value.GetAsString());
                        Debug.Log("[SettingsManager] Cloud Saveから設定を読み込みました");
                        return;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SettingsManager] Cloud Save読み込み失敗: {e.Message}");
                }
            }

            // フォールバック: PlayerPrefsから読み込み
            LoadFromPlayerPrefs();
            Debug.Log("[SettingsManager] PlayerPrefsから設定を読み込みました");
        }

        private void SaveToPlayerPrefs(Dictionary<string, object> data)
        {
            PlayerPrefs.SetInt("Settings_TargetFrameRate", targetFrameRate);
            PlayerPrefs.SetInt("Settings_VSyncEnabled", vSyncEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Settings_QualityLevel", qualityLevel);
            PlayerPrefs.SetFloat("Settings_MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("Settings_MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("Settings_SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("Settings_MouseSensitivity", mouseSensitivity);
            PlayerPrefs.SetInt("Settings_InvertYAxis", invertYAxis ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadFromPlayerPrefs()
        {
            if (PlayerPrefs.HasKey("Settings_TargetFrameRate"))
            {
                targetFrameRate = PlayerPrefs.GetInt("Settings_TargetFrameRate", 60);
                vSyncEnabled = PlayerPrefs.GetInt("Settings_VSyncEnabled", 1) == 1;
                qualityLevel = PlayerPrefs.GetInt("Settings_QualityLevel", 2);
                masterVolume = PlayerPrefs.GetFloat("Settings_MasterVolume", 1.0f);
                musicVolume = PlayerPrefs.GetFloat("Settings_MusicVolume", 0.8f);
                sfxVolume = PlayerPrefs.GetFloat("Settings_SFXVolume", 1.0f);
                mouseSensitivity = PlayerPrefs.GetFloat("Settings_MouseSensitivity", 1.0f);
                invertYAxis = PlayerPrefs.GetInt("Settings_InvertYAxis", 0) == 1;
            }
        }

        private void LoadFromJson(string json)
        {
            // 簡易的なJSON解析（実際にはJsonUtilityやNewtonsoft.Jsonを使用）
            // ここでは省略
        }
        #endregion

        #region Reset
        /// <summary>
        /// 設定をデフォルトに戻す
        /// </summary>
        public void ResetToDefaults()
        {
            targetFrameRate = 60;
            vSyncEnabled = true;
            qualityLevel = 2;
            masterVolume = 1.0f;
            musicVolume = 0.8f;
            sfxVolume = 1.0f;
            mouseSensitivity = 1.0f;
            invertYAxis = false;

            ApplyAllSettings();
            _ = SaveSettings();

            Debug.Log("[SettingsManager] 設定をデフォルトに戻しました");
        }
        #endregion

        #region Debug Context Menu
#if UNITY_EDITOR
        [ContextMenu("Test: Save Settings")]
        private void TestSaveSettings()
        {
            _ = SaveSettings();
        }

        [ContextMenu("Test: Load Settings")]
        private void TestLoadSettings()
        {
            _ = LoadSettings();
        }

        [ContextMenu("Test: Reset to Defaults")]
        private void TestResetToDefaults()
        {
            ResetToDefaults();
        }
#endif
        #endregion
    }
}

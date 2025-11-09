// 経験値・レベル管理: 経験値取得、レベルアップ処理、報酬付与
using System;
using UnityEngine;

namespace Project.Core.Player
{
    /// <summary>
    /// 経験値とレベルアップを管理
    /// Phase 2実装: 経験値取得、レベルアップ処理、レベルアップ報酬
    /// </summary>
    public class ExperienceManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats playerStats;

        [Header("Experience Settings")]
        [SerializeField] private AnimationCurve experienceCurve = AnimationCurve.EaseInOut(1f, 100f, 100f, 10000f);
        [SerializeField] private int baseExperienceRequired = 100;
        [SerializeField] private float experienceMultiplier = 1.5f;

        // イベント
        public event Action<int> OnExperienceGained;
        public event Action<float, float> OnExperienceChanged; // current, required
        public event Action<int> OnLevelUp;
        public event Action<int, int> OnLevelUpWithOldLevel; // oldLevel, newLevel

        private int currentExperience = 0;
        private int currentLevel = 1;

        // Public accessors
        public int CurrentExperience => currentExperience;
        public int CurrentLevel => currentLevel;

        private void Awake()
        {
            if (playerStats == null)
            {
                Debug.LogError("[ExperienceManager] PlayerStats reference is missing!");
            }
        }

        private void Start()
        {
            // PlayerStatsから初期値を取得
            if (playerStats != null)
            {
                currentLevel = playerStats.level;
                currentExperience = playerStats.experience;
            }
        }

        /// <summary>
        /// 経験値を付与
        /// </summary>
        public void GrantExperience(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[ExperienceManager] Invalid experience amount: {amount}");
                return;
            }

            // Remote Configの経験値倍率を適用
            float multiplier = 1.0f;
            if (Services.RemoteConfigManager.Instance != null)
            {
                multiplier = Services.RemoteConfigManager.Instance.ExperienceMultiplier;
            }

            int finalAmount = Mathf.RoundToInt(amount * multiplier);
            currentExperience += finalAmount;
            OnExperienceGained?.Invoke(finalAmount);

            if (multiplier != 1.0f)
            {
                Debug.Log($"[ExperienceManager] Gained {amount} EXP (x{multiplier}) = {finalAmount} EXP (Total: {currentExperience})");
            }
            else
            {
                Debug.Log($"[ExperienceManager] Gained {finalAmount} EXP (Total: {currentExperience})");
            }

            // レベルアップ判定
            CheckLevelUp();

            // イベント発火
            int required = GetRequiredExperience(currentLevel);
            OnExperienceChanged?.Invoke(currentExperience, required);
        }

        /// <summary>
        /// レベルアップ判定と処理
        /// </summary>
        private void CheckLevelUp()
        {
            int requiredExp = GetRequiredExperience(currentLevel);
            
            while (currentExperience >= requiredExp)
            {
                // 経験値を消費してレベルアップ
                currentExperience -= requiredExp;
                int oldLevel = currentLevel;
                currentLevel++;

                Debug.Log($"[ExperienceManager] Level Up! {oldLevel} -> {currentLevel}");

                // レベルアップ処理
                ProcessLevelUp(oldLevel, currentLevel);

                // 次のレベルの必要経験値を取得
                requiredExp = GetRequiredExperience(currentLevel);

                // イベント発火
                OnLevelUp?.Invoke(currentLevel);
                OnLevelUpWithOldLevel?.Invoke(oldLevel, currentLevel);
            }
        }

        /// <summary>
        /// レベルアップ処理（ステータス更新、報酬付与）
        /// </summary>
        private void ProcessLevelUp(int oldLevel, int newLevel)
        {
            if (playerStats == null) return;

            // ステータス再計算
            playerStats.SetLevel(newLevel);

            // レベルアップ報酬（HP/MP全回復）
            playerStats.RestoreAll();

            Debug.Log($"[ExperienceManager] Level up rewards applied (Level {newLevel})");
        }

        /// <summary>
        /// 指定レベルに必要な経験値を計算
        /// </summary>
        public int GetRequiredExperience(int level)
        {
            if (level < 1) level = 1;

            // カーブを使用した経験値計算
            if (experienceCurve != null && experienceCurve.length > 0)
            {
                return Mathf.RoundToInt(experienceCurve.Evaluate(level));
            }

            // フォールバック: 指数関数的な成長
            return Mathf.RoundToInt(baseExperienceRequired * Mathf.Pow(experienceMultiplier, level - 1));
        }

        /// <summary>
        /// 現在の経験値進捗率を取得（0.0 ~ 1.0）
        /// </summary>
        public float GetExperienceProgress()
        {
            int required = GetRequiredExperience(currentLevel);
            return required > 0 ? (float)currentExperience / required : 0f;
        }

        /// <summary>
        /// 現在のレベルを取得
        /// </summary>
        public int GetCurrentLevel()
        {
            return currentLevel;
        }

        /// <summary>
        /// 現在の経験値を取得
        /// </summary>
        public int GetCurrentExperience()
        {
            return currentExperience;
        }

        /// <summary>
        /// レベルを直接設定（デバッグ用）
        /// </summary>
        public void SetLevel(int level)
        {
            if (level < 1)
            {
                Debug.LogWarning("[ExperienceManager] Level must be at least 1");
                return;
            }

            int oldLevel = currentLevel;
            currentLevel = level;
            currentExperience = 0;

            if (playerStats != null)
            {
                playerStats.SetLevel(level);
                playerStats.RestoreAll();
            }

            OnLevelUp?.Invoke(currentLevel);
            OnLevelUpWithOldLevel?.Invoke(oldLevel, currentLevel);
            OnExperienceChanged?.Invoke(currentExperience, GetRequiredExperience(currentLevel));

            Debug.Log($"[ExperienceManager] Level set to {level}");
        }

        /// <summary>
        /// 経験値を直接設定（デバッグ用）
        /// </summary>
        public void SetExperience(int experience)
        {
            if (experience < 0) experience = 0;
            
            currentExperience = experience;
            CheckLevelUp();
            
            OnExperienceChanged?.Invoke(currentExperience, GetRequiredExperience(currentLevel));
        }

        /// <summary>
        /// 次のレベルまでの残り経験値を取得
        /// </summary>
        public int GetRemainingExperience()
        {
            int required = GetRequiredExperience(currentLevel);
            return Mathf.Max(0, required - currentExperience);
        }

        /// <summary>
        /// シリアライズ用データに変換
        /// </summary>
        public ExperienceData ToData()
        {
            return new ExperienceData
            {
                level = currentLevel,
                experience = currentExperience
            };
        }

        /// <summary>
        /// シリアライズデータから復元
        /// </summary>
        public void FromData(ExperienceData data)
        {
            currentLevel = data.level;
            currentExperience = data.experience;

            if (playerStats != null)
            {
                playerStats.SetLevel(currentLevel);
            }

            OnExperienceChanged?.Invoke(currentExperience, GetRequiredExperience(currentLevel));
        }

#if UNITY_EDITOR
        [ContextMenu("Test: Grant 50 EXP")]
        private void TestGrant50Exp()
        {
            GrantExperience(50);
        }

        [ContextMenu("Test: Grant 500 EXP")]
        private void TestGrant500Exp()
        {
            GrantExperience(500);
        }

        [ContextMenu("Test: Level Up")]
        private void TestLevelUp()
        {
            int required = GetRequiredExperience(currentLevel);
            GrantExperience(required);
        }

        [ContextMenu("Test: Set Level 10")]
        private void TestSetLevel10()
        {
            SetLevel(10);
        }
#endif
    }

    /// <summary>
    /// シリアライズ用の経験値データ
    /// </summary>
    [Serializable]
    public class ExperienceData
    {
        public int level;
        public int experience;
    }
}

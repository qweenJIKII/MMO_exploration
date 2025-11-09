// プレイヤーステータス: HP/MP/スタミナなどの戦闘ステータス管理
using System;
using UnityEngine;

namespace Project.Core.Player
{
    /// <summary>
    /// プレイヤーの戦闘ステータスを管理
    /// Phase 2実装: HP/MP/スタミナ、能力値、ステータス計算
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        // 基本ステータス
        [Header("基本ステータス")]
        public int level = 1;
        public int experience = 0;

        // 戦闘ステータス（現在値）
        [Header("戦闘ステータス")]
        public float currentHealth = 100f;
        public float currentMana = 100f;
        public float currentStamina = 100f;

        // 戦闘ステータス（最大値）
        public float maxHealth = 100f;
        public float maxMana = 100f;
        public float maxStamina = 100f;

        // 能力値（基本値）
        [Header("能力値")]
        public int strength = 10;      // 攻撃力に影響
        public int vitality = 10;      // HP/防御力に影響
        public int intelligence = 10;  // MP/魔力に影響
        public int dexterity = 10;     // 素早さ/回避に影響
        public int luck = 10;          // クリティカル率に影響

        // 計算済み能力値（装備・バフ反映後）
        [Header("計算済み能力値")]
        public float attackPower = 10f;
        public float defensePower = 5f;
        public float magicPower = 10f;
        public float speed = 5f;
        public float criticalRate = 0.05f;

        // イベント
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action<float, float> OnStaminaChanged;
        public event Action<int> OnLevelChanged;
        public event Action OnDeath;

        private void Awake()
        {
            // 初期化時にステータスを計算
            RecalculateStats();
            RestoreAll();
        }

        /// <summary>
        /// デフォルトのステータスを生成
        /// </summary>
        public static PlayerStats CreateDefault(int level = 1)
        {
            var stats = new PlayerStats
            {
                level = level,
                experience = 0
            };
            stats.RecalculateStats();
            stats.RestoreAll();
            return stats;
        }

        /// <summary>
        /// レベルに基づいてステータスを再計算
        /// </summary>
        public void RecalculateStats()
        {
            // レベルに応じた基本能力値の成長
            strength = 10 + (level - 1) * 2;
            vitality = 10 + (level - 1) * 2;
            intelligence = 10 + (level - 1) * 2;
            dexterity = 10 + (level - 1) * 2;
            luck = 10 + (level - 1);

            // 最大値の計算
            maxHealth = 100f + (vitality * 5f) + (level * 10f);
            maxMana = 100f + (intelligence * 5f) + (level * 5f);
            maxStamina = 100f + (dexterity * 3f);

            // 計算済み能力値
            attackPower = strength * 2f + level;
            defensePower = vitality * 1.5f + level * 0.5f;
            magicPower = intelligence * 2f + level;
            speed = 5f + dexterity * 0.5f;
            criticalRate = 0.05f + (luck * 0.001f);

            Debug.Log($"[PlayerStats] Stats recalculated for level {level}");
        }

        /// <summary>
        /// レベルを設定（ステータス再計算）
        /// </summary>
        public void SetLevel(int newLevel)
        {
            if (newLevel < 1) newLevel = 1;
            
            level = newLevel;
            RecalculateStats();
            OnLevelChanged?.Invoke(level);
        }

        /// <summary>
        /// HPを変更
        /// </summary>
        public void ModifyHealth(float amount)
        {
            float oldHealth = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            
            if (Mathf.Abs(currentHealth - oldHealth) > 0.01f)
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                
                // 死亡判定
                if (currentHealth <= 0f && oldHealth > 0f)
                {
                    OnDeath?.Invoke();
                    Debug.Log("[PlayerStats] Player died!");
                }
            }
        }

        /// <summary>
        /// MPを変更
        /// </summary>
        public void ModifyMana(float amount)
        {
            float oldMana = currentMana;
            currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
            
            if (Mathf.Abs(currentMana - oldMana) > 0.01f)
            {
                OnManaChanged?.Invoke(currentMana, maxMana);
            }
        }

        /// <summary>
        /// スタミナを変更
        /// </summary>
        public void ModifyStamina(float amount)
        {
            float oldStamina = currentStamina;
            currentStamina = Mathf.Clamp(currentStamina + amount, 0f, maxStamina);
            
            if (Mathf.Abs(currentStamina - oldStamina) > 0.01f)
            {
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            }
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage)
        {
            // 防御力による軽減（簡易計算）
            float reducedDamage = Mathf.Max(1f, damage - defensePower * 0.5f);
            ModifyHealth(-reducedDamage);
            Debug.Log($"[PlayerStats] Took {reducedDamage:F1} damage (HP: {currentHealth:F0}/{maxHealth:F0})");
        }

        /// <summary>
        /// 回復
        /// </summary>
        public void Heal(float amount)
        {
            ModifyHealth(amount);
            Debug.Log($"[PlayerStats] Healed {amount:F1} HP (HP: {currentHealth:F0}/{maxHealth:F0})");
        }

        /// <summary>
        /// MP消費
        /// </summary>
        public bool ConsumeMana(float amount)
        {
            if (currentMana >= amount)
            {
                ModifyMana(-amount);
                return true;
            }
            return false;
        }

        /// <summary>
        /// スタミナ消費
        /// </summary>
        public bool ConsumeStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                ModifyStamina(-amount);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 全ステータスを最大値に回復
        /// </summary>
        public void RestoreAll()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
            currentStamina = maxStamina;
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            
            Debug.Log("[PlayerStats] All stats restored");
        }

        /// <summary>
        /// 生存しているか
        /// </summary>
        public bool IsAlive()
        {
            return currentHealth > 0f;
        }

        /// <summary>
        /// HP割合を取得（0.0 ~ 1.0）
        /// </summary>
        public float GetHealthPercent()
        {
            return maxHealth > 0f ? currentHealth / maxHealth : 0f;
        }

        /// <summary>
        /// MP割合を取得（0.0 ~ 1.0）
        /// </summary>
        public float GetManaPercent()
        {
            return maxMana > 0f ? currentMana / maxMana : 0f;
        }

        /// <summary>
        /// スタミナ割合を取得（0.0 ~ 1.0）
        /// </summary>
        public float GetStaminaPercent()
        {
            return maxStamina > 0f ? currentStamina / maxStamina : 0f;
        }

        /// <summary>
        /// シリアライズ用データに変換
        /// </summary>
        public PlayerStatsData ToData()
        {
            return new PlayerStatsData
            {
                level = level,
                experience = experience,
                currentHealth = currentHealth,
                currentMana = currentMana,
                currentStamina = currentStamina,
                maxHealth = maxHealth,
                maxMana = maxMana,
                maxStamina = maxStamina,
                strength = strength,
                vitality = vitality,
                intelligence = intelligence,
                dexterity = dexterity,
                luck = luck
            };
        }

        /// <summary>
        /// シリアライズデータから復元
        /// </summary>
        public void FromData(PlayerStatsData data)
        {
            level = data.level;
            experience = data.experience;
            currentHealth = data.currentHealth;
            currentMana = data.currentMana;
            currentStamina = data.currentStamina;
            maxHealth = data.maxHealth;
            maxMana = data.maxMana;
            maxStamina = data.maxStamina;
            strength = data.strength;
            vitality = data.vitality;
            intelligence = data.intelligence;
            dexterity = data.dexterity;
            luck = data.luck;

            RecalculateStats();
        }
    }

    /// <summary>
    /// シリアライズ用のステータスデータ
    /// </summary>
    [Serializable]
    public class PlayerStatsData
    {
        public int level;
        public int experience;
        public float currentHealth;
        public float currentMana;
        public float currentStamina;
        public float maxHealth;
        public float maxMana;
        public float maxStamina;
        public int strength;
        public int vitality;
        public int intelligence;
        public int dexterity;
        public int luck;
    }
}

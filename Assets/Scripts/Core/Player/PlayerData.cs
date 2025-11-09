// プレイヤーデータ: Cloud Saveで永続化されるプレイヤー情報
using System;
using UnityEngine;

namespace Project.Core.Player
{
    /// <summary>
    /// プレイヤーの基本情報を保持するデータクラス
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public string playerId;        // UGS Player ID
        public string playerName;      // プレイヤー名
        public int level;              // レベル
        public int experience;         // 経験値
        public Vector3 position;       // 位置
        public Quaternion rotation;    // 回転
        public string createdAt;       // 作成日時 (ISO 8601)
        public string lastLoginAt;     // 最終ログイン日時 (ISO 8601)

        /// <summary>
        /// デフォルトのプレイヤーデータを生成
        /// </summary>
        public static PlayerData CreateDefault(string playerId)
        {
            return new PlayerData
            {
                playerId = playerId,
                playerName = "NewPlayer",
                level = 1,
                experience = 0,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                createdAt = DateTime.UtcNow.ToString("o"),
                lastLoginAt = DateTime.UtcNow.ToString("o")
            };
        }

        /// <summary>
        /// 最終ログイン日時を更新
        /// </summary>
        public void UpdateLastLogin()
        {
            lastLoginAt = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// 経験値を追加してレベルアップ判定
        /// </summary>
        public void AddExperience(int amount)
        {
            experience += amount;
            
            // 簡易レベルアップ計算（100経験値ごとにレベルアップ）
            int requiredExp = level * 100;
            while (experience >= requiredExp)
            {
                experience -= requiredExp;
                level++;
                requiredExp = level * 100;
                Debug.Log($"[PlayerData] Level Up! New Level: {level}");
            }
        }

        /// <summary>
        /// 次のレベルまでに必要な経験値を取得
        /// </summary>
        public int GetRequiredExperience()
        {
            return level * 100;
        }

        /// <summary>
        /// 経験値の進捗率を取得（0.0 ~ 1.0）
        /// </summary>
        public float GetExperienceProgress()
        {
            int required = GetRequiredExperience();
            return required > 0 ? (float)experience / required : 0f;
        }
    }
}

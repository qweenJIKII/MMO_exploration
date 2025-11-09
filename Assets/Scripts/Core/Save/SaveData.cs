// SaveData: セーブデータ構造
using System;
using UnityEngine;

namespace Project.Core.Save
{
    /// <summary>
    /// プレイヤーのセーブデータ
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // バージョン管理
        public int version = 1;
        public string saveDate;

        // プレイヤー基本情報
        public string playerName;
        public int level;
        public int experience;

        // ステータス
        public float currentHealth;
        public float maxHealth;
        public float currentMana;
        public float maxMana;
        public float currentStamina;
        public float maxStamina;

        // 能力値
        public int strength;
        public int defense;
        public int magic;
        public int agility;
        public float criticalRate;

        // 位置情報
        public Vector3Data position;
        public Vector3Data rotation;
        public string currentScene;

        // 通貨
        public int gold;

        // インベントリ（簡易版）
        public InventoryData inventory;

        // クエスト進行状況
        public QuestProgressData[] questProgress;

        public SaveData()
        {
            version = 1;
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            position = new Vector3Data();
            rotation = new Vector3Data();
            inventory = new InventoryData();
        }
    }

    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data() { }

        public Vector3Data(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public class InventoryData
    {
        public int maxSlots = 20;
        public ItemData[] items;

        public InventoryData()
        {
            items = new ItemData[0];
        }
    }

    [Serializable]
    public class ItemData
    {
        public string itemId;
        public int quantity;
        public int slotIndex;
    }

    [Serializable]
    public class QuestProgressData
    {
        public string questId;
        public int progress;
        public bool isCompleted;
    }
}

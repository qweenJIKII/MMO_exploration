# Phase 3: Inventory/Items 実装計画

ドキュメント種別: Implementation Plan  
対象プロジェクト: MMO_exploration (Unity)  
版数: v1.0  
作成日: 2025-11-09  
前提: Phase 2完了（UI/Settings/Analytics）

---

## 概要

Phase 3では、MMOゲームの基幹システムであるインベントリ・アイテムシステムを実装します。Unity Gaming Services (Economy)と連携し、クライアント・サーバー間でのアイテム管理を実現します。

---

## 目標

### 主要目標
1. **インベントリシステム**: アイテムの保持・管理
2. **アイテムシステム**: アイテムデータ・使用ロジック
3. **Economy統合**: UGS Economyとの連携
4. **UI実装**: インベントリUI（UI Toolkit）
5. **永続化**: Cloud Saveとの統合

### 成功基準
- アイテムの取得・使用・削除が正常動作
- インベントリUIが直感的に操作可能
- UGS Economyと同期
- セーブ/ロード時にインベントリが保持される
- Analytics統合（アイテムイベント記録）

---

## アーキテクチャ設計

### システム構成

```
┌─────────────────────────────────────────┐
│         Inventory System                │
├─────────────────────────────────────────┤
│  InventoryManager (Singleton)           │
│  - アイテム管理                          │
│  - スロット管理                          │
│  - イベント発行                          │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│         Item System                     │
├─────────────────────────────────────────┤
│  ItemData (ScriptableObject)            │
│  - アイテム定義                          │
│  - 統計情報                              │
│  ItemInstance (Class)                   │
│  - インスタンスデータ                    │
│  - スタック数                            │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│         UGS Economy Integration         │
├─────────────────────────────────────────┤
│  EconomyManager                         │
│  - Economy API連携                      │
│  - 通貨管理                              │
│  - アイテム購入                          │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│         UI Layer (UI Toolkit)           │
├─────────────────────────────────────────┤
│  InventoryUI                            │
│  - グリッド表示                          │
│  - ドラッグ&ドロップ                     │
│  - アイテム詳細                          │
└─────────────────────────────────────────┘
```

---

## 実装項目

### 1. データ構造設計

#### 1.1 ItemData (ScriptableObject)
```csharp
[CreateAssetMenu(fileName = "Item_", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;
    public string itemName;
    public string description;
    public Sprite icon;
    
    [Header("Properties")]
    public ItemType itemType;
    public ItemRarity rarity;
    public int maxStackSize;
    public bool isConsumable;
    public bool isTradeable;
    
    [Header("Stats")]
    public int value; // 売却価格
    public ItemEffect[] effects;
}
```

**優先度**: 高  
**工数**: 0.5日

#### 1.2 ItemInstance (Class)
```csharp
[System.Serializable]
public class ItemInstance
{
    public string instanceId;
    public ItemData itemData;
    public int stackSize;
    public Dictionary<string, object> customData;
    
    // 装備品用
    public int durability;
    public List<ItemModifier> modifiers;
}
```

**優先度**: 高  
**工数**: 0.5日

#### 1.3 Enums
```csharp
public enum ItemType
{
    Consumable,  // 消耗品
    Equipment,   // 装備
    Material,    // 素材
    QuestItem,   // クエストアイテム
    Currency     // 通貨
}

public enum ItemRarity
{
    Common,      // コモン
    Uncommon,    // アンコモン
    Rare,        // レア
    Epic,        // エピック
    Legendary    // レジェンダリー
}
```

**優先度**: 高  
**工数**: 0.5日

---

### 2. InventoryManager実装

#### 2.1 コア機能
```csharp
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private int maxSlots = 50;
    
    // インベントリデータ
    private List<ItemInstance> items = new List<ItemInstance>();
    
    // イベント
    public event Action<ItemInstance> OnItemAdded;
    public event Action<ItemInstance> OnItemRemoved;
    public event Action<ItemInstance> OnItemUsed;
    public event Action OnInventoryChanged;
    
    // API
    public bool AddItem(ItemData itemData, int quantity = 1);
    public bool RemoveItem(string instanceId, int quantity = 1);
    public bool UseItem(string instanceId);
    public ItemInstance GetItem(string instanceId);
    public List<ItemInstance> GetAllItems();
    public int GetItemCount(string itemId);
    public bool HasSpace(int requiredSlots = 1);
}
```

**優先度**: 高  
**工数**: 2日

#### 2.2 スタック管理
- 同一アイテムの自動スタック
- maxStackSizeの制限
- スタック分割機能

**優先度**: 中  
**工数**: 1日

#### 2.3 ソート・フィルター
- タイプ別ソート
- レアリティ別ソート
- 名前検索

**優先度**: 低  
**工数**: 1日

---

### 3. UGS Economy統合

#### 3.1 EconomyManager
```csharp
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }
    
    // 通貨管理
    public async Task<int> GetCurrency(string currencyId);
    public async Task<bool> AddCurrency(string currencyId, int amount);
    public async Task<bool> SpendCurrency(string currencyId, int amount);
    
    // アイテム購入
    public async Task<bool> PurchaseItem(string itemId, int quantity);
    
    // インベントリ同期
    public async Task SyncInventory();
    public async Task<List<InventoryItemDefinition>> GetInventoryItems();
}
```

**優先度**: 高  
**工数**: 2日

#### 3.2 Economy Dashboard設定
- 通貨定義（Gold, Gems）
- アイテムカタログ作成
- 価格設定

**優先度**: 高  
**工数**: 1日

---

### 4. UI実装 (UI Toolkit)

#### 4.1 InventoryUI
```
InventoryPanel (UXML)
├── Header
│   ├── Title
│   ├── CloseButton
│   └── SortButton
├── GridContainer
│   └── ItemSlot (x50)
│       ├── Icon
│       ├── StackCount
│       └── RarityBorder
└── DetailPanel
    ├── ItemIcon
    ├── ItemName
    ├── ItemDescription
    ├── ItemStats
    └── ActionButtons
        ├── UseButton
        ├── DropButton
        └── SplitButton
```

**優先度**: 高  
**工数**: 3日

#### 4.2 ドラッグ&ドロップ
- アイテムスロット間の移動
- アイテム破棄（ドロップ）
- スタック分割

**優先度**: 中  
**工数**: 2日

#### 4.3 USS スタイリング
```css
/* FantasyTheme.uss 拡張 */
.inventory-panel {
    background-color: rgba(15, 15, 20, 0.95);
    border-color: rgba(255, 215, 0, 0.8);
    border-width: 3px;
    border-radius: 16px;
}

.item-slot {
    width: 64px;
    height: 64px;
    background-color: rgba(30, 30, 40, 0.9);
    border-width: 2px;
}

.item-slot-common { border-color: rgb(150, 150, 150); }
.item-slot-uncommon { border-color: rgb(30, 255, 0); }
.item-slot-rare { border-color: rgb(0, 112, 221); }
.item-slot-epic { border-color: rgb(163, 53, 238); }
.item-slot-legendary { border-color: rgb(255, 128, 0); }
```

**優先度**: 中  
**工数**: 1日

---

### 5. アイテム使用システム

#### 5.1 ItemEffect (ScriptableObject)
```csharp
public abstract class ItemEffect : ScriptableObject
{
    public abstract void Apply(PlayerController player);
}

// 例: HealEffect
public class HealEffect : ItemEffect
{
    public int healAmount;
    
    public override void Apply(PlayerController player)
    {
        player.Heal(healAmount);
    }
}
```

**優先度**: 中  
**工数**: 2日

#### 5.2 消耗品システム
- 体力回復ポーション
- マナ回復ポーション
- バフアイテム

**優先度**: 中  
**工数**: 1日

---

### 6. 永続化

#### 6.1 SaveManager統合
```csharp
// InventoryData追加
[System.Serializable]
public class InventoryData
{
    public List<ItemInstanceData> items;
    public Dictionary<string, int> currencies;
}

// SaveManager拡張
public async Task SaveInventory()
{
    var inventoryData = InventoryManager.Instance.GetSaveData();
    await SaveToCloudSave("inventory", inventoryData);
}

public async Task LoadInventory()
{
    var inventoryData = await LoadFromCloudSave<InventoryData>("inventory");
    InventoryManager.Instance.LoadData(inventoryData);
}
```

**優先度**: 高  
**工数**: 1日

---

### 7. Analytics統合

#### 7.1 アイテムイベント記録
```csharp
// AnalyticsManager拡張（既存メソッド活用）
AnalyticsManager.Instance.RecordItemAcquired(itemId, itemName, quantity, source);
AnalyticsManager.Instance.RecordItemUsed(itemId, itemName, quantity);
```

**優先度**: 中  
**工数**: 0.5日

---

### 8. テスト実装

#### 8.1 単体テスト
```csharp
public class InventoryManagerTests
{
    [Test]
    public void AddItem_AddsItemToInventory()
    [Test]
    public void RemoveItem_RemovesItemFromInventory()
    [Test]
    public void UseItem_ConsumesItem()
    [Test]
    public void StackItems_CombinesIdenticalItems()
}
```

**優先度**: 高  
**工数**: 1日

#### 8.2 統合テスト
- Economy同期テスト
- Save/Loadテスト
- UI操作テスト

**優先度**: 中  
**工数**: 1日

---

## 実装スケジュール

### Week 1: データ構造・コアシステム
- **Day 1-2**: ItemData, ItemInstance, Enums実装
- **Day 3-5**: InventoryManager実装
- **Day 6-7**: EconomyManager実装

### Week 2: UI・機能拡張
- **Day 8-10**: InventoryUI実装（UI Toolkit）
- **Day 11-12**: ドラッグ&ドロップ実装
- **Day 13-14**: アイテム使用システム実装

### Week 3: 統合・テスト
- **Day 15**: SaveManager統合
- **Day 16**: Analytics統合
- **Day 17-18**: 単体テスト・統合テスト
- **Day 19-20**: バグ修正・最適化
- **Day 21**: ドキュメント作成・Phase 3完了

**総工数**: 21日（3週間）

---

## Phase 3 Exit条件

### 必須項目
- [ ] アイテムの追加・削除・使用が正常動作
- [ ] インベントリUIが完全動作（開閉・表示・操作）
- [ ] ドラッグ&ドロップでアイテム移動可能
- [ ] UGS Economyと同期
- [ ] Cloud Saveでインベントリ永続化
- [ ] Analytics記録（アイテム取得・使用）
- [ ] 単体テスト・統合テスト完了

### オプション項目
- [ ] アイテムソート・フィルター
- [ ] スタック分割機能
- [ ] アイテムツールチップ
- [ ] アイテムドロップアニメーション

---

## 技術スタック

### Unity Packages
- **Unity Gaming Services**
  - Economy (com.unity.services.economy)
  - Cloud Save (com.unity.services.cloudsave)
  - Analytics (com.unity.services.analytics)
- **UI Toolkit** (com.unity.ui)
- **TextMeshPro** (com.unity.textmeshpro)

### 新規追加予定
- なし（既存パッケージで実装可能）

---

## リスク管理

### 技術リスク

#### 1. UGS Economy学習コスト
**リスク**: Economy APIの理解に時間がかかる  
**対策**: 
- 公式ドキュメント精読
- サンプルコード参照
- 段階的実装（まずローカル、次にEconomy統合）

#### 2. UI Toolkitのドラッグ&ドロップ
**リスク**: UI Toolkitでのドラッグ&ドロップ実装が複雑  
**対策**:
- Unity公式サンプル参照
- カスタムManipulator実装
- 必要に応じてuGUIへの切り替え検討

#### 3. パフォーマンス
**リスク**: 大量アイテム表示時のパフォーマンス低下  
**対策**:
- 仮想スクロール実装
- アイテムプーリング
- 非同期ロード

---

## 参考資料

### Unity公式
- [Unity Gaming Services - Economy](https://docs.unity.com/economy/)
- [UI Toolkit Manual](https://docs.unity3d.com/Manual/UIElements.html)
- [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html)

### 実装例
- [Unity Inventory System Tutorial](https://learn.unity.com/)
- [UI Toolkit Drag and Drop](https://docs.unity3d.com/Manual/UIE-create-drag-and-drop-ui.html)

---

## 次のフェーズ予告

### Phase 4: Combat System（戦闘システム）
- スキルシステム
- ダメージ計算
- エフェクト・アニメーション
- 敵AI

### Phase 5: Quest System（クエストシステム）
- クエスト管理
- 進行状況トラッキング
- 報酬システム
- ダイアログシステム

---

## まとめ

Phase 3では、MMOゲームの基幹となるインベントリ・アイテムシステムを構築します。UGS Economyとの統合により、サーバー側でのアイテム管理を実現し、Phase 2で構築したAnalyticsと連携してプレイヤー行動を記録します。

**重点項目**:
1. 堅牢なデータ構造設計
2. UGS Economy完全統合
3. 直感的なUI/UX
4. 包括的なテスト

**成功の鍵**:
- 段階的実装（ローカル → Economy統合）
- 早期のUI実装とフィードバック
- 継続的なテストとリファクタリング

---

**Phase 3実装開始準備完了！** 🚀

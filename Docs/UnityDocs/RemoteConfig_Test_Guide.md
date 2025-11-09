# Remote Config統合テストガイド

## 概要
RemoteConfigManagerの動作確認とテスト方法を説明します。

---

## 前提条件

### 1. Unity Gaming Services (UGS) の設定
1. **Unity Dashboard** (https://dashboard.unity3d.com/) にアクセス
2. プロジェクトを選択
3. **Remote Config** サービスを有効化
4. **Project ID** をUnityエディタに設定
   - `Edit → Project Settings → Services` で確認

### 2. 必要なパッケージ
- ✅ `com.unity.remote-config` (4.2.2) - インストール済み
- ✅ `com.unity.services.core` - インストール済み
- ✅ `com.unity.services.authentication` - インストール済み

---

## テスト方法

### 🎯 方法1: エディタメニューからデバッグUI作成（推奨）

#### ステップ1: デバッグUIの作成
1. Unityエディタで任意のシーンを開く
2. メニューから **`MMO → Debug → Create Remote Config Debug UI`** を選択
3. Canvas上に自動的にデバッグUIが作成される

#### ステップ2: RemoteConfigManagerの配置
1. Hierarchy上で右クリック → `Create Empty`
2. 名前を `RemoteConfigManager` に変更
3. `RemoteConfigManager.cs` コンポーネントをアタッチ
4. Inspector設定:
   - ✅ **Auto Fetch On Start**: true（起動時に自動取得）
   - **Auto Fetch Interval**: 300（5分ごとに自動更新）

#### ステップ3: 実行とテスト
1. **Play** ボタンを押してゲームを開始
2. **F8キー** を押してデバッグパネルを表示
3. 以下の情報が表示されることを確認:
   - Experience Multiplier（経験値倍率）
   - Drop Rate Multiplier（ドロップ率倍率）
   - Max Level（最大レベル）
   - Daily Reward Gold（デイリー報酬ゴールド）
   - Event Status（イベント状態）

#### ステップ4: 動作確認
- **Refresh ボタン**: 設定を手動で再取得
- **F8キー**: パネルの開閉
- **Close ボタン**: パネルを閉じる

---

### 🎯 方法2: コンテキストメニューからテスト

#### RemoteConfigManagerのテスト機能
1. Hierarchy上で `RemoteConfigManager` を選択
2. Inspectorで右クリック → コンテキストメニューを表示
3. 以下のテスト機能を実行:

##### **Test: Fetch Config**
- Remote Configをサーバーから取得
- Consoleに取得結果が表示される

##### **Test: Show Current Config**
- 現在の設定値をConsoleに出力
```
=== Current Remote Config ===
Experience Multiplier: 1.00
Drop Rate Multiplier: 1.00
Event Enabled: False
Event Message: No active event
Max Level: 100
Daily Reward Gold: 100
==============================
```

##### **Test: Apply Default Config**
- デフォルト設定値を適用
- サーバー接続なしでテスト可能

---

### 🎯 方法3: スクリプトから利用

#### 経験値倍率の取得例
```csharp
using Project.Core.Services;

public class ExperienceManager : MonoBehaviour
{
    private void Start()
    {
        // RemoteConfigManagerのインスタンスを取得
        var configManager = RemoteConfigManager.Instance;
        
        if (configManager != null)
        {
            // 経験値倍率を取得
            float expMultiplier = configManager.ExperienceMultiplier;
            Debug.Log($"経験値倍率: {expMultiplier}x");
            
            // イベント状態を確認
            if (configManager.EventEnabled)
            {
                Debug.Log($"イベント実施中: {configManager.EventMessage}");
            }
        }
    }
}
```

#### イベント購読例
```csharp
private void OnEnable()
{
    var configManager = RemoteConfigManager.Instance;
    if (configManager != null)
    {
        // 設定取得成功時のイベント
        configManager.OnConfigFetched += OnConfigUpdated;
        
        // エラー時のイベント
        configManager.OnConfigError += OnConfigError;
    }
}

private void OnDisable()
{
    var configManager = RemoteConfigManager.Instance;
    if (configManager != null)
    {
        configManager.OnConfigFetched -= OnConfigUpdated;
        configManager.OnConfigError -= OnConfigError;
    }
}

private void OnConfigUpdated()
{
    Debug.Log("Remote Config更新完了");
    // 設定値を反映
}

private void OnConfigError(string error)
{
    Debug.LogWarning($"Remote Config取得失敗: {error}");
}
```

---

## Unity Dashboardでの設定

### Remote Config設定値の追加

1. **Unity Dashboard** → **Remote Config** に移動
2. **Create Config** をクリック
3. 以下のキーと値を設定:

| Key | Type | Default Value | 説明 |
|-----|------|---------------|------|
| `experienceMultiplier` | Float | 1.0 | 経験値倍率 |
| `dropRateMultiplier` | Float | 1.0 | ドロップ率倍率 |
| `eventEnabled` | Bool | false | イベント有効化 |
| `eventMessage` | String | "No active event" | イベントメッセージ |
| `maxLevel` | Int | 100 | 最大レベル |
| `dailyRewardGold` | Int | 100 | デイリー報酬ゴールド |

4. **Save** をクリック
5. **Publish** をクリックして設定を公開

### イベント実施例
経験値2倍イベントを実施する場合:
1. `experienceMultiplier` を `2.0` に変更
2. `eventEnabled` を `true` に変更
3. `eventMessage` を `"経験値2倍イベント開催中！"` に変更
4. **Publish** をクリック
5. ゲーム内で **Refresh** ボタンを押すか、5分待つと自動更新

---

## トラブルシューティング

### ❌ "Unity Services未初期化" エラー
**原因**: Unity Gaming Servicesが初期化されていない

**解決策**:
1. `BootstrapManager.cs` で `UnityServices.InitializeAsync()` が呼ばれているか確認
2. Project IDが正しく設定されているか確認
3. インターネット接続を確認

### ❌ "Remote Config取得失敗" エラー
**原因**: ネットワークエラーまたはサービス未設定

**解決策**:
1. Unity Dashboardで Remote Config が有効化されているか確認
2. 設定値が公開（Publish）されているか確認
3. デフォルト値で動作するため、エラーでもゲームは継続可能

### ❌ デバッグUIが表示されない
**原因**: F8キーが反応しない

**解決策**:
1. Input Systemパッケージがインストールされているか確認
2. Consoleで `[RemoteConfigDebugUI] 初期化完了` が表示されているか確認
3. コンテキストメニューから `Test: Toggle Panel` を実行

---

## 期待される動作

### ✅ 正常動作時
1. ゲーム起動時に自動的にRemote Configを取得
2. Console に `[RemoteConfigManager] Remote Config取得成功` と表示
3. F8キーでデバッグパネルが開閉
4. 設定値が正しく表示される
5. 5分ごとに自動更新

### ⚠️ オフライン時
1. `[RemoteConfigManager] Unity Services未初期化 - デフォルト値を使用` と表示
2. デフォルト値で動作継続
3. エラーでもゲームプレイに影響なし

---

## 次のステップ

### Phase 2完了後
- [ ] 環境別設定（Dev/Staging/Production）の実装
- [ ] A/Bテスト用の UserAttributes/AppAttributes 実装
- [ ] Analytics連携でRemote Config効果測定

### Phase 3以降
- [ ] 動的イベント管理システム
- [ ] サーバー側からのプッシュ通知連携
- [ ] 設定変更の即時反映（WebSocket経由）

---

## 参考リンク

- [Unity Remote Config Documentation](https://docs.unity.com/remote-config/)
- [Unity Gaming Services Dashboard](https://dashboard.unity3d.com/)
- [Remote Config Best Practices](https://docs.unity.com/remote-config/BestPractices.html)

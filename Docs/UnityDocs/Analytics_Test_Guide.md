# Unity Analytics 6.1.1 自動テストガイド

ドキュメント種別: Test Guide  
対象プロジェクト: MMO_exploration (Unity)  
版数: v1.0  
作成日: 2025-11-09

---

## 概要

Unity Analytics 6.1.1の自動テストシステムの使用方法を説明します。

### テストの種類

1. **AnalyticsAutoTest** - PlayMode自動テスト（実行時テスト）
2. **AnalyticsManagerTests** - Unity Test Framework（単体テスト）

---

## 1. AnalyticsAutoTest（PlayMode自動テスト）

### セットアップ

1. **テストシーンを作成**
   ```
   File > New Scene
   Scene名: AnalyticsTestScene
   保存先: Assets/Scenes/
   ```

2. **必要なGameObjectを配置**
   ```
   - AnalyticsManager (AnalyticsManager.cs)
   - AnalyticsAutoTest (AnalyticsAutoTest.cs)
   ```

3. **AnalyticsAutoTestの設定**
   ```
   Inspector設定:
   - Run On Start: ✅ チェック
   - Delay Between Tests: 1.0秒
   - Verbose Logging: ✅ チェック
   
   Test Selection:
   - Test Session Events: ✅
   - Test Player Events: ✅
   - Test Quest Events: ✅
   - Test Economy Events: ✅
   - Test UI Events: ✅
   - Test Standard Events: ✅
   ```

### 実行方法

#### 方法1: シーン起動時に自動実行
```
1. AnalyticsTestSceneを開く
2. Playボタンをクリック
3. Consoleでテスト結果を確認
```

#### 方法2: Context Menuから手動実行
```
1. AnalyticsAutoTestを右クリック
2. 実行したいテストを選択:
   - Run All Tests
   - Run Session Tests
   - Run Player Tests
   - Run Quest Tests
   - Run Economy Tests
   - Run UI Tests
   - Run Standard Events Tests
```

### 出力例

```
=== Analytics自動テスト開始 ===

【セッションイベントテスト】
✓ セッション開始イベント: 自動送信済み

【プレイヤーイベントテスト】
✓ レベルアップイベント: Level 5, XP 1000
✓ プレイヤー死亡イベント: enemy_attack, Level 5
✓ アイテム取得イベント: 鉄の剣 x1
✓ アイテム使用イベント: 体力ポーション x1

【クエストイベントテスト】
✓ クエスト開始イベント: 初めてのクエスト
✓ クエスト完了イベント: 120.5秒で完了

【経済イベントテスト】
✓ 通貨取得イベント: Gold +100
✓ 通貨消費イベント: Gold -50

【UIイベントテスト】
✓ 画面遷移イベント: TitleScreen → MainMenu
✓ ボタンクリックイベント: StartButton

【標準イベントテスト (Analytics 6.1.1)】
✓ トランザクションイベント: $9.99 USD
✓ プレイヤープログレスイベント: Tutorial完了

==================================================
テスト結果サマリー
==================================================
総テスト数: 13
成功: 13
失敗: 0
成功率: 100.0%
✓ すべてのテストが成功しました！
==================================================

Analytics統計:
- 初期化状態: 完了
- 有効状態: 有効
- セッション送信イベント数: 13
- デバッグモード: ON
```

---

## 2. AnalyticsManagerTests（Unity Test Framework）

### セットアップ

1. **Test Runnerを開く**
   ```
   Window > General > Test Runner
   ```

2. **テストアセンブリの確認**
   ```
   Project.Tests.asmdef が存在することを確認
   参照:
   - Project.Core
   - Unity.Services.Analytics
   - UnityEngine.TestRunner
   - NUnit
   ```

### 実行方法

#### EditModeテスト
```
1. Test Runnerウィンドウを開く
2. EditModeタブを選択
3. "Run All" をクリック
```

#### PlayModeテスト
```
1. Test Runnerウィンドウを開く
2. PlayModeタブを選択
3. "Run All" をクリック
```

### テスト項目

#### 基本機能テスト
- ✅ Singletonインスタンスの存在確認
- ✅ GetAnalyticsStats()の動作確認

#### プレイヤーイベントテスト
- ✅ RecordLevelUp()
- ✅ RecordPlayerDeath()
- ✅ RecordItemAcquired()
- ✅ RecordItemUsed()

#### クエストイベントテスト
- ✅ RecordQuestStart()
- ✅ RecordQuestComplete()

#### 経済イベントテスト
- ✅ RecordCurrencyGained()
- ✅ RecordCurrencySpent()

#### UIイベントテスト
- ✅ RecordScreenView()
- ✅ RecordButtonClick()

#### 標準イベントテスト (Analytics 6.1.1)
- ✅ RecordTransaction()
- ✅ RecordPlayerProgress()

#### カスタムイベントテスト
- ✅ SendCustomEvent() (パラメータなし)
- ✅ SendCustomEvent() (パラメータあり)
- ✅ FlushEvents()

#### PlayModeテスト
- ✅ 初期化の確認
- ✅ 複数イベント連続送信

---

## 3. CI/CD統合

### GitHub Actionsでの自動テスト

```yaml
# .github/workflows/unity-test.yml
name: Unity Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - uses: game-ci/unity-test-runner@v2
        with:
          unityVersion: 6.2.0f1
          testMode: all
          
      - uses: actions/upload-artifact@v2
        with:
          name: Test results
          path: artifacts
```

---

## 4. トラブルシューティング

### テストが失敗する場合

#### Unity Servicesが初期化されていない
**症状**: "Unity Servicesが初期化されていません" エラー

**解決策**:
```csharp
// ProjectSetup.csなどで初期化
await UnityServices.InitializeAsync();
```

#### Assembly Definition参照エラー
**症状**: コンパイルエラー

**解決策**:
```
Project.Tests.asmdef に以下を追加:
- Unity.Services.Analytics
- Project.Core
```

#### テストがスキップされる
**症状**: テストが実行されない

**解決策**:
```
1. Test Runnerで "Enable playmode tests for all assemblies" を確認
2. Project.Tests.asmdef の "Auto Referenced" をチェック
```

---

## 5. ベストプラクティス

### テスト実行のタイミング

1. **開発中**: 機能追加・変更時に毎回実行
2. **コミット前**: すべてのテストをパス確認
3. **CI/CD**: プルリクエスト時に自動実行

### テストカバレッジ

- ✅ すべてのpublicメソッドをテスト
- ✅ 正常系と異常系の両方をテスト
- ✅ エッジケースもカバー

### テストの保守

- 📝 テストコードもレビュー対象
- 📝 失敗したテストは必ず修正
- 📝 新機能追加時はテストも追加

---

## 6. 次のステップ

### Phase 3での拡張

- インベントリイベントのテスト追加
- アイテムクラフトイベントのテスト追加
- パフォーマンステストの追加

### 統合テスト

- SaveManager + AnalyticsManager
- RemoteConfig + AnalyticsManager
- NotificationManager + AnalyticsManager

---

## まとめ

### テスト実行チェックリスト

- [ ] AnalyticsAutoTestでPlayModeテスト実行
- [ ] Test RunnerでEditModeテスト実行
- [ ] Test RunnerでPlayModeテスト実行
- [ ] すべてのテストがパス
- [ ] Unity Dashboardでイベント確認（数時間後）

### 成功基準

- ✅ すべてのテストが成功
- ✅ エラーログなし
- ✅ 成功率100%
- ✅ Unity Dashboardにイベントが記録される

---

**自動テストシステムの構築完了！** 🎉

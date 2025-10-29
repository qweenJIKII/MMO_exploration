# Phase 0: ビルドテストガイド

**作成日**: 2025-10-28  
**目的**: Phase 0の実装が正しく動作することを確認

---

## 事前準備

### 1. Unity Editorでの確認

1. Unity Editor（6000.2.0f2）でプロジェクトを開く
2. Console ウィンドウを開く（Window > General > Console）
3. Warnings/Errorsが0件であることを確認

### 2. Assembly Definition の確認

以下のファイルが正しく認識されていることを確認：
- `Assets/Scripts/Core/Project.Core.asmdef`
- `Assets/Scripts/Dev/Project.Dev.asmdef`
- `Assets/Scripts/Bootstrap/Project.Bootstrap.asmdef`

**確認方法**: Project ウィンドウで各.asmdefファイルを選択し、Inspectorで設定を確認

---

## テストシーンの作成

### 1. 新規シーン作成

1. `File > New Scene` で新規シーンを作成
2. `Assets/Scenes/Phase0_Test.unity` として保存

### 2. SceneBootstrap の配置

1. Hierarchy で右クリック > `Create Empty`
2. 名前を `SceneBootstrap` に変更
3. `SceneBootstrap` コンポーネントをアタッチ
   - `Assets/Scripts/Bootstrap/SceneBootstrap.cs`
4. Inspector で以下を確認：
   - Enable Dev Console: ✓
   - Enable Profiling: ✓
   - Enable Metrics Sampling: ✓

### 3. テスト用オブジェクトの配置（オプション）

1. Hierarchy で右クリック > `3D Object > Cube`
2. Position を (0, 0, 0) に設定
3. これでデバッグ描画のテストが可能

---

## 動作確認

### 1. Play Mode での確認

1. Play ボタンを押してゲームを実行
2. Console に以下のログが表示されることを確認：
   ```
   [Bootstrap] Scene 'Phase0_Test' initializing...
   [Bootstrap] DevConsole initialized.
   [Bootstrap] ProfilingManager initialized.
   [Bootstrap] MetricsSampler initialized.
   [Bootstrap] DebugDrawManager initialized.
   [Bootstrap] GameStateManager initialized.
   [Bootstrap] Scene initialization completed.
   [GameStateManager] State changed: Initializing -> Title
   ```

### 2. DevConsole のテスト

1. Play Mode 中に **`F1`キー** を押す
2. DevConsole が表示されることを確認
3. 以下のコマンドを実行してテスト：
   ```
   help
   fps
   time.scale 0.5
   time.scale 1.0
   ugs.auth
   ```
4. 各コマンドが正しく動作することを確認

### 3. ProfilingManager のテスト

1. Play Mode 中に **`F2`キー** を押す
2. Profiling Stats ウィンドウが表示されることを確認
3. Update/FixedUpdate/LateUpdate 等の統計情報が表示されることを確認

### 4. DebugDrawManager のテスト

テスト用スクリプトを作成：

```csharp
using UnityEngine;
using Project.Dev;

public class DebugDrawTest : MonoBehaviour
{
    private void Update()
    {
        // 赤いラインを描画
        DebugDrawManager.DrawLine(Vector3.zero, Vector3.up * 2, Color.red, 0.1f);
        
        // 緑のワイヤーキューブを描画
        DebugDrawManager.DrawWireCube(Vector3.right * 2, Vector3.one, Color.green, 0.1f);
        
        // 青のワイヤースフィアを描画
        DebugDrawManager.DrawWireSphere(Vector3.left * 2, 0.5f, Color.blue, 0.1f);
    }
}
```

1. 上記スクリプトを `Assets/Scripts/Dev/DebugDrawTest.cs` として保存
2. 任意のGameObjectにアタッチ
3. Play Mode で Scene ビューを確認
4. 描画が表示されることを確認

### 5. LoggerService のテスト

テスト用スクリプトを作成：

```csharp
using UnityEngine;
using Project.Core.Observability;

public class LoggerTest : MonoBehaviour
{
    private void Start()
    {
        LoggerService.Debug("Test", "Debug message");
        LoggerService.Info("Test", "Info message");
        LoggerService.Warning("Test", "Warning message");
        LoggerService.Error("Test", "Error message");
        
        LoggerService.LogCustomEvent("TestEvent", new { value = 123, name = "test" });
    }
}
```

1. 上記スクリプトを `Assets/Scripts/Dev/LoggerTest.cs` として保存
2. 任意のGameObjectにアタッチ
3. Play Mode で実行
4. `Application.persistentDataPath/Logs/events_YYYYMMDD.jsonl` にログが出力されることを確認

**ログファイルの場所**:
- Windows: `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\Logs\`
- Mac: `~/Library/Application Support/<CompanyName>/<ProductName>/Logs/`

### 6. MetricsSampler のテスト

1. Play Mode で1分以上待機
2. `Application.persistentDataPath/Logs/metrics.csv` が作成されることを確認
3. ファイルを開き、FPS・メモリ使用量が記録されていることを確認

---

## ビルドテスト

### 1. ビルド設定

1. `File > Build Settings` を開く
2. `Add Open Scenes` で `Phase0_Test` シーンを追加
3. Platform を `Windows` に設定
4. `Player Settings` を開く
5. `Company Name` と `Product Name` を設定

### 2. ビルド実行

1. `Build` ボタンをクリック
2. ビルド先フォルダを選択
3. ビルドが成功することを確認
4. Warnings/Errors が 0 件であることを確認

### 3. ビルド後の動作確認

1. ビルドされた実行ファイルを起動
2. **`F1`キー** で DevConsole が開くことを確認
3. **`F2`キー** で Profiling Stats が表示されることを確認
4. DevConsole で以下のコマンドをテスト：
   ```
   ugs.auth
   fps
   ```
5. ログファイルの確認：
   - `C:\Users\[ユーザー名]\AppData\LocalLow\DefaultCompany\MMO_exploration\Logs\`
   - `events_YYYYMMDD.jsonl` が作成されていることを確認
6. 正常に動作することを確認

---

## トラブルシューティング

### コンパイルエラーが発生する場合

1. `Assets > Reimport All` を実行
2. Unity Editor を再起動
3. `Library` フォルダを削除して再度開く

### UGS関連のエラーが発生する場合

1. `Project Settings > Services` で UGS が正しく設定されているか確認
2. `UgsInitializer.cs` が正しく動作しているか確認

### Assembly Definition が認識されない場合

1. .asmdef ファイルの JSON 構文が正しいか確認
2. `Assets > Reimport All` を実行
3. Unity Editor を再起動

---

## Exit条件の最終確認

- [ ] 空ビルド成功
- [ ] Warning 0件
- [ ] ログ・メトリクス出力確認
- [ ] DevConsole動作確認
- [ ] Debug Draw動作確認
- [ ] Profiling計測可能確認

すべてのチェックが完了したら、Phase 0 は完了です。

次は **Phase 1: Player Core** に進んでください。

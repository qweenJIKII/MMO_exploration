# Phase 1: Player Core 実装計画

**作成日**: 2025-10-30  
**ステータス**: 🔄 準備中  
**推定期間**: 2週間  
**参照**: `Implementation_Roadmap.md`, `Phase0_Implementation_Status.md`

---

## 概要

Phase 0で確立した基盤の上に、プレイヤーの基本操作システムを実装します。

### 主要目標

1. ✅ Input Actions作成（New Input System完全移行）
2. ✅ プレイヤーデータ管理（Cloud Save連携）
3. ✅ キャラクター制御（移動・ジャンプ）
4. ✅ カメラシステム（Cinemachine）
5. ✅ 基本UI（HUD）

---

## 実装タスク

### 1. Input Actions作成 [2日]

#### 目標
New Input Systemに完全移行し、旧Input APIを削除

#### タスク詳細

**1.1 Input Actions Asset作成**
- ファイル: `Assets/Settings/InputActions.inputactions`
- Action Maps:
  - **Player**: プレイヤー操作
    - Move (Vector2): WASD, 左スティック
    - Jump (Button): Space, A button
    - Sprint (Button): Left Shift, B button
  - **UI**: UI操作
    - ToggleConsole (Button): F1
    - ToggleProfiling (Button): F2
    - Navigate (Vector2): Arrow keys, D-pad
    - Submit (Button): Enter, A button
    - Cancel (Button): Esc, B button
  - **Camera**: カメラ操作
    - Look (Vector2): Mouse Delta, 右スティック
    - Zoom (Axis): Mouse Scroll, Triggers

**1.2 Player Input Component設定**
- Behavior: Send Messages または Invoke Unity Events
- Default Map: Player
- UI Input Module連携

**1.3 DevConsole/ProfilingManager改修**
- Input Actionsを使用するように変更
- 旧Input APIフォールバックを削除
- InputActionReference使用

**1.4 Player Settings更新**
- Active Input Handling: **Input System Package (New)** に変更
- 旧Input Manager無効化

#### 成果物
- `Assets/Settings/InputActions.inputactions`
- `DevConsole.cs` 改修版
- `ProfilingManager.cs` 改修版

---

### 2. プレイヤーデータ管理 [1日]

#### 目標
プレイヤーの基本情報を管理し、Cloud Saveと連携

#### タスク詳細

**2.1 PlayerData.cs作成**
```csharp
// Assets/Scripts/Core/Player/PlayerData.cs
[System.Serializable]
public class PlayerData
{
    public string playerId;        // UGS Player ID
    public string playerName;      // プレイヤー名
    public int level;              // レベル
    public int experience;         // 経験値
    public Vector3 position;       // 位置
    public Quaternion rotation;    // 回転
    public string createdAt;       // 作成日時
    public string lastLoginAt;     // 最終ログイン日時
}
```

**2.2 PlayerDataManager.cs作成**
- シングルトンパターン
- Cloud Save連携（Save/Load）
- 自動保存機能（定期・シーン切替時）
- デフォルト値生成

**2.3 初回ログイン処理**
- プレイヤー名入力UI
- デフォルトデータ生成
- Cloud Saveへ保存

#### 成果物
- `Assets/Scripts/Core/Player/PlayerData.cs`
- `Assets/Scripts/Core/Player/PlayerDataManager.cs`
- `Assets/Scripts/Core/Player.asmdef`

---

### 3. キャラクター制御 [3日]

#### 目標
CharacterControllerを使用した移動・ジャンプ実装

#### タスク詳細

**3.1 キャラクターモデル準備**
- Capsule使用（仮モデル）
- 簡易リグ設定
- Animator Controller作成
  - Idle, Walk, Run, Jump アニメーション

**3.2 PlayerController.cs作成**
```csharp
// Assets/Scripts/Core/Player/PlayerController.cs
- CharacterController使用
- Input Actionsから入力取得
- 移動処理（WASD）
- ジャンプ処理（Space）
- ダッシュ処理（Shift）
- 重力・地面判定
- アニメーション制御
```

**3.3 移動パラメータ調整**
- 移動速度: 5.0 m/s
- ダッシュ速度: 8.0 m/s
- ジャンプ力: 5.0 m/s
- 重力: -9.81 m/s²

#### 成果物
- `Assets/Scripts/Core/Player/PlayerController.cs`
- `Assets/Prefabs/Player.prefab`
- `Assets/Animations/Player/PlayerAnimator.controller`

---

### 4. カメラシステム [2日]

#### 目標
Cinemachineを使用したプレイヤー追従カメラ

#### タスク詳細

**4.1 Cinemachine導入**
- Package Manager: Cinemachine インストール

**4.2 Virtual Camera設定**
- CM vcam1 作成
- Follow: Player Transform
- Look At: Player Transform + Offset
- Body: Framing Transposer
- Aim: Composer

**4.3 カメラ回転制御**
- マウス/右スティックで回転
- Input Actionsから入力取得
- 上下角度制限（-30° ~ 60°）
- 左右360°回転

**4.4 カメラズーム**
- マウスホイール/トリガーでズーム
- 距離: 3.0m ~ 10.0m

#### 成果物
- Cinemachine Virtual Camera設定
- `Assets/Scripts/Core/Player/CameraController.cs`

---

### 5. 基本UI（HUD） [2日]

#### 目標
プレイヤー情報を表示するHUD実装

#### タスク詳細

**5.1 Canvas設定**
- Screen Space - Overlay
- Canvas Scaler: Scale With Screen Size
- Reference Resolution: 1920x1080

**5.2 TextMeshPro導入**
- Package Manager: TextMeshPro インストール
- Essential Resources インポート

**5.3 HUD要素作成**
- プレイヤー名表示
- レベル表示
- 経験値バー
- HP/MP表示（Phase 5で実装）
- FPS表示（DevConsole連携）

**5.4 HUDManager.cs作成**
- PlayerDataManagerから情報取得
- UI更新処理
- 表示/非表示切替

#### 成果物
- `Assets/Prefabs/UI/HUD.prefab`
- `Assets/Scripts/Core/UI/HUDManager.cs`
- `Assets/Scripts/Core/UI.asmdef`

---

### 6. テスト・調整 [3日]

#### タスク詳細

**6.1 動作確認**
- 移動・ジャンプ動作
- カメラ追従・回転
- Input Actions動作
- Cloud Save連携
- HUD表示

**6.2 パフォーマンス最適化**
- ProfilingManagerで計測
- ボトルネック特定
- 最適化実施

**6.3 ドキュメント更新**
- Phase1_Implementation_Status.md作成
- Implementation_Roadmap.md更新

---

## Exit条件

### 機能確認
- [ ] Input Actions動作（Player/UI/Camera）
- [ ] プレイヤー移動・ジャンプ動作
- [ ] カメラ追従・回転動作
- [ ] Cloud Save連携（Save/Load）
- [ ] HUD表示（名前・レベル・経験値）

### 技術確認
- [ ] New Input System完全移行
- [ ] 旧Input API削除
- [ ] CharacterController正常動作
- [ ] Cinemachine正常動作
- [ ] TextMeshPro正常動作

### ビルドテスト
- [ ] Development Build成功
- [ ] ビルド実行成功
- [ ] 全機能動作確認

### ドキュメント
- [ ] Phase1_Implementation_Status.md作成
- [ ] Implementation_Roadmap.md更新

---

## 技術スタック

### 新規導入
- **Cinemachine**: カメラシステム
- **TextMeshPro**: テキスト表示

### 既存使用
- **New Input System**: 入力管理
- **CharacterController**: キャラクター制御
- **UGS Cloud Save**: データ永続化
- **URP**: レンダリング

---

## リスク管理

### 想定リスク

1. **Input System移行の複雑さ**
   - 対策: 段階的移行、フォールバック実装

2. **CharacterController制御の難しさ**
   - 対策: 既存アセット参考、段階的実装

3. **Cinemachine学習コスト**
   - 対策: 公式ドキュメント参照、シンプルな設定から開始

4. **Cloud Save遅延**
   - 対策: ローカルキャッシュ、非同期処理

---

## 次のステップ（Phase 2）

Phase 1完了後、以下に進みます：

1. UI/Settings実装
2. Remote Config統合
3. Analytics統合
4. セーブ/ロードUI

詳細は`Implementation_Roadmap.md`を参照してください。

---

**Phase 1実装を開始する準備が整いました。**

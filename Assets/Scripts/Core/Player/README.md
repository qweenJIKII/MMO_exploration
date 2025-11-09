# Player Core System

Phase 1で実装されたプレイヤーコアシステムのドキュメントです。

## 概要

プレイヤーの基本操作、データ管理、カメラ制御を提供します。

## コンポーネント

### PlayerData.cs
プレイヤーの基本情報を保持するデータクラス。

**プロパティ:**
- `playerId`: UGS Player ID
- `playerName`: プレイヤー名
- `level`: レベル
- `experience`: 経験値
- `position`: 位置
- `rotation`: 回転
- `createdAt`: 作成日時
- `lastLoginAt`: 最終ログイン日時

**メソッド:**
- `CreateDefault(playerId)`: デフォルトデータを生成
- `UpdateLastLogin()`: 最終ログイン日時を更新
- `AddExperience(amount)`: 経験値を追加してレベルアップ判定
- `GetRequiredExperience()`: 次のレベルまでに必要な経験値を取得
- `GetExperienceProgress()`: 経験値の進捗率を取得（0.0～1.0）

---

### PlayerDataManager.cs
プレイヤーデータの管理とCloud Save連携を行うシングルトン。

**機能:**
- Cloud Saveからのデータ読み込み
- Cloud Saveへのデータ保存
- 自動保存（60秒ごと）
- アプリケーション終了時の保存

**メソッド:**
- `LoadPlayerDataAsync()`: プレイヤーデータを読み込み
- `SavePlayerDataAsync()`: プレイヤーデータを保存
- `SetPlayerName(name)`: プレイヤー名を設定
- `AddExperience(amount)`: 経験値を追加
- `UpdatePosition(position, rotation)`: 位置と回転を更新
- `ForceSave()`: 手動保存をトリガー

**使用例:**
```csharp
// データ読み込み
var playerData = await PlayerDataManager.Instance.LoadPlayerDataAsync();

// 経験値追加
PlayerDataManager.Instance.AddExperience(50);

// 手動保存
PlayerDataManager.Instance.ForceSave();
```

---

### PlayerController.cs
CharacterControllerを使用したプレイヤーの移動・ジャンプ制御。

**設定項目:**
- **Movement Settings**
  - `moveSpeed`: 移動速度（デフォルト: 5.0 m/s）
  - `sprintSpeed`: ダッシュ速度（デフォルト: 8.0 m/s）
  - `rotationSpeed`: 回転速度（デフォルト: 10.0 rad/s）

- **Jump Settings**
  - `jumpForce`: ジャンプ力（デフォルト: 5.0 m/s）
  - `gravity`: 重力（デフォルト: -9.81 m/s²）

- **Ground Check**
  - `groundCheckDistance`: 地面判定距離（デフォルト: 0.2m）
  - `groundLayer`: 地面レイヤー

- **Input Actions**
  - `moveAction`: 移動アクション
  - `jumpAction`: ジャンプアクション
  - `sprintAction`: ダッシュアクション

**メソッド:**
- `Teleport(position, rotation)`: プレイヤーをテレポート
- `GetCurrentSpeed()`: 現在の移動速度を取得
- `IsGrounded()`: 地面にいるかどうかを取得

**Animatorパラメータ:**
- `Speed` (Float): 移動速度
- `IsGrounded` (Bool): 地面判定
- `Jump` (Trigger): ジャンプトリガー
- `IsSprinting` (Bool): ダッシュ判定

---

### CameraController.cs
Cinemachineと連携したカメラ制御。

**設定項目:**
- **Camera Settings**
  - `virtualCamera`: Cinemachine Virtual Camera
  - `cameraTarget`: カメラが追従するターゲット

- **Rotation Settings**
  - `mouseSensitivity`: マウス感度（デフォルト: 2.0）
  - `gamepadSensitivity`: ゲームパッド感度（デフォルト: 100.0）
  - `minVerticalAngle`: 最小垂直角度（デフォルト: -30°）
  - `maxVerticalAngle`: 最大垂直角度（デフォルト: 60°）

- **Zoom Settings**
  - `minDistance`: 最小距離（デフォルト: 3.0m）
  - `maxDistance`: 最大距離（デフォルト: 10.0m）
  - `zoomSpeed`: ズーム速度（デフォルト: 2.0）

- **Input Actions**
  - `lookAction`: カメラ回転アクション
  - `zoomAction`: カメラズームアクション

**メソッド:**
- `SetRotation(horizontal, vertical)`: カメラの角度を設定
- `SetDistance(distance)`: カメラの距離を設定
- `SetVirtualCamera(vcam)`: Virtual Cameraを設定

---

## セットアップ手順

### 1. Player Prefab作成

1. 空のGameObjectを作成（名前: Player）
2. 以下のコンポーネントを追加：
   - `CharacterController`
   - `PlayerController`
   - `CameraController`
3. Capsule Colliderを追加（仮モデル）
4. Animator Controllerを作成・設定

### 2. Cinemachine Virtual Camera設定

1. Cinemachine > Create Virtual Camera
2. Body: Framing Transposer
3. Aim: Composer
4. Follow/LookAt: Player/CameraTarget
5. CameraControllerの`virtualCamera`フィールドに設定

### 3. Input Actions設定

1. `Assets/Settings/InputActions.inputactions`を開く
2. "Generate C# Class"を有効化
3. 各コンポーネントのInputActionReferenceフィールドに対応するアクションを設定：
   - PlayerController: Move, Jump, Sprint
   - CameraController: Look, Zoom

### 4. PlayerDataManager設定

1. シーンに空のGameObjectを作成（名前: PlayerDataManager）
2. `PlayerDataManager`コンポーネントを追加
3. DontDestroyOnLoadで永続化される

### 5. 地面設定

1. 地面となるGameObjectにColliderを追加
2. 適切なLayerを設定
3. PlayerControllerの`groundLayer`に設定

---

## 使用例

### プレイヤーのスポーン

```csharp
// プレイヤーデータを読み込み
var playerData = await PlayerDataManager.Instance.LoadPlayerDataAsync();

// プレイヤーをスポーン
GameObject player = Instantiate(playerPrefab);
PlayerController controller = player.GetComponent<PlayerController>();

// 保存された位置にテレポート
if (playerData != null)
{
    controller.Teleport(playerData.position, playerData.rotation);
}
```

### 経験値の追加

```csharp
// 敵を倒したときなど
PlayerDataManager.Instance.AddExperience(100);
```

### カメラの設定

```csharp
CameraController cameraController = player.GetComponent<CameraController>();

// カメラ角度を設定
cameraController.SetRotation(0f, 30f);

// カメラ距離を設定
cameraController.SetDistance(5.0f);
```

---

## 依存関係

- **Unity.InputSystem**: 入力管理
- **Unity.Services.CloudSave**: データ永続化
- **Cinemachine**: カメラシステム
- **CharacterController**: キャラクター制御

---

## 注意事項

1. **Input System設定**
   - Player Settings > Active Input Handling を "Input System Package (New)" に設定してください

2. **Cloud Save認証**
   - PlayerDataManagerを使用する前に、UGS認証が完了している必要があります

3. **Cinemachine**
   - Virtual CameraのBodyは"Framing Transposer"を使用してください

4. **地面判定**
   - 地面となるオブジェクトには必ずColliderを設定してください

---

## トラブルシューティング

### プレイヤーが動かない
- Input Actionsが有効化されているか確認
- InputActionReferenceが正しく設定されているか確認
- CharacterControllerが追加されているか確認

### カメラが追従しない
- Virtual CameraのFollow/LookAtが設定されているか確認
- CameraControllerのvirtualCameraフィールドが設定されているか確認

### データが保存されない
- UGS認証が完了しているか確認
- Cloud Saveサービスが有効化されているか確認
- ネットワーク接続を確認

---

**Phase 1実装完了。詳細は`Phase1_Implementation_Status.md`を参照してください。**

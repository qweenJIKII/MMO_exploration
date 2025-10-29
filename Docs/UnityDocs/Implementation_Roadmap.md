# MMO_exploration 段階的実装ロードマップ

ドキュメント種別: Implementation Guide  
対象プロジェクト: MMO_exploration (Unity)  
版数: v1.1  
作成日: 2025-10-27  
最終更新: 2025-10-30  
参照: `Game_Creation_Plan.md`, `Gameplay_Feature_Status.md`, `Phase0_Implementation_Status.md`

---

## 0. 現状確認（2025-10-27時点）

### 実装済み
- ✅ **UGS統合基盤**: Authentication/Cloud Save/Cloud Code が完全動作
  - `BackendBootstrap` / `UgsBackendService` 実装済み
  - Cloud Code モジュール (`exchange_module`) デプロイ済み
  - 診断関数群動作確認済み
- ✅ **テスター**: `CloudCodeExchangeTester.cs` で各種API呼び出し可能
- ✅ **基本関数**: ExchangeV2, GrantCurrencyDebug, CheckSecrets 実装済み

### 技術スタック（確定）
- Unity: 6.2 (6000.2.0f2)
- Render Pipeline: URP
- Input System: New Input System
- Camera: Cinemachine
- Backend: UGS
  - **Phase 0〜**: Authentication, Cloud Save, Cloud Code, Economy
  - **Phase 2〜**: Remote Config, Analytics
  - **Phase 4〜**: Leaderboards
  - **Phase 5〜**: Cloud Code Scheduled Jobs

---

## 1. 実装フェーズ概要

| Phase | マイルストーン | 期間 | 主要成果物 |
|-------|--------------|------|-----------|
| 0 | 基盤整備完了 | 1週 | ログ/メトリクス/DevConsole |
| 1 | Player Core | 2週 | 移動/カメラ/Input |
| 2 | UI/Settings | 2週 | HUD/Menu/Save |
| 3 | World/Minimap | 2週 | Loading/Minimap |
| 4 | Economy/Social | 3週 | Inventory/Quest/Gacha/Chat/Mail |
| 4.5 | Web Admin Dashboard | 2週 | 管理ツール/監視ダッシュボード |
| 5 | Combat/Ability | 3週 | 戦闘/VFX/Audio |
| 6 | Trade/Market/Guild | 3週 | トレード/マーケット/ギルド |
| 7 | Network | 4週+ | オンライン化 |

**合計**: 約22週間（約5.5ヶ月）

### 主要機能一覧（UGS統合）

#### Phase 2で実装
- **Remote Config**: 設定値の動的取得、A/Bテスト
- **Analytics**: プレイヤー行動分析、ファネル分析

#### Phase 4で実装
- **インベントリ/装備**: Cloud Save連携、スタック・フィルタ
- **クエスト**: Cloud Save連携、進行管理・報酬
- **ガチャ**: Cloud Code連携、確率抽選・演出
- **チャット**: チャンネル管理、ミュート
- **メール**: Cloud Code + Cloud Save、添付・受領
- **パーティ**: 招待・離脱・リーダー移譲
- **採集**: リスポーン・レア度・ツール
- **銀行**: Cloud Save連携、口座・履歴
- **Leaderboards**: ランキング機能、複数ランキング管理
- **ジョブシステム（スキル制融合型）**: 個別スキル成長、スキルセットによる職業認定、複数職業保持、社会的影響

#### Phase 5で実装
- **Scheduled Jobs**: デイリーリセット、ランキング集計、イベント管理

#### Phase 6で実装
- **トレード**: Cloud Code連携、P2P取引
- **マーケット**: Cloud Code + Cloud Save、オークション
- **銀行拡張**: ポイント・ランク・特典
- **ギルド**: Cloud Code + Cloud Save、倉庫・イベント

---

## Phase 0: 基盤整備（1週間）

### 目標
開発基盤を確立し、M0を完了する。

### タスク

#### プロジェクト構造 [1日]
```
Assets/
  ├── Scenes/          # シーン
  ├── Scripts/
  │   ├── Bootstrap/   # 起動処理
  │   ├── Core/
  │   │   ├── Online/  # UGS（完了）
  │   │   ├── Player/  # プレイヤー
  │   │   ├── UI/      # UI基盤
  │   │   └── World/   # ワールド
  │   ├── Dev/         # 開発ツール
  │   └── Storage/     # データ永続化
  ├── Settings/        # 設定アセット
  └── Prefabs/         # プレハブ
```

#### コード品質 [1日]
- Roslyn Analyzer導入
- `.editorconfig` 作成
- Warning修正

#### ログ・メトリクス [1日]
- `LoggerService`: JSONL形式ログ
- `MetricsCollector`: 1分粒度CSV
- `BackupManager`: 3世代ローテ

#### DevConsole [1日]
- コンソールUI（`~`キー）
- 基本コマンド: help, clear, quit
- UGSコマンド: ugs.auth, ugs.save
- `Assets/Scripts/Dev/DevConsole.cs`

#### Debug Draw/Profiling [1日]
- `DebugDrawManager.cs`: Gizmo描画管理
  - ライン・ボックス・スフィア描画
  - 持続時間・色指定
  - `Assets/Scripts/Dev/DebugDrawManager.cs`
- `ProfilingManager.cs`: ProfilerMarker管理
  - カテゴリ別計測
  - 結果表示UI
  - `Assets/Scripts/Dev/ProfilingManager.cs`

### Exit条件
- ✅ 空ビルド成功（Development Build, 4秒）
- ✅ Warning 0件
- ✅ ログ・メトリクス出力確認
- ✅ DevConsole動作（F1キー）
- ✅ ProfilingManager動作（F2キー）
- ✅ DebugDrawManager実装
- ✅ UGS統合（認証・Cloud Save）
- ✅ Input System対応（New + 旧APIフォールバック）
- ✅ ビルドテスト完了

**ステータス**: ✅ Phase 0 完了（2025-10-30）

---

## Phase 1: Player Core（2週間）

### 目標
基本的なプレイヤー操作を実装（M1完了）。

### タスク

#### プレイヤー基本情報 [1日]
- `PlayerData.cs`: プレイヤー基本データ
  - Player ID（UGS Authentication自動生成）
  - プレイヤー名（初回ログイン時に設定）
  - 作成日時
  - 最終ログイン日時
  - `Assets/Scripts/Core/Player/PlayerData.cs`
- `PlayerNameUI.cs`: 名前設定UI
  - 初回ログイン時の名前入力
  - 名前変更機能（オプション）
  - 重複チェック（Cloud Code経由）
  - `Assets/Scripts/Core/UI/Player/PlayerNameUI.cs`
- Cloud Save連携
  - プレイヤー名の保存・取得
  - 名前重複チェック用Cloud Code関数

#### キャラクター [1日]
- Capsule + 簡易リグ
- 基本アニメーション（Idle/Walk/Run/Jump）
- プレイヤー名表示（頭上UI）

#### コントローラ [3日]
- `PlayerController.cs`: 移動・ジャンプ
  - PlayerData参照
  - `Assets/Scripts/Core/Player/PlayerController.cs`
- `PlayerAnimationController.cs`: Animator連携

#### カメラ [2日]
- Cinemachine導入
- `PlayerCamera.cs`: Follow/Orbit/Zoom

#### Input System [3日]
- Input Actions作成
- `InputManager.cs`: 統合管理
- Rebind UI: キー再割当

### Exit条件
- ✅ プレイヤー名設定・表示
- ✅ Player ID取得
- ✅ 移動・ジャンプ動作
- ✅ カメラ追従
- ✅ Input Rebind保存
- ✅ FPS 60維持

---

## Phase 2: UI/Settings（2週間）

### 目標
UI・設定・セーブ機能を実装（M2完了）。

### タスク

#### プレイヤーステータスシステム [2日]
- `PlayerStats.cs`: ステータス管理
  - 基本ステータス（レベル/経験値）
  - 戦闘ステータス（HP/MP/スタミナ）
  - 能力値（攻撃力/防御力/魔力/素早さ等）
  - ステータス計算（装備・バフ反映）
  - `Assets/Scripts/Core/Player/PlayerStats.cs`
- `ExperienceManager.cs`: 経験値・レベル管理
  - 経験値取得
  - レベルアップ処理
  - レベルアップ報酬
  - `Assets/Scripts/Core/Player/ExperienceManager.cs`
- Cloud Save連携
  - ステータスの保存・取得
  - レベル・経験値の永続化

#### ブートストラップ/状態管理 [2日]
- `GameStateManager.cs`: ゲーム状態管理
  - Title/InGame/Loading/Pause状態
  - 状態遷移制御
  - `Assets/Scripts/Core/GameStateManager.cs`
- `SceneBootstrap.cs`: シーン初期化
  - 各シーンの初期化処理
  - 依存関係の解決
  - PlayerData/PlayerStats初期化
  - `Assets/Scripts/Bootstrap/SceneBootstrap.cs`

#### HUD [2日]
- `HUDManager.cs`: HUD統合管理
  - HP/MP/スタミナ表示
  - レベル・経験値表示
  - プレイヤー名表示
  - ステータス連携・イベント駆動
  - `Assets/Scripts/Core/UI/HUDManager.cs`
- `HealthBar.cs` / `ResourceBar.cs`: プログレスバー
  - スムーズな変化アニメーション
  - `Assets/Scripts/Core/UI/Components/`
- `PlayerInfoPanel.cs`: プレイヤー情報パネル
  - 名前・レベル・経験値表示
  - `Assets/Scripts/Core/UI/Components/PlayerInfoPanel.cs`

#### メニュー [3日]
- `MenuManager.cs`: メニュー制御
- Pause Menu UI
- Settings Menu UI

#### 設定 [2日]
- `SettingsManager.cs`: Cloud Save連携
- Graphics/Audio/Input設定

#### セーブ/ロード [3日]
- `SaveManager.cs`: バージョニング対応
- `SaveData.cs`: データ構造
- Cloud Save + ローカルバックアップ

#### 通知 [2日]
- `NotificationManager.cs`: トースト通知
- キュー制御・優先度管理

#### Remote Config統合 [1日]
- `RemoteConfigManager.cs`: Remote Config統合
  - 設定値の動的取得
  - 環境別設定（Dev/Staging/Production）
  - キャッシュ・フォールバック
  - `Assets/Scripts/Core/Online/RemoteConfigManager.cs`
- 設定項目例
  - ガチャ確率テーブル
  - イベント期間
  - 経験値倍率
  - 機能フラグ（A/Bテスト用）

#### Analytics統合 [1日]
- `AnalyticsManager.cs`: Analytics統合
  - カスタムイベント送信
  - プレイヤー行動トラッキング
  - ファネル分析用イベント
  - `Assets/Scripts/Core/Online/AnalyticsManager.cs`
- 基本イベント定義
  - チュートリアル進行
  - レベルアップ
  - アイテム取得・使用
  - 課金イベント（将来用）

### Exit条件
- ✅ プレイヤーステータス表示（HP/MP/レベル等）
- ✅ 経験値取得・レベルアップ動作
- ✅ HUD表示（名前・ステータス）
- ✅ メニュー動作
- ✅ 設定永続化
- ✅ セーブ/ロード動作（ステータス含む）
- ✅ Remote Config取得・適用
- ✅ Analytics送信動作

---

## Phase 3: World/Minimap（2週間）

### 目標
シーンローディングとミニマップを実装（M3完了）。

### タスク

#### シーンローディング [3日]
- `SceneLoader.cs`: Additive Loading
- `LoadingScreen.cs`: ロード画面UI

#### ワールド管理 [2日]
- `WorldManager.cs`: シーン遷移管理
- テストシーン作成

#### ミニマップ [4日]
- `MinimapCamera.cs`: RenderTexture
- `MinimapUI.cs`: マップ表示・ズーム
- `MinimapIcon.cs`: アイコン管理

#### Addressables [2日]
- 導入検討・設定
- ドキュメント作成

### Exit条件
- ✅ シーン遷移動作
- ✅ ロード中UI制御
- ✅ ミニマップ表示
- ✅ ズーム・追従動作

---

## Phase 4: Economy/Social（3週間）

### 目標
経済・ソーシャル機能の基礎を実装（M4完了）。

### タスク

#### インベントリ/装備 [5日]
- `InventoryManager.cs`: アイテム管理（Cloud Save連携）
  - スタック対応
  - フィルタ・ソート機能
  - `Assets/Scripts/Core/Economy/InventoryManager.cs`
- `Item.cs` / `ItemData.cs`: ScriptableObject定義
  - アイテムタイプ（消費/装備/素材等）
  - レアリティ・説明文
  - `Assets/Scripts/Core/Economy/Items/`
- `EquipmentManager.cs`: 装備管理
  - 装備スロット（武器/防具/アクセサリ）
  - ステータス反映
  - `Assets/Scripts/Core/Economy/EquipmentManager.cs`
- インベントリUI: グリッド表示・D&D
- 装備UI: スロット表示・装備変更

#### クエストシステム [4日]
- `QuestManager.cs`: クエスト管理（Cloud Save連携）
  - クエスト進行状態保存
  - 報酬付与処理
  - `Assets/Scripts/Core/Quest/QuestManager.cs`
- `Quest.cs` / `QuestData.cs`: ScriptableObject定義
  - 目標・条件・報酬
  - `Assets/Scripts/Core/Quest/Quests/`
- クエストジャーナルUI
  - 進行中・完了クエスト表示
  - 目標進捗表示
  - `Assets/Scripts/Core/UI/Quest/QuestJournalUI.cs`

#### ガチャシステム [3日]
- `GachaManager.cs`: ガチャ管理（Cloud Code連携）
  - 排出テーブル管理
  - 確率計算・抽選処理
  - 履歴保存（Cloud Save）
  - `Assets/Scripts/Core/Economy/GachaManager.cs`
- `GachaTable.cs`: ScriptableObject定義
  - レアリティ別確率
  - 排出アイテムリスト
  - `Assets/Scripts/Core/Economy/Gacha/`
- ガチャUI
  - 演出（アニメーション）
  - 結果表示・履歴
  - `Assets/Scripts/Core/UI/Gacha/GachaUI.cs`

#### チャット [3日]
- `ChatManager.cs`: チャンネル管理
  - Global/Party/Guild/Whisper
  - ミュート機能
  - ログ保存（ローカル）
  - `Assets/Scripts/Core/Social/ChatManager.cs`
- チャットUI: メッセージ表示・入力

#### メール [3日]
- `MailManager.cs`: メール管理（Cloud Code + Cloud Save連携）
  - 送受信処理（Cloud Code経由）
  - 添付アイテム処理（冪等性保証）
  - 受領処理（Cloud Save更新）
  - `Assets/Scripts/Core/Social/MailManager.cs`
- メールUI: 受信箱・受領・通知

#### パーティ [2日]
- `PartyManager.cs`: パーティ管理
  - 招待・参加・離脱
  - リーダー移譲
  - `Assets/Scripts/Core/Social/PartyManager.cs`
- パーティUI: メンバー表示・管理

#### 採集（ギャザリング） [2日]
- `GatheringManager.cs`: 採集管理
  - リスポーン処理
  - レア度・ツール要件
  - `Assets/Scripts/Core/World/GatheringManager.cs`
- `GatheringNode.cs`: 採集ポイント
  - インタラクション
  - アイテムドロップ

#### 銀行 [2日]
- `BankManager.cs`: 口座管理（Cloud Save連携）
  - 預入・引出処理
  - 取引履歴保存
  - 手数料・利息計算（雛形）
  - `Assets/Scripts/Core/Economy/BankManager.cs`
- 銀行UI: 口座表示・取引履歴

#### Leaderboards統合 [2日]
- `LeaderboardManager.cs`: Leaderboards統合
  - ランキング登録・取得
  - 複数ランキング管理
  - 期間限定ランキング
  - `Assets/Scripts/Core/Online/LeaderboardManager.cs`
- ランキング種類
  - プレイヤーレベルランキング
  - ギルドランキング
  - イベントスコアランキング
- ランキングUI
  - トップランキング表示
  - 自分の順位表示
  - `Assets/Scripts/Core/UI/Leaderboard/LeaderboardUI.cs`

#### ジョブシステム（スキル制融合型） [5日]

**設計思想**: 「スキル（個別能力）の集合体が職業（社会的役割）を形成する」
- プレイヤーは個別スキルを自由に習得・成長させる
- 特定のスキルセットを満たすと「職業」として認定される
- 職業は社会的地位・称号であり、スキルの証明書のような存在

##### スキルシステム [2日]
- `SkillManager.cs`: スキル管理（Cloud Save連携）
  - 個別スキルの習得・レベルアップ
  - スキル経験値管理（使用回数・成功率で成長）
  - スキル前提条件チェック
  - `Assets/Scripts/Core/Player/Skills/SkillManager.cs`
  
- `Skill.cs` / `SkillData.cs`: ScriptableObject定義
  - **基本スキル例**:
    - 戦闘系: 剣術、槍術、弓術、体術、盾防御
    - 魔法系: 火魔法、水魔法、風魔法、土魔法、光魔法、闇魔法
    - 補助系: 身体強化、詠唱速度、魔力制御、気配察知
    - 生産系: 鍛冶、錬金術、料理、採掘、採集
  - スキルレベル（1〜100）
  - 成長曲線（使用回数による熟練度）
  - 前提スキル・レベル要件
  - 効果・威力計算式
  - `Assets/Scripts/Core/Player/Skills/SkillData/`

##### ジョブ認定システム [2日]
- `JobCertificationManager.cs`: 職業認定管理（Cloud Save連携）
  - スキルセット判定による職業認定
  - 公的機関での資格取得（NPC・クエスト経由）
  - 複数職業の同時保持可能
  - 職業ランク管理（見習い/一人前/熟練/達人）
  - `Assets/Scripts/Core/Player/Jobs/JobCertificationManager.cs`

- `JobDefinition.cs` / `JobData.cs`: ScriptableObject定義
  - **職業例と必要スキルセット**:
    - **火炎魔術師**: 火魔法Lv60+、詠唱速度Lv40+、魔力制御Lv30+
    - **魔導戦士**: 剣術Lv50+、身体強化Lv50+、魔力制御Lv30+
    - **聖騎士**: 剣術Lv40+、盾防御Lv40+、光魔法Lv40+
    - **暗殺者**: 体術Lv50+、気配察知Lv40+、闇魔法Lv30+
    - **賢者**: 火/水/風/土魔法すべてLv50+、魔力制御Lv60+
    - **鍛冶職人**: 鍛冶Lv70+、採掘Lv50+、火魔法Lv30+
  - 職業認定条件（スキルレベル要件）
  - 職業ボーナス（ステータス補正、特殊能力解放）
  - 社会的地位・称号
  - 認定機関情報（どこで資格を取得できるか）
  - `Assets/Scripts/Core/Player/Jobs/JobDefinitions/`

- `JobQuestManager.cs`: 職業認定クエスト管理
  - 職業認定試験クエスト（スキル要件を満たした後に受験）
  - 実技試験・筆記試験の実装
  - 合格による正式認定
  - `Assets/Scripts/Core/Player/Jobs/JobQuestManager.cs`

##### UI実装 [1日]
- `SkillTreeUI.cs`: スキルツリー表示
  - カテゴリ別スキル一覧（戦闘/魔法/補助/生産）
  - スキルレベル・経験値表示
  - スキルポイント割り振り
  - 前提スキル関係の可視化
  - `Assets/Scripts/Core/UI/Skills/SkillTreeUI.cs`

- `JobCertificationUI.cs`: 職業認定UI
  - 取得可能職業一覧（スキル要件の達成度表示）
  - 保持中の職業・ランク表示
  - 職業ボーナス詳細表示
  - 認定機関への案内
  - `Assets/Scripts/Core/UI/Jobs/JobCertificationUI.cs`

- `ActiveJobSelectorUI.cs`: アクティブ職業選択
  - 保持職業から現在の「肩書き」を選択
  - 選択した職業のボーナスが適用される
  - 職業切り替え（戦闘外のみ）
  - `Assets/Scripts/Core/UI/Jobs/ActiveJobSelectorUI.cs`

##### ゲームプレイへの統合
- **スキル成長**: 戦闘・採集・生産行動でスキル経験値が自動蓄積
- **職業認定**: スキル要件を満たすと通知、NPCに話しかけて認定試験を受験
- **社会的影響**: 
  - 特定職業保持者のみアクセス可能なエリア・クエスト
  - 職業ランクによる報酬・割引率の変化
  - ギルド加入条件（特定職業の保持）
- **バランス設計**:
  - 複数職業の同時保持は可能だが、アクティブは1つのみ
  - スキルレベル上限により、全スキルを極めることは困難
  - 職業の組み合わせで独自のビルドを構築可能

### Exit条件
- ✅ アイテム取得・装備動作
- ✅ クエスト進行・報酬受領
- ✅ ガチャ抽選・演出動作
- ✅ チャット・メール動作
- ✅ パーティ機能動作
- ✅ 採集・銀行動作
- ✅ Leaderboards登録・表示
- ✅ スキル使用・経験値蓄積・レベルアップ動作
- ✅ 職業認定条件判定・認定試験受験動作
- ✅ 複数職業保持・アクティブ職業切り替え動作
- ✅ 職業ボーナス適用・社会的影響（エリア制限等）動作
- ✅ 全データがCloud Saveに永続化

---

## Phase 4.5: Web Admin Dashboard（2週間）

### 目標
ゲーム世界を監視・管理するWEB管理ツールを実装。

### 前提条件
- Phase 4完了（プレイヤーデータ・経済システムが実装済み）
- UGS Service Account取得済み

### 技術スタック
- **Frontend**: Next.js 14 + TypeScript + TailwindCSS
- **UI**: shadcn/ui + Recharts
- **API**: UGS REST API
- **Auth**: UGS Service Account
- **Deploy**: Vercel（無料枠）

### タスク

#### プロジェクトセットアップ [0.5日]
```bash
npx create-next-app@latest admin-dashboard --typescript --tailwind --app
npm install @shadcn/ui recharts date-fns axios
```
- Next.js 14プロジェクト作成
- shadcn/ui導入
- 基本レイアウト構築

#### UGS API統合 [1日]
- `lib/ugs/auth.ts`: Service Account認証
- `lib/ugs/cloudSave.ts`: Cloud Save API
- `lib/ugs/cloudCode.ts`: Cloud Code API
- `lib/ugs/analytics.ts`: Analytics API
- `lib/ugs/remoteConfig.ts`: Remote Config API
- 環境変数設定（`.env.local`）

#### Cloud Code Admin関数実装 [2日]
- `AdminGetPlayerData`: プレイヤーデータ取得
- `AdminUpdatePlayerStats`: ステータス更新
- `AdminGrantItems`: アイテム付与
- `AdminSendMailToAll`: 全体メール送信
- `AdminGetAnalytics`: Analytics取得
- `AdminUpdateRemoteConfig`: Remote Config更新
- Service Token検証（管理者のみ実行可能）
- `exchange_module/Project/AdminFunctions.cs`

#### ダッシュボード画面 [2日]
- リアルタイムメトリクス表示
  - アクティブプレイヤー数（CCU）
  - 新規登録数（日次/週次/月次）
  - DAU/MAU
  - 平均プレイ時間
- サーバー状態表示
  - Cloud Code実行状況
  - エラーログ
  - API呼び出し頻度
- 経済監視
  - 通貨流通量グラフ
  - アイテム流通量
  - ガチャ実行統計
- `app/dashboard/page.tsx`

#### プレイヤー管理画面 [3日]
- プレイヤー検索
  - Player ID/名前/レベル範囲検索
  - `app/dashboard/players/page.tsx`
- プレイヤー詳細表示
  - 基本情報（ID/名前/レベル/経験値）
  - ステータス（HP/MP/攻撃力/防御力）
  - ジョブ情報
  - インベントリ一覧
  - 装備一覧
  - クエスト進行状況
  - 所持通貨
  - `app/dashboard/players/[id]/page.tsx`
- プレイヤー編集機能
  - ステータス変更
  - アイテム付与/削除
  - 通貨付与
  - BANステータス変更

#### Analytics Viewer [1日]
- イベント分析
  - カスタムイベント一覧
  - ファネル分析
  - リテンション分析
- グラフ表示
  - 時系列グラフ
  - 円グラフ
  - ヒートマップ
- `app/dashboard/analytics/page.tsx`

#### Remote Config Editor [1日]
- 設定値編集UI
  - ガチャ確率テーブル
  - イベント期間
  - 経験値倍率
  - 機能フラグ
- 環境別管理（Dev/Staging/Production）
- 変更履歴表示
- `app/dashboard/config/page.tsx`

#### ランキング管理 [0.5日]
- ランキング表示
  - プレイヤーレベルランキング
  - ギルドランキング
  - イベントランキング
- ランキングリセット機能
- `app/dashboard/leaderboards/page.tsx`

#### メール一括送信 [1日]
- メール作成UI
  - 全プレイヤー宛
  - 条件指定（レベル範囲等）
  - 添付アイテム設定
- 送信履歴表示
- `app/dashboard/mail/page.tsx`

#### ログビューア [0.5日]
- ログ検索
  - Player ID指定
  - 日時範囲指定
  - ログレベルフィルタ
- ログ詳細表示
- `app/dashboard/logs/page.tsx`

#### セキュリティ実装 [1日]
- Service Account認証
- アクセス制御（管理者ロール）
- 監査ログ（全操作記録）
- IP制限（オプション）

#### デプロイ [0.5日]
- Vercelデプロイ設定
- 環境変数設定
- ドメイン設定（オプション）

### ディレクトリ構成
```
admin-dashboard/
├── src/
│   ├── app/
│   │   ├── dashboard/
│   │   │   ├── page.tsx              # ダッシュボード
│   │   │   ├── players/              # プレイヤー管理
│   │   │   ├── analytics/            # Analytics
│   │   │   ├── config/               # Remote Config
│   │   │   ├── leaderboards/         # ランキング
│   │   │   ├── mail/                 # メール送信
│   │   │   └── logs/                 # ログビューア
│   │   └── layout.tsx
│   ├── components/
│   │   ├── ui/                        # shadcn/ui
│   │   └── dashboard/                 # カスタムコンポーネント
│   ├── lib/
│   │   └── ugs/                       # UGS API統合
│   └── types/                         # TypeScript型定義
├── .env.local                         # UGS認証情報
└── package.json
```

### Exit条件
- ✅ ダッシュボードが表示される
- ✅ プレイヤー検索・詳細表示が動作
- ✅ プレイヤー編集（ステータス/アイテム）が動作
- ✅ Analytics表示が動作
- ✅ Remote Config編集が動作
- ✅ ランキング表示が動作
- ✅ メール一括送信が動作
- ✅ ログ検索が動作
- ✅ Service Account認証が動作
- ✅ Vercelにデプロイ済み

### コスト
- **Next.js Hosting**: Vercel無料枠
- **UGS API**: 既存無料枠内
- **追加コスト**: $0

---

## Phase 5: Combat/Ability（3週間）

### 目標
戦闘・アビリティシステムを実装（M5完了）。

### タスク

#### ステータス [2日]
- `CharacterStats.cs`: HP/MP/攻撃力
- `StatusEffect.cs`: バフ・デバフ

#### ダメージ [3日]
- `DamageCalculator.cs`: ダメージ計算
- `Health.cs`: HP管理・死亡処理

#### アビリティ [5日]
- `AbilitySystem.cs`: アビリティ管理
- `Ability.cs`: ScriptableObject定義
- Basic Attack実装

#### 戦闘UI [2日]
- `CombatUI.cs`: ダメージ表示
- アビリティバー: スキルアイコン

#### VFX/Audio [3日]
- `VFXManager.cs`: エフェクト管理
- `AudioManager.cs`: BGM/SE管理
- 基本エフェクト作成

#### 敵AI [3日]
- `EnemyAI.cs`: 巡回・追跡・攻撃
- 敵キャラクター作成

#### Cloud Code Scheduled Jobs [2日]
- **デイリーリセット処理**
  - クエストリセット
  - デイリー報酬配布
  - ログインボーナス
- **ランキング集計処理**
  - 週次/月次ランキング確定
  - 報酬配布
- **イベント管理処理**
  - イベント開始/終了
  - 期間限定コンテンツ制御
- **メンテナンス処理**
  - データクリーンアップ
  - 古いメール削除
- Cloud Code Module実装
  - `exchange_module/Project/ScheduledJobs.cs`
  - Cron式でスケジュール設定

### Exit条件
- ✅ 攻撃・ダメージ動作
- ✅ アビリティ使用可能
- ✅ VFX/SE再生
- ✅ 敵AI動作
- ✅ Scheduled Jobs動作確認

---

## Phase 6: Trade/Market/Guild（任意・3週間）

### 目標
トレード・マーケット・ギルド機能を実装（M6完了）。

### タスク

#### トレード（P2P） [5日]
- `TradeManager.cs`: P2Pトレード（Cloud Code連携）
  - 在庫・権利検証
  - アイテムロック処理
  - 承認・決済（両者承認後に実行）
  - `Assets/Scripts/Core/Economy/TradeManager.cs`
- トレードUI
  - アイテム配置・表示
  - 承認ボタン・キャンセル
  - `Assets/Scripts/Core/UI/Trade/TradeUI.cs`

#### マーケット（オークション） [5日]
- `MarketManager.cs`: マーケット管理（Cloud Code + Cloud Save連携）
  - 出品・入札・落札処理
  - 手数料計算
  - 期限管理
  - `Assets/Scripts/Core/Economy/MarketManager.cs`
- マーケットUI
  - 出品リスト表示
  - 検索・フィルタ・ソート
  - 入札・即決購入
  - `Assets/Scripts/Core/UI/Market/MarketUI.cs`

#### 銀行拡張（ポイント/特典） [2日]
- `BankPointSystem.cs`: ポイント管理
  - 取引額に応じたポイント還元
  - ランク管理
  - 特典付与処理
  - `Assets/Scripts/Core/Economy/BankPointSystem.cs`
- 銀行特典UI
  - ポイント残高表示
  - ランク・特典一覧
  - `Assets/Scripts/Core/UI/Bank/BankRewardsUI.cs`

#### ギルドシステム [6日]
- `GuildManager.cs`: ギルド管理（Cloud Code + Cloud Save連携）
  - ギルド作成・解散
  - メンバー管理（招待・除名）
  - ロール管理（マスター/オフィサー/メンバー）
  - `Assets/Scripts/Core/Social/GuildManager.cs`
- `GuildStorage.cs`: ギルド倉庫
  - 共有アイテム管理
  - 権限別アクセス制御
  - `Assets/Scripts/Core/Social/GuildStorage.cs`
- `GuildEvent.cs`: ギルドイベント
  - イベント作成・参加
  - 報酬配布
  - `Assets/Scripts/Core/Social/GuildEvent.cs`
- ギルドUI
  - ギルド情報表示
  - メンバーリスト
  - 倉庫UI
  - イベントUI
  - `Assets/Scripts/Core/UI/Guild/`

### Exit条件
- ✅ トレード動作（両者承認・決済）
- ✅ マーケット動作（出品・入札・落札）
- ✅ 銀行ポイント・特典動作
- ✅ ギルド作成・管理動作
- ✅ ギルド倉庫・イベント動作
- ✅ UIフロー整合

---

## Phase 7: Network（後段・4週間以降）

### 前提条件
- M5完了
- CCU > 30 連日 or 外部テスト必要
- 予算確保

### タスク

#### ライブラリ選定 [1週]
- 候補調査（NGO/Mirror/Photon）
- プロトタイプ・性能評価

#### 統合 [2週]
- NetworkManager実装
- Transform/Animation同期
- RPC統合

#### 最適化 [1週]
- 予測・補間
- 帯域最適化

### Exit条件
- ✅ 同時接続動作
- ✅ レイテンシ許容範囲
- ✅ 同期安定

---

## 2. 品質管理

### 各フェーズ共通チェック
- [ ] コンパイルエラー 0件
- [ ] Warning 0件
- [ ] FPS 60維持
- [ ] メモリリーク無し
- [ ] UGS連携正常

### コードレビュー基準
- 命名規約準拠
- コメント適切（日本語）
- 例外処理・null安全性
- パフォーマンス考慮

---

## 3. 進捗管理

### 週次レビュー
- 進捗確認
- リスク評価
- 次週計画

### マイルストーンレビュー
- Exit条件確認
- 品質評価
- 次フェーズ承認

### ドキュメント更新
- `Gameplay_Feature_Status.md` 更新
- 本ロードマップ更新

---

## 変更履歴
- 2025-10-28: Phase 0 実装完了
  - .editorconfig拡充、Assembly Definition作成
  - LoggerService, BackupManager実装
  - DevConsole, DebugDrawManager, ProfilingManager実装
  - GameStateManager, SceneBootstrap実装
  - Phase 0実装状況ドキュメント作成
- 2025-10-27: 初版作成（UGS統合完了状態から）

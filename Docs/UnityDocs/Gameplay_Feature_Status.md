# Gameplay Feature Status

ドキュメント種別: Status Tracking  
対象プロジェクト: MMO_exploration (Unity)  
版数: v1.0  
作成日: 2025-10-10  
最終更新: 2025-10-10  
作成者: TBD  
承認者: TBD  
関連資料: `Docs/NewUI/UI_Creation_Guide.md`, `Docs/NewDocs/*`

---

## 1. 目的 / 範囲
- 目的: 本プロジェクトのゲームプレイ機能の実装状況を可視化し、計画・レビュー・リリース判断を支援する。
- 非範囲: 価格/運用、外部SaaSの契約条件等は別資料。

---

## 2. 前提 / 環境
 - Unity Version: 6.2（6000.2.0f2）
 - Render Pipeline: URP
 - Target platforms: Windows（優先）
 - Input System: New Input System
  - Networking: MMO前提（初期はオフライン、将来候補: NGO/Mirror/Photon）
  - シーン配置: `Assets/Scenes/`
  - 命名規約（要約）:
  - Scripts: PascalCase（役割接尾辞: `Controller`/`Manager`）
  - Prefabs: PascalCase（接頭辞: `UI_`, `FX_` 等）
  - Folders: `Assets/{Domain}/{Feature}/...`
  - データ永続化（補足）: フェーズ0は UGS 最小（Authentication/Cloud Save/Cloud Code、必要に応じて Economy）。UIKit は未使用で、SDK はコードから直接呼び出し。

## 3. 用語定義
- Scope: 規模の目安。S=~0.5–1d, M=~1–3d, L=~3d+
- Status:
  - Not started: 企画/設計未着手
  - Designing: 仕様/設計中（要件定義・技術検証）
  - Implementing: 実装中（PR作業）
  - Review: 実装完了・レビュー/結合テスト中
  - Done: リリース水準で完了
  - Blocked: 依存・外部要因で停止

---

## 4. 概況サマリ
- 実装済み: Backend 初期化の土台（`BackendBootstrap` / `BackendService` / `UgsBackendService` スタブ）。Supabase依存はスタブ化。
- 進行中: なし
- リスク: RP選定未決、ネットワーク方針未決、アニメーション仕様未決

---

## 5. 機能別ステータス（Feature Matrix）

| Category | Feature | Scope | Owner | Status | 根拠/成果物（Unity） | Notes |
|---|---|---:|---|---|---|---|
| Player | インベントリ/装備 | M | TBD | Not started | TBD | 所持/装備、スタック、フィルタ、UI |
| Quest | クエスト | M | TBD | Not started | TBD | ジャーナル、進行管理、報酬 |
| Gacha | ガチャ | M | TBD | Not started | TBD | テーブル、演出、履歴 |
| Economy | 銀行（口座/取引履歴/UI） | L | TBD | Not started | TBD | 口座、履歴、手数料/利息 |
| Economy | 銀行ポイント/特典 | M | TBD | Not started | TBD | 還元/ランク/特典付与 |
| Trade | トレード（P2P） | L | TBD | Not started | TBD | 在庫/権利検証、ロック、承認/決済 |
| Market | マーケット（オークション） | L | TBD | Not started | TBD | 出品/入札/落札、手数料 |
| Social | チャット | M | TBD | Not started | TBD | チャンネル/ミュート/ログ |
| Social | メール（添付/受領） | M | TBD | Not started | TBD | 添付/受領UI/通知 |
| Social | パーティ（基礎） | M | TBD | Not started | TBD | 招待/離脱/リーダー移譲 |
| Social | ギルド（機能/倉庫/イベント） | L | TBD | Not started | TBD | ロール、倉庫、イベント運用 |
| Combat | 能力システム（アビリティ基盤） | L | TBD | Not started | TBD | アビリティ定義/適用/クールダウン |
| World | ギャザリング/採集 | M | TBD | Not started | TBD | リスポーン、レア度、ツール |
| Network | レプリケーション最適化 | L | TBD | Not started | TBD | 予測/補間、帯域最適化 |
| UI | HUD（HP/MP/通知連携） | M | TBD | Not started | TBD | ステータス連携/イベント駆動 |
| UI | メニュー（Pause/Settings） | S | TBD | Not started | `Assets/Settings/*` | 入出力/映像/音量 |
| UI | 通知/トースト | S | TBD | Not started | TBD | キュー制御、抑制 |
| Core | セーブ/ロード（ローカル） | M | TBD | Not started | TBD | バージョニング/将来クラウド |
| Core | ブートストラップ/状態管理 | M | TBD | Not started | `Assets/Scenes/SampleScene.unity` | Title/InGame/Loading |
| World | シーンローディング（Additive） | M | TBD | Not started | TBD | Addressables/Streaming 検討 |
| Map | ミニマップ | M | TBD | Not started | TBD | RenderTexture/Overlay |
| Player | キャラコントローラ（移動/ジャンプ） | M | TBD | Not started | TBD | KCC/CharacterController |
| Player | カメラ（Follow/Orbit） | S | TBD | Not started | TBD | Cinemachine 検討 |
| Anim | アニメーション（Idle/Run/Jump） | M | TBD | Not started | TBD | Humanoid/Retarget 戦略 |
| Audio | BGM/SE 管理 | S | TBD | Not started | TBD | Mixer/カテゴリ |
| VFX | ヒット/トレイル/インパクト | S | TBD | Not started | TBD | RP差異考慮 |
| Tools | Dev Console/Logger | S | TBD | Not started | TBD | ログカテゴリ/保存 |
| Tools | Debug Draw/Profiling | S | TBD | Not started | TBD | Gizmo/ProfilerMarker |

注: Owner/根拠/成果物は着手時に埋めること。

---

## 6. マイルストーン

- M0: Project Bootstrap（基盤整備）
  - 含む: フォルダ構成/命名規約、URP設定、ビルド設定、`Assets/Scenes/SampleScene.unity` 起動、`Assets/Settings/*` 初期化、ロギング基盤
  - 依存: なし
  - Exit条件:
    - 空テンプレのビルド成功（Windows）
    - Lint/Analyzer 設定済み（最低限）
    - ドキュメント（本ファイル含む）版管理開始
  - Status: Not started
  - 予定: TBD

- M1: Player Core + Camera + Input
  - 含む: キャラコントローラ（移動/ジャンプ）、カメラ（Follow/Orbit）、Input Mapping、Rebind UI（最低限）
  - 依存: M0
  - Exit条件:
    - プレイヤー操作が安定（基本操作のバグなし）
    - 入力再割当が保存・復元
  - Status: Not started
  - 予定: TBD

- M2: UI/Settings + Save/Load（UGS 最小 or ローカル）
  - 含む: HUD（HP/MP/通知連携の骨格）、メニュー（Pause/Settings）、Settings-保存連携、セーブ/ロード（バージョニング雛形）
  - 依存: M1
  - Exit条件:
    - 設定変更がプレイ中に反映・永続化
    - セーブ/ロードが単一スロットで往復可能（当面はUGS最小 or ローカルのいずれかで確認）
  - Status: Not started
  - 予定: TBD

- M3: World Loading + Minimap
  - 含む: シーンローディング（Additive）フロー、Addressables/Streaming 検討反映、ミニマップ（Overlay or RenderTexture）
  - 依存: M2
  - Exit条件:
    - ロード/遷移中の入力・UI制御が破綻しない
    - ミニマップの追従・ズーム（最低限）
  - Status: Not started
  - 予定: TBD

- M4: Economy/Social 基礎
  - 含む: インベントリ/装備、チャット、メール（添付/受領）、パーティ（基礎）、ギャザリング/採集、銀行（口座/履歴/UIの雛形）
  - 依存: M3
  - Exit条件:
    - アイテムの取得/消費/装備の一連が安定
    - チャット/メールの基本導線が利用可能（オフライン前提でもダミーで確認）
  - Status: Not started
  - 予定: TBD

- M5: Combat/Ability 基礎 + UI連携
  - 含む: 能力システム（アビリティ定義/適用/クールダウン）、Basic Attack、Damage/Health、HUD連動、VFX/Audio骨格
  - 依存: M4
  - Exit条件:
    - 単体敵に対する攻撃→ダメージ→死亡までの一連が安定
    - 基本VFX/SE が再生・抑制できる
  - Status: Not started
  - 予定: TBD

- M6: Trade/Market + Economy 拡張（任意）
  - 含む: トレード（P2P）骨格、マーケット（出品/入札/落札）、銀行ポイント/特典（雛形）
  - 依存: M4
  - Exit条件:
    - オフライン前提でUIフローの整合が確認できる（将来のオンライン化を阻害しない設計）
  - Status: Not started
  - 予定: TBD

- M7: ネットワーク適用・最適化（後段）
  - 含む: セッション/トランスポート選定、Transform/Anim 同期、予測/補間、帯域最適化の方針試作
  - 依存: M5 以降（基礎ゲームループ確立後）
  - Exit条件:
    - 小規模同時接続での体験検証が可能（社内テスト）
  - Status: Not started
  - 予定: TBD

---

## 7. バックログ
- [ ] コーディング規約/Analyzer設定（Roslyn/StyleCop 等）
- [ ] CI（LFS/Cache/Build Pipeline）設計
- [ ] Addressables 設計/命名指針
- [ ] セーブデータ仕様（バージョニング/移行）

---

## 8. リスクと依存
- Render Pipelineの後追い変更は高コスト → 初期決定を厳守
- Networking方針未定 → オフライン→オンライン拡張の前提確認
- アニメーション仕様（Humanoid/Generic）→ Retarget戦略の事前合意
- アセット参照方法（直参照/Addressables/Resources）の混在防止

---

## 9. 運用ルール
- 更新単位: 機能完了/レビュー移行時、マイルストーン変化時
- 記法統一:
  - Statusは上記定義のみ使用
  - 表の列は固定（列追加時は本節に追記）
- 変更申請: PRにこのファイルの差分を含め、Reviewerで承認を得る

---

## 10. 変更履歴
- 2025-10-10: 初版作成（新規プロジェクト初期状態を Not started で定義）
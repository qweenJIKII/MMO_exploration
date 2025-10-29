# ゲーム作成計画（MMO_exploration / Unity）

更新日: 2025-10-10  
参照: `Docs/UnityDocs/Gameplay_Feature_Status.md`, `Docs/UnityDocs/Zero_Budget_Dev_Operations_Guide.md`

---

## 1. 目的 / ビジョン
- **目的**: 個人開発・予算ほぼゼロの制約下で、MMOを前提にコア体験を段階的に実現し、後段のオンライン化にスムーズに拡張できる土台を構築する。
- **ビジョン**: MMO前提で設計しつつ、当面はオフライン完結の基礎ループ（移動→UI→セーブ→ミニマップ→軽い経済/ソーシャル→戦闘基礎）を短サイクルで積み上げ、導線・UI・運用性（観測/バックアップ）を早期に整備する。最終的には段階的に“万人がプレイ可能”な環境（スケールアウト前提）を目指す。

---

## 2. スコープ / 非スコープ
 - **スコープ（初期段階）**: 
  - Player 基礎（移動/ジャンプ/カメラ/入力再割当）
  - UI 基礎（HUD/メニュー/通知）
  - セーブ/ロード（ローカル、バージョニング雛形）
  - シーンローディング（Additive）/ ミニマップ
  - Economy/Social 骨格（UGS最小: Authentication/Cloud Save/Cloud Code を中核。必要に応じて Economy、将来: Quest 等、チャット/メールUI、パーティ基礎、採集、銀行雛形）
  - Combat/Ability 骨格（Basic Attack、Damage/Health、HUD 連動、VFX/Audio 骨格）
  - Dev ツール（DevConsole/Logger、DebugDraw/Profiling）
- **非スコープ（当面）**:
  - マネージドサービス依存（フェーズ1以降で段階導入）。

---

## 3. 開発方針（ゼロ予算運用の反映）
  - **段階導入**: 当面はローカル実行だが、バックエンドは UGS 最小で開始（Authentication/Cloud Save/Cloud Code、必要に応じて Economy）。Supabase は完全スタブ化済み。
  - **観測性の先行**: 早い段階で JSONL ログ、1分粒度CSVメトリクス、バックアップ 3世代を導入。
  - **ヘッドレス準備**: 将来のオンライン化に備え、Headless 実行パラメータでの動作確認導線を用意（ローカルで十分）。
  - **抽象化**: `Assets/Scripts/Core/Online/Backend/` に `IBackendService` と `UgsBackendService`（スタブ→段階実装）を用意。起動は `BackendBootstrap`。冪等IDとリトライ規約を統一。

---

## 4. 技術スタック（決定提案・承認待ち）
  - Unity Version: 6.2（6000.2.0f2）
  - Render Pipeline: URP
  - Target Platform: Windows（優先）
  - Input System: New Input System（Rebind API 利用）
  - Networking: MMO前提。初期はオフライン進行、将来候補: NGO/Mirror/Photon
  - データ永続化/オンライン機能: UGS（Authentication/Cloud Save/Cloud Code を中核、必要に応じて Economy。将来の拡張は Cloud Code/Cloud Save を基盤に）。UIKit は未使用で、SDK をコードから直接呼び出す。ローカルSQLiteは未使用。
  - アセット参照: 直参照で開始→将来 Addressables へ移行指針を整備
  - カメラ: Cinemachine

---

## 4.5 UGS 採用機能（段階導入計画）
 - **Phase A（現在）**: Authentication（匿名）、Cloud Save（セーブ/簡易インベントリ）、Cloud Code（受領/冪等ロジック）
 - **Phase B（任意）**: Economy（通貨/アイテム）、Quest 等（Cloud Code 連携）、Remote Config（フラグ）
 - **Phase C（検討）**: Guild/チャット等は別途サービス選定。Realtime要件は将来のネットワーク方式に合わせて検討。
備考: P2Pギフトは Cloud Code の冪等処理 + Cloud Save/Economy 更新の二相構成を想定。

---

## 5. マイルストーン（`Gameplay_Feature_Status.md` と整合）
- M0: Project Bootstrap（基盤整備）
  - 含む: フォルダ/命名規約、RP選定、ビルド設定、`Assets/Scenes/SampleScene.unity` 起動、`Assets/Settings/*` 初期化、ロギング基盤
  - Exit: Windows で空テンプレビルド成功、Lint/Analyzer 最低限、ドキュメント版管理開始
- M1: Player Core + Camera + Input
  - 含む: キャラコントローラ（移動/ジャンプ）、カメラ Follow/Orbit、Input Mapping & Rebind保存
  - Exit: 基本操作安定、再割当の保存/復元
  - 導入タイミング（プレイヤーキャラ）:
    - M1 前半: プレースホルダ（Capsule/Basic Rig）で実装を進行（移動/ジャンプ/カメラ連携の確認）
    - M1 後半: 簡易キャラクターモデル＋基本モーション（Idle/Walk/Run/Jump）を適用し挙動確認
    - M5 以降: 本番モデル/リグ/モーションへ差し替え（アビリティ/VFX/Audio と統合）
- M2: UI/Settings + Save/Load（UGS最小 or ローカル）
  - 含む: HUD 骨格、メニュー（Pause/Settings）、設定保存、UGS最小データ（Authentication/Cloud Save/Cloud Code）の呼び出し導線
  - Exit: 設定や基本データがUGSまたはローカルで永続化（当面は最小のUGS連携：Authentication/Cloud Save）
- M3: World Loading + Minimap
  - 含む: Additive ローディングフロー、ミニマップ（Overlay/RT）
  - Exit: 遷移中の入力/UI破綻なし、追従/ズーム最低限
- M4: Economy/Social 基礎（UGS）
  - 含む: インベントリ/装備（Cloud Save/Economy）、メール代替（Cloud Code + Cloud Save）、チャット、パーティ基礎、採集、銀行（雛形）。
  - Exit: 取得/消費/装備が安定、受領フロー（Cloud Code + Cloud Save/Economy）が可能。`BackendService` 経由で UGS を呼ぶ。
- M5: Combat/Ability + UI 連携
  - 含む: アビリティ定義/適用/CD、Basic Attack、Damage/Health、HUD連動、VFX/Audio 骨格
  - Exit: 単体敵に対する一連の流れが安定、基本VFX/SE 再生/抑制
- M6: Trade/Market + Economy 拡張（任意）
  - 含む: P2P トレード、オークション雛形、銀行ポイント/特典（雛形）
  - Exit: オフラインUIフロー整合（将来オンライン阻害なし）
- M7: ネットワーク適用・最適化（後段）
  - 含む: セッション/トランスポート選定、Transform/Anim 同期、予測/補間、帯域最適化
  - Exit: 小規模同時接続の体験検証（社内）

スケジュール目安（承認後確定）:
- M0: 1週, M1: 1–2週, M2: 1–2週, M3: 1–2週, M4: 2–3週, M5: 2–3週, M6: 2–3週, M7: 3週〜

---

## 6. WBS（初期2スプリント例）
- Sprint 1（M0〜M1 途中）
  - 1) URP 設定とプロファイル初期化
  - 2) ビルド設定/起動検証（Windows）
  - 3) Lint/Analyzer 導入（最低限）
  - 4) PlayerController（移動/ジャンプ）
  - 5) Cinemachine カメラ Follow/Orbit（暫定）
  - 6) InputActions & Rebind 保存/復元
  - 7) JSONL ログ・1分粒度CSV メトリクス雛形
- Sprint 2（M1 完了〜M2 着手）
  - 1) HUD 骨格（HP/MP/通知フック）
  - 2) メニュー（Pause/Settings）+ 保存/反映
  - 3) セーブ/ロード（ローカル/SQLite → 将来 PostgreSQL）
  - 4) DevConsole コマンド（inv/mail self-test）
  - 5) バックアップ 3世代ローテ

---

## 7. 成果物 / リポジトリ構造
- `Assets/Scenes/` サンプルシーン起動
- `Assets/Settings/` 共通設定アセット
- 命名規約: Scripts=PascalCase（`Controller`/`Manager` 接尾辞）、Prefabs=PascalCase（`UI_`/`FX_` 接頭辞）、Folders=`Assets/{Domain}/{Feature}/...`
- ドキュメント: 本計画、`Gameplay_Feature_Status.md`、運用ガイド、UIガイド

---

## 8. リスクと対応（統合）
- RP 後追い変更コスト大 → 初期決定を厳守、M0で確定
- Networking 未定 → オフライン前提で拡張余地を設計（抽象化/依存隔離）
- アニメーション仕様未決 → Humanoid/Generic 方針と Retarget を早期合意
- 参照方法の混在 → 直参照で開始、Addressables への移行手順/命名を整理

---

## 9. KPI / 品質基準
 - Cost/CCU（月）: 目標 $0.40–$0.70（フェーズ1以降）
 - クライアント: FPS 60 を目標（標準画質・1080p想定）、エラー率、ロード時間（プラットフォーム標準）
 - サーバ（導入時）: CPU/メモリ/帯域、セッション数、Tick/FPS
 - DB: QPS/スロークエリ/ストレージ使用量
 - ネットワーク: egress/リクエスト数（CDN有無差）
 - 障害: MTTR、失敗率

## 9.5 MMO移行タイミング（オンライン移行ゲート）
- **機能ゲート**: M3（World Loading+Minimap）、M4（Economy/Social基礎）、M5（Combat/Ability基礎）の主要Exitを満たす。
 - **技術ゲート**: Headless実行での擬似サーバ運用が安定。基本セッション/同期設計のドラフト完了。
- **品質/KPIゲート**: FPS目標達成、致命的クラッシュ率の低さ、ロード時間/入力遅延の許容内。
- **規模/コストゲート**: `Zero_Budget_Dev_Operations_Guide.md` の「有料化のゲート」に準拠。
  - 目安: CCU > 30 が連日 or 常設外部テストの必要性が発生 → フェーズ1（最安VPS + 小型DB）へ移行検討。
- **決定フロー**: M5完了直後にオンライン化Go/No-Go審査。必要に応じてM6（Trade/Market）前に小規模公開テストを挿入。
## 10. チェックリスト（着手順）
- [ ] M0: URP設定/ビルド/Analyzer/ドキュメント整備
- [ ] Sprint1: Player/Camera/Input/ログ&メトリクス
- [ ] M2: HUD/Settings/Save-Load
- [ ] M3: Additive Loading/Minimap
- [ ] M4: Inventory/Chat/Mail/Party/Bank/採集
- [ ] M5: Ability/Combat/VFX/Audio
- [ ] フェーズ1: VPS/DB/自動再起動/CDN（必要時）
- [ ] オンライン移行判断（ゲート）レビュー（M5完了後目安）

---

## 11. 承認事項（要決定）
- Networking ライブラリ候補（導入時期含む）

---

## 12. 変更管理
- 変更は PR ベース。`Gameplay_Feature_Status.md` の Status 更新と本計画の差分を同時にレビュー。
- マイルストーン更新時・機能レビュー移行時にサマリーを追記。

---

## 変更履歴
- 2025-10-10: 初版作成（2文書の統合計画）

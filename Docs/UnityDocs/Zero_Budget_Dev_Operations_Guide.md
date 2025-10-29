# 個人開発・予算ほぼゼロ向け 運用手順ガイド（Unity版）

更新日: 2025-10-10  
対象プロジェクト: MMO_exploration (Unity)  
関連ドキュメント: `Docs/UnityDocs/Gameplay_Feature_Status.md`

---

## 目的と前提
- 目的: ベータ前までクラウド費用を発生させずに、Unityでの開発〜小規模テストを成立させる具体手順を示す。
- 対象: 個人開発者 / 予算ゼロ〜極小 / オフライン中心→後段でオンラインに拡張予定。
- 方針: 段階導入（保存→観測→軽量サーバ）。有料化は明確なゲートを越えるまで回避。

---

## フェーズ0: ローカル完結構成（ゼロコスト）

### 1. データ永続化（ローカル）
- 保存先: `Application.persistentDataPath` 配下に作成（例: `.../MMO_exploration/Database/`）。
- 選択肢:
  - A) JSON ファイル（最短・依存ゼロ）
    - 例: `inventory.json`, `mailbox.json` を1プレイヤー1ファイル or 単一DB風集約
    - Pros: 追加パッケージ不要 / すぐ使える。Cons: 同時書込に弱い・クエリ不可。
  - B) SQLite（無料プラグイン利用）
    - `Sqlite4Unity3d` などのフリー実装で十分（IL2CPP可否は要確認）。
    - 将来の PostgreSQL 互換を意識した型/制約を設計。
- バックアップ: 起動/終了時ローテ（`*.bak` 3世代推奨）。

（参考）SQLite 最小スキーマ（将来移行を意識）
```sql
-- Inventory
CREATE TABLE IF NOT EXISTS inventory (
  player_id TEXT NOT NULL,
  item_id   TEXT NOT NULL,
  amount    INTEGER NOT NULL CHECK (amount >= 0),
  meta_json TEXT,
  updated_at INTEGER NOT NULL,
  PRIMARY KEY (player_id, item_id)
);
CREATE INDEX IF NOT EXISTS idx_inventory_updated ON inventory(updated_at);

-- Mail (inbox)
CREATE TABLE IF NOT EXISTS mail (
  mail_id    TEXT PRIMARY KEY,
  player_id  TEXT NOT NULL,
  subject    TEXT NOT NULL,
  body       TEXT NOT NULL,
  attachments_json TEXT NOT NULL,
  expire_at  INTEGER NOT NULL,
  created_at INTEGER NOT NULL,
  claimed    INTEGER NOT NULL DEFAULT 0
);
CREATE INDEX IF NOT EXISTS idx_mail_player ON mail(player_id);
```

テスト運用（Unityでの最小操作例）
- エディタ用メニュー: `Tools/Dev/` に「EnsureSchema」「BackupNow」「SelfTest」等を用意。
- In-Game DevConsole（任意）: `mail.send p01 "Test" "Hello" 604800` / `mail.claimAll p01` / `inv.add p01 wood 10 {}` などのコマンドを登録。

### 2. 実行構成（ローカル・スタンドアロン）
- クライアント: エディタの PlayMode / Standalone ビルド。
- 疑似サーバ（任意・後段準備）:
  - Headless 実行（Windows/Linux）: `-batchmode -nographics -logfile` を利用。
  - 目的: 将来のオンライン化を見据え、サーバ・クライアント分離のテストを可能に。

例: Windows（PowerShell）参考
```powershell
# Headless(疑似サーバ) 起動例
"Build/Server/MMO_exploration.exe" -batchmode -nographics -logfile "Logs/server_%DATE:~0,10%.log" --port 7777 --nosteam

# クライアント起動例（ログ分離）
"Build/Client/MMO_exploration.exe" -logfile "Logs/client_%DATE:~0,10%.log"
```

注意/トラブルシュート
- 同一バージョンのクライアント/サーバでのみ接続可（将来のネット導入時）。
- Windows ファイアウォールの許可（UDP/TCP は採用ライブラリ次第）。
- 複数クライアント検証はエディタ多重起動より Standalone の方が実態に近い。

### 3. 認証/ID
- 開発中はゲストIDで十分: `PlayerPrefs` or `Guid.NewGuid()` を初回発行→保存。
- フレンド/外部IDは後段（オンライン移行時）に導入。

### 4. 観測性（ローカルのみ）
- ログ: JSON Lines（1行=1イベント）。1日ローテーション、最大100MB。
- メトリクス（最低限）: 1分粒度 CSV 追記（`tick_rate`, `fps`, `conn`, `ram_mb`, `cpu_pct`）。
- HUD: インベントリ増減/メール受領/通知トーストなど、開発UIで可視化。

### 5. バックグラウンド処理
- `Coroutine` / `InvokeRepeating` / `Timer` で代替。キュー/外部ワーカーは導入しない。
- 例: 1分おきメール失効、5分おき銀行利息バッチ。

---

## フェーズ1: 公開ミニテスト（月 $5 前後）

### 1. 最安VPS（任意、ネット導入時）
- 例: Hetzner CX11 / Contabo 最小 / Oracle Free（在庫運）。
- 想定: CCU < 10–20、単一障害点を許容。

### 2. セキュリティ初期設定
- 新規ユーザ + SSH 公開鍵ログイン、UFW/Windows Firewall、不要ポート遮断。
- ゲームポートのみ開放（例: UDP 7777）。

### 3. データベース（必要時）
- 同居 PostgreSQL を導入し、毎日バックアップ。
- まずはローカルJSON/SQLite→PostgreSQL→（将来）マネージドへ。

### 4. Headless サーバ同居運用
- 1プロセスから開始。CPU/メモリ/帯域を監視。
- 障害時は自動再起動（systemd など）。

### 5. 配信（任意）
- Cloudflare Free で静的配信（パッチ/画像）。大容量配布は最小限。

---

## 有料化のゲート
- CCU > 30 が連日 or 常設外部テストが必要 → 初めてマネージド導入を検討。
- 導入順序の目安:
  1) DB を小型クラウドへ（Supabase/Neon 最小 等）
  2) CDN / 画像ストレージ（Cloudflare R2 + CDN）
  3) Game サーバマネージド（PlayFab/Multiplay/GameLift/Agones 等）

---

## KPI と運用メトリクス
- Cost/CCU（月）: 目標 $0.40–$0.70。
- クライアント: FPS、エラー率、ロード時間。
- サーバ（導入時）: CPU/メモリ/帯域、セッション数、Tick/FPS。
- DB: QPS/スロークエリ/ストレージ使用量。
- ネットワーク: egress/リクエスト数（CDN有無の差）。
- 障害: MTTR、失敗率。

---

## リスクと回避策
- 単一障害点: 受容し、バックアップ/自動再起動で緩和。
- データ破損: 毎日バックアップ + 復元演習。
- 無料枠の制限: スリープ/帯域制限 → 必要時のみ少額課金へ。
- ベンダーロック: DB/ネット処理は抽象化層で隠蔽（移行容易性）。

---

## チェックリスト（実施順）
- [ ] ローカル保存（JSON or SQLite）実装・バックアップ 3世代
- [ ] PlayMode/Standalone 起動スクリプト整備
- [ ] 構造化ログ（JSONL）と1日ローテーション
- [ ] KPI 収集（1分粒度CSV）
- [ ] 最安VPS 手配（必要時）
- [ ] PostgreSQL 導入 + 毎日バックアップ
- [ ] 自動再起動（systemd 等）
- [ ] CDN（任意）
- [ ] 有料化ゲートの数値を定義（CCU、障害頻度 等）

---

## 付録: 参考コマンド/設定

Unity Headless（参考・環境に合わせて調整）
```powershell
# Headless server-like 実行（Windows）
"Build/Server/MMO_exploration.exe" `
  -batchmode -nographics `
  -logfile "Logs/server_%DATE:~0,10%.log" `
  --port 7777 --nosteam
```

systemd（Linux, 参考）
```ini
[Unit]
Description=MMO_exploration Headless Server
After=network.target

[Service]
Type=simple
ExecStart=/opt/mmo/MMO_exploration.x86_64 -batchmode -nographics -logfile /var/log/mmo/server.log --port 7777
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

---

## フェーズ2以降（将来計画の概略）
- DB 移行: JSON/SQLite → PostgreSQL → マネージド（Aurora/Supabase/Neon）
- サーバ: PlayFab/Multiplay/GameLift/Agones 等の導入検討
- 配信: S3 + CloudFront（署名URL）/ Cloudflare R2 + CDN
- 観測性: CloudWatch/ELK/Opentelemetry（段階導入）
- バックグラウンド: キュー/Lambda or コンテナワーカー（冪等性設計）

---

## 変更履歴
- 2025-10-10: 初版（Unity 向けにゼロコスト運用手順を整理）

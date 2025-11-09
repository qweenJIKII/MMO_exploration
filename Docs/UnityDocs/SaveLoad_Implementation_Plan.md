# セーブ/ロード実装計画

## 概要
MMOゲームにおけるセーブ/ロード処理の実装タイミングと詳細設計

---

## Phase 2（現在） - 基本実装 ✅

### 実装済み機能

#### 1. 自動セーブ
- **定期自動セーブ**: 5分間隔で自動保存
- **アプリケーション終了時**: `OnApplicationQuit()` で自動保存
- **アプリケーション一時停止時**: `OnApplicationPause()` で自動保存（モバイル対応）
- **ゲーム終了ボタン**: `QuitGame()` 実行時に保存

#### 2. 自動ロード
- **ゲーム開始時**: `SaveManager.Start()` で自動ロード
- **Cloud Save + ローカルバックアップ**: Cloud Save失敗時はローカルから復元

#### 3. ログアウト時のセーブ
- **タイトル画面に戻る**: `ReturnToTitle()` 実行時に保存
- **メニューからのログアウト**: セーブ完了後にシーン遷移

### セーブデータ構造

```csharp
[System.Serializable]
public class SaveData
{
    // プレイヤー基本情報
    public int level;
    public int experience;
    
    // ステータス
    public float currentHealth;
    public float maxHealth;
    public float currentMana;
    public float maxMana;
    public float currentStamina;
    public float maxStamina;
    
    // 能力値
    public int strength;
    public int defense;
    public int magic;
    public int agility;
    public float criticalRate;
    
    // 位置情報
    public Vector3Data position;
    public Vector3Data rotation;
    public string currentScene;
    
    // メタデータ
    public string saveDate;
    public int saveVersion;
}
```

### セーブタイミング一覧

| タイミング | 実装場所 | 説明 |
|-----------|---------|------|
| 定期自動セーブ | `SaveManager.Update()` | 5分間隔 |
| アプリ終了時 | `SaveManager.OnApplicationQuit()` | 確実に保存 |
| アプリ一時停止時 | `SaveManager.OnApplicationPause()` | モバイル対応 |
| ゲーム終了ボタン | `MenuManager.QuitGame()` | 二重保険 |
| ログアウト | `MenuManager.ReturnToTitle()` | シーン遷移前 |
| 手動セーブ | F5キー（デバッグ） | テスト用 |

### ロードタイミング一覧

| タイミング | 実装場所 | 説明 |
|-----------|---------|------|
| ゲーム開始時 | `SaveManager.Start()` | UGS初期化後 |
| 手動ロード | F5キー（デバッグ） | テスト用 |

---

## Phase 3（次フェーズ） - MMO連携実装

### 実装予定機能

#### 1. サーバー連携ログアウト
```csharp
public async void Logout()
{
    // 1. ローカルセーブ
    await SaveManager.Instance.SaveGame();
    
    // 2. サーバーにログアウト通知
    await NetworkManager.Instance.SendLogoutRequest();
    
    // 3. オンライン状態を更新
    await UpdateOnlineStatus(false);
    
    // 4. パーティ/ギルドから離脱
    await LeavePartyAndGuild();
    
    // 5. タイトル画面に遷移
    SceneManager.LoadScene("TitleScene");
}
```

#### 2. サーバー連携ログイン
```csharp
public async Task<bool> Login(string username, string password)
{
    // 1. 認証
    var authResult = await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
    
    // 2. プレイヤーデータをサーバーから取得
    var serverData = await NetworkManager.Instance.FetchPlayerData();
    
    // 3. ローカルデータと比較
    var localData = await SaveManager.Instance.LoadGame();
    
    // 4. 新しい方を採用（タイムスタンプ比較）
    var finalData = CompareAndMerge(serverData, localData);
    
    // 5. データを適用
    await ApplyPlayerData(finalData);
    
    // 6. ゲームシーンに遷移
    SceneManager.LoadScene("GameScene");
    
    return true;
}
```

#### 3. キャラクター選択画面
```csharp
public async void SelectCharacter(int characterSlot)
{
    // 1. キャラクターデータをロード
    var characterData = await LoadCharacterData(characterSlot);
    
    // 2. データを適用
    await ApplyCharacterData(characterData);
    
    // 3. ゲームシーンに遷移
    SceneManager.LoadScene(characterData.lastScene);
}
```

#### 4. サーバー同期
- **定期同期**: 5分ごとにサーバーと同期
- **重要イベント時**: レベルアップ、アイテム取得時に即座に同期
- **競合解決**: タイムスタンプベースの競合解決

---

## Phase 4 - 拡張機能

### 実装予定機能

#### 1. 複数キャラクター対応
- キャラクタースロット管理（最大3キャラ）
- キャラクター削除機能
- キャラクター名変更機能

#### 2. バックアップ管理
- 自動バックアップ（最大3世代）
- 手動バックアップ作成
- バックアップからの復元

#### 3. データ移行
- セーブデータバージョン管理
- 旧バージョンからの自動移行
- データ整合性チェック

#### 4. クラウド同期強化
- オフライン時のキュー管理
- 再接続時の自動同期
- 競合解決UI

---

## セキュリティ対策

### Phase 2（現在）
- ✅ ローカルファイルの暗号化（JSON形式）
- ✅ Cloud Saveによるバックアップ
- ✅ セーブデータバージョン管理

### Phase 3（予定）
- 🔲 サーバー側でのデータ検証
- 🔲 チート検出（異常値チェック）
- 🔲 セーブデータの署名検証

### Phase 4（予定）
- 🔲 セーブデータの暗号化強化
- 🔲 改ざん検出機能
- 🔲 ロールバック機能

---

## エラーハンドリング

### セーブ失敗時
1. **Cloud Save失敗**: ローカルに保存（警告ログ）
2. **ローカル保存失敗**: エラーログ、ユーザーに通知
3. **ディスク容量不足**: 古いバックアップを削除して再試行

### ロード失敗時
1. **Cloud Save失敗**: ローカルから復元
2. **ローカル失敗**: デフォルト値で新規開始
3. **データ破損**: バックアップから復元

### ネットワークエラー時（Phase 3）
1. **ログアウト時**: ローカルセーブ後、オフライン状態で終了
2. **ログイン時**: ローカルデータで開始、再接続時に同期
3. **同期失敗**: キューに保存、次回接続時に再送

---

## テスト方法

### Phase 2テスト

#### 1. 自動セーブテスト
```
1. ゲームを開始
2. プレイヤーのHPを変更
3. 5分待機
4. Consoleで "[SaveManager] ゲームをセーブしました" を確認
```

#### 2. 自動ロードテスト
```
1. ゲームを開始
2. プレイヤーのHPを50に変更
3. F5キーで手動セーブ
4. ゲームを再起動
5. HPが50で復元されることを確認
```

#### 3. ログアウトセーブテスト
```
1. ゲームを開始
2. プレイヤーのレベルを5に変更
3. ESCキー → "タイトルに戻る"
4. Consoleで "[MenuManager] ログアウト前にセーブを実行" を確認
5. ゲームを再起動
6. レベル5で復元されることを確認
```

#### 4. アプリ終了テスト
```
1. ゲームを開始
2. プレイヤーの位置を移動
3. ゲームを終了（Alt+F4 or Quit）
4. Consoleで "[SaveManager] アプリケーション終了 - セーブ実行" を確認
5. ゲームを再起動
6. 同じ位置で復元されることを確認
```

### Phase 3テスト（予定）

#### 1. サーバー同期テスト
```
1. デバイスAでログイン、レベルアップ
2. ログアウト
3. デバイスBで同じアカウントでログイン
4. レベルが同期されていることを確認
```

#### 2. 競合解決テスト
```
1. オフライン状態でプレイ（ローカル変更）
2. サーバー側でデータを変更（管理ツール）
3. オンライン復帰
4. 新しいタイムスタンプのデータが採用されることを確認
```

---

## パフォーマンス考慮

### セーブ処理
- **非同期処理**: `async/await` で UI をブロックしない
- **JSON最適化**: 必要最小限のデータのみシリアライズ
- **バックグラウンド保存**: ゲームプレイを中断しない

### ロード処理
- **遅延ロード**: 必要なデータから順次ロード
- **キャッシュ**: 頻繁にアクセスするデータはメモリに保持
- **プログレス表示**: ロード中の進捗をユーザーに表示

---

## 実装チェックリスト

### Phase 2（現在） ✅
- [x] SaveManager基本実装
- [x] 自動セーブ（5分間隔）
- [x] アプリ終了時セーブ
- [x] アプリ一時停止時セーブ
- [x] ゲーム開始時ロード
- [x] ログアウト時セーブ
- [x] ゲーム終了時セーブ
- [x] Cloud Save連携
- [x] ローカルバックアップ
- [x] デバッグUI（F5キー）

### Phase 3（次フェーズ）
- [ ] サーバー連携ログアウト
- [ ] サーバー連携ログイン
- [ ] キャラクター選択画面
- [ ] サーバー定期同期
- [ ] 競合解決機能
- [ ] オンライン状態管理
- [ ] パーティ/ギルド離脱処理

### Phase 4（将来）
- [ ] 複数キャラクター対応
- [ ] バックアップ管理UI
- [ ] データ移行機能
- [ ] クラウド同期強化
- [ ] セキュリティ強化

---

## まとめ

### 現在の状態（Phase 2）
- ✅ **基本的なセーブ/ロード機能は完全実装済み**
- ✅ **ログアウト時のセーブも実装済み**
- ✅ **アプリ終了時の自動セーブも実装済み**

### 次のステップ（Phase 3）
- サーバー連携機能の実装
- MMO特有の機能（パーティ、ギルド）との連携
- オンライン状態管理

### 推奨事項
1. **Phase 2で十分**: 現在の実装で基本的なセーブ/ロードは完璧に動作
2. **Phase 3で拡張**: サーバー実装時にMMO連携機能を追加
3. **段階的実装**: 一度に全機能を実装せず、必要に応じて拡張

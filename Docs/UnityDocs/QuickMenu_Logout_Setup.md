# クイックメニュー ログアウト機能 セットアップガイド

## 概要
クイックメニューのオプションボタンから、設定とログアウトにアクセスできる機能を実装しました。

---

## 🚀 クイックスタート（自動生成）

### エディタースクリプトで自動作成

#### 標準作成
1. Unity エディタで **Tools → MMO → Create Quick Options Panel** を選択
2. 自動的に以下が作成されます:
   - QuickOptionsPanel（パネル本体）
   - Background（背景）
   - SettingsButton（設定ボタン）
   - LogoutButton（ログアウトボタン）
   - CloseButton（閉じるボタン）
   - QuickMenuController への自動接続
3. 完了！すぐにテスト可能です

#### カスタム位置で作成 ⭐ NEW
1. **Tools → MMO → Create Quick Options Panel (Custom Position)** を選択
2. 設定ウィンドウが表示されます:
   - **Position**: パネルの位置（X, Y）
   - **Size**: パネルのサイズ（幅, 高さ）
   - **Sorting Order**: 表示順序（大きいほど前面）
3. プリセットボタンで簡単設定:
   - **デフォルト**: 標準位置（-10, -80）
   - **右上（大きめ）**: 大きめのパネル（250x180）
   - **右上（小さめ）**: 小さめのパネル（180x120）
4. **パネルを作成** ボタンをクリック

**注意**: GothicHUD_Canvas/QuickMenu の下に作成されます

### ドラッグで位置調整 ⭐ NEW
1. **Tools → MMO → Position Quick Options Panel (Drag Mode)** を選択
2. Scene ビューで QuickOptionsPanel が選択される
3. Rect Tool (T キー) でドラッグして位置を調整
4. **Tools → MMO → Save Panel Position** で位置を保存
5. ゲームを開始して確認

**ヒント**:
- Shift を押しながらドラッグで微調整
- Inspector で数値入力も可能
- キャンセルする場合: **Tools → MMO → Cancel Panel Positioning**

### 手動接続（接続が外れた場合）
- **Tools → MMO → Connect Quick Options Panel** で再接続できます

### 削除方法
- **Tools → MMO → Remove Quick Options Panel** で削除できます

---

## 実装内容

### QuickMenuController.cs の拡張

#### 追加された機能
1. **クイックオプションメニュー**: オプションボタンをクリックすると表示される小さなメニュー
2. **設定ボタン**: MenuManagerの設定メニューを開く
3. **ログアウトボタン**: セーブ処理を実行してタイトル画面に戻る
4. **閉じるボタン**: クイックオプションメニューを閉じる

#### 新しいシリアライズフィールド
```csharp
[Header("Quick Options Menu")]
[SerializeField] private GameObject quickOptionsPanel;
[SerializeField] private Button settingsButton;
[SerializeField] private Button logoutButton;
[SerializeField] private Button closeOptionsButton;
```

---

## Unity エディタでのセットアップ

### 1. クイックオプションパネルの作成

#### Hierarchy構成
```
GothicHUD_Canvas
└── QuickMenu (既存)
    └── OptionsButton (既存)
    └── QuickOptionsPanel (新規作成)
        ├── Background (Image)
        ├── SettingsButton (Button)
        ├── LogoutButton (Button)
        └── CloseButton (Button)
```

**注意**: 
- 実際のプロジェクトでは `GothicHUD_Canvas` を使用しています
- `QuickMenuController` は `GothicHUD_Canvas` または `QuickMenu` にアタッチされている必要があります

### 2. QuickOptionsPanel の作成手順

#### Step 1: パネル作成
1. `QuickMenu` の下に空のGameObjectを作成
2. 名前を `QuickOptionsPanel` に変更
3. `RectTransform` を設定:
   - **Anchor**: Top Right
   - **Position**: オプションボタンの近く（例: X=-100, Y=-50）
   - **Size**: Width=200, Height=150
4. `Canvas` コンポーネントを追加:
   - **Override Sorting**: ✓ チェック
   - **Sorting Order**: 1000（最前面に表示）
5. `GraphicRaycaster` コンポーネントを追加（クリック可能にする）

#### Step 2: Background 追加
1. `QuickOptionsPanel` の下に `Image` を追加
2. 名前を `Background` に変更
3. 設定:
   - **Color**: 半透明の黒 (R:0, G:0, B:0, A:200)
   - **Stretch to Fill Parent**: Anchor Presets → Stretch (Alt+Shift+クリック)

#### Step 3: SettingsButton 作成
1. `QuickOptionsPanel` の下に `Button` を追加
2. 名前を `SettingsButton` に変更
3. 設定:
   - **Position**: X=0, Y=40
   - **Size**: Width=180, Height=30
4. Text設定:
   - **Text**: "設定 (Settings)"
   - **Font Size**: 16
   - **Alignment**: Center

#### Step 4: LogoutButton 作成
1. `QuickOptionsPanel` の下に `Button` を追加
2. 名前を `LogoutButton` に変更
3. 設定:
   - **Position**: X=0, Y=0
   - **Size**: Width=180, Height=30
4. Text設定:
   - **Text**: "ログアウト (Logout)"
   - **Font Size**: 16
   - **Alignment**: Center
   - **Color**: 赤系 (R:255, G:100, B:100) - 警告色

#### Step 5: CloseButton 作成
1. `QuickOptionsPanel` の下に `Button` を追加
2. 名前を `CloseButton` に変更
3. 設定:
   - **Position**: X=0, Y=-40
   - **Size**: Width=180, Height=30
4. Text設定:
   - **Text**: "閉じる (Close)"
   - **Font Size**: 14
   - **Alignment**: Center

### 3. QuickMenuController への接続

1. `QuickMenuController` コンポーネントを選択
2. Inspector で以下を設定:

#### Quick Options Menu セクション
- **Quick Options Panel**: 作成した `QuickOptionsPanel` をドラッグ
- **Settings Button**: `SettingsButton` をドラッグ
- **Logout Button**: `LogoutButton` をドラッグ
- **Close Options Button**: `CloseButton` をドラッグ

---

## 動作フロー

### オプションボタンをクリック
```
1. ユーザーがオプションボタンをクリック
   ↓
2. QuickOptionsPanel が表示される
   ↓
3. カーソルが表示される
```

### 設定ボタンをクリック
```
1. ユーザーが設定ボタンをクリック
   ↓
2. QuickOptionsPanel が閉じる
   ↓
3. MenuManager.OpenSettingsMenu() が呼ばれる
   ↓
4. 設定メニューが表示される
```

### ログアウトボタンをクリック
```
1. ユーザーがログアウトボタンをクリック
   ↓
2. QuickOptionsPanel が閉じる
   ↓
3. SaveManager.SaveGame() が実行される（自動セーブ）
   ↓
4. MenuManager.ReturnToTitle() が呼ばれる
   ↓
5. タイトルシーンに遷移
```

### 閉じるボタンをクリック
```
1. ユーザーが閉じるボタンをクリック
   ↓
2. QuickOptionsPanel が閉じる
   ↓
3. カーソルが非表示になる
```

---

## コード詳細

### ToggleQuickOptions()
```csharp
public void ToggleQuickOptions()
{
    if (quickOptionsPanel != null)
    {
        bool isActive = quickOptionsPanel.activeSelf;
        quickOptionsPanel.SetActive(!isActive);
        
        if (!isActive)
        {
            // 開く時: カーソル表示
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // 閉じる時: カーソル非表示
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
```

### OpenSettings()
```csharp
public void OpenSettings()
{
    // クイックオプションメニューを閉じる
    CloseQuickOptions();
    
    // MenuManagerの設定メニューを開く
    MenuManager menuManager = FindFirstObjectByType<MenuManager>();
    if (menuManager != null)
    {
        menuManager.OpenSettingsMenu();
    }
}
```

### Logout()
```csharp
public async void Logout()
{
    // クイックオプションメニューを閉じる
    CloseQuickOptions();
    
    // MenuManagerのログアウト処理を実行
    MenuManager menuManager = FindFirstObjectByType<MenuManager>();
    if (menuManager != null)
    {
        // MenuManager.ReturnToTitle() が自動的にセーブを実行
        menuManager.ReturnToTitle();
    }
    else
    {
        // フォールバック: 直接セーブ&ログアウト
        var saveManager = Save.SaveManager.Instance;
        if (saveManager != null)
        {
            await saveManager.SaveGame();
        }
        SceneManager.LoadScene(0);
    }
}
```

---

## テスト方法

### 1. クイックオプションメニュー表示テスト
```
1. ゲームを開始
2. オプションボタンをクリック
3. クイックオプションメニューが表示されることを確認
4. カーソルが表示されることを確認
```

### 2. 設定ボタンテスト
```
1. オプションボタンをクリック
2. 設定ボタンをクリック
3. クイックオプションメニューが閉じることを確認
4. 設定メニューが開くことを確認
```

### 3. ログアウトボタンテスト
```
1. ゲームを開始
2. プレイヤーのレベルを変更（テスト用）
3. オプションボタンをクリック
4. ログアウトボタンをクリック
5. Consoleで "[MenuManager] ログアウト前にセーブを実行" を確認
6. タイトルシーンに遷移することを確認
7. ゲームを再起動
8. レベルが保存されていることを確認
```

### 4. 閉じるボタンテスト
```
1. オプションボタンをクリック
2. 閉じるボタンをクリック
3. クイックオプションメニューが閉じることを確認
4. カーソルが非表示になることを確認
```

---

## UI デザイン推奨事項

### カラースキーム
- **背景**: 半透明の黒 (R:0, G:0, B:0, A:200)
- **設定ボタン**: 通常色 (白またはゴールド)
- **ログアウトボタン**: 警告色 (赤系 R:255, G:100, B:100)
- **閉じるボタン**: グレー系

### レイアウト
```
┌─────────────────────┐
│  QuickOptionsPanel  │
├─────────────────────┤
│  [設定 (Settings)]  │
│                     │
│ [ログアウト (Logout)]│
│                     │
│  [閉じる (Close)]   │
└─────────────────────┘
```

### アニメーション（オプション）
- **フェードイン**: パネル表示時に透明度アニメーション
- **スケールアニメーション**: 0.9 → 1.0 のスケールアニメーション
- **ボタンホバー**: ボタンにマウスオーバー時の色変化

---

## トラブルシューティング

### Q1: ログアウトボタンが動作しない
**A**: 以下を確認してください:
1. `MenuManager` がシーンに存在するか
2. `SaveManager` がシーンに存在するか（DontDestroyOnLoad）
3. Consoleにエラーが出ていないか

### Q2: クイックオプションメニューが表示されない
**A**: 以下を確認してください:
1. `quickOptionsPanel` が正しくアサインされているか
   - **Tools → MMO → Connect Quick Options Panel** で再接続
2. `QuickOptionsPanel` の初期状態が非表示（Inactive）になっているか
3. Canvas の Render Mode が正しく設定されているか

### Q5: "Quick Options Panel is not assigned!" エラーが出る
**A**: QuickMenuController への接続が外れています:
1. **Tools → MMO → Connect Quick Options Panel** を実行
2. または、手動で QuickMenuController の Inspector で接続
   - Quick Options Panel → QuickOptionsPanel をドラッグ
   - Settings Button → SettingsButton をドラッグ
   - Logout Button → LogoutButton をドラッグ
   - Close Options Button → CloseButton をドラッグ

### Q3: カーソルが表示されない
**A**: 以下を確認してください:
1. 他のスクリプトがカーソルを制御していないか
2. `Cursor.lockState` が正しく設定されているか
3. Input System の設定が正しいか

### Q4: セーブが実行されない
**A**: 以下を確認してください:
1. `SaveManager.Instance` が null でないか
2. Cloud Save が正しく初期化されているか
3. `SceneBootstrap` が存在し、UGS が初期化されているか

---

## 関連ドキュメント

- **MenuManager.cs**: ポーズメニュー・設定メニュー管理
- **SaveManager.cs**: セーブ/ロード管理
- **SaveLoad_Implementation_Plan.md**: セーブ/ロード実装計画
- **Phase2_Implementation_Status.md**: Phase 2実装状況

---

## 今後の拡張予定

### Phase 3
- **キャラクター切り替え**: ログアウト前にキャラクター選択画面を表示
- **確認ダイアログ**: ログアウト前に確認ダイアログを表示
- **サーバー通知**: ログアウト時にサーバーに通知

### Phase 4
- **クイックアクセス拡張**: よく使う機能をクイックメニューに追加
- **カスタマイズ**: ユーザーがボタンをカスタマイズ可能に
- **アニメーション強化**: より洗練されたUIアニメーション

---

## まとめ

✅ **実装完了項目**
- クイックオプションメニューの表示/非表示
- 設定メニューへの遷移
- ログアウト機能（自動セーブ付き）
- カーソル表示制御

🎯 **使用方法**
1. オプションボタンをクリック
2. クイックオプションメニューから選択
   - **設定**: ゲーム設定を変更
   - **ログアウト**: セーブしてタイトル画面に戻る
   - **閉じる**: メニューを閉じる

📝 **注意事項**
- ログアウト時は自動的にセーブされます
- タイトルシーンが存在しない場合は最初のシーンに戻ります
- MenuManagerが見つからない場合はフォールバック処理が実行されます

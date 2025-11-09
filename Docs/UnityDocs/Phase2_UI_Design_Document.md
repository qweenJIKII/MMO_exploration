# Phase 2: UI/Settings 設計書（ファンタジーMMOスタイル）

ドキュメント種別: Design Document  
対象プロジェクト: MMO_exploration (Unity)  
版数: v1.0  
作成日: 2025-11-01  
参照元: `AAA Game Screen` React UIコンポーネント  
参照: `Implementation_Roadmap.md`

---

## 概要

`AAA Game Screen`のReact/TypeScript UIデザインをUnity UI Toolkitに移植し、ファンタジーMMOスタイルのHUD・メニュー・設定システムを実装します。

### デザインコンセプト
- **ファンタジーテーマ**: 琥珀色（Amber）とゴールドのアクセント
- **装飾的なフレーム**: 角に装飾、境界線
- **半透明レイヤー**: Stone-900/950背景
- **グロー効果**: 琥珀色の発光
- **アニメーション**: スムーズなトランジション

---

## 1. UI実装方式

### 推奨: Unity UI Toolkit
- CSS-likeなスタイリング（USS）
- コンポーネント指向
- パフォーマンス優位
- Unity 6.2で成熟

---

## 2. カラーパレット

```csharp
// Assets/Scripts/Core/UI/Theme/UIColorPalette.cs
public static class UIColorPalette
{
    // Amber/Gold
    public static readonly Color Amber300 = new Color(0.988f, 0.835f, 0.502f, 1f);
    public static readonly Color Amber600 = new Color(0.851f, 0.467f, 0.024f, 1f);
    public static readonly Color Amber800 = new Color(0.573f, 0.251f, 0.055f, 1f);
    
    // Stone
    public static readonly Color Stone900 = new Color(0.110f, 0.098f, 0.090f, 1f);
    public static readonly Color Stone950 = new Color(0.051f, 0.047f, 0.043f, 1f);
    
    // Health/Mana
    public static readonly Color Red600 = new Color(0.863f, 0.149f, 0.149f, 1f);
    public static readonly Color Blue600 = new Color(0.145f, 0.408f, 0.878f, 1f);
}
```

---

## 3. コンポーネント一覧

### 実装優先度

| コンポーネント | 説明 | 優先度 | 工数 |
|------------|------|-------|------|
| FantasyPlayerFrame | HP/MP Orb + スキルバー | 高 | 2日 |
| FantasyPartyFrames | パーティメンバー表示 | 高 | 1日 |
| HUDManager | 統合管理 | 高 | 1日 |
| FantasyMiniMap | ミニマップ | 中 | 1.5日 |
| FantasyQuestTracker | クエスト進捗 | 中 | 1日 |
| FantasyChatWindow | チャット | 中 | 1日 |
| FantasyCurrency | 通貨表示 | 低 | 0.5日 |
| FantasyBuffBar | バフ/デバフ | 低 | 0.5日 |
| FantasyTargetFrame | ターゲット情報 | 低 | 1日 |

---

## 4. USS スタイル定義

```css
/* Assets/UI/Styles/FantasyTheme.uss */

/* 共通フレーム */
.fantasy-frame {
    background-color: rgba(28, 25, 23, 0.9);
    border-color: rgba(217, 119, 6, 0.5);
    border-width: 2px;
    border-radius: 12px;
    padding: 12px;
}

/* HP Orb */
.health-orb {
    width: 128px;
    height: 128px;
    border-radius: 50%;
    background-color: rgba(26, 10, 10, 1);
    border-color: rgba(217, 119, 6, 0.6);
    border-width: 2px;
}

.health-orb-fill {
    background-color: rgb(220, 38, 38);
    border-radius: 50%;
}

/* MP Orb */
.mana-orb {
    width: 128px;
    height: 128px;
    border-radius: 50%;
    background-color: rgba(10, 10, 26, 1);
    border-color: rgba(217, 119, 6, 0.6);
    border-width: 2px;
}

.mana-orb-fill {
    background-color: rgb(37, 99, 235);
    border-radius: 50%;
}

/* スキルスロット */
.skill-slot {
    width: 56px;
    height: 56px;
    background-color: rgba(41, 37, 36, 1);
    border-color: rgba(120, 48, 13, 0.6);
    border-width: 2px;
    border-radius: 8px;
    margin: 4px;
}

.skill-hotkey {
    position: absolute;
    top: -8px;
    right: -8px;
    width: 24px;
    height: 24px;
    background-color: rgb(217, 119, 6);
    border-color: rgb(120, 48, 13);
    border-width: 2px;
    border-radius: 50%;
    -unity-text-align: middle-center;
    color: rgb(255, 254, 240);
    font-size: 12px;
}

/* プログレスバー */
.progress-bar-container {
    height: 12px;
    background-color: rgba(13, 12, 11, 0.6);
    border-color: rgba(41, 37, 36, 1);
    border-width: 1px;
    border-radius: 4px;
    overflow: hidden;
}

.progress-bar-fill {
    height: 100%;
    transition-property: width;
    transition-duration: 0.5s;
}

.health-bar {
    background-color: rgb(220, 38, 38);
}

.mana-bar {
    background-color: rgb(37, 99, 235);
}

/* ミニマップ */
.minimap-container {
    width: 224px;
    height: 192px;
}

.minimap-display {
    background-color: rgba(13, 12, 11, 1);
    border-color: rgba(120, 48, 13, 0.4);
    border-width: 2px;
    border-radius: 8px;
}

.minimap-icon-player-local {
    width: 12px;
    height: 12px;
    background-color: rgb(251, 191, 36);
    border-radius: 50%;
}

.minimap-icon-player {
    width: 8px;
    height: 8px;
    background-color: rgb(74, 222, 128);
    border-radius: 50%;
}

.minimap-icon-enemy {
    width: 8px;
    height: 8px;
    background-color: rgb(239, 68, 68);
    border-radius: 4px;
}

.minimap-icon-enemy-elite {
    width: 8px;
    height: 8px;
    background-color: rgb(168, 85, 247);
    border-radius: 4px;
}

/* クエストトラッカー */
.quest-entry {
    background-color: rgba(13, 12, 11, 0.4);
    border-color: rgba(120, 48, 13, 0.3);
    border-width: 1px;
    border-radius: 8px;
    padding: 10px;
    margin-bottom: 12px;
}

.quest-title {
    color: rgb(252, 211, 77);
    font-size: 14px;
    margin-bottom: 8px;
}

.quest-objective-text {
    color: rgb(214, 211, 209);
    font-size: 12px;
}

.quest-objective-text.completed {
    color: rgb(120, 113, 108);
    text-decoration: line-through;
}

/* チャットウィンドウ */
.chat-window {
    width: 320px;
}

.chat-header {
    background-color: rgba(120, 48, 13, 0.1);
    border-bottom-color: rgba(120, 48, 13, 0.3);
    border-bottom-width: 1px;
    padding: 12px;
    cursor: pointer;
}

.chat-messages {
    max-height: 128px;
    background-color: rgba(13, 12, 11, 0.4);
    padding: 8px;
}

.chat-input {
    background-color: rgba(13, 12, 11, 0.6);
    border-color: rgba(120, 48, 13, 0.4);
    border-width: 1px;
    border-radius: 4px;
    padding: 6px 12px;
    color: rgb(214, 211, 209);
}
```

---

## 5. UXML レイアウト例

```xml
<!-- Assets/UI/UXML/GameHUD.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="game-hud" class="game-hud-root">
        
        <!-- Top Left: Party Frames -->
        <ui:VisualElement name="party-frames" class="top-left">
            <ui:VisualElement name="player-party-frame" class="fantasy-frame" />
        </ui:VisualElement>
        
        <!-- Top Right: Currency & Minimap -->
        <ui:VisualElement name="top-right-container" class="top-right">
            <ui:VisualElement name="currency" class="fantasy-frame" />
            <ui:VisualElement name="minimap" class="fantasy-frame minimap-container" />
        </ui:VisualElement>
        
        <!-- Left: Quest Tracker -->
        <ui:VisualElement name="quest-tracker" class="left-side fantasy-frame" />
        
        <!-- Bottom Left: Chat -->
        <ui:VisualElement name="chat-window" class="bottom-left fantasy-frame chat-window" />
        
        <!-- Bottom Center: Player Frame -->
        <ui:VisualElement name="player-frame" class="bottom-center">
            <ui:VisualElement name="health-orb" class="health-orb" />
            <ui:VisualElement name="skill-bar" class="skill-bar-container" />
            <ui:VisualElement name="mana-orb" class="mana-orb" />
        </ui:VisualElement>
        
        <!-- Top Center: Target Frame -->
        <ui:VisualElement name="target-frame" class="top-center fantasy-frame" />
        
        <!-- Top Center: Buff Bar -->
        <ui:VisualElement name="buff-bar" class="top-center-buffs" />
        
    </ui:VisualElement>
</ui:UXML>
```

---

## 6. 実装手順

### Week 1
1. **Day 1-2**: UIColorPalette、USS基本スタイル作成
2. **Day 3-4**: FantasyPlayerFrame実装（HP/MP Orb）
3. **Day 5**: HUDManager統合、PlayerStats連携

### Week 2
1. **Day 1**: FantasyPartyFrames実装
2. **Day 2**: FantasyMiniMap実装
3. **Day 3**: FantasyQuestTracker実装
4. **Day 4**: FantasyChatWindow実装
5. **Day 5**: 統合テスト、調整

---

## 7. 次のステップ

- **Phase 3**: ミニマップカメラ実装
- **Phase 4**: クエストシステム統合
- **Phase 5**: VFX/Audio統合（スキルエフェクト）

---

## 参考リンク

- React UI: `g:\Unity\MMO_exploration\AAA Game Screen\src\components\`
- Unity UI Toolkit: https://docs.unity3d.com/Manual/UIElements.html
- USS Reference: https://docs.unity3d.com/Manual/UIE-USS.html

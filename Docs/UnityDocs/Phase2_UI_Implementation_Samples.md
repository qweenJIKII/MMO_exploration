# Phase 2: UI実装サンプルコード

参照: `Phase2_UI_Design_Document.md`

---

## 1. HUDManager（統合管理）

```csharp
// Assets/Scripts/Core/UI/HUDManager.cs
using UnityEngine;
using UnityEngine.UIElements;
using MMOExploration.Core.Player;

namespace MMOExploration.Core.UI
{
    public class HUDManager : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private ExperienceManager experienceManager;

        private VisualElement root;
        private FantasyPlayerFrame playerFrame;
        private FantasyPartyFrames partyFrames;

        private void Awake()
        {
            root = uiDocument.rootVisualElement;
            InitializeComponents();
            BindEvents();
        }

        private void InitializeComponents()
        {
            playerFrame = new FantasyPlayerFrame(root.Q("player-frame"));
            partyFrames = new FantasyPartyFrames(root.Q("party-frames"));
        }

        private void BindEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnHealthChanged += (current, max) => 
                    playerFrame?.UpdateHealth(current, max);
                playerStats.OnManaChanged += (current, max) => 
                    playerFrame?.UpdateMana(current, max);
            }

            if (experienceManager != null)
            {
                experienceManager.OnLevelUp += (level) => 
                    playerFrame?.SetLevel(level);
            }
        }
    }
}
```

---

## 2. FantasyPlayerFrame

```csharp
// Assets/Scripts/Core/UI/Components/FantasyPlayerFrame.cs
using UnityEngine;
using UnityEngine.UIElements;

namespace MMOExploration.Core.UI.Components
{
    public class FantasyPlayerFrame
    {
        private readonly VisualElement root;
        private readonly VisualElement healthOrbFill;
        private readonly VisualElement manaOrbFill;
        private readonly Label healthText;
        private readonly Label manaText;
        private readonly Label levelText;

        public FantasyPlayerFrame(VisualElement root)
        {
            this.root = root;
            healthOrbFill = root.Q("health-orb-fill");
            manaOrbFill = root.Q("mana-orb-fill");
            healthText = root.Q<Label>("health-text");
            manaText = root.Q<Label>("mana-text");
            levelText = root.Q<Label>("level-text");
        }

        public void UpdateHealth(float current, float max)
        {
            float percent = Mathf.Clamp01(current / max);
            
            // Orbの塗りつぶし（下から上へ）
            healthOrbFill.style.height = Length.Percent(percent * 100f);
            healthText.text = $"{current:N0}";
            
            // アニメーション
            AnimateValue(healthOrbFill, "height", percent * 100f, 500);
        }

        public void UpdateMana(float current, float max)
        {
            float percent = Mathf.Clamp01(current / max);
            
            manaOrbFill.style.height = Length.Percent(percent * 100f);
            manaText.text = $"{current:N0}";
            
            AnimateValue(manaOrbFill, "height", percent * 100f, 500);
        }

        public void SetLevel(int level)
        {
            levelText.text = level.ToString();
            AnimateLevelUp();
        }

        private void AnimateValue(VisualElement element, string property, float targetValue, int durationMs)
        {
            // UI Toolkit Transitionを使用
            element.style.transitionProperty = new StyleList<StylePropertyName>(
                new StylePropertyName[] { new StylePropertyName(property) }
            );
            element.style.transitionDuration = new StyleList<TimeValue>(
                new TimeValue[] { new TimeValue(durationMs / 1000f) }
            );
        }

        private void AnimateLevelUp()
        {
            // スケールアニメーション
            levelText.style.scale = new Scale(new Vector3(1.5f, 1.5f, 1f));
            levelText.schedule.Execute(() => {
                levelText.style.scale = new Scale(Vector3.one);
            }).StartingIn(300);
        }
    }
}
```

---

## 3. FantasyMiniMap

```csharp
// Assets/Scripts/Core/UI/Components/FantasyMiniMap.cs
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MMOExploration.Core.UI.Components
{
    public class FantasyMiniMap
    {
        private readonly VisualElement root;
        private readonly VisualElement mapDisplay;
        private readonly Label locationLabel;
        private readonly Dictionary<string, VisualElement> icons;

        public FantasyMiniMap(VisualElement root)
        {
            this.root = root;
            mapDisplay = root.Q("map-display");
            locationLabel = root.Q<Label>("location-label");
            icons = new Dictionary<string, VisualElement>();
        }

        public void SetLocation(string locationName)
        {
            locationLabel.text = locationName;
        }

        public void UpdateIcon(string id, Vector2 normalizedPosition, IconType type)
        {
            if (!icons.TryGetValue(id, out var icon))
            {
                icon = CreateIcon(type);
                mapDisplay.Add(icon);
                icons[id] = icon;
            }

            // 正規化座標（0-1）をパーセントに変換
            icon.style.left = Length.Percent(normalizedPosition.x * 100f);
            icon.style.top = Length.Percent(normalizedPosition.y * 100f);
        }

        public void RemoveIcon(string id)
        {
            if (icons.TryGetValue(id, out var icon))
            {
                mapDisplay.Remove(icon);
                icons.Remove(id);
            }
        }

        private VisualElement CreateIcon(IconType type)
        {
            var icon = new VisualElement();
            icon.style.position = Position.Absolute;
            
            switch (type)
            {
                case IconType.PlayerLocal:
                    icon.AddToClassList("minimap-icon-player-local");
                    break;
                case IconType.Player:
                    icon.AddToClassList("minimap-icon-player");
                    break;
                case IconType.Enemy:
                    icon.AddToClassList("minimap-icon-enemy");
                    break;
                case IconType.EnemyElite:
                    icon.AddToClassList("minimap-icon-enemy-elite");
                    break;
            }
            
            return icon;
        }

        public enum IconType
        {
            PlayerLocal,
            Player,
            Enemy,
            EnemyElite
        }
    }
}
```

---

## 4. FantasyQuestTracker

```csharp
// Assets/Scripts/Core/UI/Components/FantasyQuestTracker.cs
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MMOExploration.Core.UI.Components
{
    public class FantasyQuestTracker
    {
        private readonly VisualElement root;
        private readonly VisualElement questList;
        private readonly Dictionary<string, QuestEntry> quests;

        public FantasyQuestTracker(VisualElement root)
        {
            this.root = root;
            questList = root.Q("quest-list");
            quests = new Dictionary<string, QuestEntry>();
        }

        public void AddQuest(string questId, string title)
        {
            if (quests.ContainsKey(questId))
                return;

            var entry = new QuestEntry(title);
            questList.Add(entry.Root);
            quests[questId] = entry;
        }

        public void AddObjective(string questId, string text, int current, int total)
        {
            if (quests.TryGetValue(questId, out var entry))
            {
                entry.AddObjective(text, current, total);
            }
        }

        public void UpdateObjective(string questId, int index, int current, int total)
        {
            if (quests.TryGetValue(questId, out var entry))
            {
                entry.UpdateObjective(index, current, total);
            }
        }

        public void CompleteObjective(string questId, int index)
        {
            if (quests.TryGetValue(questId, out var entry))
            {
                entry.CompleteObjective(index);
            }
        }

        public void RemoveQuest(string questId)
        {
            if (quests.TryGetValue(questId, out var entry))
            {
                questList.Remove(entry.Root);
                quests.Remove(questId);
            }
        }

        private class QuestEntry
        {
            public VisualElement Root { get; }
            private readonly VisualElement objectivesContainer;
            private readonly List<ObjectiveEntry> objectives;

            public QuestEntry(string title)
            {
                Root = new VisualElement();
                Root.AddToClassList("quest-entry");

                var titleLabel = new Label(title);
                titleLabel.AddToClassList("quest-title");
                Root.Add(titleLabel);

                objectivesContainer = new VisualElement();
                objectivesContainer.AddToClassList("quest-objectives");
                Root.Add(objectivesContainer);

                objectives = new List<ObjectiveEntry>();
            }

            public void AddObjective(string text, int current, int total)
            {
                var obj = new ObjectiveEntry(text, current, total);
                objectivesContainer.Add(obj.Root);
                objectives.Add(obj);
            }

            public void UpdateObjective(int index, int current, int total)
            {
                if (index >= 0 && index < objectives.Count)
                {
                    objectives[index].UpdateProgress(current, total);
                }
            }

            public void CompleteObjective(int index)
            {
                if (index >= 0 && index < objectives.Count)
                {
                    objectives[index].Complete();
                }
            }
        }

        private class ObjectiveEntry
        {
            public VisualElement Root { get; }
            private readonly VisualElement checkbox;
            private readonly Label textLabel;
            private readonly VisualElement progressBarFill;
            private readonly Label progressLabel;
            private bool isCompleted;

            public ObjectiveEntry(string text, int current, int total)
            {
                Root = new VisualElement();
                Root.AddToClassList("quest-objective");

                checkbox = new VisualElement();
                checkbox.AddToClassList("quest-checkbox");
                Root.Add(checkbox);

                textLabel = new Label(text);
                textLabel.AddToClassList("quest-objective-text");
                Root.Add(textLabel);

                if (total > 1)
                {
                    var progressContainer = new VisualElement();
                    progressContainer.AddToClassList("progress-bar-container");

                    progressBarFill = new VisualElement();
                    progressBarFill.AddToClassList("progress-bar-fill");
                    progressContainer.Add(progressBarFill);

                    progressLabel = new Label($"{current}/{total}");
                    progressLabel.AddToClassList("quest-progress-label");
                    Root.Add(progressLabel);

                    Root.Add(progressContainer);

                    UpdateProgress(current, total);
                }
            }

            public void UpdateProgress(int current, int total)
            {
                if (progressBarFill != null)
                {
                    float percent = Mathf.Clamp01((float)current / total);
                    progressBarFill.style.width = Length.Percent(percent * 100f);
                    progressLabel.text = $"{current}/{total}";

                    if (current >= total && !isCompleted)
                    {
                        Complete();
                    }
                }
            }

            public void Complete()
            {
                isCompleted = true;
                checkbox.AddToClassList("checked");
                textLabel.AddToClassList("completed");
            }
        }
    }
}
```

---

## 5. FantasyChatWindow

```csharp
// Assets/Scripts/Core/UI/Components/FantasyChatWindow.cs
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MMOExploration.Core.UI.Components
{
    public class FantasyChatWindow
    {
        private readonly VisualElement root;
        private readonly VisualElement header;
        private readonly ScrollView messagesScroll;
        private readonly VisualElement messagesContainer;
        private readonly TextField inputField;
        private bool isExpanded = true;

        public FantasyChatWindow(VisualElement root)
        {
            this.root = root;
            header = root.Q("chat-header");
            messagesScroll = root.Q<ScrollView>("chat-messages-scroll");
            messagesContainer = messagesScroll.contentContainer;
            inputField = root.Q<TextField>("chat-input");

            header.RegisterCallback<ClickEvent>(evt => ToggleExpand());
            inputField.RegisterCallback<KeyDownEvent>(OnInputKeyDown);
        }

        public void AddMessage(string sender, string message, ChatChannel channel)
        {
            var msgElement = new VisualElement();
            msgElement.AddToClassList("chat-message");

            var senderLabel = new Label(sender);
            senderLabel.AddToClassList(GetChannelClass(channel));
            msgElement.Add(senderLabel);

            var separatorLabel = new Label(": ");
            separatorLabel.AddToClassList("chat-separator");
            msgElement.Add(separatorLabel);

            var messageLabel = new Label(message);
            messageLabel.AddToClassList("chat-message-text");
            msgElement.Add(messageLabel);

            messagesContainer.Add(msgElement);

            // 自動スクロール
            messagesScroll.schedule.Execute(() => {
                messagesScroll.scrollOffset = new Vector2(0, messagesScroll.contentContainer.layout.height);
            }).StartingIn(50);
        }

        private void ToggleExpand()
        {
            isExpanded = !isExpanded;
            messagesScroll.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            inputField.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                string message = inputField.value;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    SendMessage(message);
                    inputField.value = string.Empty;
                }
                evt.StopPropagation();
            }
        }

        private void SendMessage(string message)
        {
            // TODO: ChatManagerに送信
            Debug.Log($"[Chat] {message}");
        }

        private string GetChannelClass(ChatChannel channel)
        {
            return channel switch
            {
                ChatChannel.System => "chat-sender-system",
                ChatChannel.Party => "chat-sender-party",
                ChatChannel.Guild => "chat-sender-guild",
                _ => "chat-sender-default"
            };
        }

        public enum ChatChannel
        {
            Global,
            Party,
            Guild,
            Whisper,
            System
        }
    }
}
```

---

## 6. 使用例

```csharp
// Assets/Scripts/Core/UI/HUDController.cs
using UnityEngine;
using MMOExploration.Core.UI.Components;

public class HUDController : MonoBehaviour
{
    private FantasyPlayerFrame playerFrame;
    private FantasyMiniMap miniMap;
    private FantasyQuestTracker questTracker;
    private FantasyChatWindow chatWindow;

    private void Start()
    {
        // テストデータ
        TestPlayerFrame();
        TestMiniMap();
        TestQuestTracker();
        TestChatWindow();
    }

    private void TestPlayerFrame()
    {
        playerFrame.UpdateHealth(8750, 10500);
        playerFrame.UpdateMana(3200, 4500);
        playerFrame.SetLevel(47);
    }

    private void TestMiniMap()
    {
        miniMap.SetLocation("Darkwood Forest");
        miniMap.UpdateIcon("player", new Vector2(0.5f, 0.5f), FantasyMiniMap.IconType.PlayerLocal);
        miniMap.UpdateIcon("enemy1", new Vector2(0.6f, 0.25f), FantasyMiniMap.IconType.EnemyElite);
    }

    private void TestQuestTracker()
    {
        questTracker.AddQuest("quest1", "Slay the Ancient Wyrm");
        questTracker.AddObjective("quest1", "Find dragon's lair", 1, 1);
        questTracker.CompleteObjective("quest1", 0);
        questTracker.AddObjective("quest1", "Defeat Ancient Wyrm", 0, 1);

        questTracker.AddQuest("quest2", "Gather Moonflowers");
        questTracker.AddObjective("quest2", "Collect Ancient Moonflowers", 7, 10);
    }

    private void TestChatWindow()
    {
        chatWindow.AddMessage("Phoenix", "Ready for the dragon!", FantasyChatWindow.ChatChannel.Party);
        chatWindow.AddMessage("System", "Quest updated", FantasyChatWindow.ChatChannel.System);
        chatWindow.AddMessage("You", "Let's do this!", FantasyChatWindow.ChatChannel.Party);
    }
}
```

---

## 7. 次のステップ

1. **USS完成**: 全スタイル定義
2. **UXML作成**: レイアウト構築
3. **アニメーション**: Transition/Schedule活用
4. **統合テスト**: PlayerStats/ExperienceManager連携
5. **最適化**: パフォーマンス測定

---

## 参考

- UI Toolkit Manual: https://docs.unity3d.com/Manual/UIElements.html
- USS Properties: https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html
- Transitions: https://docs.unity3d.com/Manual/UIE-Transitions.html

// FantasyQuestTracker: クエスト進捗表示
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Project.Core.UI.Components
{
    /// <summary>
    /// クエストトラッカー
    /// Phase 2実装: AAA Game Screen FantasyQuestTracker.tsxを参考
    /// </summary>
    public class FantasyQuestTracker
    {
        private readonly VisualElement root;
        private readonly VisualElement questList;
        private readonly Dictionary<string, QuestEntry> quests;

        public FantasyQuestTracker(VisualElement root)
        {
            this.root = root;

            if (root == null)
            {
                Debug.LogError("[FantasyQuestTracker] Root element is null!");
                return;
            }

            questList = root.Q("quest-list");
            quests = new Dictionary<string, QuestEntry>();
        }

        /// <summary>
        /// クエストを追加
        /// </summary>
        public void AddQuest(string questId, string title)
        {
            if (quests.ContainsKey(questId) || questList == null)
                return;

            var entry = new QuestEntry(title);
            questList.Add(entry.Root);
            quests[questId] = entry;
        }

        /// <summary>
        /// 目標を追加
        /// </summary>
        public void AddObjective(string questId, string text, int current, int total)
        {
            if (quests.TryGetValue(questId, out var entry))
            {
                entry.AddObjective(text, current, total);
            }
        }

        /// <summary>
        /// 目標の進捗を更新
        /// </summary>
        public void UpdateObjective(string questId, int index, int current, int total)
        {
            if (quests.TryGetValue(questId, out var entry))
            {
                entry.UpdateObjective(index, current, total);
            }
        }

        /// <summary>
        /// 目標を完了
        /// </summary>
        public void CompleteObjective(string questId, int index)
        {
            if (quests.TryGetValue(questId, out var entry))
            {
                entry.CompleteObjective(index);
            }
        }

        /// <summary>
        /// クエストを削除
        /// </summary>
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
                    progressContainer.AddToClassList("quest-progress-container");

                    var progressBar = new VisualElement();
                    progressBar.AddToClassList("quest-progress-bar");

                    progressBarFill = new VisualElement();
                    progressBarFill.AddToClassList("quest-progress-fill");
                    progressBar.Add(progressBarFill);

                    progressContainer.Add(progressBar);

                    progressLabel = new Label($"{current}/{total}");
                    progressLabel.AddToClassList("quest-progress-label");
                    progressContainer.Add(progressLabel);

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

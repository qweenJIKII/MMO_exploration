// FantasyChatWindow: チャットウィンドウ
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Core.UI.Components
{
    /// <summary>
    /// チャットウィンドウ
    /// Phase 2実装: AAA Game Screen FantasyChatWindow.tsxを参考
    /// </summary>
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

            if (root == null)
            {
                Debug.LogError("[FantasyChatWindow] Root element is null!");
                return;
            }

            header = root.Q("chat-header");
            messagesScroll = root.Q<ScrollView>("chat-messages-scroll");
            messagesContainer = messagesScroll?.contentContainer;
            inputField = root.Q<TextField>("chat-input");

            // ヘッダークリックで展開/折りたたみ
            header?.RegisterCallback<ClickEvent>(evt => ToggleExpand());

            // Enterキーで送信
            inputField?.RegisterCallback<KeyDownEvent>(OnInputKeyDown);
        }

        /// <summary>
        /// メッセージを追加
        /// </summary>
        public void AddMessage(string sender, string message, ChatChannel channel)
        {
            if (messagesContainer == null) return;

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
            messagesScroll?.schedule.Execute(() => {
                messagesScroll.scrollOffset = new Vector2(0, messagesScroll.contentContainer.layout.height);
            }).StartingIn(50);
        }

        /// <summary>
        /// 展開/折りたたみ
        /// </summary>
        private void ToggleExpand()
        {
            isExpanded = !isExpanded;
            
            if (messagesScroll != null)
            {
                messagesScroll.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            }

            var inputContainer = root.Q("chat-input-container");
            if (inputContainer != null)
            {
                inputContainer.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// 入力キー処理
        /// </summary>
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

        /// <summary>
        /// メッセージ送信
        /// </summary>
        private void SendMessage(string message)
        {
            // TODO: ChatManagerに送信
            Debug.Log($"[Chat] {message}");
            AddMessage("You", message, ChatChannel.Global);
        }

        /// <summary>
        /// チャンネルに応じたCSSクラスを取得
        /// </summary>
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

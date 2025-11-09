// NotificationManager: トースト通知システム
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Core.UI
{
    /// <summary>
    /// 通知システム管理
    /// トースト通知のキュー制御、優先度管理、アニメーション
    /// Phase 2実装: ユーザーフィードバック改善
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private float defaultDuration = 3f;
        [SerializeField] private int maxVisibleNotifications = 3;
        [SerializeField] private float notificationSpacing = 10f;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float slideDistance = 50f;

        // 通知キュー
        private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        private List<ActiveNotification> activeNotifications = new List<ActiveNotification>();

        // UI要素
        private VisualElement notificationContainer;

        // 通知タイプ
        public enum NotificationType
        {
            Info,       // 情報（青）
            Success,    // 成功（緑）
            Warning,    // 警告（黄）
            Error,      // エラー（赤）
            Achievement // 実績（金）
        }

        // 通知優先度
        public enum NotificationPriority
        {
            Low = 0,
            Normal = 1,
            High = 2,
            Critical = 3
        }

        // 通知データ
        private class NotificationData
        {
            public string Message;
            public NotificationType Type;
            public NotificationPriority Priority;
            public float Duration;
            public string IconClass;

            public NotificationData(string message, NotificationType type, NotificationPriority priority, float duration, string iconClass = null)
            {
                Message = message;
                Type = type;
                Priority = priority;
                Duration = duration;
                IconClass = iconClass;
            }
        }

        // アクティブ通知
        private class ActiveNotification
        {
            public VisualElement Element;
            public float RemainingTime;
            public NotificationData Data;
            public bool IsFadingOut;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[NotificationManager] 初期化完了");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeUI();
        }

        private void Update()
        {
            UpdateActiveNotifications();
            ProcessQueue();
        }

        /// <summary>
        /// UI初期化
        /// </summary>
        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogWarning("[NotificationManager] UIDocumentが設定されていません");
                return;
            }

            var root = uiDocument.rootVisualElement;
            notificationContainer = root.Q<VisualElement>("notification-container");

            if (notificationContainer == null)
            {
                // コンテナが存在しない場合は作成
                notificationContainer = new VisualElement();
                notificationContainer.name = "notification-container";
                notificationContainer.AddToClassList("notification-container");
                notificationContainer.style.position = Position.Absolute;
                notificationContainer.style.top = 20;
                notificationContainer.style.right = 20;
                notificationContainer.style.width = 400;
                root.Add(notificationContainer);
            }

            Debug.Log("[NotificationManager] UI初期化完了");
        }

        /// <summary>
        /// 通知を表示（パブリックAPI）
        /// </summary>
        public void Show(string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal, float duration = 0f)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[NotificationManager] メッセージが空です");
                return;
            }

            float actualDuration = duration > 0 ? duration : defaultDuration;
            var notification = new NotificationData(message, type, priority, actualDuration);

            // 優先度が高い場合はキューの先頭に追加
            if (priority >= NotificationPriority.High)
            {
                // キューを一時的にリストに変換して優先度順に並べ替え
                var tempList = new List<NotificationData>(notificationQueue);
                tempList.Add(notification);
                tempList.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                
                notificationQueue.Clear();
                foreach (var item in tempList)
                {
                    notificationQueue.Enqueue(item);
                }
            }
            else
            {
                notificationQueue.Enqueue(notification);
            }
        }

        /// <summary>
        /// 情報通知
        /// </summary>
        public void ShowInfo(string message, float duration = 0f)
        {
            Show(message, NotificationType.Info, NotificationPriority.Normal, duration);
        }

        /// <summary>
        /// 成功通知
        /// </summary>
        public void ShowSuccess(string message, float duration = 0f)
        {
            Show(message, NotificationType.Success, NotificationPriority.Normal, duration);
        }

        /// <summary>
        /// 警告通知
        /// </summary>
        public void ShowWarning(string message, float duration = 0f)
        {
            Show(message, NotificationType.Warning, NotificationPriority.High, duration);
        }

        /// <summary>
        /// エラー通知
        /// </summary>
        public void ShowError(string message, float duration = 0f)
        {
            Show(message, NotificationType.Error, NotificationPriority.High, duration);
        }

        /// <summary>
        /// 実績通知
        /// </summary>
        public void ShowAchievement(string message, float duration = 5f)
        {
            Show(message, NotificationType.Achievement, NotificationPriority.High, duration);
        }

        /// <summary>
        /// キュー処理
        /// </summary>
        private void ProcessQueue()
        {
            if (notificationQueue.Count == 0 || activeNotifications.Count >= maxVisibleNotifications)
            {
                return;
            }

            var data = notificationQueue.Dequeue();
            ShowNotification(data);
        }

        /// <summary>
        /// 通知を表示
        /// </summary>
        private void ShowNotification(NotificationData data)
        {
            if (notificationContainer == null)
            {
                Debug.LogWarning("[NotificationManager] notificationContainerがnullです");
                return;
            }

            // 通知要素を作成
            var notification = CreateNotificationElement(data);
            notificationContainer.Add(notification);

            // アクティブリストに追加
            var activeNotification = new ActiveNotification
            {
                Element = notification,
                RemainingTime = data.Duration,
                Data = data,
                IsFadingOut = false
            };
            activeNotifications.Add(activeNotification);

            // フェードインアニメーション
            AnimateFadeIn(notification);

            // 位置を更新
            UpdateNotificationPositions();
        }

        /// <summary>
        /// 通知要素を作成
        /// </summary>
        private VisualElement CreateNotificationElement(NotificationData data)
        {
            var notification = new VisualElement();
            notification.AddToClassList("notification");
            notification.AddToClassList($"notification-{data.Type.ToString().ToLower()}");

            // アイコン
            var icon = new VisualElement();
            icon.AddToClassList("notification-icon");
            icon.AddToClassList(GetIconClass(data.Type));
            notification.Add(icon);

            // メッセージ
            var message = new Label(data.Message);
            message.AddToClassList("notification-message");
            notification.Add(message);

            // 閉じるボタン
            var closeButton = new Button(() => CloseNotification(notification));
            closeButton.text = "×";
            closeButton.AddToClassList("notification-close");
            notification.Add(closeButton);

            return notification;
        }

        /// <summary>
        /// アイコンクラスを取得
        /// </summary>
        private string GetIconClass(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => "icon-info",
                NotificationType.Success => "icon-success",
                NotificationType.Warning => "icon-warning",
                NotificationType.Error => "icon-error",
                NotificationType.Achievement => "icon-achievement",
                _ => "icon-info"
            };
        }

        /// <summary>
        /// フェードインアニメーション
        /// </summary>
        private void AnimateFadeIn(VisualElement element)
        {
            element.style.opacity = 0;
            element.style.translate = new Translate(slideDistance, 0);

            // アニメーション（簡易実装）
            float elapsed = 0;
            void UpdateAnimation()
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeInDuration);
                
                element.style.opacity = progress;
                element.style.translate = new Translate(slideDistance * (1 - progress), 0);

                if (progress < 1)
                {
                    element.schedule.Execute(UpdateAnimation).StartingIn(16); // 約60fps
                }
            }
            element.schedule.Execute(UpdateAnimation).StartingIn(16);
        }

        /// <summary>
        /// フェードアウトアニメーション
        /// </summary>
        private void AnimateFadeOut(VisualElement element, Action onComplete)
        {
            float elapsed = 0;
            void UpdateAnimation()
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / fadeOutDuration);
                
                element.style.opacity = 1 - progress;
                element.style.translate = new Translate(slideDistance * progress, 0);

                if (progress >= 1)
                {
                    onComplete?.Invoke();
                }
                else
                {
                    element.schedule.Execute(UpdateAnimation).StartingIn(16);
                }
            }
            element.schedule.Execute(UpdateAnimation).StartingIn(16);
        }

        /// <summary>
        /// アクティブ通知を更新
        /// </summary>
        private void UpdateActiveNotifications()
        {
            for (int i = activeNotifications.Count - 1; i >= 0; i--)
            {
                var notification = activeNotifications[i];

                if (notification.IsFadingOut)
                {
                    continue;
                }

                notification.RemainingTime -= Time.deltaTime;

                if (notification.RemainingTime <= 0)
                {
                    CloseNotification(notification.Element);
                }
            }
        }

        /// <summary>
        /// 通知を閉じる
        /// </summary>
        private void CloseNotification(VisualElement element)
        {
            var notification = activeNotifications.Find(n => n.Element == element);
            if (notification == null || notification.IsFadingOut)
            {
                return;
            }

            notification.IsFadingOut = true;

            AnimateFadeOut(element, () =>
            {
                notificationContainer.Remove(element);
                activeNotifications.Remove(notification);
                UpdateNotificationPositions();
            });
        }

        /// <summary>
        /// 通知の位置を更新
        /// </summary>
        private void UpdateNotificationPositions()
        {
            float yOffset = 0;
            foreach (var notification in activeNotifications)
            {
                notification.Element.style.top = yOffset;
                yOffset += notification.Element.resolvedStyle.height + notificationSpacing;
            }
        }

        /// <summary>
        /// すべての通知をクリア
        /// </summary>
        public void ClearAll()
        {
            notificationQueue.Clear();
            
            foreach (var notification in activeNotifications)
            {
                notificationContainer.Remove(notification.Element);
            }
            activeNotifications.Clear();
        }

        #region デバッグ機能
        [ContextMenu("Test: Info Notification")]
        private void TestInfo()
        {
            ShowInfo("これは情報通知のテストです");
        }

        [ContextMenu("Test: Success Notification")]
        private void TestSuccess()
        {
            ShowSuccess("アイテムを取得しました！");
        }

        [ContextMenu("Test: Warning Notification")]
        private void TestWarning()
        {
            ShowWarning("HPが少なくなっています");
        }

        [ContextMenu("Test: Error Notification")]
        private void TestError()
        {
            ShowError("接続エラーが発生しました");
        }

        [ContextMenu("Test: Achievement Notification")]
        private void TestAchievement()
        {
            ShowAchievement("実績解除: 初めての冒険");
        }

        [ContextMenu("Test: Multiple Notifications")]
        private void TestMultiple()
        {
            ShowInfo("通知1");
            ShowSuccess("通知2");
            ShowWarning("通知3");
            ShowError("通知4");
        }
        #endregion
    }
}

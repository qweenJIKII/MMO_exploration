using System;

namespace Project.Core.Online.Backend
{
    // 受信箱メッセージの最小DTO
    public sealed class InboxMessageDto
    {
        public string MessageId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsReceived { get; set; }

        public InboxMessageDto() {}

        public InboxMessageDto(string messageId, string title, string body,
            DateTime? expiredAt, bool isRead, bool isReceived)
        {
            MessageId = messageId;
            Title = title;
            Body = body;
            ExpiredAt = expiredAt;
            IsRead = isRead;
            IsReceived = isReceived;
        }
    }
}

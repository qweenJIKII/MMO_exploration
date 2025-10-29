using System.Threading.Tasks;

namespace Project.Core.Online.Backend
{
    // バックエンドIF: 後でGS2実装へ差し替え可能
    public interface IBackendService
    {
        // ゲスト/Identifier等でサインインし、ユーザーIDを返す
        Task<string> SignInAsync();

        // インベントリ取得
        Task<ItemDto[]> GetInventoryAsync();

        // アイテム付与（冪等IDで重複防止）
        Task<bool> GrantItemAsync(string itemId, int quantity, string idempotencyKey);

        // 任意: 通貨取得/加算
        Task<int> GetCurrencyAsync(string kind);
        Task<bool> AddCurrencyAsync(string kind, int delta, string idempotencyKey);

        /// <summary>
        /// 受信箱（Inbox）のメッセージ一覧を取得。
        /// </summary>
        Task<InboxMessageDto[]> ListMailAsync();

        /// <summary>
        /// 指定メッセージを受領し、含まれる AcquireAction を実行。
        /// 冪等キーで重複受領を防止。
        /// </summary>
        Task<bool> ReceiveMailAsync(string messageId, string idempotencyKey);
    }
}

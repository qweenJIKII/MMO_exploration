using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Project.Core.Online.Backend
{
    // Unity Gaming Services 実装。
    public sealed class UgsBackendService : IBackendService
    {
        private string _userId;

        public async Task<string> SignInAsync()
        {
            // UgsInitializer 側で初期化/サインイン済みを前提にしつつ、未サインインなら匿名サインイン。
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            _userId = AuthenticationService.Instance.PlayerId;
            return _userId;
        }

        public async Task<ItemDto[]> GetInventoryAsync()
        {
            // TODO: UGS Cloud Save / Cloud Code / Economy で実装
            await Task.CompletedTask;
            return Array.Empty<ItemDto>();
        }

        public async Task<bool> GrantItemAsync(string itemId, int quantity, string idempotencyKey)
        {
            // TODO: Cloud Code 経由でEconomy/Cloud Save 更新などに置換。idempotencyKeyを活用。
            await Task.CompletedTask;
            return true;
        }

        public async Task<int> GetCurrencyAsync(string kind)
        {
            // TODO: UGS Economy 通貨取得
            await Task.CompletedTask;
            return 0;
        }

        public async Task<bool> AddCurrencyAsync(string kind, int delta, string idempotencyKey)
        {
            // TODO: UGS Economy 通貨加算（冪等性はCloud Codeで担保）
            await Task.CompletedTask;
            return true;
        }

        public async Task<InboxMessageDto[]> ListMailAsync()
        {
            // TODO: Cloud Code + Cloud Save 組合せでInbox代替
            await Task.CompletedTask;
            return Array.Empty<InboxMessageDto>();
        }

        public async Task<bool> ReceiveMailAsync(string messageId, string idempotencyKey)
        {
            // TODO: Cloud Code 経由でメッセージ受領 + Economy/Cloud Save 更新
            await Task.CompletedTask;
            return true;
        }
    }
}

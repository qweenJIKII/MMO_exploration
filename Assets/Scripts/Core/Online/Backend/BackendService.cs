using UnityEngine;

namespace Project.Core.Online.Backend
{
    // 現在利用中のバックエンド実装へのアクセサ（シングルトン的に利用）
    public static class BackendService
    {
        private static IBackendService _current;

        // 起動時に差し替え。デフォルトはGS2実装のスタブ
        public static IBackendService Current
        {
            get
            {
                if (_current == null)
                {
                    Debug.Log("[BackendService] Initialize default UgsBackendService.");
                    _current = new UgsBackendService();
                }
                return _current;
            }
            set
            {
                _current = value;
                Debug.Log("[BackendService] Backend implementation set: " + (_current?.GetType().Name ?? "null"));
            }
        }
    }
}

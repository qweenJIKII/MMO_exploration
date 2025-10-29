// Supabaseクライアントの初期化と取得
// 注意: 実運用ではURLとAnonKeyはSupabaseSettings(ScriptableObject)から読み込みます。
using System.Threading.Tasks;
using UnityEngine;
using Supabase;

namespace Project.Core.Online
{
    public static class SupabaseClientProvider
    {
        private static Client _client;
        public static Client Client => _client;

        public static bool IsInitialized => _client != null;

        public static async Task InitializeAsync(SupabaseSettings settings)
        {
            if (_client != null) return;
            if (settings == null || string.IsNullOrWhiteSpace(settings.url) || string.IsNullOrWhiteSpace(settings.anonKey))
            {
                Debug.LogWarning("[Supabase] Settings not configured. Initialization skipped.");
                return;
            }

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = settings.autoConnectRealtime,
                AutoRefreshToken = settings.autoRefreshToken
            };
            _client = new Client(settings.url, settings.anonKey, options);
            await _client.InitializeAsync();
            Debug.Log("[Supabase] Initialized");
        }

        public static async Task InitializeFromResourcesAsync(string resourcePath = "SupabaseSettings")
        {
            if (_client != null) return;
            var settings = Resources.Load<SupabaseSettings>(resourcePath);
            if (settings == null)
            {
                Debug.LogWarning($"[Supabase] Settings not found at Resources/{resourcePath}. Create it via Tools/Setup/Supabase/Create Settings.");
                return;
            }
            await InitializeAsync(settings);
        }
    }
}

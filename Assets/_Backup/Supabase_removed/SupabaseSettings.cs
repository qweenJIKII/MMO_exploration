// Supabase 接続設定（ScriptableObject）
// 注意: 実キーはリポジトリにコミットしないでください。Editor用メニューでローカルに作成・編集します。
using UnityEngine;

namespace Project.Core.Online
{
    [CreateAssetMenu(fileName = "SupabaseSettings", menuName = "Online/Supabase Settings", order = 10)]
    public class SupabaseSettings : ScriptableObject
    {
        [Header("Supabase Project")] public string url = ""; // 例: https://xxxx.supabase.co
        [TextArea] public string anonKey = ""; // Public anon key（service_roleは絶対に入れない）

        [Header("Runtime Options")] public bool autoConnectRealtime = false;
        public bool autoRefreshToken = true;
    }
}

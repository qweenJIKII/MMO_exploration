// Supabase 認証ヘルパー
using System.Threading.Tasks;
using UnityEngine;

namespace Project.Core.Online
{
    public static class SupabaseAuth
    {
        // スタブ: 常に未サインイン扱い
        public static bool IsSignedIn => false;

        // スタブ: ユーザーID無し
        public static string CurrentUserId => null;

        // Email OTP（Magic Link）送信
        public static async Task<bool> SendEmailOtpAsync(string email)
        {
            // スタブ: 認証機能は無効化
            await Task.CompletedTask;
            Debug.Log("[Auth][Stub] SendEmailOtpAsync is disabled.");
            return false;
        }


        // Email + Password（必要なら有効化して使用）
        public static async Task<bool> SignInWithEmailPasswordAsync(string email, string password)
        {
            // スタブ: 認証機能は無効化
            await Task.CompletedTask;
            Debug.Log("[Auth][Stub] SignInWithEmailPasswordAsync is disabled.");
            return false;
        }

        public static async Task SignOutAsync()
        {
            // スタブ: サインアウト動作無し
            await Task.CompletedTask;
            Debug.Log("[Auth][Stub] SignOutAsync is disabled.");
        }
    }
}


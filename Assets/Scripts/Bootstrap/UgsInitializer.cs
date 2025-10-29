// コメント: UGS初期化＋匿名サインイン（シーン開始時に一度だけ実行）
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

namespace Project.Core.Bootstrap
{
    public class UgsInitializer : MonoBehaviour
    {
        private static bool s_Initialized;

        private async void Awake()
        {
            if (s_Initialized) return;
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("[UgsInitializer] Unity Services Initialized.");

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[UgsInitializer] Signed in. User ID: {AuthenticationService.Instance.PlayerId}");
                }

                s_Initialized = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UgsInitializer] Initialization failed: {e.Message}");
            }
        }
    }
}

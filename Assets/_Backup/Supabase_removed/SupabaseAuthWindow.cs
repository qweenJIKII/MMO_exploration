#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Project.Core.Online;

public class SupabaseAuthWindow : EditorWindow
{
    private string email = "";
    private string status = "";

    public static void ShowWindow()
    {
        var win = GetWindow<SupabaseAuthWindow>(true, "Supabase Auth", true);
        win.minSize = new Vector2(360, 140);
        win.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Supabase Auth (Email OTP)", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        using (new EditorGUI.DisabledScope(!Project.Core.Online.SupabaseClientProvider.IsInitialized))
        {
            email = EditorGUILayout.TextField("Email", email);

            if (GUILayout.Button("Send OTP (Magic Link)"))
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    status = "Input email.";
                }
                else
                {
                    _ = SendOtpAsync();
                }
            }

            if (GUILayout.Button("Sign Out"))
            {
                _ = SupabaseAuth.SignOutAsync();
                status = "Signed out.";
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Initialized", Project.Core.Online.SupabaseClientProvider.IsInitialized ? "Yes" : "No");
        EditorGUILayout.LabelField("Signed In", SupabaseAuth.IsSignedIn ? "Yes" : "No");
        EditorGUILayout.HelpBox(status, MessageType.Info);

        if (!Project.Core.Online.SupabaseClientProvider.IsInitialized)
        {
            EditorGUILayout.HelpBox("Initialize client first: Tools/Dev/Supabase/Initialize Client", MessageType.Warning);
        }
    }

    private async System.Threading.Tasks.Task SendOtpAsync()
    {
        status = "Sending OTP...";
        Repaint();
        var ok = await SupabaseAuth.SendEmailOtpAsync(email);
        status = ok ? "OTP sent. Check your mailbox and complete sign-in." : "Failed to send OTP.";
        Repaint();
    }
}
#endif

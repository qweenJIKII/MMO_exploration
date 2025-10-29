#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Project.Core.Online;
using Project.Core.Online.Repositories;

public static class SupabaseTools
{
    [MenuItem("Tools/Dev/Supabase/Create Settings (Resources)")]
    public static void CreateSettingsInResources()
    {
        const string resourcesDir = "Assets/Resources";
        const string assetPath = resourcesDir + "/SupabaseSettings.asset";
        if (!System.IO.Directory.Exists(resourcesDir)) System.IO.Directory.CreateDirectory(resourcesDir);

        var existing = AssetDatabase.LoadAssetAtPath<Project.Core.Online.SupabaseSettings>(assetPath);
        if (existing == null)
        {
            var settings = ScriptableObject.CreateInstance<Project.Core.Online.SupabaseSettings>();
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(settings);
            Debug.Log("[Supabase][Tools] Created Resources/SupabaseSettings.asset. Fill URL and Anon Key.");
        }
        else
        {
            EditorGUIUtility.PingObject(existing);
            Debug.Log("[Supabase][Tools] Settings already exists at Resources/SupabaseSettings.asset");
        }
    }

    [MenuItem("Tools/Dev/Supabase/Initialize Client")] 
    public static async void InitializeClient()
    {
        await SupabaseClientProvider.InitializeFromResourcesAsync();
        if (SupabaseClientProvider.IsInitialized)
            Debug.Log("[Supabase][Tools] Client initialized.");
        else
            Debug.LogWarning("[Supabase][Tools] Client not initialized (check Resources/SupabaseSettings.asset)");
    }

    [MenuItem("Tools/Dev/Supabase/Auth Window...")]
    public static void OpenAuthWindow()
    {
        SupabaseAuthWindow.ShowWindow();
    }

    [MenuItem("Tools/Dev/Supabase/Inventory Self Test")]
    public static async void InventorySelfTest()
    {
        if (!SupabaseClientProvider.IsInitialized)
        {
            Debug.LogWarning("[Supabase][Tools] Initialize client first.");
            return;
        }
        if (!SupabaseAuth.IsSignedIn)
        {
            Debug.LogWarning("[Supabase][Tools] Sign-in required. Open Auth Window and complete OTP.");
            return;
        }
        await InventoryRepo.UpsertAsync("potion", 1);
        var rows = await InventoryRepo.GetMineAsync();
        Debug.Log($"[Supabase][Tools] Inventory rows: {rows.Length}");
    }
}
#endif

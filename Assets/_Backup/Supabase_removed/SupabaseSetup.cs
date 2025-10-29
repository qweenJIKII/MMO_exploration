#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Project.Core.Online;
using System.IO;

public static class SupabaseSetup
{
    [MenuItem("Tools/Setup/Supabase/Create Settings (Resources)")]
    public static void CreateSettings()
    {
        const string resourcesDir = "Assets/Resources";
        const string assetPath = resourcesDir + "/SupabaseSettings.asset";
        if (!Directory.Exists(resourcesDir)) Directory.CreateDirectory(resourcesDir);

        var settings = ScriptableObject.CreateInstance<SupabaseSettings>();
        AssetDatabase.CreateAsset(settings, assetPath);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(settings);
        Debug.Log("[Supabase] Created Resources/SupabaseSettings.asset. Fill URL and Anon Key.");
    }
}
#endif

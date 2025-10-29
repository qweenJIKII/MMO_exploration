using UnityEditor;

public static class DevMenu_SQLite
{
    // Editor メニュー: Tools/Dev/SQLite/Ensure Schema
    [MenuItem("Tools/Dev/SQLite/Ensure Schema", priority = 100)]
    public static void EnsureSchema()
    {
        SQLiteDbUtil.EnsureSchema();
    }

    // Editor メニュー: Tools/Dev/SQLite/Backup Now
    [MenuItem("Tools/Dev/SQLite/Backup Now", priority = 101)]
    public static void BackupNow()
    {
        SQLiteDbUtil.BackupNow();
    }

    // Editor メニュー: Tools/Dev/SQLite/Self Test
    [MenuItem("Tools/Dev/SQLite/Self Test", priority = 102)]
    public static void SelfTest()
    {
        SQLiteDbUtil.SelfTest();
    }
}

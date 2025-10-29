// Editorメニュー: SQLiteスキーマ適用、バックアップ、セルフテスト
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Project.Core.Database;

public static class DatabaseTools
{
    private static bool IsLocalDbEnabled()
    {
        var list = Resources.FindObjectsOfTypeAll<Project.Core.Database.DatabaseBootstrapper>();
        if (list == null || list.Length == 0) return true; // 未配置なら許可（従来挙動）
        foreach (var b in list)
        {
            if (b != null && !b.useLocalSQLite) return false;
        }
        return true;
    }

    [MenuItem("Tools/Dev/Database/Ensure Schema")]
    public static void EnsureSchema()
    {
        try
        {
            if (!IsLocalDbEnabled()) { Debug.Log("[DB][Tools] EnsureSchema skipped: Local SQLite is disabled on Bootstrapper."); return; }
            DatabaseBootstrapper.EnsureSchema();
            Debug.Log("[DB][Tools] EnsureSchema done.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DB][Tools] EnsureSchema failed: {ex}");
        }
    }

    [MenuItem("Tools/Dev/Database/Delete DB (Danger)")]
    public static void DeleteDb()
    {
        if (!EditorUtility.DisplayDialog(
            "Delete DB",
            "This will DELETE the local SQLite DB file and backups under persistentDataPath. Proceed?",
            "Delete",
            "Cancel"))
        {
            return;
        }

        try
        {
            // 現在の既定/レガシーパス候補を列挙
            var paths = new System.Collections.Generic.List<string>();
            var dir1 = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Database");
            var dir2 = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "MMO_exploration/Database"); // legacy
            var fileConfigured = Project.Core.Database.DatabaseBootstrapper.DbFilePath;
            if (!string.IsNullOrEmpty(fileConfigured)) paths.Add(fileConfigured);
            paths.Add(System.IO.Path.Combine(dir1, "game.db"));
            paths.Add(System.IO.Path.Combine(dir2, "game.db"));

            int deleted = 0, backups = 0;
            foreach (var p in paths)
            {
                var norm = p.Replace("\\", "/");
                if (System.IO.File.Exists(norm))
                {
                    System.IO.File.Delete(norm);
                    deleted++;
                    Debug.Log($"[DB][Tools] Deleted: {norm}");
                }

                // 既知のバックアップも削除 *.bak-<n>
                var dir = System.IO.Path.GetDirectoryName(norm);
                var name = System.IO.Path.GetFileNameWithoutExtension(norm);
                var ext = System.IO.Path.GetExtension(norm);
                if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        var bak = System.IO.Path.Combine(dir, $"{name}.bak-{i}{ext}");
                        if (System.IO.File.Exists(bak)) { System.IO.File.Delete(bak); backups++; }
                    }
                }
            }

            EditorUtility.DisplayDialog("Delete DB", $"Deleted DB files: {deleted}, backups: {backups}", "OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DB][Tools] DeleteDb failed: {ex}");
        }
    }

    [MenuItem("Tools/Dev/Database/Backup Now")]
    public static void BackupNow()
    {
        try
        {
            if (!IsLocalDbEnabled()) { Debug.Log("[DB][Tools] Backup skipped: Local SQLite is disabled on Bootstrapper."); return; }
            if (!File.Exists(DatabaseBootstrapper.DbFilePath))
            {
                Debug.LogWarning("[DB][Tools] No DB file to backup.");
                return;
            }
            var mi = typeof(DatabaseBootstrapper).GetMethod("RotateBackups", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            mi?.Invoke(null, new object[] { 3 }); // デフォルト3世代
            Debug.Log("[DB][Tools] Backup rotation done.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DB][Tools] Backup failed: {ex}");
        }
    }

    [MenuItem("Tools/Dev/Database/Self Test")]
    public static void SelfTest()
    {
        #if SQLITE_UNITY_KIT
        try
        {
            if (!IsLocalDbEnabled()) { Debug.Log("[DB][Tools] SelfTest skipped: Local SQLite is disabled on Bootstrapper."); return; }
            // 単純な往復: トランザクションクエリでDDLを再適用→SELECTで確認
            string initQuery = SqlSchema.CreateAll();
            // DbFilePath が未初期化のケースに対応（シーンでBootstrapper未実行）
            string dir = Project.Core.Database.DatabaseBootstrapper.DbDirectoryPath;
            string file = Project.Core.Database.DatabaseBootstrapper.DbFilePath;
            if (string.IsNullOrEmpty(dir))
            {
                dir = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Database");
            }
            dir = dir.Replace("\\", "/");
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            string dbName = string.IsNullOrEmpty(file) ? "game.db" : System.IO.Path.GetFileName(file);

            var fullPath = System.IO.Path.Combine(dir, dbName);
            if (UnityEditor.EditorApplication.isPlaying)
            {
                Debug.LogWarning("[DB][Tools] SelfTest skipped while Play Mode is running (DB may be locked). Stop Play and retry.");
                return;
            }
            bool exists = System.IO.File.Exists(fullPath);
            bool force = !exists; // 既存ならforce=falseでコピー競合を避ける
            using (var db = new global::Tetr4lab.UnityEngine.SQLite.SQLite<global::Tetr4lab.UnityEngine.SQLite.SQLiteTable<global::Tetr4lab.UnityEngine.SQLite.SQLiteRow>, global::Tetr4lab.UnityEngine.SQLite.SQLiteRow>(
                fullPath,
                initQuery,
                null,
                force))
            {
                Debug.Log($"[DB][Tools] File exists before ensure: {exists}, fullPath={fullPath}");
                // 既存DBでテーブルが無い場合にもDDLを確実に適用
                var ok = db.TransactionQueries(initQuery);
                if (!ok) Debug.LogWarning("[DB][Tools] TransactionQueries returned false in SelfTest. Falling back to per-statement.");
                foreach (var stmt in initQuery.Split(';'))
                {
                    var s = stmt.Trim();
                    if (string.IsNullOrEmpty(s)) continue;
                    try
                    {
                        db.ExecuteNonQuery(s);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[DB][Tools] DDL exec failed: {s} :: {ex.Message}");
                    }
                }
                Debug.Log($"[DB][Tools] Path: {dir}, DB: {dbName}");
                var ver = db.ExecuteQuery("SELECT sqlite_version() AS v", null);
                if (ver is System.Collections.IEnumerable ven)
                {
                    foreach (var r in ven)
                    {
                        if (r is global::Tetr4lab.UnityEngine.SQLite.SQLiteRow row && row.ContainsKey("v"))
                        {
                            Debug.Log($"[DB][Tools] SQLite version: {row["v"]}");
                            break;
                        }
                    }
                }

                // 件数をCOUNT(*)で取得
                var cntTable = db.ExecuteQuery("SELECT COUNT(*) AS c FROM sqlite_master WHERE type='table'", null);
                int count = 0;
                if (cntTable is System.Collections.IEnumerable cntEn)
                {
                    foreach (var r in cntEn)
                    {
                        if (r is global::Tetr4lab.UnityEngine.SQLite.SQLiteRow row)
                        {
                            if (row.ContainsKey("c"))
                            {
                                var v = row["c"]; if (v is int ci) count = ci; else if (v is long cl) count = (int)cl; else if (v != null && int.TryParse(v.ToString(), out var cp)) count = cp;
                            }
                            else
                            {
                                // 別実装対策: 最初の列を採用
                                foreach (var kv in row)
                                {
                                    var v = kv.Value; if (v is int ci2) count = ci2; else if (v is long cl2) count = (int)cl2; else if (v != null && int.TryParse(v.ToString(), out var cp2)) count = cp2; break;
                                }
                            }
                            break;
                        }
                    }
                }
                Debug.Log($"[DB][Tools] Tables (COUNT): {count}");

                // テーブル名の一覧も出力
                var listTable = db.ExecuteQuery("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name", null);
                if (listTable is System.Collections.IEnumerable listEn)
                {
                    int i = 0;
                    foreach (var r in listEn)
                    {
                        if (r is global::Tetr4lab.UnityEngine.SQLite.SQLiteRow row)
                        {
                            if (row.ContainsKey("name")) Debug.Log($"[DB][Tools] Table[{i++}]: {row["name"]}");
                            else
                            {
                                // 行の内容をダンプ
                                foreach (var kv in row) { Debug.Log($"[DB][Tools] TableRow[{i}]: {kv.Key}={kv.Value}"); }
                                i++;
                            }
                        }
                    }
                }
                else
                {
                    var pi = listTable.GetType().GetProperty("Rows") ?? listTable.GetType().GetProperty("rows");
                    var rowsObj = pi?.GetValue(listTable, null) as System.Collections.IEnumerable;
                    if (rowsObj != null)
                    {
                        int i = 0;
                        foreach (var r in rowsObj)
                        {
                            var dict = r as System.Collections.Generic.Dictionary<string, object>;
                            if (dict != null)
                            {
                                if (dict.TryGetValue("name", out var name)) Debug.Log($"[DB][Tools] Table[{i++}]: {name}");
                                else { foreach (var kv in dict) { Debug.Log($"[DB][Tools] TableRow[{i}]: {kv.Key}={kv.Value}"); } i++; }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[DB][Tools] listTable is not IEnumerable and has no Rows; type={listTable.GetType().FullName}");
                    }
                }

                // 追加診断: 書き込み可否を検証（_probe テーブルを作成）
                try
                {
                    db.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS _probe(id INTEGER PRIMARY KEY, v INTEGER)", (global::Tetr4lab.UnityEngine.SQLite.SQLiteRow)null);
                    db.ExecuteNonQuery("INSERT INTO _probe(v) VALUES (1)", (global::Tetr4lab.UnityEngine.SQLite.SQLiteRow)null);
                    var probe = db.ExecuteQuery("SELECT COUNT(*) AS c FROM _probe", null);
                    int pc = 0;
                    if (probe is System.Collections.IEnumerable pen)
                    {
                        foreach (var r in pen)
                        {
                            if (r is global::Tetr4lab.UnityEngine.SQLite.SQLiteRow row)
                            {
                                foreach (var kv in row)
                                {
                                    Debug.Log($"[DB][Tools] Probe row: {kv.Key}={kv.Value}");
                                }
                                if (row.ContainsKey("c"))
                                {
                                    var v = row["c"]; if (v is int vi) pc = vi; else if (v is long vl) pc = (int)vl; else if (v != null && int.TryParse(v.ToString(), out var vp)) pc = vp;
                                }
                                else
                                {
                                    foreach (var kv in row)
                                    {
                                        var v = kv.Value; if (v is int vi2) { pc = vi2; break; } if (v is long vl2) { pc = (int)vl2; break; } if (v != null && int.TryParse(v.ToString(), out var vp2)) { pc = vp2; break; }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    Debug.Log($"[DB][Tools] Probe rows: {pc}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[DB][Tools] Probe failed: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DB][Tools] SelfTest failed: {ex}");
        }
        #else
        Debug.LogWarning("[DB][Tools] SelfTest skipped (SQLITE_UNITY_KIT not defined). Install the package and define the symbol.");
        #endif
    }
}
#endif

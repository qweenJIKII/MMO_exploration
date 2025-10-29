// 起動時にSQLiteUnityKitを初期化し、スキーマ適用とバックアップを行う
using System;
using System.IO;
using UnityEngine;

namespace Project.Core.Database
{
#if SQLITE_UNITY_KIT
    // SQLiteUnityKit の型（グローバル名前空間想定）
    // public class SQLite<TTable, TRow> : IDisposable where TTable : SQLiteTable<TRow>, new() where TRow : SQLiteRow, new()
    // public class SQLiteTable<TRow> where TRow : SQLiteRow, new()
    // public class SQLiteRow : System.Collections.Generic.Dictionary<string, object>
    public class DatabaseBootstrapper : MonoBehaviour
    {
        [Tooltip("DBファイル名（拡張子 .db 推奨)")]
        public string databaseName = "game.db";

        [Tooltip("サブディレクトリ（persistentDataPath 以下)")]
        public string subDirectory = "Database";

        [Tooltip("ローカルSQLiteを使用する（オフライン一時無効化用）")]
        public bool useLocalSQLite = false;

        [Tooltip("起動時にスキーマEnsureを実行するか")] public bool ensureSchemaOnStart = true;
        [Tooltip("起動時にバックアップをローテーションするか")] public bool rotateBackupOnStart = true;
        [Tooltip("バックアップ保持世代")] [Range(1, 20)] public int backupGenerations = 3;

        public static string DbDirectoryPath { get; private set; } = string.Empty;
        public static string DbFilePath { get; private set; } = string.Empty;

        private void OnValidate()
        {
            // レガシー値の自動是正とパス正規化
            if (string.IsNullOrEmpty(subDirectory)) subDirectory = "Database";
            subDirectory = subDirectory.Replace("\\", "/");
            if (subDirectory.Contains("MMO_exploration/Database")) subDirectory = "Database";
            subDirectory = subDirectory.Trim('/');
        }

        private void Awake()
        {
            // DBパス構築
            DbDirectoryPath = Path.Combine(Application.persistentDataPath, subDirectory.Replace("\\", "/"));
            if (!Directory.Exists(DbDirectoryPath)) Directory.CreateDirectory(DbDirectoryPath);
            DbFilePath = Path.Combine(DbDirectoryPath, databaseName);

            // オフラインでSQLiteを使わない設定ならここで終了
            if (!useLocalSQLite)
            {
                Debug.Log("[DB] Local SQLite is disabled. Skipping ensure/backup.");
                return;
            }

            if (rotateBackupOnStart)
            {
                try { RotateBackups(backupGenerations); }
                catch (Exception ex) { Debug.LogError($"[DB] Backup rotation failed: {ex}"); }
            }

            if (ensureSchemaOnStart)
            {
                try
                {
                    EnsureSchema();
                    Debug.Log($"[DB] Schema ensured at {DbFilePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DB] Ensure schema failed: {ex}");
                }
            }
        }

        public static void EnsureSchema()
        {
            // 実体のブートストラッパ設定に従って無効化（Editor等から静的呼び出しされるケース）
            var instances = Resources.FindObjectsOfTypeAll<DatabaseBootstrapper>();
            if (instances != null)
            {
                foreach (var ins in instances)
                {
                    if (ins != null && !ins.useLocalSQLite)
                    {
                        Debug.Log("[DB] EnsureSchema skipped because Local SQLite is disabled on Bootstrapper.");
                        return;
                    }
                }
            }
            // 初期化クエリにDDLを渡すと、SQLiteUnityKitが初回作成時に適用してくれる
            string initQuery = SqlSchema.CreateAll();
            // Awake() 前に呼ばれても動作するようにパスを補正
            string dir = DbDirectoryPath;
            string file = DbFilePath;
            if (string.IsNullOrEmpty(dir))
            {
                dir = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "Database");
            }
            dir = dir.Replace("\\", "/");
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            string dbName = string.IsNullOrEmpty(file) ? "game.db" : System.IO.Path.GetFileName(file);

            var fullPath = System.IO.Path.Combine(dir, dbName);
            using (var db = new global::Tetr4lab.UnityEngine.SQLite.SQLite<global::Tetr4lab.UnityEngine.SQLite.SQLiteTable<global::Tetr4lab.UnityEngine.SQLite.SQLiteRow>, global::Tetr4lab.UnityEngine.SQLite.SQLiteRow>(
                fullPath,                    // dbName (full path)
                initQuery,                   // query
                null,                        // path (unused when full path is provided)
                true))                       // force: 初期化クエリを確実に適用
            {
                // 既存DBの場合でも、冪等DDLを再適用しておく
                var ok = db.TransactionQueries(initQuery);
                if (!ok)
                {
                    UnityEngine.Debug.LogWarning("[DB] TransactionQueries returned false during EnsureSchema. Will try statements one by one.");
                }
                // 実装差異に備えて、セミコロン区切りで個別実行も行う
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
                        UnityEngine.Debug.LogWarning($"[DB] ExecuteNonQuery failed for statement: {s} :: {ex}");
                    }
                }
            }
        }

        public static void RotateBackups(int generations)
        {
            if (!File.Exists(DbFilePath)) return; // まだDBなし
            var dir = Path.GetDirectoryName(DbFilePath)!;
            var name = Path.GetFileNameWithoutExtension(DbFilePath);
            var ext = Path.GetExtension(DbFilePath);

            // 古い世代を削除/ローテーション
            for (int i = generations; i >= 1; i--)
            {
                var src = Path.Combine(dir, $"{name}.bak-{i}{ext}");
                if (File.Exists(src))
                {
                    if (i == generations) File.Delete(src);
                    else
                    {
                        var dst = Path.Combine(dir, $"{name}.bak-{i + 1}{ext}");
                        if (File.Exists(dst)) File.Delete(dst);
                        File.Move(src, dst);
                    }
                }
            }
            // 最新をbak-1へ
            var bak1 = Path.Combine(dir, $"{name}.bak-1{ext}");
            File.Copy(DbFilePath, bak1, overwrite: true);
        }
    }
#else
    // パッケージ未導入でもプロジェクトがコンパイルできるNo-Op版
    public class DatabaseBootstrapper : MonoBehaviour
    {
        public string databaseName = "game.db";
        public string subDirectory = "MMO_exploration/Database";
        // Editor拡張側から参照されるため、No-Op版にもダミー定義を持たせる
        public bool useLocalSQLite = false;
        public bool ensureSchemaOnStart = true;
        public bool rotateBackupOnStart = true;
        [Range(1, 20)] public int backupGenerations = 3;

        public static string DbDirectoryPath { get; private set; } = string.Empty;
        public static string DbFilePath { get; private set; } = string.Empty;

        private void Awake()
        {
            DbDirectoryPath = Path.Combine(Application.persistentDataPath, subDirectory.Replace("\\", "/"));
            if (!Directory.Exists(DbDirectoryPath)) Directory.CreateDirectory(DbDirectoryPath);
            DbFilePath = Path.Combine(DbDirectoryPath, databaseName);
            Debug.LogWarning("[DB] SQLiteUnityKit is not available. Define Scripting Symbol 'SQLITE_UNITY_KIT' and install the package to enable DB features.");
        }

        public static void EnsureSchema() { Debug.LogWarning("[DB] EnsureSchema skipped (SQLITE_UNITY_KIT not defined)"); }
        public static void RotateBackups() { Debug.LogWarning("[DB] Backup skipped (SQLITE_UNITY_KIT not defined)"); }
    }
#endif
}

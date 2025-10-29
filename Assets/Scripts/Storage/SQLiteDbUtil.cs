using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Tetr4lab.UnityEngine.SQLite; // SQLiteUnityKit の名前空間

// NOTE: このサンプルは tetr4lab/SQLiteUnityKit の API 仕様に基づきます。
// public class SQLite<TTable, TRow> : IDisposable where TTable : SQLiteTable<TRow>, new () where TRow : SQLiteRow, new ()
// メソッド例: ExecuteNonQuery, ExecuteQuery, TransactionQueries

public static class SQLiteDbUtil
{
    // 保存先ディレクトリ/ファイル
    public static string DbDir => Path.Combine(Application.persistentDataPath, "Database");
    public static string DbName => "game.db";
    public static string DbPath => Path.Combine(DbDir, DbName);
    public static string BackupDir => Path.Combine(DbDir, "backup");

    // スキーマ（将来PostgreSQL互換を意識）
    private static readonly string[] Schema = new[]
    {
        // Inventory
        "CREATE TABLE IF NOT EXISTS inventory (\n" +
        "  player_id TEXT NOT NULL,\n" +
        "  item_id   TEXT NOT NULL,\n" +
        "  amount    INTEGER NOT NULL CHECK (amount >= 0),\n" +
        "  meta_json TEXT,\n" +
        "  updated_at INTEGER NOT NULL,\n" +
        "  PRIMARY KEY (player_id, item_id)\n" +
        ")",
        "CREATE INDEX IF NOT EXISTS idx_inventory_updated ON inventory(updated_at)",

        // Mail (inbox)
        "CREATE TABLE IF NOT EXISTS mail (\n" +
        "  mail_id    TEXT PRIMARY KEY,\n" +
        "  player_id  TEXT NOT NULL,\n" +
        "  subject    TEXT NOT NULL,\n" +
        "  body       TEXT NOT NULL,\n" +
        "  attachments_json TEXT NOT NULL,\n" +
        "  expire_at  INTEGER NOT NULL,\n" +
        "  created_at INTEGER NOT NULL,\n" +
        "  claimed    INTEGER NOT NULL DEFAULT 0\n" +
        ")",
        "CREATE INDEX IF NOT EXISTS idx_mail_player ON mail(player_id)"
    };

    // スキーマ適用
    public static void EnsureSchema()
    {
        EnsureDirs();
        using (var db = new SQLite<SQLiteTable<SQLiteRow>, SQLiteRow>(DbName, null, DbDir, false))
        {
            var ok = db.TransactionQueries(Schema);
            Debug.Log($"[SQLiteDbUtil] EnsureSchema: {(ok ? "OK" : "FAILED")} => {DbPath}");
        }
    }

    // 自己テスト（Insert/Queryの最小動作）
    public static void SelfTest()
    {
        EnsureSchema();
        using (var db = new SQLite<SQLiteTable<SQLiteRow>, SQLiteRow>(DbName, null, DbDir, false))
        {
            // シンプルなデータ投入（擬似バインドは未使用：まずは確実に動かす）
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var insert1 = $"INSERT OR REPLACE INTO inventory (player_id,item_id,amount,meta_json,updated_at) VALUES ('p01','wood',10,'{{}}',{now})";
            var insert2 = $"INSERT OR REPLACE INTO inventory (player_id,item_id,amount,meta_json,updated_at) VALUES ('p01','stone',3,'{{}}',{now})";
            db.ExecuteNonQuery(insert1);
            db.ExecuteNonQuery(insert2);

            // 取得
            SQLiteTable<SQLiteRow> rows = db.ExecuteQuery("SELECT player_id,item_id,amount,updated_at FROM inventory WHERE player_id='p01'");
            int count = 0;
            foreach (SQLiteRow r in rows)
            {
                var pid = SafeGet(r, "player_id");
                var item = SafeGet(r, "item_id");
                var amt = SafeGet(r, "amount");
                var upd = SafeGet(r, "updated_at");
                Debug.Log($"[SQLiteDbUtil] Row[{count}] player={pid}, item={item}, amount={amt}, updated_at={upd}");
                count++;
            }
            Debug.Log($"[SQLiteDbUtil] Query count = {count}");
        }
    }

    // バックアップ（世代維持: 最新3つ）
    public static void BackupNow(int keep = 3)
    {
        EnsureDirs();
        if (!File.Exists(DbPath))
        {
            Debug.LogWarning($"[SQLiteDbUtil] Backup skipped: DB not found => {DbPath}");
            return;
        }
        Directory.CreateDirectory(BackupDir);
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var dst = Path.Combine(BackupDir, $"{DbName}.{stamp}.bak");
        File.Copy(DbPath, dst, overwrite: true);
        Debug.Log($"[SQLiteDbUtil] Backup: {dst}");

        // ローテーション
        var files = new List<string>(Directory.GetFiles(BackupDir, $"{DbName}.*.bak"));
        files.Sort((a,b) => File.GetCreationTimeUtc(b).CompareTo(File.GetCreationTimeUtc(a))); // 新しい順
        for (int i = keep; i < files.Count; i++)
        {
            try { File.Delete(files[i]); }
            catch (Exception e) { Debug.LogWarning($"[SQLiteDbUtil] Backup rotate delete failed: {files[i]} => {e.Message}"); }
        }
    }

    private static void EnsureDirs()
    {
        if (!Directory.Exists(DbDir)) Directory.CreateDirectory(DbDir);
    }

    // SQLiteRow から安全に string を取得
    private static string SafeGet(SQLiteRow row, string key)
    {
        try
        {
            if (row != null && row.TryGetValue(key, out var v)) return v?.ToString();
        }
        catch {}
        return string.Empty;
    }
}

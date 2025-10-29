// スキーマ定義: SQLiteのDDLを保持
using System.Text;

public static class SqlSchema
{
    // インベントリとメールの最小スキーマ
    public static string CreateAll()
    {
        var sb = new StringBuilder();
        sb.AppendLine("CREATE TABLE IF NOT EXISTS inventory (\n  player_id TEXT NOT NULL,\n  item_id   TEXT NOT NULL,\n  amount    INTEGER NOT NULL CHECK (amount >= 0),\n  meta_json TEXT,\n  updated_at INTEGER NOT NULL,\n  PRIMARY KEY (player_id, item_id)\n);");
        sb.AppendLine("CREATE INDEX IF NOT EXISTS idx_inventory_updated ON inventory(updated_at);");
        sb.AppendLine();
        sb.AppendLine("CREATE TABLE IF NOT EXISTS mail (\n  mail_id    TEXT PRIMARY KEY,\n  player_id  TEXT NOT NULL,\n  subject    TEXT NOT NULL,\n  body       TEXT NOT NULL,\n  attachments_json TEXT NOT NULL,\n  expire_at  INTEGER NOT NULL,\n  created_at INTEGER NOT NULL,\n  claimed    INTEGER NOT NULL DEFAULT 0\n);");
        sb.AppendLine("CREATE INDEX IF NOT EXISTS idx_mail_player ON mail(player_id);");
        return sb.ToString();
    }
}

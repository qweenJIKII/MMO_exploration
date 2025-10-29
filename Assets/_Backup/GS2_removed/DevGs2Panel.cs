using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Project.Core.Online.Backend;

namespace Project.Dev
{
    // 開発用の簡易パネル。シーンに置くだけでOnGUIにボタンが出ます。
    public class DevGs2Panel : MonoBehaviour
    {
        private string _lastUserId = "";
        private string _grantItemId = "potion"; // テスト用デフォルト
        private int _grantQty = 1;
        private string _inboxLastMessageId = "";
        private string _log = "";

        [Header("UI Settings")]
        [SerializeField] private int _panelWidth = 640;
        [SerializeField] private int _fontSize = 16;
        [SerializeField] private int _lineHeight = 32;
        [SerializeField] private int _padding = 12;

        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _textFieldStyle;
        private GUIStyle _textAreaStyle;
        private Vector2 _logScroll;
        private Vector2 _inboxScroll;
        private Project.Core.Online.Backend.InboxMessageDto[] _inboxCache = Array.Empty<Project.Core.Online.Backend.InboxMessageDto>();

        // JSON整形ヘルパー（簡易、外部依存なし）
        private static string PrettyJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // JSONらしき場合のみ整形（先頭が { または [ ）
            char c = text.TrimStart().Length > 0 ? text.TrimStart()[0] : '\0';
            if (c != '{' && c != '[') return text;
            try
            {
                var sb = new System.Text.StringBuilder();
                int indent = 0;
                bool inString = false;
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];
                    if (ch == '"')
                    {
                        // 直前がエスケープでない場合のみトグル
                        bool escaped = i > 0 && text[i - 1] == '\\';
                        if (!escaped) inString = !inString;
                        sb.Append(ch);
                    }
                    else if (!inString && (ch == '{' || ch == '['))
                    {
                        sb.Append(ch);
                        sb.Append('\n');
                        indent++;
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (!inString && (ch == '}' || ch == ']'))
                    {
                        sb.Append('\n');
                        indent = Math.Max(0, indent - 1);
                        sb.Append(new string(' ', indent * 2));
                        sb.Append(ch);
                    }
                    else if (!inString && ch == ',')
                    {
                        sb.Append(ch);
                        sb.Append('\n');
                        sb.Append(new string(' ', indent * 2));
                    }
                    else if (!inString && ch == ':')
                    {
                        sb.Append(" : ");
                    }
                    else if (!inString && (ch == '\n' || ch == '\r' || ch == '\t'))
                    {
                        // 既存の改行/タブは無視
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                return sb.ToString();
            }
            catch { return text; }
        }

        // メタデータ(JSON)の簡易パーサ
        [Serializable]
        private class MetaReward { public string type; public string inventoryName; public string itemName; public int count; }
        [Serializable]
        private class MetaRoot { public string title; public string message; public MetaReward[] rewards; }
        private static bool TryParseMeta(string body, out string title, out string message, out string rewardsSummary)
        {
            title = null; message = null; rewardsSummary = null;
            if (string.IsNullOrEmpty(body)) return false;
            var trimmed = body.TrimStart();
            if (!(trimmed.StartsWith("{") || trimmed.StartsWith("["))) return false;
            try
            {
                var root = JsonUtility.FromJson<MetaRoot>(body);
                if (root == null) return false;
                title = root.title;
                message = root.message;
                if (root.rewards != null && root.rewards.Length > 0)
                {
                    var parts = root.rewards
                        .Where(r => r != null)
                        .Select(r =>
                        {
                            var inv = string.IsNullOrEmpty(r.inventoryName) ? "-" : r.inventoryName;
                            var item = string.IsNullOrEmpty(r.itemName) ? "-" : r.itemName;
                            var cnt = r.count;
                            return $"{inv}/{item} x {cnt}";
                        });
                    rewardsSummary = string.Join(", ", parts);
                }
                return !string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(message) || !string.IsNullOrEmpty(rewardsSummary);
            }
            catch { return false; }
        }

        private void Append(string msg)
        {
            _log = $"[{DateTime.Now:HH:mm:ss}] {msg}\n" + _log;
            Debug.Log(msg);
        }

        private async Task SafeRun(Func<Task> action)
        {
            try { await action(); }
            catch (Exception ex) { Append($"[DevGs2Panel] Error: {ex.Message}"); }
        }

        private void OnGUI()
        {
            // スタイルの初期化（大きめフォント）
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = _fontSize };
                _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = _fontSize, fixedHeight = _lineHeight };
                _textFieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = _fontSize, fixedHeight = _lineHeight };
                _textAreaStyle = new GUIStyle(GUI.skin.textArea) { fontSize = _fontSize, wordWrap = true };
            }

            var area = new Rect(_padding, _padding, _panelWidth, Screen.height - _padding * 2);
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("GS2 Dev Panel (Debug)", _labelStyle);

            if (GUILayout.Button("SignIn (Identifier)", _buttonStyle))
            {
                _ = SafeRun(async () =>
                {
                    var userId = await BackendService.Current.SignInAsync();
                    _lastUserId = userId ?? "(null)";
                    Append($"Signed in. userId={_lastUserId}");
                });
            }

            GUILayout.Space(6);
            GUILayout.Label("Inventory", _labelStyle);
            if (GUILayout.Button("GetInventory", _buttonStyle))
            {
                _ = SafeRun(async () =>
                {
                    var items = await BackendService.Current.GetInventoryAsync();
                    var summary = string.Join(", ", items.Select(i => $"{i.ItemId}:{i.Quantity}"));
                    Append($"Inventory -> [{summary}]");
                });
            }

            GUILayout.BeginHorizontal();
            _grantItemId = GUILayout.TextField(_grantItemId, _textFieldStyle, GUILayout.Width(260));
            _grantQty = int.TryParse(GUILayout.TextField(_grantQty.ToString(), _textFieldStyle, GUILayout.Width(80)), out var q) ? q : 1;
            if (GUILayout.Button("Grant", _buttonStyle))
            {
                _ = SafeRun(async () =>
                {
                    var idem = $"dev:{Guid.NewGuid():N}"; // 冪等キー
                    var ok = await BackendService.Current.GrantItemAsync(_grantItemId, _grantQty, idem);
                    Append($"Grant itemId={_grantItemId} qty={_grantQty} ok={ok} idem={idem}");
                });
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Inbox", _labelStyle);
            if (GUILayout.Button("ListMail", _buttonStyle))
            {
                _ = SafeRun(async () =>
                {
                    var list = await BackendService.Current.ListMailAsync();
                    var first = list.FirstOrDefault();
                    _inboxLastMessageId = first?.MessageId ?? "";
                    _inboxCache = list;
                    var cnt = list.Length;
                    Append($"Inbox: {cnt} messages. first={(first?.MessageId ?? "(none)")}");
                });
            }

            GUILayout.BeginHorizontal();
            _inboxLastMessageId = GUILayout.TextField(_inboxLastMessageId, _textFieldStyle, GUILayout.Width(_panelWidth - _padding * 2 - 120));
            if (GUILayout.Button("Receive", _buttonStyle))
            {
                _ = SafeRun(async () =>
                {
                    var idem = $"dev:{Guid.NewGuid():N}";
                    var ok = await BackendService.Current.ReceiveMailAsync(_inboxLastMessageId, idem);
                    Append($"Receive messageId={_inboxLastMessageId} ok={ok} idem={idem}");
                    // 受領後、自動で在庫を再取得
                    var items = await BackendService.Current.GetInventoryAsync();
                    var summary = string.Join(", ", items.Select(i => $"{i.ItemId}:{i.Quantity}"));
                    Append($"Inventory -> [{summary}]");
                });
            }
            GUILayout.EndHorizontal();

            // 受信箱の一覧（本文付き）
            if (_inboxCache != null && _inboxCache.Length > 0)
            {
                GUILayout.Space(6);
                GUILayout.Label($"Inbox Messages ({_inboxCache.Length})", _labelStyle);
                _inboxScroll = GUILayout.BeginScrollView(_inboxScroll, GUILayout.Height(240));
                foreach (var m in _inboxCache)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    // 受領済み/未受領で色分け
                    var oldColor = GUI.color;
                    if (m.IsReceived) GUI.color = new Color(0.8f, 1f, 0.8f); // 受領済み: 薄い緑
                    else if (m.IsRead) GUI.color = new Color(1f, 1f, 0.8f);   // 既読: 薄い黄
                    else GUI.color = Color.white;                              // 未読

                    GUILayout.Label($"Name: {m.MessageId}", _labelStyle);
                    GUILayout.Label($"Read: {m.IsRead}  Received: {m.IsReceived}  Expire: {(m.ExpiredAt.HasValue ? m.ExpiredAt.Value.ToString("u") : "-")}", _labelStyle);

                    // 要約表示（title/message/rewards）
                    if (TryParseMeta(m.Body, out var ttl, out var msg, out var rew))
                    {
                        if (!string.IsNullOrEmpty(ttl)) GUILayout.Label($"Title: {ttl}", _labelStyle);
                        if (!string.IsNullOrEmpty(msg)) GUILayout.Label($"Message: {msg}", _labelStyle);
                        if (!string.IsNullOrEmpty(rew)) GUILayout.Label($"Rewards: {rew}", _labelStyle);
                    }

                    GUILayout.Label("Body:", _labelStyle);
                    var shown = string.IsNullOrEmpty(m.Body) ? "(no metadata)" : PrettyJson(m.Body);
                    GUILayout.TextArea(shown, _textAreaStyle);
                    GUI.color = oldColor;
                    if (GUILayout.Button("Receive this", _buttonStyle))
                    {
                        var msgId = m.MessageId;
                        _ = SafeRun(async () =>
                        {
                            var idem = $"dev:{Guid.NewGuid():N}";
                            var ok = await BackendService.Current.ReceiveMailAsync(msgId, idem);
                            Append($"Receive messageId={msgId} ok={ok} idem={idem}");
                            // refresh list
                            var list = await BackendService.Current.ListMailAsync();
                            _inboxCache = list;
                            // 在庫も再取得
                            var items = await BackendService.Current.GetInventoryAsync();
                            var summary = string.Join(", ", items.Select(i => $"{i.ItemId}:{i.Quantity}"));
                            Append($"Inventory -> [{summary}]");
                        });
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }

            GUILayout.Space(8);
            GUILayout.Label("Log", _labelStyle);
            _logScroll = GUILayout.BeginScrollView(_logScroll, GUILayout.ExpandHeight(true));
            GUILayout.TextArea(_log, _textAreaStyle, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }
    }
}

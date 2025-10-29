// 開発者コンソール: `~`キーで開閉、コマンド実行
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;

namespace Project.Dev
{
    /// <summary>
    /// 開発者向けコンソールUI（`~`キーで開閉）
    /// </summary>
    public class DevConsole : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private int maxLogLines = 30;
        [SerializeField] private int consoleHeight = 400;
        [SerializeField] private int fontSize = 16;
        [SerializeField] private bool anchorToBottom = true; // 下部に固定

        private bool _isVisible = false;
        private string _inputText = "";
        private List<string> _logLines = new List<string>();
        private List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;
        private Vector2 _scrollPosition;

        private Dictionary<string, Action<string[]>> _commands;

        private void Awake()
        {
            RegisterCommands();
        }

        private void Update()
        {
            // New Input Systemを試行
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            bool f1Pressed = false;
            bool upPressed = false;
            bool downPressed = false;

            if (keyboard != null)
            {
                f1Pressed = keyboard.f1Key.wasPressedThisFrame;
                upPressed = keyboard.upArrowKey.wasPressedThisFrame;
                downPressed = keyboard.downArrowKey.wasPressedThisFrame;
            }
            else
            {
                // Keyboard.currentがnullの場合の警告（初回のみ）
                if (Time.frameCount == 1)
                {
                    UnityEngine.Debug.LogWarning("[DevConsole] Keyboard.current is null. Please set Active Input Handling to 'Both' in Project Settings > Player > Other Settings.");
                }
                
                // Fallback: 旧Input APIを試行（Input System Packageのみの場合は動作しない）
                try
                {
                    f1Pressed = Input.GetKeyDown(KeyCode.F1);
                    upPressed = Input.GetKeyDown(KeyCode.UpArrow);
                    downPressed = Input.GetKeyDown(KeyCode.DownArrow);
                }
                catch
                {
                    // Input Manager無効の場合は何もしない
                }
            }

            // F1キーでトグル
            if (f1Pressed)
            {
                _isVisible = !_isVisible;
                UnityEngine.Debug.Log($"[DevConsole] F1 pressed. Visible: {_isVisible}");
            }

            // コマンド履歴のナビゲーション（↑↓キー）
            if (_isVisible && _commandHistory.Count > 0)
            {
                if (upPressed)
                {
                    _historyIndex = Mathf.Max(0, _historyIndex - 1);
                    _inputText = _commandHistory[_historyIndex];
                }
                else if (downPressed)
                {
                    _historyIndex = Mathf.Min(_commandHistory.Count - 1, _historyIndex + 1);
                    _inputText = _commandHistory[_historyIndex];
                }
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            // フォントサイズを設定
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize };
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = fontSize };
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = fontSize };

            // コンソールウィンドウの位置を計算
            float yPos = anchorToBottom ? Screen.height - consoleHeight - 10 : 10;
            GUILayout.BeginArea(new Rect(10, yPos, Screen.width - 20, consoleHeight));
            GUILayout.BeginVertical("box");

            // ヘッダー
            GUILayout.Label("<b>Dev Console (F1 to toggle)</b>", new GUIStyle(labelStyle) { richText = true, fontSize = fontSize + 2 });
            GUILayout.Space(5);

            // ログ表示エリア
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(consoleHeight - 100));
            foreach (string line in _logLines.TakeLast(maxLogLines))
            {
                GUILayout.Label(line, labelStyle);
            }
            GUILayout.EndScrollView();

            // 入力エリア
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("ConsoleInput");
            _inputText = GUILayout.TextField(_inputText, textFieldStyle, GUILayout.ExpandWidth(true), GUILayout.Height(fontSize + 10));

            if (GUILayout.Button("Execute", buttonStyle, GUILayout.Width(120), GUILayout.Height(fontSize + 10)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "ConsoleInput"))
            {
                ExecuteCommand(_inputText);
                _inputText = "";
                GUI.FocusControl("ConsoleInput");
            }

            if (GUILayout.Button("Copy", buttonStyle, GUILayout.Width(100), GUILayout.Height(fontSize + 10)))
            {
                CopyLogsToClipboard();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();

            // 自動フォーカス
            if (_isVisible && GUI.GetNameOfFocusedControl() != "ConsoleInput")
            {
                GUI.FocusControl("ConsoleInput");
            }
        }

        private void RegisterCommands()
        {
            _commands = new Dictionary<string, Action<string[]>>
            {
                { "help", CmdHelp },
                { "clear", CmdClear },
                { "copy", CmdCopy },
                { "quit", CmdQuit },
                { "ugs.auth", CmdUgsAuth },
                { "ugs.save.get", CmdUgsSaveGet },
                { "ugs.save.set", CmdUgsSaveSet },
                { "ugs.save.list", CmdUgsSaveList },
                { "fps", CmdFps },
                { "time.scale", CmdTimeScale }
            };

            Log("DevConsole initialized. Type 'help' for available commands.");
        }

        private void ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            Log($"> {input}");
            _commandHistory.Add(input);
            _historyIndex = _commandHistory.Count;

            string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string commandName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            if (_commands.TryGetValue(commandName, out Action<string[]> command))
            {
                try
                {
                    command(args);
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");
                }
            }
            else
            {
                Log($"Unknown command: {commandName}. Type 'help' for available commands.");
            }
        }

        private void Log(string message)
        {
            _logLines.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            _scrollPosition = new Vector2(0, float.MaxValue); // 自動スクロール
        }

        private void CopyLogsToClipboard()
        {
            if (_logLines.Count == 0)
            {
                Log("No logs to copy.");
                return;
            }

            string allLogs = string.Join("\n", _logLines);
            GUIUtility.systemCopyBuffer = allLogs;
            Log($"Copied {_logLines.Count} lines to clipboard.");
        }

        // ========== コマンド実装 ==========

        private void CmdHelp(string[] args)
        {
            Log("Available commands:");
            Log("  help - Show this help message");
            Log("  clear - Clear console");
            Log("  copy - Copy all logs to clipboard");
            Log("  quit - Quit application");
            Log("  ugs.auth - Show UGS authentication status");
            Log("  ugs.save.get <key> - Get Cloud Save value");
            Log("  ugs.save.set <key> <value> - Set Cloud Save value");
            Log("  ugs.save.list - List all Cloud Save keys");
            Log("  fps - Show current FPS");
            Log("  time.scale <value> - Set Time.timeScale");
        }

        private void CmdClear(string[] args)
        {
            _logLines.Clear();
        }

        private void CmdCopy(string[] args)
        {
            CopyLogsToClipboard();
        }

        private void CmdQuit(string[] args)
        {
            Log("Quitting application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void CmdUgsAuth(string[] args)
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Log($"Authenticated as: {AuthenticationService.Instance.PlayerId}");
            }
            else
            {
                Log("Not authenticated.");
            }
        }

        private async void CmdUgsSaveGet(string[] args)
        {
            if (args.Length == 0)
            {
                Log("Usage: ugs.save.get <key>");
                return;
            }

            try
            {
                string key = args[0];
                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
                if (data.TryGetValue(key, out var item))
                {
                    Log($"{key} = {item.Value.GetAsString()}");
                }
                else
                {
                    Log($"Key not found: {key}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private async void CmdUgsSaveSet(string[] args)
        {
            if (args.Length < 2)
            {
                Log("Usage: ugs.save.set <key> <value>");
                return;
            }

            try
            {
                string key = args[0];
                string value = string.Join(" ", args.Skip(1));
                var data = new Dictionary<string, object> { { key, value } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                Log($"Saved: {key} = {value}");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private async void CmdUgsSaveList(string[] args)
        {
            try
            {
                var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
                if (keys.Count == 0)
                {
                    Log("No keys found.");
                }
                else
                {
                    Log($"Cloud Save keys ({keys.Count}):");
                    foreach (var key in keys)
                    {
                        Log($"  - {key}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private void CmdFps(string[] args)
        {
            float fps = 1.0f / Time.unscaledDeltaTime;
            Log($"FPS: {fps:F1}");
        }

        private void CmdTimeScale(string[] args)
        {
            if (args.Length == 0)
            {
                Log($"Current Time.timeScale: {Time.timeScale}");
                return;
            }

            if (float.TryParse(args[0], out float scale))
            {
                Time.timeScale = Mathf.Max(0, scale);
                Log($"Time.timeScale set to: {Time.timeScale}");
            }
            else
            {
                Log("Invalid value. Usage: time.scale <value>");
            }
        }
    }
}

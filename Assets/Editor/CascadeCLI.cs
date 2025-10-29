// Copyright (c) 2025
// Cascade Unity MCP CLI entry points
// 目的: Unityをバッチモードから安全に操作し、標準出力へJSONを返すためのEditor拡張
// 注意: Addressables未導入でもコンパイルが落ちないようにガードしています。

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

#if UNITY_2020_1_OR_NEWER
using UnityEditor.TestTools.TestRunner.Api;
#endif

namespace Cascade.UnityMCP
{
    // シンプルなJSONユーティリティ（依存を増やさないため簡易実装）
    internal static class Json
    {
        public static string Escape(string s)
        {
            if (s == null) return "null";
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
        }
        public static string Bool(bool b) => b ? "true" : "false";
        public static string Obj(params (string key, string value)[] kv)
        {
            return "{" + string.Join(",", kv.Select(p => Escape(p.key) + ":" + (p.value ?? "null"))) + "}";
        }
        public static string Arr(IEnumerable<string> items)
        {
            return "[" + string.Join(",", items) + "]";
        }
    }

    internal static class CliArgs
    {
        // コマンドライン引数から"-key value"形式を抽出
        public static string Get(string key, string defaultValue = null)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length) return args[i + 1];
                    return string.Empty; // フラグのみ
                }
            }
            return defaultValue;
        }

        public static bool Has(string key)
        {
            var args = Environment.GetCommandLineArgs();
            return args.Any(a => string.Equals(a, key, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class CascadeCLI
    {
        // JSONを標準出力へ統一フォーマットで出す
        private static void PrintOk(object data = null)
        {
            string payload = data as string ?? "null";
            Debug.Log("CASCADE_JSON:" + Json.Obj(("ok", "true"), ("data", payload), ("error", "null")));
            Console.WriteLine("CASCADE_JSON:" + Json.Obj(("ok", "true"), ("data", payload), ("error", "null")));
        }

        private static void PrintErr(string message, object data = null)
        {
            string payload = data as string ?? "null";
            string err = Json.Escape(message);
            Debug.LogError("CASCADE_JSON:" + Json.Obj(("ok", "false"), ("data", payload), ("error", err)));
            Console.WriteLine("CASCADE_JSON:" + Json.Obj(("ok", "false"), ("data", payload), ("error", err)));
        }

        // 1) アセットリフレッシュ
        public static void RefreshAssets()
        {
            try
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                var summary = Json.Obj(("refreshed", Json.Bool(true)), ("time", Json.Escape(DateTime.UtcNow.ToString("o"))));
                PrintOk(summary);
            }
            catch (Exception ex)
            {
                PrintErr("RefreshAssets failed: " + ex.Message);
                EditorApplication.Exit(1);
            }
        }

        // 2) シーン切替（-scenePath <path>）
        public static void OpenScene()
        {
            string scenePath = CliArgs.Get("-scenePath");
            if (string.IsNullOrEmpty(scenePath))
            {
                PrintErr("-scenePath is required.");
                EditorApplication.Exit(2);
                return;
            }
            try
            {
                var opened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var summary = Json.Obj(("scene", Json.Escape(opened.path)));
                PrintOk(summary);
            }
            catch (Exception ex)
            {
                PrintErr("OpenScene failed: " + ex.Message, Json.Obj(("scene", Json.Escape(scenePath))));
                EditorApplication.Exit(3);
            }
        }

        // 3) テスト実行（-testMode editmode|playmode, -resultsPath <file>）
        public static void RunTests()
        {
#if UNITY_2020_1_OR_NEWER
            string mode = (CliArgs.Get("-testMode", "editmode") ?? "editmode").ToLowerInvariant();
            string resultsPath = CliArgs.Get("-resultsPath", "TestResults/results.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(resultsPath) ?? ".");

            var api = new TestRunnerApi();
            var filter = new Filter()
            {
                testMode = mode == "playmode" ? TestMode.PlayMode : TestMode.EditMode,
            };

            int total = 0, passed = 0, failed = 0, inconclusive = 0, skipped = 0;
            var completed = false;

            api.RegisterCallbacks(new CallbackHandler(
                onTestStarted: _ => { },
                onTestFinished: r =>
                {
                    total++;
                    switch (r.TestStatus)
                    {
                        case TestStatus.Passed: passed++; break;
                        case TestStatus.Failed: failed++; break;
                        case TestStatus.Inconclusive: inconclusive++; break;
                        case TestStatus.Skipped: skipped++; break;
                    }
                },
                onRunFinished: _ => { completed = true; }
            ));

            api.Execute(new ExecutionSettings(filter));

            // 同期待ち（バッチモード想定で適当なポーリング）
            var start = DateTime.UtcNow;
            while (!completed)
            {
                System.Threading.Thread.Sleep(100);
                if ((DateTime.UtcNow - start).TotalMinutes > 30)
                {
                    PrintErr("Test run timeout.");
                    EditorApplication.Exit(4);
                    return;
                }
            }

            // 簡易XML（NUnit風）: 依存回避のため最小限
            try
            {
                var xml = $"<test-run total=\"{total}\" passed=\"{passed}\" failed=\"{failed}\" inconclusive=\"{inconclusive}\" skipped=\"{skipped}\" />";
                File.WriteAllText(resultsPath, xml);
            }
            catch (Exception ex)
            {
                PrintErr("Failed to write results: " + ex.Message);
                EditorApplication.Exit(5);
                return;
            }

            var data = Json.Obj(
                ("mode", Json.Escape(mode)),
                ("total", total.ToString()),
                ("passed", passed.ToString()),
                ("failed", failed.ToString()),
                ("skipped", skipped.ToString()),
                ("resultsPath", Json.Escape(resultsPath))
            );
            PrintOk(data);
#else
            PrintErr("RunTests requires Unity 2020.1 or newer.");
            EditorApplication.Exit(6);
#endif
        }

        // 4) プレイヤービルド（-buildTarget Win64|... , -outputPath <path>）
        public static void BuildPlayer()
        {
            string buildTargetStr = CliArgs.Get("-buildTarget", "Win64");
            string outputPath = CliArgs.Get("-outputPath", Path.Combine("Builds", "Output"));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                PrintErr("No scenes enabled in EditorBuildSettings.");
                EditorApplication.Exit(7);
                return;
            }

            BuildTarget target = MapBuildTarget(buildTargetStr);
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                locationPathName = outputPath,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            var data = Json.Obj(
                ("result", Json.Escape(summary.result.ToString())),
                ("totalSize", summary.totalSize.ToString()),
                ("outputPath", Json.Escape(outputPath)),
                ("target", Json.Escape(target.ToString()))
            );

            if (summary.result == BuildResult.Succeeded)
            {
                PrintOk(data);
            }
            else
            {
                PrintErr("Build failed.", data);
                EditorApplication.Exit(8);
            }
        }

        private static BuildTarget MapBuildTarget(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "win":
                case "win64":
                case "windows":
                case "windows64":
                    return BuildTarget.StandaloneWindows64;
                case "win32":
                    return BuildTarget.StandaloneWindows;
                case "mac":
                case "macos":
                    return BuildTarget.StandaloneOSX;
                case "linux":
                case "linux64":
                    return BuildTarget.StandaloneLinux64;
                case "android":
                    return BuildTarget.Android;
                case "ios":
                    return BuildTarget.iOS;
                default:
                    return EditorUserBuildSettings.activeBuildTarget;
            }
        }

        // 5) Addressablesビルド（-profileName <name>）
        public static void BuildAddressables()
        {
#if ADDRESSABLES_PRESENT || UNITY_ADDRESSABLES
            try
            {
                string profile = CliArgs.Get("-profileName", "Default");
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    PrintErr("Addressables settings not found. Is Addressables set up?");
                    EditorApplication.Exit(9);
                    return;
                }
                settings.ActiveProfileId = settings.profileSettings.GetProfileId(profile) ?? settings.ActiveProfileId;
                AddressableAssetSettings.CleanPlayerContent();
                var task = AddressableAssetSettings.BuildPlayerContentAsync();
                while (!task.IsDone)
                {
                    System.Threading.Thread.Sleep(100);
                }
                if (task.Result != null && task.Result.Error != null)
                {
                    PrintErr("Addressables build error: " + task.Result.Error);
                    EditorApplication.Exit(10);
                    return;
                }
                var data = Json.Obj(("profile", Json.Escape(profile)));
                PrintOk(data);
            }
            catch (Exception ex)
            {
                PrintErr("BuildAddressables failed: " + ex.Message);
                EditorApplication.Exit(11);
            }
#else
            // Addressablesが無い環境向けの安全な応答
            PrintErr("Addressables package not detected. Install 'com.unity.addressables' to use BuildAddressables.");
            EditorApplication.Exit(12);
#endif
        }

        // ヘルプ: 使用できるメソッドを列挙
        public static void Help()
        {
            var methods = new[] { "RefreshAssets", "OpenScene", "RunTests", "BuildPlayer", "BuildAddressables", "Help" };
            var data = Json.Obj(("methods", Json.Arr(methods.Select(Json.Escape))));
            PrintOk(data);
        }

#region Editor Menu
        // Editorメニューから簡単に呼べるショートカット。GUI操作時の利便性向上用。

        [MenuItem("Tools/Cascade/Refresh Assets", priority = 1000)]
        public static void Menu_RefreshAssets()
        {
            RefreshAssets();
        }

        [MenuItem("Tools/Cascade/Open SampleScene", priority = 1001)]
        public static void Menu_OpenSampleScene()
        {
            // プロジェクト標準のサンプルシーンに合わせたデフォルトパス
            var defaultScene = "Assets/Scenes/SampleScene.unity";
            // 直接OpenScene()を呼ぶとコマンドライン引数が無いので、内部APIをそのまま使う
            try
            {
                var opened = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(defaultScene, OpenSceneMode.Single);
                Debug.Log($"Opened scene: {opened.path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to open scene '{defaultScene}': {ex.Message}");
            }
        }

        [MenuItem("Tools/Cascade/Run Tests (EditMode)", priority = 1002)]
        public static void Menu_RunTests_EditMode()
        {
            // RunTestsはデフォルトでEditModeかつ TestResults/results.xml を出力
            RunTests();
        }

        [MenuItem("Tools/Cascade/Build Addressables", priority = 1003)]
        public static void Menu_BuildAddressables()
        {
            BuildAddressables();
        }

        [MenuItem("Tools/Cascade/Build Player (Defaults)", priority = 1004)]
        public static void Menu_BuildPlayer_Defaults()
        {
            // 既定：buildTarget=Win64, outputPath=Builds/Output
            // エディタOSや対象に応じて変わる場合はCLIで明示指定推奨
            BuildPlayer();
        }

        [MenuItem("Tools/Cascade/Help", priority = 1099)]
        public static void Menu_Help()
        {
            Help();
        }
#endregion

#if UNITY_2020_1_OR_NEWER
        private class CallbackHandler : ICallbacks
        {
            private readonly Action<ITestAdaptor> _onRunStarted;
            private readonly Action<ITestResultAdaptor> _onRunFinished;
            private readonly Action<ITestAdaptor> _onTestStarted;
            private readonly Action<ITestResultAdaptor> _onTestFinished;

            public CallbackHandler(Action<ITestAdaptor> onRunStarted = null, Action<ITestResultAdaptor> onRunFinished = null,
                                   Action<ITestAdaptor> onTestStarted = null, Action<ITestResultAdaptor> onTestFinished = null)
            {
                _onRunStarted = onRunStarted ?? (_ => { });
                _onRunFinished = onRunFinished ?? (_ => { });
                _onTestStarted = onTestStarted ?? (_ => { });
                _onTestFinished = onTestFinished ?? (_ => { });
            }
            public void RunStarted(ITestAdaptor testsToRun) => _onRunStarted(testsToRun);
            public void RunFinished(ITestResultAdaptor result) => _onRunFinished(result);
            public void TestStarted(ITestAdaptor test) => _onTestStarted(test);
            public void TestFinished(ITestResultAdaptor result) => _onTestFinished(result);
        }
#endif
    }
}
#endif

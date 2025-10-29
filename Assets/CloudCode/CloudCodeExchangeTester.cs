using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.HelloWorld;

public class CloudCodeExchangeTester : MonoBehaviour
{
    private string _fromId = "Gold";
    private string _toId = "Electrum";
    private long _amount = 1000L;
    private bool _execute = false;
    private string _idemKey = "";
    private string _lastLog = "";
    private bool _busy = false;
    private string _grantCurrencyId = "Gold";
    private long _grantAmount = 2000L;
    private string _lastDiag = "";
    private string _saClientId = "";   // DEV: Service Account Client ID
    private string _saClientSecret = ""; // DEV: Service Account Client Secret
    private string _output = ""; // 日本語コメント: 実行結果の表示/コピー用
    private Vector2 _scrollLeft;
    private Vector2 _scrollRight;

    // 日本語コメント: ログを横取りしてUIに出し、シーン変更でも消えないよう維持
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Application.logMessageReceived += HandleLog;
    }

    private async Task RunDiagCloudSaveAdminWriteV3Async()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // UGS 2025パターン: 生成バインディングが利用可能になるまでCallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var res = await svc.CallModuleEndpointAsync<string>(
                "exchange_module",
                "DiagCloudSaveAdminWriteV3",
                null
            );
            _lastLog = "DiagCloudSaveAdminWriteV3 Done";
            _output = res;
        }
        catch (CloudCodeException cce)
        {
            _lastLog = $"DiagCloudSaveAdminWriteV3 CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastLog;
        }
        catch (Exception e)
        {
            _lastLog = $"DiagCloudSaveAdminWriteV3 ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task RunDiagGameApiClientPingAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // UGS 2025パターン: 生成バインディングが利用可能になるまでCallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var res = await svc.CallModuleEndpointAsync<string>(
                "exchange_module",
                "DiagGameApiClientPing",
                null
            );
            _lastLog = "DiagGameApiClientPing Done";
            _output = res;
        }
        catch (CloudCodeException cce)
        {
            _lastLog = $"DiagGameApiClientPing CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastLog;
        }
        catch (Exception e)
        {
            _lastLog = $"DiagGameApiClientPing ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task RunDiagCloudSaveAdminWriteV2Async()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // UGS 2025パターン: 生成バインディングが利用可能になるまでCallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var res = await svc.CallModuleEndpointAsync<string>(
                "exchange_module",
                "DiagCloudSaveAdminWriteV2",
                null
            );
            _lastLog = "DiagCloudSaveAdminWriteV2 Done";
            _output = res;
        }
        catch (CloudCodeException cce)
        {
            _lastLog = $"DiagCloudSaveAdminWriteV2 CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastLog;
        }
        catch (Exception e)
        {
            _lastLog = $"DiagCloudSaveAdminWriteV2 ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task RunDiagCloudSaveAdminWriteAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // UGS 2025パターン: 生成バインディングが利用可能になるまでCallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var res = await svc.CallModuleEndpointAsync<string>(
                "exchange_module",
                "DiagCloudSaveAdminWrite",
                null
            );
            _output = res;
            _lastLog = "DiagCloudSaveAdminWrite Done";
        }
        catch (CloudCodeException cce)
        {
            _lastLog = $"DiagCloudSaveAdminWrite CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastLog;
        }
        catch (Exception e)
        {
            _lastLog = $"DiagCloudSaveAdminWrite ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task RunDiagEconomyAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // UGS 2025パターン: 生成バインディングが利用可能になるまでCallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var res = await svc.CallModuleEndpointAsync<string>(
                "exchange_module",
                "DiagEconomy",
                null
            );
            _output = res;
            _lastLog = "DiagEconomy Done";
        }
        catch (CloudCodeException cce)
        {
            _lastLog = $"DiagEconomy CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastLog;
        }
        catch (Exception e)
        {
            _lastLog = $"DiagEconomy ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task RunDiagAllAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // UGS 2025パターン: 生成バインディングが利用可能になるまでCallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var res = await svc.CallModuleEndpointAsync<string>(
                "exchange_module",
                "DiagAll",
                null
            );
            _output = res;
            _lastLog = "DiagAll Done";
        }
        catch (CloudCodeException cce)
        {
            _lastLog = $"DiagAll CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastLog;
        }
        catch (Exception e)
        {
            _lastLog = $"DiagAll ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
        finally
        {
            _busy = false;
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            _output = $"{type}: {condition}\n{stackTrace}";
        }
    }

    // 日本語コメント: 起動時の初期化を安全に実行
    private async void Start()
    {
        try
        {
            await EnsureServicesAsync();
            // 日本語コメント: UgsInitializer 等が並行でサインインしている場合に備え、サインイン完了を待つ
            await WaitForAuthorizationAsync();
            _lastLog = "Ready";
        }
        catch (Exception e)
        {
            _lastLog = $"Init ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
    }

    private async Task RunCheckTokenAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // UGS 2025パターン: 生成バインディングが利用可能になるまでCallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var res = await svc.CallModuleEndpointAsync<System.Collections.Generic.Dictionary<string, object>>(
                "exchange_module",
                "CheckToken",
                null
            );

            object v;
            res.TryGetValue("managedTokenPresent", out v); var managed = v;
            res.TryGetValue("accessTokenPresent", out v); var access = v;
            res.TryGetValue("serviceTokenPresent", out v); var service = v;
            res.TryGetValue("projectId", out v); var proj = v;
            res.TryGetValue("environmentId", out v); var env = v;
            _lastDiag = $"CheckToken: managed={managed}, access={access}, service={service}, projectId={proj}, env={env}";
            _output = _lastDiag;
        }
        catch (CloudCodeException cce)
        {
            _lastDiag = $"CheckToken CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastDiag;
        }
        catch (Exception e)
        {
            _lastDiag = $"CheckToken ERR {e.GetType().Name}: {e.Message}";
            _output = _lastDiag;
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task RunCheckSecretsAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // 注意: CheckSecrets関数がExample.csに存在しない場合は、旧式メソッドを使用
            var svc = CloudCodeService.Instance;
            var res = await svc.CallModuleEndpointAsync<System.Collections.Generic.Dictionary<string, object>>(
                "exchange_module",
                "CheckSecrets",
                null
            );

            object v;
            res.TryGetValue("clientIdSet", out v); var idSet = v;
            res.TryGetValue("clientSecretSet", out v); var secSet = v;
            res.TryGetValue("projectId", out v); var proj = v;
            res.TryGetValue("environmentId", out v); var env = v;
            _lastDiag = $"CheckSecrets: clientIdSet={idSet}, clientSecretSet={secSet}, projectId={proj}, env={env}";
            _output = _lastDiag;
        }
        catch (CloudCodeException cce)
        {
            _lastDiag = $"CheckSecrets CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastDiag;
        }
        catch (Exception e)
        {
            _lastDiag = $"CheckSecrets ERR {e.GetType().Name}: {e.Message}";
            _output = _lastDiag;
        }
        finally
        {
            _busy = false;
        }
    }

    private async Task RunGrantAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // 注意: GrantCurrencyDebug関数がExample.csに存在しない場合は、旧式メソッドを使用
            // 型安全なバインディングが生成されていない可能性があるため、CallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var args = new System.Collections.Generic.Dictionary<string, object>
            {
                {
                    "req",
                    new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "playerId", AuthenticationService.Instance.PlayerId },
                        { "currencyId", _grantCurrencyId },
                        { "amount", _grantAmount },
                        { "saClientId", string.IsNullOrWhiteSpace(_saClientId) ? null : _saClientId.Trim() },
                        { "saClientSecret", string.IsNullOrWhiteSpace(_saClientSecret) ? null : _saClientSecret.Trim() }
                    }
                }
            };

            var res = await svc.CallModuleEndpointAsync<System.Collections.Generic.Dictionary<string, object>>(
                "exchange_module",
                "GrantCurrencyDebug",
                args
            );

            _lastLog = "Grant OK (UGS 2025)";
            _output = _lastLog;
        }
        catch (CloudCodeException cce)
        {
            _lastLog = $"Grant CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastLog;
        }
        catch (Exception e)
        {
            _lastLog = $"Grant ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
        finally
        {
            _busy = false;
        }
    }

    private static async Task EnsureServicesAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        // 日本語コメント: すでに他所でサインイン進行/完了している場合はスキップ
        if (AuthenticationService.Instance.IsSignedIn || AuthenticationService.Instance.IsAuthorized)
        {
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException)
        {
            // 進行中や二重呼び出し時は無視
        }
    }

    // 日本語コメント: 認証完了(AccessToken発行)まで待機するユーティリティ
    private static async Task WaitForAuthorizationAsync(int timeoutMs = 10000)
    {
        var start = System.Diagnostics.Stopwatch.StartNew();
        while (!AuthenticationService.Instance.IsAuthorized)
        {
            if (start.ElapsedMilliseconds > timeoutMs)
            {
                throw new System.TimeoutException("Authentication did not complete in time.");
            }
            await Task.Delay(100);
        }
    }

    private async Task RunExchangeAsync()
    {
        if (_busy) return;
        _busy = true;
        try
        {
            // 日本語コメント: 実行時にIdempotencyKeyが空なら自動生成
            if (_execute && string.IsNullOrWhiteSpace(_idemKey))
            {
                _idemKey = Guid.NewGuid().ToString("N");
            }
            var idem = string.IsNullOrWhiteSpace(_idemKey) ? null : _idemKey.Trim();

            // 注意: ExchangeV2関数がExample.csに存在しない場合は、旧式メソッドを使用
            // 型安全なバインディングが生成されていない可能性があるため、CallModuleEndpointAsyncを使用
            var svc = CloudCodeService.Instance;
            var args = new System.Collections.Generic.Dictionary<string, object>
            {
                {
                    "req",
                    new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "playerId", AuthenticationService.Instance.PlayerId },
                        { "from", new System.Collections.Generic.Dictionary<string, object> { { "currencyId", _fromId }, { "amount", _amount } } },
                        { "toCurrencyId", _toId },
                        { "execute", _execute },
                        { "idempotencyKey", idem },
                        { "saClientId", string.IsNullOrWhiteSpace(_saClientId) ? null : _saClientId.Trim() },
                        { "saClientSecret", string.IsNullOrWhiteSpace(_saClientSecret) ? null : _saClientSecret.Trim() }
                    }
                }
            };

            var res = await svc.CallModuleEndpointAsync<System.Collections.Generic.Dictionary<string, object>>(
                "exchange_module",
                "ExchangeV2",
                args
            );

            object v;
            res.TryGetValue("credited", out v); var credited = v;
            res.TryGetValue("debited", out v); var debited = v;
            res.TryGetValue("feeApplied", out v); var feeApplied = v;
            res.TryGetValue("rateUsed", out v); var rateUsed = v;
            _lastLog = $"OK (UGS 2025) credited={credited}, debited={debited}, fee={feeApplied}, rate={rateUsed}";
            _output = _lastLog;

            if (_execute && idem != null) _idemKey = idem;
        }
        catch (CloudCodeException cce)
        {
            _lastLog = $"CloudCodeException: {cce.ErrorCode} - {cce.Message}";
            _output = _lastLog;
        }
        catch (Exception e)
        {
            _lastLog = $"ERR {e.GetType().Name}: {e.Message}";
            _output = _lastLog;
        }
        finally
        {
            _busy = false;
        }
    }

    private void OnGUI()
    {
        // 日本語コメント: 画面全体を使用し、左に操作パネル、右に結果表示エリアを配置
        var margin = 12f;
        var full = new Rect(margin, margin, Screen.width - margin * 2, Screen.height - margin * 2);
        GUILayout.BeginArea(full);
        GUILayout.BeginHorizontal();

        // 左カラム（操作パネル）
        GUILayout.BeginVertical(GUI.skin.window, GUILayout.Width(Mathf.Max(420f, full.width * 0.45f)));
        GUILayout.Label("Cloud Code ExchangeV2 Tester");

        _scrollLeft = GUILayout.BeginScrollView(_scrollLeft, false, true, GUILayout.Height(full.height - 80f));

        GUILayout.Label("From CurrencyId");
        _fromId = GUILayout.TextField(_fromId);

        GUILayout.Label("To CurrencyId");
        _toId = GUILayout.TextField(_toId);

        GUILayout.Label("Amount");
        var amtStr = GUILayout.TextField(_amount.ToString());
        if (long.TryParse(amtStr, out var parsed)) _amount = parsed;

        _execute = GUILayout.Toggle(_execute, "execute (true to run)");

        GUILayout.BeginHorizontal();
        GUILayout.Label("IdempotencyKey");
        _idemKey = GUILayout.TextField(_idemKey);
        if (GUILayout.Button("Generate", GUILayout.Width(90)))
        {
            _idemKey = Guid.NewGuid().ToString("N");
        }
        GUILayout.EndHorizontal();

        GUI.enabled = !_busy;
        if (GUILayout.Button(_busy ? "Running..." : "Call ExchangeV2"))
        {
            _ = RunExchangeAsync();
        }
        GUI.enabled = true;

        GUILayout.Space(12);
        GUILayout.Label("Grant Currency (debug)");
        GUILayout.BeginHorizontal();
        GUILayout.Label("CurrencyId", GUILayout.Width(80));
        _grantCurrencyId = GUILayout.TextField(_grantCurrencyId);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Amount", GUILayout.Width(80));
        var gAmtStr = GUILayout.TextField(_grantAmount.ToString());
        if (long.TryParse(gAmtStr, out var gParsed)) _grantAmount = gParsed;
        GUILayout.EndHorizontal();

        GUI.enabled = !_busy;
        if (GUILayout.Button(_busy ? "Granting..." : "GrantCurrencyDebug"))
        {
            _ = RunGrantAsync();
        }
        GUI.enabled = true;

        GUILayout.Space(12);
        GUILayout.Label("Service Account (DEV override)");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Client ID", GUILayout.Width(80));
        _saClientId = GUILayout.TextField(_saClientId);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Client Secret", GUILayout.Width(80));
        _saClientSecret = GUILayout.TextField(_saClientSecret);
        GUILayout.EndHorizontal();

        GUILayout.Space(12);
        GUILayout.Label("Diagnostics");
        GUI.enabled = !_busy;
        if (GUILayout.Button(_busy ? "Checking..." : "CheckSecrets"))
        {
            _ = RunCheckSecretsAsync();
        }
        GUI.enabled = !_busy;
        if (GUILayout.Button(_busy ? "Checking..." : "CheckToken"))
        {
            _ = RunCheckTokenAsync();
        }
        if (GUILayout.Button(_busy ? "Diagnosing..." : "DiagAll (Cloud Save reachability)"))
        {
            _ = RunDiagAllAsync();
        }
        if (GUILayout.Button(_busy ? "Diagnosing..." : "DiagCloudSaveAdminWrite (SA token)"))
        {
            _ = RunDiagCloudSaveAdminWriteAsync();
        }
        if (GUILayout.Button(_busy ? "Diagnosing..." : "DiagCloudSaveAdminWrite V2 (IGameApiClient)"))
        {
            _ = RunDiagCloudSaveAdminWriteV2Async();
        }
        if (GUILayout.Button(_busy ? "Diagnosing..." : "DiagGameApiClientPing (DI check)"))
        {
            _ = RunDiagGameApiClientPingAsync();
        }
        if (GUILayout.Button(_busy ? "Diagnosing..." : "DiagCloudSaveAdminWrite V3 (IGameApiClient)"))
        {
            _ = RunDiagCloudSaveAdminWriteV3Async();
        }
        if (GUILayout.Button(_busy ? "Diagnosing..." : "GetPlayerCurrencies (Economy v2)"))
        {
            _ = RunDiagEconomyAsync();
        }
        GUI.enabled = true;

        GUILayout.Space(8);
        GUILayout.Label(_lastLog);
        if (!string.IsNullOrEmpty(_lastDiag))
        {
            GUILayout.Label(_lastDiag);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // 右カラム（出力表示・コピー）
        GUILayout.Space(8);
        GUILayout.BeginVertical(GUI.skin.window, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.Label("Output");
        _scrollRight = GUILayout.BeginScrollView(_scrollRight, true, true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        var outputShown = _output ?? string.Empty;
        // 日本語コメント: 大きな非編集テキスト領域
        GUILayout.TextArea(outputShown, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy Output", GUILayout.Width(140)))
        {
            GUIUtility.systemCopyBuffer = outputShown;
        }
        if (GUILayout.Button("Clear", GUILayout.Width(100)))
        {
            _output = string.Empty;
            _lastLog = string.Empty;
            _lastDiag = string.Empty;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}

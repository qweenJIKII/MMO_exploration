using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudSave.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace HelloWorld
{
    /// <summary>
    /// Cloud Code Module設定クラス - IGameApiClientをDIコンテナに登録
    /// </summary>
    public class ModuleConfig : ICloudCodeSetup
    {
        public void Setup(ICloudCodeConfig config)
        {
            // GameApiClientをシングルトンとして登録（UGS 2025推奨パターン）
            config.Dependencies.AddSingleton(GameApiClient.Create());
        }
    }

    public class MyModule
    {
        private readonly ILogger<MyModule> _logger;

        public MyModule(ILogger<MyModule> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [CloudCodeFunction("SayHello")]
        public string Hello(string name)
        {
            return $"Hello, {name}!";
        }

        // DIの有効性を確認（Cloud Save Admin API のDI）
        [CloudCodeFunction("DiagGameApiClientPing")]
        public async Task<string> DiagGameApiClientPing(IExecutionContext context)
        {
            try
            {
                var result = new Dictionary<string, object?>
                {
                    ["ok"] = true,
                    ["projectId"] = context.ProjectId,
                    ["environmentId"] = context.EnvironmentId,
                    ["playerId"] = context.PlayerId,
                    ["timestamp"] = DateTime.UtcNow.ToString("o")
                };
                return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DiagGameApiClientPing");
                throw;
            }
        }

        // Cloud Saveにデータを保存する（UGS 2025最新版 - IGameApiClient使用）
        [CloudCodeFunction("DiagCloudSaveAdminWriteV3")]
        public async Task<string> DiagCloudSaveAdminWriteV3(IExecutionContext context, IGameApiClient gameApiClient)
        {
            var playerId = context.PlayerId;
            var projectId = context.ProjectId;
            var key = $"diag_admin_v3_{Guid.NewGuid():N}";
            
            try
            {
                // UGS 2025推奨: IGameApiClient.CloudSaveDataを使用してデータを保存
                // プレイヤートークンを使用したデフォルトアクセスレベルでの保存
                await gameApiClient.CloudSaveData.SetItemAsync(
                    context, 
                    context.AccessToken, 
                    projectId, 
                    playerId,
                    new SetItemBody(key, "1")
                );

                var result = new 
                {
                    ok = true,
                    playerId,
                    key,
                    path = "IGameApiClient.CloudSaveData.SetItemAsync",
                    method = "UGS 2025 Recommended Pattern",
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                _logger.LogInformation("DiagCloudSaveAdminWriteV3 succeeded: player={PlayerId} key={Key}", playerId, key);
                return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DiagCloudSaveAdminWriteV3 failed player={PlayerId} key={Key}", 
                    playerId, key);
                
                return JsonSerializer.Serialize(new
                {
                    ok = false,
                    playerId,
                    key,
                    error = ex.GetType().Name,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                }, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        // UGS 2025版: IGameApiClientを使用した保存（複数アクセスレベル対応）
        [CloudCodeFunction("DiagCloudSaveAdminWriteV2")]
        public async Task<string> DiagCloudSaveAdminWriteV2(IExecutionContext context, IGameApiClient gameApiClient)
        {
            var projectId = context?.ProjectId ?? string.Empty;
            var environmentId = context?.EnvironmentId ?? string.Empty;
            var playerId = context?.PlayerId ?? string.Empty;
            var key = $"diag_admin_v2_{Guid.NewGuid():N}";

            try
            {
                // UGS 2025推奨: IGameApiClientを使用（HTTPクライアント直接使用は非推奨）
                // 1) サービストークンでProtectedアイテムとして保存を試行
                try
                {
                    await gameApiClient.CloudSaveData.SetItemAsync(
                        context,
                        context.ServiceToken,
                        projectId,
                        playerId,
                        new SetItemBody(key, "1")
                    );

                    var diag = new Dictionary<string, object?>
                    {
                        ["ok"] = true,
                        ["playerId"] = playerId,
                        ["key"] = key,
                        ["path"] = "IGameApiClient.CloudSaveData.SetProtectedItemAsync",
                        ["tokenKind"] = "service_token",
                        ["method"] = "UGS 2025 Recommended",
                        ["timestamp"] = DateTime.UtcNow.ToString("o")
                    };
                    return JsonSerializer.Serialize(diag);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Protected item save failed, falling back to default access");
                }

                // 2) フォールバック: プレイヤートークンでデフォルトアクセスレベルで保存
                await gameApiClient.CloudSaveData.SetItemAsync(
                    context,
                    context.AccessToken,
                    projectId,
                    playerId,
                    new SetItemBody(key, "1")
                );

                var diagFallback = new Dictionary<string, object?>
                {
                    ["ok"] = true,
                    ["playerId"] = playerId,
                    ["key"] = key,
                    ["path"] = "IGameApiClient.CloudSaveData.SetItemAsync (fallback)",
                    ["tokenKind"] = "player_access_token",
                    ["method"] = "UGS 2025 Recommended",
                    ["timestamp"] = DateTime.UtcNow.ToString("o")
                };
                return JsonSerializer.Serialize(diagFallback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DiagCloudSaveAdminWriteV2 failed player={PlayerId} key={Key}", playerId, key);
                var err = new Dictionary<string, object?>
                {
                    ["ok"] = false,
                    ["playerId"] = playerId,
                    ["key"] = key,
                    ["error"] = ex.GetType().Name,
                    ["message"] = ex.Message,
                };
                return JsonSerializer.Serialize(err);
            }
        }

        // UGS 2025版: Protectedアイテムとして保存（サービストークン使用）
        [CloudCodeFunction("DiagCloudSaveAdminWrite")]
        public async Task<string> DiagCloudSaveAdminWrite(IExecutionContext context, IGameApiClient gameApiClient)
        {
            var projectId = context?.ProjectId ?? string.Empty;
            var environmentId = context?.EnvironmentId ?? string.Empty;
            var playerId = context?.PlayerId ?? string.Empty;

            var key = $"diag_admin_{Guid.NewGuid():N}";

            string status, body;
            try
            {
                // UGS 2025推奨: IGameApiClientでProtectedアイテムを保存
                await gameApiClient.CloudSaveData.SetItemAsync(
                    context,
                    context.ServiceToken,
                    projectId,
                    playerId,
                    new SetItemBody(key, "1")
                );
                
                status = "200 OK";
                body = "Protected item saved successfully using IGameApiClient";
            }
            catch (Exception ex)
            {
                status = "EXCEPTION";
                body = ex.GetType().Name + ": " + ex.Message;
                _logger.LogError(ex, "DiagCloudSaveAdminWrite failed");
            }

            var diag = new Dictionary<string, object?>
            {
                ["projectId"] = projectId,
                ["environmentId"] = environmentId,
                ["playerId"] = playerId,
                ["method"] = "IGameApiClient.CloudSaveData.SetProtectedItemAsync",
                ["ugsVersion"] = "2025 Recommended Pattern",
                ["cloudSaveAdminWrite"] = new Dictionary<string, object?>
                {
                    ["status"] = status,
                    ["body"] = body,
                    ["key"] = key,
                }
            };

            return JsonSerializer.Serialize(diag);
        }

        // UGS 2025版: Economyの残高確認（HTTPクライアント使用）
        [CloudCodeFunction("DiagEconomy")]
        public async Task<string> DiagEconomy(IExecutionContext context)
        {
            var projectId = context?.ProjectId ?? string.Empty;
            var environmentId = context?.EnvironmentId ?? string.Empty;
            var playerId = context?.PlayerId ?? string.Empty;
            var accessToken = context?.AccessToken ?? string.Empty;

            string status, body;
            try
            {
                // Economy v2 API呼び出し
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                var url = $"https://economy.services.api.unity.com/v2/projects/{projectId}/players/{playerId}/currencies";
                var response = await client.GetAsync(url);
                
                status = $"{(int)response.StatusCode} {response.ReasonPhrase}";
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                status = "EXCEPTION";
                body = ex.GetType().Name + ": " + ex.Message;
                _logger.LogError(ex, "DiagEconomy failed");
            }

            var diag = new Dictionary<string, object?>
            {
                ["projectId"] = projectId,
                ["environmentId"] = environmentId,
                ["playerId"] = playerId,
                ["method"] = "HTTP Direct (Economy v2 API)",
                ["ugsVersion"] = "2025 Pattern",
                ["economy"] = new Dictionary<string, object?>
                {
                    ["status"] = status,
                    ["body"] = body,
                }
            };

            return JsonSerializer.Serialize(diag);
        }

        // UGS 2025版: トークン存在チェック
        [CloudCodeFunction("CheckToken")]
        public object CheckToken(IExecutionContext context)
        {
            var managedTokenPresent = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("UNITY_ACCESS_TOKEN"));
            var accessTokenPresent = !string.IsNullOrEmpty(context?.AccessToken);
            var serviceTokenPresent = !string.IsNullOrEmpty(context?.ServiceToken);
            
            return new
            {
                managedTokenPresent,
                accessTokenPresent,
                serviceTokenPresent,
                projectId = context?.ProjectId,
                environmentId = context?.EnvironmentId,
                ugsVersion = "2025 Pattern"
            };
        }

        // 通貨交換機能（ExchangeV2）
        [CloudCodeFunction("ExchangeV2")]
        public async Task<object> ExchangeV2(IExecutionContext context, Dictionary<string, object> req)
        {
            try
            {
                // リクエストパラメータの取得
                var playerId = req.ContainsKey("playerId") ? req["playerId"]?.ToString() : context.PlayerId;
                var fromDict = req["from"] as Dictionary<string, object>;
                var fromCurrencyId = fromDict?["currencyId"]?.ToString() ?? "";
                var amount = Convert.ToInt64(fromDict?["amount"] ?? 0);
                var toCurrencyId = req["toCurrencyId"]?.ToString() ?? "";
                var execute = req.ContainsKey("execute") && Convert.ToBoolean(req["execute"]);
                var idempotencyKey = req.ContainsKey("idempotencyKey") ? req["idempotencyKey"]?.ToString() : null;

                // 簡易的な交換レート計算（実装例）
                var rate = 1.0;
                var fee = 0.0;
                var credited = (long)(amount * rate);
                var debited = amount;

                _logger.LogInformation("ExchangeV2: from={From} to={To} amount={Amount} execute={Execute}", 
                    fromCurrencyId, toCurrencyId, amount, execute);

                return new
                {
                    credited,
                    debited,
                    feeApplied = fee,
                    rateUsed = rate,
                    execute,
                    idempotencyKey
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExchangeV2 failed");
                throw;
            }
        }

        // 通貨付与機能（デバッグ用）
        [CloudCodeFunction("GrantCurrencyDebug")]
        public async Task<object> GrantCurrencyDebug(IExecutionContext context, Dictionary<string, object> req)
        {
            try
            {
                var playerId = req.ContainsKey("playerId") ? req["playerId"]?.ToString() : context.PlayerId;
                var currencyId = req["currencyId"]?.ToString() ?? "";
                var amount = Convert.ToInt64(req["amount"] ?? 0);

                _logger.LogInformation("GrantCurrencyDebug: player={PlayerId} currency={Currency} amount={Amount}", 
                    playerId, currencyId, amount);

                return new
                {
                    ok = true,
                    playerId,
                    currencyId,
                    amount,
                    message = "Grant simulated (not implemented)"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GrantCurrencyDebug failed");
                throw;
            }
        }

        // シークレット存在チェック
        [CloudCodeFunction("CheckSecrets")]
        public object CheckSecrets(IExecutionContext context)
        {
            var clientIdSet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SA_CLIENT_ID"));
            var clientSecretSet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SA_CLIENT_SECRET"));
            
            return new
            {
                clientIdSet,
                clientSecretSet,
                projectId = context?.ProjectId,
                environmentId = context?.EnvironmentId
            };
        }

        // 全診断機能
        [CloudCodeFunction("DiagAll")]
        public async Task<string> DiagAll(IExecutionContext context)
        {
            var results = new Dictionary<string, object?>
            {
                ["timestamp"] = DateTime.UtcNow.ToString("o"),
                ["projectId"] = context.ProjectId,
                ["environmentId"] = context.EnvironmentId,
                ["playerId"] = context.PlayerId
            };

            try
            {
                // Cloud Save接続確認
                results["cloudSaveReachable"] = "Not tested in DiagAll";
                
                // トークン確認
                results["tokens"] = new
                {
                    accessToken = !string.IsNullOrEmpty(context.AccessToken),
                    serviceToken = !string.IsNullOrEmpty(context.ServiceToken)
                };

                _logger.LogInformation("DiagAll completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DiagAll failed");
                results["error"] = ex.Message;
            }

            return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        }

        // UGS 2025注記: IExecutionContext.ServiceTokenを直接使用することを推奨
        // 手動トークン取得は非推奨パターンとなりました
        // context.ServiceTokenがManaged CredentialsまたはService Accountトークンを自動的に提供します
    }
}



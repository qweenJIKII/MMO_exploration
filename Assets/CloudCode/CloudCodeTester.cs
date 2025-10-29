// コメント: 簡易テスト用
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class CloudCodeExchangeResponse
{
    public long credited;
    public long debited;
    public long feeApplied;
    public string rateUsed;
}

public class CloudCodeTester : MonoBehaviour
{
    async void Start()
    {
        // 環境指定は Project Settings > Services の選択に依存（SetEnvironmentName 未対応SDKのため）
        await UnityServices.InitializeAsync();
        // ここではサインインしない（UgsInitializer が担当）。サインイン完了を待つ。
        for (int i = 0; i < 100 && !AuthenticationService.Instance.IsSignedIn; i++)
        {
            await System.Threading.Tasks.Task.Delay(100);
        }

        var payloadObj = new Dictionary<string, object> {
            { "playerId", AuthenticationService.Instance.PlayerId },
            { "from", new Dictionary<string, object>{{"currencyId","Gold"},{"amount",1000}} },
            { "to",   new Dictionary<string, object>{{"currencyId","Silver"}} }
        };
        // ログ用にJSON文字列を作成
        var inputJson = JsonConvert.SerializeObject(payloadObj);
        // 入力はオブジェクトで渡し、冗長に from/to もトップレベルで送る
        var args = new Dictionary<string, object> {
          { "input", payloadObj },
          { "from", payloadObj["from"] },
          { "to", payloadObj["to"] },
          { "debug", true }
        };
        Debug.Log($"[CloudCodeTester] sending args keys: {string.Join(",", args.Keys)} | input: {inputJson}");

        try
        {
            var res = await CloudCodeService.Instance.CallEndpointAsync<CloudCodeExchangeResponse>("exchangeCurrency_v2", args);
            Debug.Log(JsonUtility.ToJson(res, true));
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CloudCodeTester] Call failed: {ex}");
        }
    }
}
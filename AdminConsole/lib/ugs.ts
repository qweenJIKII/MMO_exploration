// コメント: UGSのトークン取得とCloud Code呼び出し（サーバ側）
// 注意: 本コードは雛形。実環境のAPIエンドポイント/認証フローに合わせて調整してください。

export async function getAccessToken() {
  const clientId = process.env.UNITY_CLIENT_ID;
  const clientSecret = process.env.UNITY_CLIENT_SECRET;
  if (!clientId || !clientSecret) throw new Error("UGS credentials not set");
  // TODO: 実際のUnity Services OAuth2 Client Credentialsフローを実装
  // 例: POST https://services.unity.com/oauth/token (仮)
  return { access_token: "REPLACE_ME", expires_in: 3600 };
}

export type CloudCodeCallOptions = {
  functionName: string; // 例: ExchangeCurrency
  params: any;
};

export async function callCloudCode({ functionName, params }: CloudCodeCallOptions) {
  const base = process.env.CLOUD_CODE_BASE_URL || "https://services.api.unity.com/cloud-code/v1";
  const projectId = process.env.UNITY_PROJECT_ID as string;
  const environmentId = process.env.UNITY_ENVIRONMENT_ID as string;
  const serviceId = process.env.CLOUD_CODE_SERVICE_ID as string;
  if (!projectId || !environmentId || !serviceId) throw new Error("Cloud Code env not set");

  const { access_token } = await getAccessToken();

  // TODO: Cloud Code v2 実行エンドポイントに合わせてURL/ボディを整形
  const url = `${base}/projects/${projectId}/environments/${environmentId}/services/${serviceId}/scripts/${functionName}:run`;
  const res = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${access_token}`,
    },
    body: JSON.stringify({ params }),
    cache: "no-store",
  });
  if (!res.ok) {
    const errText = await res.text();
    throw new Error(`Cloud Code error ${res.status}: ${errText}`);
  }
  return res.json();
}

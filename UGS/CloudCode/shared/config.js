// コメント: Remote Configや固定設定を取得するヘルパー
// 注意: 実運用ではUnity Remote Config SDKを使用。ここではCloud Code内での擬似取得関数を定義。

/**
 * コメント: 交換に関する既定値（資料の初期設定）
 */
export const defaultExchangeConfig = {
  feeRate: {
    P_to_G: 0.005, G_to_E: 0.005, E_to_S: 0.005, S_to_C: 0.0,
    C_to_S: 0.05,  S_to_E: 0.05,  E_to_G: 0.10,  G_to_P: 0.15,
    G_to_GBar: 0.02, GBar_to_LBar: 0.03,
    GBar_to_G: 0.01, LBar_to_GBar: 0.015
  },
  minUnit: {
    S_to_E: 1000, E_to_G: 1000, G_to_P: 10000,
    G_to_GBar: 1000000, GBar_to_LBar: 100
  },
  limits: {
    perTxUp: 1e12, perTxDown: 1e13,
    dailyUpCount: 3, dailyDownCount: 10, dailyBundleCount: 1
  },
  multipliers: { }
};

/**
 * コメント: Remote Configから設定を取得（なければデフォルト）
 */
export async function getExchangeConfig() {
  // コメント: Cloud CodeからRemote Config取得処理を実装する想定。
  // ここではデフォルトを返す。
  return defaultExchangeConfig;
}

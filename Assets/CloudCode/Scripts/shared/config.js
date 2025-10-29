// コメント: Remote Configや固定設定を取得するヘルパー（Unityエディター内オーサリング向け）
// 注意: 実運用ではUnity Remote Config SDK/Cloud Code側の取得に合わせて差し替えてください。

const defaultExchangeConfig = {
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

async function getExchangeConfig() {
  // コメント: エディター内検証用のダミー。必要に応じて実装を差し替えてください。
  return defaultExchangeConfig;
}

module.exports = { defaultExchangeConfig, getExchangeConfig };

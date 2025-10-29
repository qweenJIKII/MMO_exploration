// コメント: Cloud Code 関数 - ExchangeCurrency（両替）
// 仕様: Remote Configの手数料/最小単位/日次上限を考慮し、通貨束の両替を実行
// 注意: 実際のEconomy API呼び出しは shared/economy.js のTODO部を実装してください

import { getExchangeConfig } from "./shared/config.js";
import { getWalletBalances, subtractCurrencies, grantCurrencies } from "./shared/economy.js";

// コメント: 候補Cの超高額レート（対Copper）
const RATE_C = {
  Copper: 1n,
  Silver: 1000n,
  Electrum: 10000n,
  Gold: 10000000n,
  Platinum: 1000000000n,
  GoldBar: 100000000000000n, // 10,000,000 Gold
  LegendaryBar: 100000000000000n * 1000n // 1,000 GoldBar
};

function key(from, to) { return `${from}_to_${to}`; }

function toCopper(bundle) {
  return bundle.reduce((sum, c) => sum + (BigInt(c.amount) * RATE_C[c.currencyId]), 0n);
}

function fromCopper(targetId, copper) {
  const val = copper / RATE_C[targetId];
  return Number(val);
}

function checkMinUnit(cfg, from, amount) {
  const k = key(from.currencyId, from.targetId);
  const min = cfg.minUnit?.[k];
  if (!min) return true;
  return amount >= min;
}

function calcFee(cfg, fromId, toId, debit) {
  const fr = cfg.feeRate?.[key(fromId, toId)] ?? 0;
  return Math.ceil(debit * fr);
}

export async function ExchangeCurrency(params, context) {
  const { playerId, from, to } = params;
  if (!playerId || !from?.currencyId || !to?.currencyId || !from?.amount) {
    throw new Error("INVALID_ARGS");
  }
  if (!RATE_C[from.currencyId] || !RATE_C[to.currencyId]) {
    throw new Error("UNSUPPORTED_CURRENCY");
  }

  const cfg = await getExchangeConfig();

  // 最小単位チェック
  if (!checkMinUnit(cfg, { currencyId: from.currencyId, targetId: to.currencyId }, from.amount)) {
    throw new Error("MIN_UNIT_NOT_MET");
  }

  // 限度額（単回）チェック：Copper換算
  const debitCopper = BigInt(from.amount) * RATE_C[from.currencyId];
  const isUp = RATE_C[from.currencyId] < RATE_C[to.currencyId];
  const perTxLimit = isUp ? BigInt(cfg.limits?.perTxUp ?? 0) : BigInt(cfg.limits?.perTxDown ?? 0);
  if (perTxLimit > 0n && debitCopper > perTxLimit) {
    throw new Error("PER_TX_LIMIT_EXCEEDED");
  }

  // TODO: 日次回数制限は、Cloud SaveやLeaderboardsを用いたカウンタで実装

  // レート/手数料計算
  const rawCopper = BigInt(from.amount) * RATE_C[from.currencyId];
  const rawToAmount = Number(rawCopper / RATE_C[to.currencyId]);
  const fee = calcFee(cfg, from.currencyId, to.currencyId, from.amount);
  const debit = from.amount + fee; // コメント: feeはfrom通貨側で徴収

  // 残高チェック
  const wallet = await getWalletBalances(/* context, playerId */);
  const bal = Number(wallet[from.currencyId] ?? 0);
  if (bal < debit) {
    throw new Error("INSUFFICIENT_FUNDS");
  }

  // 実行（原子的）
  await subtractCurrencies(/* context, playerId, */ [{ currencyId: from.currencyId, amount: debit }]);
  await grantCurrencies(/* context, playerId, */ [{ currencyId: to.currencyId, amount: rawToAmount }]);

  return {
    credited: rawToAmount,
    debited: debit,
    feeApplied: fee,
    rateUsed: `${from.currencyId}:1 -> ${to.currencyId}:${fromCopper(from) / RATE_C[to.currencyId]}`
  };
}

export default { ExchangeCurrency };

// コメント: ExchangeCurrency（Unityエディター内オーサリング向けスクリプト）
var sharedConfig = require("./shared/config.js");
var sharedEconomy = require("./shared/economy.js");
var getExchangeConfig = sharedConfig.getExchangeConfig;
var getWalletBalances = sharedEconomy.getWalletBalances;
var subtractCurrencies = sharedEconomy.subtractCurrencies;
var grantCurrencies = sharedEconomy.grantCurrencies;

// コメント: 候補Cの超高額レート（対Copper）
var RATE_C = {
  Copper: 1,
  Silver: 1000,
  Electrum: 10000,
  Gold: 10000000,
  Platinum: 1000000000,
  GoldBar: 100000000000000, // 10,000,000 Gold
  LegendaryBar: 100000000000000 * 1000 // 1,000 GoldBar
};

function key(from, to) { return String(from) + "_to_" + String(to); }

function toCopper(bundle) {
  var sum = 0;
  for (var i = 0; i < bundle.length; i++) {
    var c = bundle[i];
    sum += Number(c.amount) * RATE_C[c.currencyId];
  }
  return sum;
}

function fromCopper(targetId, copper) {
  return Math.floor(copper / RATE_C[targetId]);
}

function checkMinUnit(cfg, from, amount) {
  const k = key(from.currencyId, from.targetId);
  const min = cfg.minUnit && cfg.minUnit[k];
  if (!min) return true;
  return amount >= min;
}

function calcFee(cfg, fromId, toId, debit) {
  const frMap = cfg.feeRate || {};
  const k = key(fromId, toId);
  const fr = typeof frMap[k] === "number" ? frMap[k] : 0;
  return Math.ceil(debit * fr);
}

async function main(params, context) {
  var playerId = params && params.playerId;
  var from = params && params.from;
  var to = params && params.to;
  if (!playerId || !from || !from.currencyId || !to || !to.currencyId || !from.amount) {
    throw new Error("INVALID_ARGS");
  }
  if (!RATE_C[from.currencyId] || !RATE_C[to.currencyId]) {
    throw new Error("UNSUPPORTED_CURRENCY");
  }

  var cfg = await getExchangeConfig();

  // 最小単位チェック
  if (!checkMinUnit(cfg, { currencyId: from.currencyId, targetId: to.currencyId }, from.amount)) {
    throw new Error("MIN_UNIT_NOT_MET");
  }

  // 限度額（単回）チェック：Copper換算
  var debitCopper = Number(from.amount) * RATE_C[from.currencyId];
  var isUp = RATE_C[from.currencyId] < RATE_C[to.currencyId];
  var limits = cfg.limits || {};
  var perTxLimit = isUp ? Number(limits.perTxUp || 0) : Number(limits.perTxDown || 0);
  if (perTxLimit > 0 && debitCopper > perTxLimit) {
    throw new Error("PER_TX_LIMIT_EXCEEDED");
  }

  // TODO: 日次回数制限は、Cloud SaveやLeaderboardsを用いたカウンタで実装

  // レート/手数料計算
  var rawCopper = Number(from.amount) * RATE_C[from.currencyId];
  var rawToAmount = Math.floor(rawCopper / RATE_C[to.currencyId]);
  var fee = calcFee(cfg, from.currencyId, to.currencyId, from.amount);
  var debit = from.amount + fee; // コメント: feeはfrom通貨側で徴収

  // 残高チェック
  var wallet = await getWalletBalances(/* context, playerId */);
  var bal = wallet && typeof wallet[from.currencyId] === "number" ? wallet[from.currencyId] : 0;
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
    rateUsed: from.currencyId + ":1 -> " + to.currencyId + ":" + Math.floor(RATE_C[from.currencyId] / RATE_C[to.currencyId])
  };
}

module.exports = { main };

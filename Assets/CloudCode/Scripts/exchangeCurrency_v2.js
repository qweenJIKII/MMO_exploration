// コメント: ダッシュボード編集用の単一ファイル版（依存なし・ES5/CommonJS）
// 呼び出し名はファイル名（例: exchangeCurrency.single → 実運用では exchangeCurrency にリネームして使用）

// --- 設定（デフォルト） ---
var defaultExchangeConfig = {
  feeRate: {
    P_to_G: 0.005, G_to_E: 0.005, E_to_S: 0.005, S_to_C: 0.0,
    C_to_S: 0.05,  S_to_E: 0.05,  E_to_G: 0.10,  G_to_P: 0.15,
    G_to_GBar: 0.02, GBar_to_LBar: 0.03,
    GBar_to_G: 0.01, LBar_to_GBar: 0.015
  },
  minUnit: { S_to_E: 1000, E_to_G: 1000, G_to_P: 10000, G_to_GBar: 1000000, GBar_to_LBar: 100 },
  limits: { perTxUp: 1e12, perTxDown: 1e13, dailyUpCount: 3, dailyDownCount: 10, dailyBundleCount: 1 },
  multipliers: {}
};

// Cloud Save のキーは英数・ハイフン・アンダースコア・ピリオドのみ許可。コロンは不可。
var CONFIG_KEY = "cc.exchange.config.v1";

async function loadExchangeConfig(modules, logger) {
  // Cloud Save から設定を取得（失敗時はデフォルト）
  try {
    if (!modules || !modules.cloudSave || !modules.cloudSave.getData) {
      return defaultExchangeConfig;
    }
    // 実装差異・旧キー互換に備え、複数候補を順に探索
    var candidates = ["cc.exchange.config.v1", "cc_exchange_config_v1", "cc:exchange:config:v1"]; // 最後は後方互換
    var res = await modules.cloudSave.getData({ keys: candidates });
    var data = res && (res.data || res);
    var node = null;
    if (data) {
      for (var i = 0; i < candidates.length; i++) {
        var k = candidates[i];
        if (data[k] && (typeof data[k].value !== "undefined")) { node = data[k].value; break; }
        if (data[k]) { node = data[k]; break; }
      }
    }
    if (node && typeof node === "object") {
      return Object.assign({}, defaultExchangeConfig, node);
    }
  } catch (e) {
    try { if (logger && logger.info) { logger.info("cfg fallback: " + String(e && e.message)); } } catch (_) {}
  }
  return defaultExchangeConfig;
}

// --- 通貨レート（Copper換算） ---
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

function checkMinUnit(cfg, from, amount) {
  var k = key(from.currencyId, from.targetId);
  var min = cfg.minUnit && cfg.minUnit[k];
  if (!min) return true;
  return amount >= min;
}

function calcFee(cfg, fromId, toId, debit) {
  var frMap = cfg.feeRate || {};
  var k = key(fromId, toId);
  var fr = typeof frMap[k] === "number" ? frMap[k] : 0;
  return Math.ceil(debit * fr);
}

// --- Economy 残高操作（Cloud Code modules 経由） ---
async function subtractCurrencies(modules, playerId, items) {
  // items: [{ currencyId, amount }]
  for (var i = 0; i < items.length; i++) {
    var it = items[i];
    await modules.economy.playerBalances.decrementBalance({
      playerId: playerId,
      currencyId: it.currencyId,
      amount: Number(it.amount)
    });
  }
}
async function grantCurrencies(modules, playerId, items) {
  for (var i = 0; i < items.length; i++) {
    var it = items[i];
    await modules.economy.playerBalances.incrementBalance({
      playerId: playerId,
      currencyId: it.currencyId,
      amount: Number(it.amount)
    });
  }
}

// --- エントリ関数 ---
async function main(params, context, logger, modules) {
  // 受け取ったパラメータをログ（DashboardのLogsで確認可）
  try {
    if (logger && typeof logger.info === "function") { logger.info("params: " + JSON.stringify(params)); }
    else { console.log("params:", JSON.stringify(params)); }
  } catch (_) {}

  // 'input' 単一パラメータ（JSON or JSON文字列）に対応
  if (params && params.input !== undefined) {
    var input = params.input;
    if (typeof input === "string") {
      try { input = JSON.parse(input); } catch (e) { throw new Error("INVALID_ARGS"); }
    }
    if (input && typeof input === "object") {
      params = Object.assign({}, params, input);
    }
  }

  // よくあるラッピングキー(parameters/args/body/payload/data/request/Input)の内容もマージして正規化
  var __alt = null;
  if (params && typeof params.parameters === "object") { __alt = params.parameters; }
  else if (params && typeof params.args === "object") { __alt = params.args; }
  else if (params && typeof params.body === "object") { __alt = params.body; }
  else if (params && typeof params.payload === "object") { __alt = params.payload; }
  else if (params && typeof params.data === "object") { __alt = params.data; }
  else if (params && typeof params.request === "object") { __alt = params.request; }
  else if (params && typeof params.Input === "object") { __alt = params.Input; }
  if (__alt) { params = Object.assign({}, params, __alt); }

  // 大文字小文字や別名(From/To)にもフォールバック
  if (params && !params.from && typeof params.From === "object") { params.from = params.From; }
  if (params && !params.to && typeof params.To === "object") { params.to = params.To; }

  // 最終手段: params の値のどれかに {from, to} を含むJSON/オブジェクトが入っている場合は取り出す
  if (params && (params.from === undefined || params.to === undefined)) {
    try {
      var keys = Object.keys(params);
      for (var i2 = 0; i2 < keys.length; i2++) {
        var v = params[keys[i2]];
        var obj = null;
        if (v && typeof v === "string") {
          try { obj = JSON.parse(v); } catch (_) { obj = null; }
        } else if (v && typeof v === "object") {
          obj = v;
        }
        if (obj && typeof obj === "object") {
          var f = obj.from || obj.From;
          var t = obj.to || obj.To;
          if (f && t) {
            params = Object.assign({}, params, { from: f, to: t, playerId: params.playerId || obj.playerId });
            break;
          }
        }
      }
    } catch (_) {}
  }

  try {
    if (params && typeof params === "object") {
      if (logger && typeof logger.info === "function") { logger.info("paramKeys: " + JSON.stringify(Object.keys(params))); }
      else { console.log("paramKeys:", Object.keys(params)); }
    }
  } catch (_) {}

  var playerId = params && params.playerId;
  var from = params && params.from; // { currencyId, amount } or JSON string
  var to = params && params.to;     // { currencyId } or JSON string

  // debug: 受信内容をそのまま返す（検証用）
  if (params && params.debug === true) {
    try { if (logger && logger.info) { logger.info("DEBUG_ECHO params: " + JSON.stringify(params)); } } catch (_) {}
    return {
      debug: true,
      paramKeys: Object.keys(params),
      paramsEcho: params
    };
  }

  // ダッシュボードで 'JSON' ではなく 'String' 入力の場合にも対応
  if (typeof from === "string") {
    try { from = JSON.parse(from); } catch (e) { throw new Error("INVALID_ARGS"); }
  }
  if (typeof to === "string") {
    try { to = JSON.parse(to); } catch (e) { throw new Error("INVALID_ARGS"); }
  }
  // amountが文字列の場合は数値へ
  if (from && typeof from.amount === "string") {
    var n = Number(from.amount);
    if (!isFinite(n)) { throw new Error("INVALID_ARGS"); }
    from.amount = n;
  }

  // playerId が未指定なら Cloud Code 実行コンテキストから補完
  if ((!playerId || playerId === "") && context && context.playerId) {
    playerId = context.playerId;
  }

  // ログ: 正規化後の主要フィールド
  try {
    var normalized = { playerId: playerId, from: from, to: to };
    if (logger && typeof logger.info === "function") { logger.info("normalized: " + JSON.stringify(normalized)); }
    else { console.log("normalized:", normalized); }
  } catch (_) {}

  if (!playerId) { throw new Error("INVALID_ARGS: missing playerId"); }
  if (!from) { throw new Error("INVALID_ARGS: missing from"); }
  if (!from.currencyId) { throw new Error("INVALID_ARGS: missing from.currencyId"); }
  if (from.amount === undefined || from.amount === null) { throw new Error("INVALID_ARGS: missing from.amount"); }
  if (!to) { throw new Error("INVALID_ARGS: missing to"); }
  if (!to.currencyId) { throw new Error("INVALID_ARGS: missing to.currencyId"); }
  if (!isFinite(Number(from.amount))) { throw new Error("INVALID_ARGS: amount not numeric"); }
  if (!RATE_C[from.currencyId] || !RATE_C[to.currencyId]) {
    throw new Error("UNSUPPORTED_CURRENCY");
  }

  var cfg = await loadExchangeConfig(modules, logger);

  if (!checkMinUnit(cfg, { currencyId: from.currencyId, targetId: to.currencyId }, from.amount)) {
    throw new Error("MIN_UNIT_NOT_MET");
  }

  var debitCopper = Number(from.amount) * RATE_C[from.currencyId];
  var isUp = RATE_C[from.currencyId] < RATE_C[to.currencyId];
  var limits = cfg.limits || {};
  var perTxLimit = isUp ? Number(limits.perTxUp || 0) : Number(limits.perTxDown || 0);
  if (perTxLimit > 0 && debitCopper > perTxLimit) {
    throw new Error("PER_TX_LIMIT_EXCEEDED");
  }

  var rawCopper = Number(from.amount) * RATE_C[from.currencyId];
  var rawToAmount = Math.floor(rawCopper / RATE_C[to.currencyId]);
  var fee = calcFee(cfg, from.currencyId, to.currencyId, from.amount);
  var debit = from.amount + fee; // コメント: 手数料はfrom側で徴収

  // 残高チェックは decrement で保護（不足時は例外）
  try {
    await subtractCurrencies(modules, playerId, [{ currencyId: from.currencyId, amount: debit }]);
  } catch (e) {
    throw new Error("INSUFFICIENT_FUNDS");
  }
  await grantCurrencies(modules, playerId, [{ currencyId: to.currencyId, amount: rawToAmount }]);

  return {
    credited: rawToAmount,
    debited: debit,
    feeApplied: fee,
    rateUsed: from.currencyId + ":1 -> " + to.currencyId + ":" + Math.floor(RATE_C[from.currencyId] / RATE_C[to.currencyId])
  };
}

// 推奨の分割代入シグネチャ（ドキュメント準拠のアロー関数）
module.exports = async ({ params, context, logger, modules }) => {
  return main(params, context, logger, modules);
};

module.exports.params = {
  // 入力は 'input' に JSON（オブジェクト or 文字列）を渡す。ゲートウェイ上は JSON 型で受理。
  input: "JSON",
  // 互換: トップレベルで from/to を送られても受理できるよう任意にする
  from:  { type: "JSON", required: false },
  to:    { type: "JSON", required: false },
  // 任意: 受信内容をそのまま返すデバッグフラグ
  debug: "Boolean"
};

// コメント: Cloud Code 関数 - 通貨管理（Upsert/Grant/Subtract）
import { upsertCurrency, grantCurrencies, subtractCurrencies } from "./shared/economy.js";

export async function AdminUpsertCurrency(params, context) {
  const { currency } = params ?? {};
  if (!currency?.currencyId) throw new Error("INVALID_ARGS");
  const res = await upsertCurrency(/* context, */ currency);
  return { ok: !!res?.ok };
}

export async function AdminGrantCurrency(params, context) {
  const { playerId, bundle } = params ?? {};
  if (!playerId || !Array.isArray(bundle)) throw new Error("INVALID_ARGS");
  const res = await grantCurrencies(/* context, */ playerId, bundle);
  return { ok: !!res?.ok };
}

export async function AdminSubtractCurrency(params, context) {
  const { playerId, bundle } = params ?? {};
  if (!playerId || !Array.isArray(bundle)) throw new Error("INVALID_ARGS");
  const res = await subtractCurrencies(/* context, */ playerId, bundle);
  return { ok: !!res?.ok };
}

export default { AdminUpsertCurrency, AdminGrantCurrency, AdminSubtractCurrency };

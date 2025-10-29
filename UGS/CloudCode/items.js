// コメント: Cloud Code 関数 - アイテム関連（付与/耐久/カタログUpsert）
import { upsertCatalogItem, grantItem, updateInventoryItemInstance } from "./shared/economy.js";

export async function AdminUpsertCatalogItem(params, context) {
  const { catalogItem } = params ?? {};
  if (!catalogItem?.itemId) throw new Error("INVALID_ARGS");
  const res = await upsertCatalogItem(/* context, */ catalogItem);
  return { ok: !!res?.ok, version: res?.version ?? 0 };
}

export async function AddItem(params, context) {
  const { playerId, itemId, amount = 1, durability } = params ?? {};
  if (!playerId || !itemId) throw new Error("INVALID_ARGS");
  const instanceData = {};
  if (typeof durability === "number") instanceData.durability = durability;
  const res = await grantItem(/* context, */ playerId, itemId, amount, instanceData);
  return { ok: !!res?.ok, grantIds: res?.grantIds ?? [] };
}

export async function DamageItem(params, context) {
  const { playerId, inventoryItemId, amount } = params ?? {};
  if (!playerId || !inventoryItemId || typeof amount !== "number") throw new Error("INVALID_ARGS");
  // コメント: ここでは単純減算。0未満は0に丸める実装を想定。
  const patch = { op: "decrement", path: "/durability", value: amount };
  const res = await updateInventoryItemInstance(/* context, */ playerId, inventoryItemId, patch);
  return { ok: !!res?.ok };
}

export async function RepairItem(params, context) {
  const { playerId, inventoryItemId, toFull } = params ?? {};
  if (!playerId || !inventoryItemId) throw new Error("INVALID_ARGS");
  // コメント: 実装詳細はEconomy/Cloud Code SDKに依存。ここでは成功応答の形のみ定義。
  const res = await updateInventoryItemInstance(/* context, */ playerId, inventoryItemId, { op: "set", path: "/durability", value: null /* TODO */ });
  return { ok: !!res?.ok };
}

export default { AdminUpsertCatalogItem, AddItem, DamageItem, RepairItem };

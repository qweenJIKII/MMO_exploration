// コメント: Economy Admin/Player API呼び出しの共通ユーティリティ（Cloud Code側）
// 注意: 実装はUnity Cloud CodeのSDK/内蔵クライアントに合わせて置き換えてください。

/**
 * コメント: 通貨残高を取得（プレイヤー）
 */
export async function getWalletBalances(/* context, playerId */) {
  // TODO: UGS SDKで実装
  // 例: return { Gold: 0, Silver: 0, ... }
  return {};
}

/**
 * コメント: 通貨をまとめて減算（原子的）
 */
export async function subtractCurrencies(/* context, playerId, bundle */) {
  // TODO: UGS SDKで実装
  // bundle: [{ currencyId, amount }]
  return { ok: true };
}

/**
 * コメント: 通貨をまとめて加算（原子的）
 */
export async function grantCurrencies(/* context, playerId, bundle */) {
  // TODO: UGS SDKで実装
  return { ok: true };
}

/**
 * コメント: 通貨の作成/更新（管理）
 */
export async function upsertCurrency(/* context, currency */) {
  // TODO: UGS Admin API呼び出し
  return { ok: true };
}

/**
 * コメント: カタログアイテムの作成/更新（管理）
 */
export async function upsertCatalogItem(/* context, catalogItem */) {
  // TODO: UGS Admin API呼び出し
  return { ok: true };
}

/**
 * コメント: アイテム付与（プレイヤー在庫）
 */
export async function grantItem(/* context, playerId, itemId, amount, instanceData */) {
  // TODO: UGS Player API呼び出し
  return { ok: true, grantIds: [] };
}

/**
 * コメント: インベントリアイテムのインスタンスデータ更新（耐久）
 */
export async function updateInventoryItemInstance(/* context, playerId, inventoryItemId, patch */) {
  // TODO: UGS Player API呼び出し
  return { ok: true };
}

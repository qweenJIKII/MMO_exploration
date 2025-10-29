// コメント: Economy Admin/Player呼び出しの雛形（Unityエディター内オーサリング向け）
// 注意: 実運用ではCloud Code実行環境からのSDK/HTTP呼び出しに合わせて実装を差し替えてください。

async function getWalletBalances(/* context, playerId */) {
  // TODO: UGS SDKで実装
  return {};
}

async function subtractCurrencies(/* context, playerId, bundle */) {
  // TODO: UGS SDKで実装（原子的減算）
  return { ok: true };
}

async function grantCurrencies(/* context, playerId, bundle */) {
  // TODO: UGS SDKで実装（原子的加算）
  return { ok: true };
}

async function upsertCurrency(/* context, currency */) {
  // TODO: Admin APIで通貨作成/更新
  return { ok: true };
}

async function upsertCatalogItem(/* context, catalogItem */) {
  // TODO: Admin APIでカタログ作成/更新
  return { ok: true };
}

async function grantItem(/* context, playerId, itemId, amount, instanceData */) {
  // TODO: Player APIで在庫付与
  return { ok: true, grantIds: [] };
}

async function updateInventoryItemInstance(/* context, playerId, inventoryItemId, patch */) {
  // TODO: Player APIでインスタンスデータ更新
  return { ok: true };
}

module.exports = {
  getWalletBalances,
  subtractCurrencies,
  grantCurrencies,
  upsertCurrency,
  upsertCatalogItem,
  grantItem,
  updateInventoryItemInstance,
};


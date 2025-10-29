// コメント: 最小検証用 - デプロイと実行を確認するためのシンプルなエンドポイント
// 呼び出し名: "ping"（ファイル名 = エンドポイント名）
function main(params) {
  return { ok: true, echo: params || null, now: Date.now() };
}
module.exports = { main };

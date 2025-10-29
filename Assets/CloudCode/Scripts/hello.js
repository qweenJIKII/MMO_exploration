// コメント: Cloud Code 疎通確認用の最小サンプル（パラメータなし）
module.exports = async ({ context, logger }) => {
  try {
    const pid = (context && context.playerId) || null;
    if (logger && typeof logger.info === "function") {
      logger.info("hello invoked for player: " + (pid || "no-auth"));
    } else {
      // フォールバック
      try { console.log("hello invoked for player:", pid || "no-auth"); } catch (_) {}
    }
    return { message: "hello world", playerId: pid };
  } catch (e) {
    try { if (logger && typeof logger.error === "function") logger.error("hello failed: " + String(e && e.message)); } catch (_) {}
    throw e;
  }
};

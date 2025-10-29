// コメント: 通貨管理ページ（簡易スケルトン）
import { useState } from "react";

export default function CurrenciesPage() {
  const [currencyId, setCurrencyId] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [playerId, setPlayerId] = useState("");
  const [grantCurrencyId, setGrantCurrencyId] = useState("");
  const [grantAmount, setGrantAmount] = useState<number>(0);
  const [log, setLog] = useState<string>("");

  async function post(path: string, body: any) {
    const res = await fetch(path, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) });
    const json = await res.json();
    setLog(JSON.stringify(json, null, 2));
  }

  return (
    <div style={{ padding: 24 }}>
      <h1>Currencies</h1>

      <section style={{ marginBottom: 24 }}>
        <h2>Upsert Currency</h2>
        <div style={{ display: "grid", gap: 8, maxWidth: 480 }}>
          <input placeholder="currencyId" value={currencyId} onChange={(e) => setCurrencyId(e.target.value)} />
          <input placeholder="displayName" value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
          <button onClick={() => post("/api/cc/currencies/upsert", { currency: { currencyId, displayName } })}>Upsert</button>
        </div>
      </section>

      <section style={{ marginBottom: 24 }}>
        <h2>Grant / Subtract</h2>
        <div style={{ display: "grid", gap: 8, maxWidth: 480 }}>
          <input placeholder="playerId" value={playerId} onChange={(e) => setPlayerId(e.target.value)} />
          <input placeholder="currencyId" value={grantCurrencyId} onChange={(e) => setGrantCurrencyId(e.target.value)} />
          <input type="number" placeholder="amount" value={grantAmount} onChange={(e) => setGrantAmount(Number(e.target.value))} />
          <div style={{ display: "flex", gap: 8 }}>
            <button onClick={() => post("/api/cc/currencies/grant", { playerId, bundle: [{ currencyId: grantCurrencyId, amount: grantAmount }] })}>Grant</button>
            <button onClick={() => post("/api/cc/currencies/subtract", { playerId, bundle: [{ currencyId: grantCurrencyId, amount: grantAmount }] })}>Subtract</button>
          </div>
        </div>
      </section>

      <section>
        <h2>Result</h2>
        <pre style={{ background: "#111", color: "#0f0", padding: 12, borderRadius: 8, overflow: "auto" }}>{log}</pre>
      </section>
    </div>
  );
}

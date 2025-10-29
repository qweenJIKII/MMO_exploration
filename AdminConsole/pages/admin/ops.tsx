// コメント: 運用テストページ（両替/付与/修理＋所持金表示）
import { useState } from "react";

export default function OpsPage() {
  const [playerId, setPlayerId] = useState("");
  const [fromCurrency, setFromCurrency] = useState("Gold");
  const [toCurrency, setToCurrency] = useState("Silver");
  const [amount, setAmount] = useState<number>(1000000);
  const [inventoryItemId, setInventoryItemId] = useState("");
  const [log, setLog] = useState<string>("");

  async function post(path: string, body: any) {
    const res = await fetch(path, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) });
    const json = await res.json();
    setLog(JSON.stringify(json, null, 2));
  }

  return (
    <div style={{ padding: 24 }}>
      <h1>Ops</h1>

      <section style={{ marginBottom: 24 }}>
        <h2>Exchange</h2>
        <div style={{ display: "grid", gap: 8, maxWidth: 520 }}>
          <input placeholder="playerId" value={playerId} onChange={(e) => setPlayerId(e.target.value)} />
          <div style={{ display: "flex", gap: 8 }}>
            <input placeholder="from (currencyId)" value={fromCurrency} onChange={(e) => setFromCurrency(e.target.value)} />
            <input placeholder="to (currencyId)" value={toCurrency} onChange={(e) => setToCurrency(e.target.value)} />
          </div>
          <input type="number" placeholder="amount" value={amount} onChange={(e) => setAmount(Number(e.target.value))} />
          <button onClick={() => post("/api/cc/exchange", { playerId, from: { currencyId: fromCurrency, amount }, to: { currencyId: toCurrency } })}>ExchangeCurrency</button>
        </div>
      </section>

      <section style={{ marginBottom: 24 }}>
        <h2>Repair</h2>
        <div style={{ display: "grid", gap: 8, maxWidth: 520 }}>
          <input placeholder="playerId" value={playerId} onChange={(e) => setPlayerId(e.target.value)} />
          <input placeholder="inventoryItemId" value={inventoryItemId} onChange={(e) => setInventoryItemId(e.target.value)} />
          <button onClick={() => post("/api/cc/inventory/repair", { playerId, inventoryItemId, toFull: true })}>RepairItem</button>
        </div>
      </section>

      <section>
        <h2>Result</h2>
        <pre style={{ background: "#111", color: "#0f0", padding: 12, borderRadius: 8, overflow: "auto" }}>{log}</pre>
      </section>
    </div>
  );
}

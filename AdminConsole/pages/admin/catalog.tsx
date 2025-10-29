// コメント: カタログ管理ページ（簡易スケルトン）
import { useState } from "react";

export default function CatalogPage() {
  const [json, setJson] = useState<string>(
    JSON.stringify(
      {
        catalogItem: {
          itemId: "item_gs_flame_001",
          displayName: "炎の大剣",
          description: "炎の魔石が埋め込まれた巨大な剣。",
          purchasable: false,
          customData: {
            type: "equip",
            subType: "greatsword",
            rarity: 5,
            itemLevel: 120,
            bindType: "on_equip",
            stackMax: 1,
            iconKey: "Icons/gs_flame",
            prefabKey: "Prefabs/Items/GS_Flame",
            appearanceId: 2004,
            maxDurability: 100,
            repairCost: {
              primary: [{ currencyId: "Gold", perDur: 5 }],
              alternatives: [[{ currencyId: "Gem", perDur: 1 }]]
            },
            stats: [{ key: "ATK", value: 80 }],
            effects: [
              {
                effectId: "eff_fire_onhit",
                trigger: "on_hit",
                chance: 20,
                params: { extraATK: 30, element: "fire" }
              }
            ]
          }
        }
      },
      null,
      2
    )
  );
  const [log, setLog] = useState<string>("");

  async function upsert() {
    try {
      const body = JSON.parse(json);
      const res = await fetch("/api/cc/catalog/upsert", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
      });
      const out = await res.json();
      setLog(JSON.stringify(out, null, 2));
    } catch (e: any) {
      setLog(String(e?.message || e));
    }
  }

  return (
    <div style={{ padding: 24 }}>
      <h1>Catalog</h1>
      <section style={{ display: "grid", gap: 8 }}>
        <textarea
          style={{ width: "100%", height: 360, fontFamily: "monospace" }}
          value={json}
          onChange={(e) => setJson(e.target.value)}
        />
        <button onClick={upsert}>AdminUpsertCatalogItem</button>
      </section>

      <section style={{ marginTop: 24 }}>
        <h2>Result</h2>
        <pre style={{ background: "#111", color: "#0f0", padding: 12, borderRadius: 8, overflow: "auto" }}>{log}</pre>
      </section>
    </div>
  );
}

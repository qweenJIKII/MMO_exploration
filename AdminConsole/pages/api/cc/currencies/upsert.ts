// コメント: 通貨Upsert → Cloud Code AdminUpsertCurrency をプロキシ
import type { NextApiRequest, NextApiResponse } from "next";
import { callCloudCode } from "../../../../lib/ugs";

export default async function handler(req: NextApiRequest, res: NextApiResponse) {
  if (req.method !== "POST") return res.status(405).json({ error: "METHOD_NOT_ALLOWED" });
  try {
    const body = req.body || {};
    const out = await callCloudCode({ functionName: "AdminUpsertCurrency", params: body });
    res.status(200).json(out);
  } catch (e: any) {
    res.status(500).json({ error: e?.message || String(e) });
  }
}

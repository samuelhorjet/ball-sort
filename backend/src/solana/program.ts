import { Program, AnchorProvider, BorshCoder, EventParser } from "@coral-xyz/anchor";
import type { Idl } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import { connection, PROGRAM_ID } from "../config/solana.js";
import type { BallSort } from "./idl/ball_sort.js";
import IDL from "./idl/ball_sort.json" with { type: "json" };

// Read-only provider — no wallet needed for account reads
const provider = new AnchorProvider(
  connection,
  {
    publicKey: PublicKey.default,
    signTransaction: async (tx) => tx,
    signAllTransactions: async (txs) => txs,
  },
  { commitment: "confirmed" }
);

export const program = new Program<BallSort>(IDL as unknown as BallSort, provider);

// EventParser decodes Anchor events from Helius log arrays
// Anchor emits events as "Program data: <base64>" in transaction logs
export const eventParser = new EventParser(
  PROGRAM_ID,
  new BorshCoder(IDL as Idl)
);
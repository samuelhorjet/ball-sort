import { PublicKey } from "@solana/web3.js";
import { program } from "../program.js";
import { SEEDS, PROGRAM_ID } from "../../utils/constants.js";

export interface GameConfigData {
  address: string;
  authority: string;
  treasury: string;
  treasuryFeeBps: number;
  isPaused: boolean;
  tournamentCount: bigint;
  bump: number;
}

/**
 * Derive the GameConfig PDA address.
 * Seeds: ["game_config"]
 */
export function getGameConfigPda(): PublicKey {
  const [pda] = PublicKey.findProgramAddressSync([SEEDS.GAME_CONFIG], PROGRAM_ID);
  return pda;
}

/**
 * Fetch and deserialize the GameConfig account from the Solana chain.
 * Returns null if the account does not exist yet.
 */
export async function fetchGameConfig(): Promise<GameConfigData | null> {
  try {
    const pda = getGameConfigPda();
    const account = await program.account.gameConfig.fetch(pda);

    return {
      address: pda.toBase58(),
      authority: account.authority.toBase58(),
      treasury: account.treasury.toBase58(),
      treasuryFeeBps: account.treasuryFeeBps,
      isPaused: account.isPaused,
      tournamentCount: BigInt(account.tournamentCount.toString()),
      bump: account.bump,
    };
  } catch (err: any) {
    if (err?.message?.includes("Account does not exist")) return null;
    throw err;
  }
}

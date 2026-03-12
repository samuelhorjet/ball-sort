import { PublicKey } from "@solana/web3.js";
import { program } from "../program.js";
import { SEEDS, PROGRAM_ID } from "../../utils/constants.js";

export interface PlayerAuthData {
  address: string;
  wallet: string;
  sessionKey: string | null;
  sessionExpiresAt: number;
  totalPuzzlesSolved: bigint;
  puzzlesStartedNonce: bigint;
  bump: number;
}

/**
 * Derive the PlayerAuth PDA.
 * Seeds: ["player_auth", walletPubkey]
 */
export function getPlayerAuthPda(wallet: PublicKey): PublicKey {
  const [pda] = PublicKey.findProgramAddressSync(
    [SEEDS.PLAYER_AUTH, wallet.toBuffer()],
    PROGRAM_ID
  );
  return pda;
}

/**
 * Fetch and deserialize a PlayerAuth account.
 * Returns null if not yet created (player hasn't called create_player_auth).
 */
export async function fetchPlayerAuth(wallet: string): Promise<PlayerAuthData | null> {
  try {
    const walletPk = new PublicKey(wallet);
    const pda = getPlayerAuthPda(walletPk);
    const account = await program.account.playerAuth.fetch(pda);

    return {
      address: pda.toBase58(),
      wallet: account.wallet.toBase58(),
      sessionKey: account.sessionKey ? account.sessionKey.toBase58() : null,
      sessionExpiresAt: account.sessionExpiresAt.toNumber(),
      totalPuzzlesSolved: BigInt(account.totalPuzzlesSolved.toString()),
      puzzlesStartedNonce: BigInt(account.puzzlesStartedNonce.toString()),
      bump: account.bump,
    };
  } catch (err: any) {
    if (err?.message?.includes("Account does not exist")) return null;
    throw err;
  }
}

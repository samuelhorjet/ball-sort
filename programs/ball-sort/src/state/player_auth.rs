use anchor_lang::prelude::*;

#[account]
pub struct PlayerAuth {
    pub wallet: Pubkey,
    pub session_key: Option<Pubkey>,
    pub session_expires_at: i64,
    pub has_active_puzzle: bool,
    pub active_puzzle_entity: Option<Pubkey>,
    pub active_puzzle_status: u8,
    pub total_puzzles_solved: u64,
    pub soar_player_account: Pubkey,
    pub puzzles_started_nonce: u64,
    pub vrf_randomness: [u8; 32],
    pub puzzle_num_tubes: u8,
    pub puzzle_balls_per_tube: u8,
    pub puzzle_difficulty: u8,
    pub bump: u8,
}

impl PlayerAuth {
    pub const SPACE: usize = 8      // discriminator
        + 32  // wallet
        + 1+32 // session_key (Option<Pubkey>)
        + 8   // session_expires_at
        + 1   // has_active_puzzle
        + 1+32 // active_puzzle_entity (Option<Pubkey>)
        + 1   // active_puzzle_status
        + 8   // total_puzzles_solved
        + 32  // soar_player_account
        + 8   // puzzles_started_nonce
        + 32  // vrf_randomness
        + 1   // puzzle_num_tubes
        + 1   // puzzle_balls_per_tube
        + 1   // puzzle_difficulty
        + 1; // bump

    pub fn is_valid_signer(&self, signer: &Pubkey, now: i64) -> bool {
        if *signer == self.wallet {
            return true;
        }
        if let Some(sk) = self.session_key {
            if *signer == sk && now < self.session_expires_at {
                return true;
            }
        }
        false
    }
}

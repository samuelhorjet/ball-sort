use anchor_lang::prelude::*;
use crate::types::constants::*;

#[account]
pub struct GameConfig {
    pub authority: Pubkey,
    pub treasury: Pubkey,
    pub soar_game_account: Pubkey,
    pub world_program_id: Pubkey,
    pub vrf_program_id: Pubkey,
    pub vrf_authority: Pubkey,
    pub treasury_fee_bps: u16,
    pub is_paused: bool,
    pub tournament_count: u64,
    pub bump: u8,
}

impl GameConfig {
    pub const SPACE: usize = 8 + 32*6 + 2 + 1 + 8 + 1; 

    pub fn calculate_fee(&self, amount: u64) -> (u64, u64) {
        let fee = (amount as u128)
            .saturating_mul(self.treasury_fee_bps as u128)
            .checked_div(BPS_DENOMINATOR as u128)
            .unwrap_or(0) as u64;
        (fee, amount.saturating_sub(fee))
    }
}

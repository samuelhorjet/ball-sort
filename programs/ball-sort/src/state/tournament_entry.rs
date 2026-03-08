use anchor_lang::prelude::*;

#[account]
pub struct TournamentEntry {
    pub tournament:        Pubkey,
    pub player:            Pubkey,
    pub puzzle_entity:     Option<Pubkey>,  
    pub entry_deposit:     u64,
    pub parimutuel_weight: u128,
    pub has_played:        bool,
    pub completed:         bool,
    pub has_claimed:       bool,
    pub bump:              u8,
}

impl TournamentEntry {
    pub const SPACE: usize = 8 + 32+32 + 1+32 + 8 + 16 + 1+1+1 + 1;
}

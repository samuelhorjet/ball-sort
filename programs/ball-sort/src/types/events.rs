use anchor_lang::prelude::*;

#[event]
pub struct GameConfigInitialized {
    pub authority: Pubkey,
    pub treasury: Pubkey,
    pub timestamp: i64,
}

#[event]
pub struct GameConfigUpdated {
    pub authority: Pubkey,
    pub field: String,
    pub timestamp: i64,
}

#[event]
pub struct PermissionCreated {
    pub player: Pubkey,
    pub puzzle_board: Pubkey,
    pub puzzle_stats: Pubkey,
    pub puzzle_board_permission: Pubkey,
    pub puzzle_stats_permission: Pubkey,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleInitialized {
    pub player: Pubkey,
    pub puzzle_board: Pubkey,
    pub puzzle_stats: Pubkey,
    pub num_tubes: u8,
    pub balls_per_tube: u8,
    pub difficulty: u8,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleStarted {
    pub puzzle_board: Pubkey,
    pub player: Pubkey,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleFinalized {
    pub player: Pubkey,
    pub puzzle_board: Pubkey,
    pub puzzle_stats: Pubkey,
    pub move_count: u32,
    pub undo_count: u32,
    pub difficulty: u8,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleAbandoned {
    pub player: Pubkey,
    pub puzzle_board: Pubkey,
    pub puzzle_stats: Pubkey,
    pub move_count: u32,
    pub undo_count: u32,
    pub difficulty: u8,
    pub timestamp: i64,
}

#[event]
pub struct TournamentCreated {
    pub tournament: Pubkey,
    pub authority: Pubkey,
    pub entry_fee: u64,
    pub difficulty: u8,
    pub end_time: i64,
    pub treasury_fee_bps: u16,
    pub timestamp: i64,
}

#[event]
pub struct TournamentJoined {
    pub tournament: Pubkey,
    pub player: Pubkey,
    pub timestamp: i64,
}

#[event]
pub struct TournamentResultRecorded {
    pub tournament: Pubkey,
    pub player: Pubkey,
    pub weight: u128,
    pub elapsed_secs: u64,
    pub move_count: u32,
    pub timestamp: i64,
}

#[event]
pub struct TournamentClosed {
    pub tournament: Pubkey,
    pub total_entries: u32,
    pub total_completers: u32,
    pub prize_pool: u64,
    pub timestamp: i64,
}

#[event]
pub struct PrizeClaimed {
    pub tournament: Pubkey,
    pub player: Pubkey,
    pub amount: u64,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleDelegated {
    pub puzzle_board: Pubkey,
    pub player: Pubkey,
    pub validator: Option<Pubkey>,
    pub timestamp: i64,
}

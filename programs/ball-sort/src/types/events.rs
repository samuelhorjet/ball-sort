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
pub struct PlayerAuthCreated {
    pub player: Pubkey,
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
    pub puzzle_entity: Pubkey,
    pub puzzle_board: Pubkey,
    pub puzzle_stats: Pubkey,
    pub num_tubes: u8,
    pub balls_per_tube: u8,
    pub difficulty: u8,
    pub game_mode: u8,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleBoardReady {
    pub puzzle_entity: Pubkey,
    pub player: Pubkey,
    pub board_hash: [u8; 32],
    pub num_tubes: u8,
    pub balls_per_tube: u8,
    pub difficulty: u8,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleStarted {
    pub puzzle_entity: Pubkey,
    pub player: Pubkey,
    pub started_at: i64,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleSolved {
    pub puzzle_entity: Pubkey,
    pub player: Pubkey,
    pub puzzle_board: Pubkey,
    pub puzzle_stats: Pubkey,
    pub move_count: u32,
    pub undo_count: u32,
    pub elapsed_secs: u64,
    pub final_score: u64,
    pub difficulty: u8,
    pub game_mode: u8,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleAbandoned {
    pub puzzle_entity: Pubkey,
    pub player: Pubkey,
    pub puzzle_board: Pubkey,
    pub puzzle_stats: Pubkey,
    pub move_count: u32,
    pub undo_count: u32,
    pub elapsed_secs: u64,
    pub difficulty: u8,
    pub timestamp: i64,
}

#[event]
pub struct SessionOpened {
    pub player: Pubkey,
    pub session_key: Pubkey,
    pub expires_at: i64,
    pub timestamp: i64,
}

#[event]
pub struct SessionClosed {
    pub player: Pubkey,
    pub session_key: Pubkey,
    pub timestamp: i64,
}

#[event]
pub struct AchievementUnlocked {
    pub player: Pubkey,
    pub achievement_id: u8,
    pub puzzle_entity: Pubkey,
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
pub struct RefundClaimed {
    pub tournament: Pubkey,
    pub player: Pubkey,
    pub amount: u64,
    pub treasury_cut: u64,
    pub timestamp: i64,
}

#[event]
pub struct PuzzleDelegated {
    pub puzzle_entity: Pubkey,
    pub player: Pubkey,
    pub validator: Option<Pubkey>,
    pub timestamp: i64,
}

use anchor_lang::prelude::*;

#[account]
#[derive(InitSpace)]
pub struct PlayerProfile {
    pub total_puzzles_started: u32,
    pub total_puzzles_solved: u32,
    pub best_score_easy: u64,
    pub best_score_medium: u64,
    pub best_score_hard: u64,
}

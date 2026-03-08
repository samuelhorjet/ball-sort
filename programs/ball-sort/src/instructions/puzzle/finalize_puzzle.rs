use crate::state::{GameConfig, PlayerAuth, PuzzleStats};
use crate::types::constants::*;
use crate::types::{GameError, PuzzleStatus};
use anchor_lang::prelude::*;

pub fn handle_finalize_puzzle(ctx: Context<FinalizePuzzle>) -> Result<()> {
    let auth = &mut ctx.accounts.player_auth;

    require!(
        auth.active_puzzle_entity.is_some(),
        GameError::NoPuzzleActive
    );

    let stats = &ctx.accounts.puzzle_stats;

    require!(stats.is_solved, GameError::PuzzleNotSolved);

    auth.has_active_puzzle = false;
    auth.active_puzzle_entity = None;
    auth.active_puzzle_status = PuzzleStatus::Finalized as u8;
    auth.total_puzzles_solved = auth.total_puzzles_solved.saturating_add(1);

    msg!("finalize_puzzle: solved, score={}", stats.final_score);
    Ok(())
}

#[derive(Accounts)]
pub struct FinalizePuzzle<'info> {
    #[account(mut)]
    pub signer: Signer<'info>,

    #[account(mut, seeds=[SEED_PLAYER_AUTH, player_auth.wallet.as_ref()], bump=player_auth.bump,
              constraint = player_auth.wallet == signer.key()
                        || player_auth.session_key == Some(signer.key()) @ GameError::Unauthorized)]
    pub player_auth: Account<'info, PlayerAuth>,

    #[account(seeds=[SEED_GAME_CONFIG], bump=game_config.bump)]
    pub game_config: Account<'info, GameConfig>,

    #[account(
        seeds = [SEED_PUZZLE_STATS, player_auth.key().as_ref(), &player_auth.puzzles_started_nonce.saturating_sub(1).to_le_bytes()],
        bump
    )]
    pub puzzle_stats: Account<'info, PuzzleStats>,
}

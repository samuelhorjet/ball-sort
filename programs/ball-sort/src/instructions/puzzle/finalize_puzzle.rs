use crate::state::{GameConfig, PlayerAuth, PuzzleStats};
use crate::types::constants::*;
use crate::types::{GameError, PuzzleFinalized, PuzzleStatus};
use anchor_lang::prelude::*;

pub fn handle_finalize_puzzle(ctx: Context<FinalizePuzzle>) -> Result<()> {
    let puzzle_stats_key = ctx.accounts.puzzle_stats.key();
    let stats = &mut ctx.accounts.puzzle_stats;

    require!(stats.is_solved, GameError::PuzzleNotSolved);

    stats.status = PuzzleStatus::Finalized as u8;

    let auth = &mut ctx.accounts.player_auth;
    auth.total_puzzles_solved = auth.total_puzzles_solved.saturating_add(1);

    let clock = Clock::get()?;

    emit!(PuzzleFinalized {
        player: auth.wallet,
        puzzle_board: puzzle_stats_key,
        puzzle_stats: puzzle_stats_key,
        move_count: stats.move_count,
        undo_count: stats.undo_count,
        difficulty: stats.difficulty,
        timestamp: clock.unix_timestamp,
    });

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
        mut,
        seeds = [SEED_PUZZLE_STATS, player_auth.key().as_ref(), &player_auth.puzzles_started_nonce.saturating_sub(1).to_le_bytes()],
        bump
    )]
    pub puzzle_stats: Account<'info, PuzzleStats>,
}

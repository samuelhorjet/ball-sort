use crate::state::{GameConfig, PlayerAuth, PuzzleBoard, PuzzleStats};
use crate::types::constants::*;
use crate::types::{GameError, UndoError};
use crate::utils::validate_signer;
use anchor_lang::prelude::*;

pub fn handle_apply_undo(ctx: Context<ApplyUndo>) -> Result<()> {
    let clock = Clock::get()?;
    validate_signer(
        &ctx.accounts.signer.key(),
        &*ctx.accounts.player_auth,
        clock.unix_timestamp,
    )?;

    let board = &mut ctx.accounts.puzzle_board;
    let stats = &mut ctx.accounts.puzzle_stats;

    require!(stats.status == 2, UndoError::PuzzleNotActive);
    require!(board.has_undo, UndoError::NoUndoAvailable);

    let from = board.undo_from as usize;
    let to = board.undo_to as usize;
    let ball = board.undo_ball;

    let to_top =
        to * crate::state::puzzle_board::MAX_CAPACITY + (board.tube_lengths[to] as usize - 1);
    board.balls[to_top] = 0;
    board.tube_lengths[to] -= 1;

    let from_top =
        from * crate::state::puzzle_board::MAX_CAPACITY + board.tube_lengths[from] as usize;
    board.balls[from_top] = ball;
    board.tube_lengths[from] += 1;

    board.has_undo = false;
    board.undo_from = 0;
    board.undo_to = 0;
    board.undo_ball = 0;

    stats.move_count = stats.move_count.saturating_add(1);
    stats.undo_count = stats.undo_count.saturating_add(1);

    Ok(())
}

#[derive(Accounts)]
pub struct ApplyUndo<'info> {
    pub signer: Signer<'info>,

    #[account(seeds=[SEED_PLAYER_AUTH, player_auth.wallet.as_ref()], bump=player_auth.bump,
              constraint = player_auth.wallet == signer.key()
                        || player_auth.session_key == Some(signer.key()) @ GameError::Unauthorized)]
    pub player_auth: Account<'info, PlayerAuth>,

    #[account(seeds=[SEED_GAME_CONFIG], bump=game_config.bump)]
    pub game_config: Account<'info, GameConfig>,

    #[account(
        mut,
        seeds = [SEED_PUZZLE_BOARD, player_auth.key().as_ref(), &player_auth.puzzles_started_nonce.saturating_sub(1).to_le_bytes()],
        bump
    )]
    pub puzzle_board: Account<'info, PuzzleBoard>,

    #[account(
        mut,
        seeds = [SEED_PUZZLE_STATS, player_auth.key().as_ref(), &player_auth.puzzles_started_nonce.saturating_sub(1).to_le_bytes()],
        bump
    )]
    pub puzzle_stats: Account<'info, PuzzleStats>,
}

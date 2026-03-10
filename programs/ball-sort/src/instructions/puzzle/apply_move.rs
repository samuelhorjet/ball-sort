use crate::state::{GameConfig, PlayerAuth, PuzzleBoard, PuzzleStats};
use crate::types::constants::*;
use crate::types::{GameError, MoveError};
use crate::utils::{compute_score, is_solved, validate_signer};
use anchor_lang::prelude::*;

pub fn handle_apply_move(ctx: Context<ApplyMove>, from_tube: u8, to_tube: u8) -> Result<()> {
    let clock = Clock::get()?;
    validate_signer(
        &ctx.accounts.signer.key(),
        &*ctx.accounts.player_auth,
        clock.unix_timestamp,
    )?;

    let board = &mut ctx.accounts.puzzle_board;
    let stats = &mut ctx.accounts.puzzle_stats;

    require!(stats.status == 2, MoveError::PuzzleNotActive);

    let from = from_tube as usize;
    let to = to_tube as usize;
    let n = board.num_tubes as usize;

    require!(from < n && to < n, MoveError::InvalidTubeIndex);
    require!(from != to, MoveError::SameTube);
    require!(board.tube_lengths[from] > 0, MoveError::SourceTubeEmpty);
    require!(
        board.tube_lengths[to] < board.max_capacity,
        MoveError::DestinationTubeFull
    );

    let from_top =
        from * crate::state::puzzle_board::MAX_CAPACITY + (board.tube_lengths[from] as usize - 1);
    let ball = board.balls[from_top];
    board.balls[from_top] = 0;
    board.tube_lengths[from] -= 1;

    let to_top = to * crate::state::puzzle_board::MAX_CAPACITY + board.tube_lengths[to] as usize;
    board.balls[to_top] = ball;
    board.tube_lengths[to] += 1;

    board.undo_from = from_tube;
    board.undo_to = to_tube;
    board.undo_ball = ball;
    board.has_undo = true;

    stats.move_count = stats.move_count.saturating_add(1);

    if is_solved(board) {
        stats.is_solved = true;
        stats.completed_at = clock.unix_timestamp;
        stats.status = 3;

        let elapsed_secs = (stats.completed_at - stats.started_at).max(0) as u64;
        stats.final_score = compute_score(
            stats.difficulty,
            stats.move_count,
            elapsed_secs,
            stats.undo_count,
            board.num_colors,
            board.max_capacity,
        );
    }

    Ok(())
}

#[derive(Accounts)]
pub struct ApplyMove<'info> {
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

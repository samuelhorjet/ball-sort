use crate::state::{GameConfig, PlayerAuth, PuzzleBoard, PuzzleStats};
use crate::types::constants::*;
use crate::types::{GameError, PuzzleInitialized, PuzzleStatus};
use crate::utils::{build_vrf_request_ix, validate_signer};
use anchor_lang::prelude::*;
use ephemeral_vrf_sdk::anchor::vrf;

pub fn handle_init_puzzle(
    ctx: Context<InitPuzzle>,
    num_tubes: u8,
    balls_per_tube: u8,
    difficulty: u8,
    game_mode: u8,
) -> Result<()> {
    let clock = Clock::get()?;
    require!(!ctx.accounts.game_config.is_paused, GameError::GamePaused);
    require!(
        !ctx.accounts.player_auth.has_active_puzzle,
        GameError::PuzzleAlreadyActive
    );
    validate_signer(
        &ctx.accounts.signer.key(),
        &*ctx.accounts.player_auth,
        clock.unix_timestamp,
    )?;

    let player_auth_key = ctx.accounts.player_auth.key();
    let oracle_queue_key = ctx.accounts.oracle_queue.key();
    let signer_key = ctx.accounts.signer.key();

    let puzzle_board_key = ctx.accounts.puzzle_board.key();
    let puzzle_stats_key = ctx.accounts.puzzle_stats.key();

    let vrf_ix = build_vrf_request_ix(signer_key, oracle_queue_key, crate::ID, player_auth_key);

    ctx.accounts
        .invoke_signed_vrf(&ctx.accounts.signer.to_account_info(), &vrf_ix)?;

    let auth = &mut ctx.accounts.player_auth;
    auth.has_active_puzzle = true;
    auth.active_puzzle_entity = Some(puzzle_board_key);
    auth.active_puzzle_status = PuzzleStatus::Initialized as u8;
    auth.vrf_randomness = [0u8; 32];
    auth.puzzle_num_tubes = num_tubes;
    auth.puzzle_balls_per_tube = balls_per_tube;
    auth.puzzle_difficulty = difficulty;

    emit!(PuzzleInitialized {
        player: auth.wallet,
        puzzle_entity: puzzle_board_key, // Reusing field for backward compat in tests
        puzzle_board: puzzle_board_key,
        puzzle_stats: puzzle_stats_key,
        num_tubes,
        balls_per_tube,
        difficulty,
        game_mode,
        timestamp: clock.unix_timestamp,
    });

    ctx.accounts.player_auth.puzzles_started_nonce = ctx
        .accounts
        .player_auth
        .puzzles_started_nonce
        .saturating_add(1);

    Ok(())
}

#[vrf]
#[derive(Accounts)]
pub struct InitPuzzle<'info> {
    #[account(mut)]
    pub signer: Signer<'info>,

    #[account(mut, seeds=[SEED_PLAYER_AUTH, player_auth.wallet.as_ref()], bump=player_auth.bump,
              constraint = player_auth.wallet == signer.key()
                        || player_auth.session_key == Some(signer.key()) @ GameError::Unauthorized)]
    pub player_auth: Account<'info, PlayerAuth>,

    #[account(seeds=[SEED_GAME_CONFIG], bump=game_config.bump)]
    pub game_config: Account<'info, GameConfig>,

    #[account(
        init,
        payer = signer,
        space = 8 + PuzzleBoard::INIT_SPACE,
        seeds = [SEED_PUZZLE_BOARD, player_auth.key().as_ref(), &player_auth.puzzles_started_nonce.to_le_bytes()],
        bump
    )]
    pub puzzle_board: Account<'info, PuzzleBoard>,

    #[account(
        init,
        payer = signer,
        space = 8 + PuzzleStats::INIT_SPACE,
        seeds = [SEED_PUZZLE_STATS, player_auth.key().as_ref(), &player_auth.puzzles_started_nonce.to_le_bytes()],
        bump
    )]
    pub puzzle_stats: Account<'info, PuzzleStats>,

    /// CHECK: VRF oracle queue -- #[vrf] injects vrf_program/identity/slot_hashes automatically.
    #[account(mut, address = ephemeral_vrf_sdk::consts::DEFAULT_QUEUE)]
    pub oracle_queue: AccountInfo<'info>,

    pub system_program: Program<'info, System>,
}

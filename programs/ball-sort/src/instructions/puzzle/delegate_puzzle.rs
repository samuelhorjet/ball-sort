use crate::state::{GameConfig, PlayerAuth};
use crate::types::constants::*;
use crate::types::{GameError, PuzzleDelegated};
use anchor_lang::prelude::*;
use ephemeral_rollups_sdk::anchor::{delegate, commit};
use ephemeral_rollups_sdk::cpi::DelegateConfig;
use ephemeral_rollups_sdk::ephem::commit_and_undelegate_accounts;

#[delegate]
#[derive(Accounts)]
pub struct DelegatePuzzle<'info> {
    #[account(mut)]
    pub payer: Signer<'info>,

    #[account(
        seeds = [SEED_PLAYER_AUTH, player_auth.wallet.as_ref()], 
        bump = player_auth.bump,
        constraint = player_auth.wallet == payer.key() 
                  || player_auth.session_key == Some(payer.key()) @ GameError::Unauthorized
    )]
    pub player_auth: Account<'info, PlayerAuth>,

    #[account(seeds=[SEED_GAME_CONFIG], bump=game_config.bump)]
    pub game_config: Account<'info, GameConfig>,

    /// CHECK: The PuzzleBoard we want to delegate
    #[account(mut, del)]
    pub puzzle_board: AccountInfo<'info>,

    /// CHECK: The PuzzleStats we want to delegate 
    #[account(mut, del)]
    pub puzzle_stats: AccountInfo<'info>,

    /// CHECK: The MagicBlock Ephemeral Rollup Validator (TEE)
    pub validator: UncheckedAccount<'info>,
}

pub fn handle_delegate_puzzle(ctx: Context<DelegatePuzzle>) -> Result<()> {
    
    let authority = ctx.accounts.player_auth.to_account_info().key.clone();
    let puzzles_started = ctx.accounts.player_auth.puzzles_started_nonce.saturating_sub(1);
    let puzzles_started_bytes = puzzles_started.to_le_bytes();
    

    // --- Delegate Board ---
    let board_seeds = &[
        b"puzzle_board",
        authority.as_ref(),
        &puzzles_started_bytes,
    ];
    let config_board = DelegateConfig {
        validator: Some(ctx.accounts.validator.key()),
        ..DelegateConfig::default()
    };
    ctx.accounts.delegate_puzzle_board(
        &ctx.accounts.payer, 
        board_seeds, 
        config_board
    )?;

    // --- Delegate Stats ---
    let stats_seeds = &[
        b"puzzle_stats",
        authority.as_ref(),
        &puzzles_started_bytes,
    ];
    let config_stats = DelegateConfig {
        validator: Some(ctx.accounts.validator.key()),
        ..DelegateConfig::default()
    };
    ctx.accounts.delegate_puzzle_stats(
        &ctx.accounts.payer, 
        stats_seeds, 
        config_stats
    )?;

    emit!(PuzzleDelegated {
        puzzle_board: ctx.accounts.puzzle_board.key(),
        player: ctx.accounts.player_auth.wallet,
        validator: Some(ctx.accounts.validator.key()),
        timestamp: Clock::get()?.unix_timestamp,
    });

    Ok(())
}

#[commit]
#[derive(Accounts)]
pub struct UndelegatePuzzle<'info> {
    #[account(mut)]
    pub payer: Signer<'info>,

    #[account(
        seeds=[SEED_PLAYER_AUTH, player_auth.wallet.as_ref()], 
        bump=player_auth.bump,
        constraint = player_auth.wallet == payer.key() 
                  || player_auth.session_key == Some(payer.key()) @ GameError::Unauthorized
    )]
    pub player_auth: Account<'info, PlayerAuth>,

    #[account(seeds=[SEED_GAME_CONFIG], bump=game_config.bump)]
    pub game_config: Account<'info, GameConfig>,

    /// CHECK: PuzzleBoard component PDA
    #[account(mut)]
    pub puzzle_board: UncheckedAccount<'info>,

    /// CHECK: PuzzleStats component PDA
    #[account(mut)]
    pub puzzle_stats: UncheckedAccount<'info>,
}

pub fn handle_undelegate_puzzle(ctx: Context<UndelegatePuzzle>) -> Result<()> {

    commit_and_undelegate_accounts(
        &ctx.accounts.payer,
        vec![
            &ctx.accounts.puzzle_board.to_account_info(),
            &ctx.accounts.puzzle_stats.to_account_info(),
        ],
        &ctx.accounts.magic_context,
        &ctx.accounts.magic_program,
    )?;

    Ok(())
}

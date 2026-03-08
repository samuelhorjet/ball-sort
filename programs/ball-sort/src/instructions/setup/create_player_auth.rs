use crate::state::{GameConfig, PlayerAuth, PlayerProfile};
use crate::types::constants::*;
use crate::types::PlayerAuthCreated;
use anchor_lang::prelude::*;

pub fn handle_create_player_auth(ctx: Context<CreatePlayerAuth>) -> Result<()> {
    let auth = &mut ctx.accounts.player_auth;
    let clock = Clock::get()?;
    let player_key = ctx.accounts.player.key();

    let profile = &mut ctx.accounts.player_profile;
    profile.total_puzzles_started = 0;
    profile.total_puzzles_solved = 0;
    profile.best_score_easy = 0;
    profile.best_score_medium = 0;
    profile.best_score_hard = 0;

    auth.wallet = player_key;
    auth.session_key = None;
    auth.session_expires_at = 0;
    auth.has_active_puzzle = false;
    auth.active_puzzle_entity = None;
    auth.active_puzzle_status = 0;
    auth.total_puzzles_solved = 0;
    auth.soar_player_account = Pubkey::default();
    auth.puzzles_started_nonce = 0;
    auth.bump = ctx.bumps.player_auth;

    emit!(PlayerAuthCreated {
        player: auth.wallet,
        timestamp: clock.unix_timestamp,
    });

    Ok(())
}

#[derive(Accounts)]
pub struct CreatePlayerAuth<'info> {
    #[account(mut)]
    pub player: Signer<'info>,

    #[account(init, payer=player, space=PlayerAuth::SPACE,
              seeds=[SEED_PLAYER_AUTH, player.key().as_ref()], bump)]
    pub player_auth: Account<'info, PlayerAuth>,

    #[account(seeds=[SEED_GAME_CONFIG], bump=game_config.bump)]
    pub game_config: Account<'info, GameConfig>,

    #[account(init, payer=player, space=8 + PlayerProfile::INIT_SPACE,
              seeds=[b"player_profile", player.key().as_ref()], bump)]
    pub player_profile: Account<'info, PlayerProfile>,

    pub system_program: Program<'info, System>,
}

use crate::state::{GameConfig, PlayerAuth};
use crate::types::constants::*;
use anchor_lang::prelude::*;

pub fn handle_create_player_auth(ctx: Context<CreatePlayerAuth>) -> Result<()> {
    let auth = &mut ctx.accounts.player_auth;
    let player_key = ctx.accounts.player.key();

    auth.wallet = player_key;
    auth.session_key = None;
    auth.session_expires_at = 0;
    auth.total_puzzles_solved = 0;
    auth.puzzles_started_nonce = 0;
    auth.bump = ctx.bumps.player_auth;

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

    pub system_program: Program<'info, System>,
}

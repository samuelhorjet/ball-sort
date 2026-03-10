use crate::state::PlayerAuth;
use crate::types::constants::*;
use crate::types::GameError;
use anchor_lang::prelude::*;

pub fn handle_close_session(ctx: Context<CloseSession>) -> Result<()> {
    let auth = &mut ctx.accounts.player_auth;
    auth.session_key = None;
    auth.session_expires_at = 0;
    Ok(())
}

#[derive(Accounts)]
pub struct CloseSession<'info> {
    pub player: Signer<'info>,
    #[account(mut, seeds=[SEED_PLAYER_AUTH, player.key().as_ref()], bump=player_auth.bump,
              constraint = player_auth.wallet == player.key() @ GameError::Unauthorized)]
    pub player_auth: Account<'info, PlayerAuth>,
}

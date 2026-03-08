use crate::state::PlayerAuth;
use crate::types::constants::*;
use crate::types::{GameError, SessionClosed};
use anchor_lang::prelude::*;

pub fn handle_close_session(ctx: Context<CloseSession>) -> Result<()> {
    let auth = &mut ctx.accounts.player_auth;
    let old_key = auth.session_key.unwrap_or_default();
    auth.session_key = None;
    auth.session_expires_at = 0;
    emit!(SessionClosed {
        player: auth.wallet,
        session_key: old_key,
        timestamp: Clock::get()?.unix_timestamp
    });
    Ok(())
}

#[derive(Accounts)]
pub struct CloseSession<'info> {
    pub player: Signer<'info>,
    #[account(mut, seeds=[SEED_PLAYER_AUTH, player.key().as_ref()], bump=player_auth.bump,
              constraint = player_auth.wallet == player.key() @ GameError::Unauthorized)]
    pub player_auth: Account<'info, PlayerAuth>,
}

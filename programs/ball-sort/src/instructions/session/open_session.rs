use crate::state::PlayerAuth;
use crate::types::constants::*;
use crate::types::{GameError, SessionOpened};
use anchor_lang::prelude::*;

pub fn handle_open_session(
    ctx: Context<OpenSession>,
    session_key: Pubkey,
    expires_in_secs: u32,
) -> Result<()> {
    let clock = Clock::get()?;
    require!(
        (expires_in_secs as i64) >= MIN_SESSION_DURATION_SECS,
        GameError::InvalidSessionDuration
    );
    require!(
        (expires_in_secs as i64) <= MAX_SESSION_DURATION_SECS,
        GameError::InvalidSessionDuration
    );

    let auth = &mut ctx.accounts.player_auth;

    if let Some(old_key) = auth.session_key {
        if clock.unix_timestamp < auth.session_expires_at {
            msg!("Replacing valid session key {}", old_key);
        }
    }

    auth.session_key = Some(session_key);
    auth.session_expires_at = clock.unix_timestamp + expires_in_secs as i64;

    emit!(SessionOpened {
        player: auth.wallet,
        session_key,
        expires_at: auth.session_expires_at,
        timestamp: clock.unix_timestamp
    });
    Ok(())
}

#[derive(Accounts)]
pub struct OpenSession<'info> {
    #[account(mut)]
    pub player: Signer<'info>,
    #[account(mut, seeds=[SEED_PLAYER_AUTH, player.key().as_ref()], bump=player_auth.bump,
              constraint = player_auth.wallet == player.key() @ GameError::Unauthorized)]
    pub player_auth: Account<'info, PlayerAuth>,
}

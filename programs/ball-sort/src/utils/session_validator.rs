use anchor_lang::prelude::*;
use crate::state::PlayerAuth;
use crate::types::GameError;

pub fn validate_signer(signer: &Pubkey, auth: &PlayerAuth, now: i64) -> Result<()> {
    require!(auth.is_valid_signer(signer, now), GameError::Unauthorized);
    Ok(())
}

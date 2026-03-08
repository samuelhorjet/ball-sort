use crate::state::PlayerAuth;
use crate::types::PuzzleStatus;
use anchor_lang::prelude::*;

pub fn handle_consume_randomness(
    ctx: Context<ConsumeRandomness>,
    randomness: [u8; 32],
) -> Result<()> {
    let auth = &mut ctx.accounts.player_auth;
    auth.vrf_randomness = randomness;
    auth.active_puzzle_status = PuzzleStatus::BoardReady as u8;

    msg!(
        "VRF callback received. Randomness stored for puzzle {:?}",
        auth.active_puzzle_entity
    );

    Ok(())
}

#[derive(Accounts)]
pub struct ConsumeRandomness<'info> {
    /// VRF program identity PDA — proves the callback is from the VRF program
    #[account(address = ephemeral_vrf_sdk::consts::VRF_PROGRAM_IDENTITY)]
    pub vrf_program_identity: Signer<'info>,

    #[account(mut)]
    pub player_auth: Account<'info, PlayerAuth>,
}

use crate::state::{GameConfig, PlayerAuth};
use crate::types::constants::*;
use crate::types::{GameError, PuzzleStatus};
use anchor_lang::prelude::*;

pub fn handle_abandon_puzzle(ctx: Context<AbandonPuzzle>) -> Result<()> {
    let auth = &mut ctx.accounts.player_auth;

    require!(
        auth.active_puzzle_entity.is_some(),
        GameError::NoPuzzleActive
    );

    auth.has_active_puzzle = false;
    auth.active_puzzle_entity = None;
    auth.active_puzzle_status = PuzzleStatus::Abandoned as u8;

    msg!("abandon_puzzle: abandoned");
    Ok(())
}

#[derive(Accounts)]
pub struct AbandonPuzzle<'info> {
    #[account(mut)]
    pub signer: Signer<'info>,

    #[account(mut, seeds=[SEED_PLAYER_AUTH, player_auth.wallet.as_ref()], bump=player_auth.bump,
              constraint = player_auth.wallet == signer.key()
                        || player_auth.session_key == Some(signer.key()) @ GameError::Unauthorized)]
    pub player_auth: Account<'info, PlayerAuth>,

    #[account(seeds=[SEED_GAME_CONFIG], bump=game_config.bump)]
    pub game_config: Account<'info, GameConfig>,
}

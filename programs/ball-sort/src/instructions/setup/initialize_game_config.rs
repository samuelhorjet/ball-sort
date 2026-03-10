use crate::state::GameConfig;
use crate::types::constants::*;
use crate::types::{GameConfigInitialized, GameError};
use anchor_lang::prelude::*;

pub fn handle_initialize_game_config(
    ctx: Context<InitializeGameConfig>,
    params: InitGameConfigParams,
) -> Result<()> {
    require!(
        params.treasury_fee_bps <= MAX_TREASURY_FEE_BPS,
        GameError::FeeTooHigh
    );
    let cfg = &mut ctx.accounts.game_config;
    cfg.authority = ctx.accounts.authority.key();
    cfg.treasury = params.treasury;
    cfg.treasury_fee_bps = params.treasury_fee_bps;
    cfg.is_paused = false;
    cfg.tournament_count = 0;
    cfg.bump = ctx.bumps.game_config;
    emit!(GameConfigInitialized {
        authority: cfg.authority,
        treasury: cfg.treasury,
        timestamp: Clock::get()?.unix_timestamp
    });
    Ok(())
}

#[derive(AnchorSerialize, AnchorDeserialize)]
pub struct InitGameConfigParams {
    pub treasury: Pubkey,
    pub treasury_fee_bps: u16,
}

#[derive(Accounts)]
pub struct InitializeGameConfig<'info> {
    #[account(mut)]
    pub authority: Signer<'info>,
    #[account(init, payer=authority, space=GameConfig::SPACE,
              seeds=[SEED_GAME_CONFIG], bump)]
    pub game_config: Account<'info, GameConfig>,
    pub system_program: Program<'info, System>,
}

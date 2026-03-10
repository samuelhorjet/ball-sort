use crate::state::GameConfig;
use crate::types::constants::*;
use crate::types::{GameConfigUpdated, GameError};
use anchor_lang::prelude::*;

pub fn handle_update_game_config(
    ctx: Context<UpdateGameConfig>,
    p: UpdateGameConfigParams,
) -> Result<()> {
    let cfg = &mut ctx.accounts.game_config;
    let ts = Clock::get()?.unix_timestamp;

    if let Some(v) = p.treasury {
        cfg.treasury = v;
        emit!(GameConfigUpdated {
            authority: ctx.accounts.authority.key(),
            field: "treasury".into(),
            timestamp: ts
        });
    }

    if let Some(v) = p.is_paused {
        cfg.is_paused = v;
        emit!(GameConfigUpdated {
            authority: ctx.accounts.authority.key(),
            field: "is_paused".into(),
            timestamp: ts
        });
    }
    if let Some(v) = p.treasury_fee_bps {
        require!(v <= MAX_TREASURY_FEE_BPS, GameError::FeeTooHigh);
        cfg.treasury_fee_bps = v;
        emit!(GameConfigUpdated {
            authority: ctx.accounts.authority.key(),
            field: "treasury_fee_bps".into(),
            timestamp: ts
        });
    }
    Ok(())
}

#[derive(AnchorSerialize, AnchorDeserialize)]
pub struct UpdateGameConfigParams {
    pub treasury: Option<Pubkey>,
    pub treasury_fee_bps: Option<u16>,
    pub is_paused: Option<bool>,
}

#[derive(Accounts)]
pub struct UpdateGameConfig<'info> {
    pub authority: Signer<'info>,
    #[account(mut, seeds=[SEED_GAME_CONFIG], bump=game_config.bump,
              constraint = game_config.authority == authority.key() @ GameError::NotGameAuthority)]
    pub game_config: Account<'info, GameConfig>,
}

use anchor_lang::prelude::*;
use ephemeral_vrf_sdk::instructions::{create_request_randomness_ix, RequestRandomnessParams};
use ephemeral_vrf_sdk::types::SerializableAccountMeta;

pub fn build_vrf_request_ix(
    payer: Pubkey,
    oracle_queue: Pubkey,
    callback_program_id: Pubkey,
    player_auth: Pubkey,
) -> anchor_lang::solana_program::instruction::Instruction {
    create_request_randomness_ix(RequestRandomnessParams {
        payer,
        oracle_queue,
        callback_program_id,
        callback_discriminator: crate::instruction::ConsumeRandomness::DISCRIMINATOR.to_vec(),
        caller_seed: player_auth.to_bytes(),
        accounts_metas: Some(vec![SerializableAccountMeta {
            pubkey: player_auth,
            is_signer: false,
            is_writable: true,
        }]),
        ..Default::default()
    })
}

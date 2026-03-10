use crate::state::{GameConfig, PlayerAuth, PuzzleBoard, PuzzleStats};
use crate::types::constants::*;
use crate::types::{GameError, PuzzleStarted, PuzzleStatus};
use crate::utils::validate_signer;
use anchor_lang::prelude::*;

pub fn handle_start_puzzle(ctx: Context<StartPuzzle>) -> Result<()> {
    let clock = Clock::get()?;
    validate_signer(
        &ctx.accounts.signer.key(),
        &*ctx.accounts.player_auth,
        clock.unix_timestamp,
    )?;

    require!(
        ctx.accounts.player_auth.active_puzzle_status == PuzzleStatus::BoardReady as u8,
        GameError::InvalidPuzzleStatus
    );

    let puzzle_entity = ctx
        .accounts
        .player_auth
        .active_puzzle_entity
        .ok_or(error!(GameError::InvalidPuzzleStatus))?;

    let board = &mut ctx.accounts.puzzle_board;
    let stats = &mut ctx.accounts.puzzle_stats;

    require!(board.key() == puzzle_entity, GameError::Unauthorized);

    let num_tubes = ctx.accounts.player_auth.puzzle_num_tubes;
    let balls_per_tube = ctx.accounts.player_auth.puzzle_balls_per_tube;
    let difficulty = ctx.accounts.player_auth.puzzle_difficulty;
    let vrf_seed = ctx.accounts.player_auth.vrf_randomness;
    let num_colors = num_tubes - 1;

    let mut balls = [0u8; crate::state::puzzle_board::BALLS_LEN];
    let mut tube_lengths = [0u8; crate::state::puzzle_board::MAX_TUBES];

    // 1. Initial setup (sorted perfectly)
    for color in 0..num_colors as usize {
        for slot in 0..balls_per_tube as usize {
            balls[color * crate::state::puzzle_board::MAX_CAPACITY as usize + slot] =
                (color + 1) as u8;
        }
        tube_lengths[color] = balls_per_tube;
    }

    let filled = num_colors as usize * balls_per_tube as usize;
    let mut rng = SeededRng::new(vrf_seed);

    // HELPER: Maps a contiguous index (0 to 'filled') to the actual padded board index
    let get_real_index = |idx: usize| -> usize {
        let bpt = balls_per_tube as usize;
        let max_cap = crate::state::puzzle_board::MAX_CAPACITY as usize;
        (idx / bpt) * max_cap + (idx % bpt)
    };

    // 2. Intelligent Shuffle based on Difficulty
    match difficulty {
        0 => {
            // EASY: "Local Shuffle"
            // Swap balls within a small sliding window. This keeps similar colors
            // somewhat clumped together, avoiding extreme fragmentation.
            for i in (1..filled).rev() {
                let window = 5.min(i as u64); // Only swap with nearby neighbors
                let j = i - rng.next_bounded(window + 1) as usize;
                balls.swap(get_real_index(i), get_real_index(j));
            }
        }
        1 => {
            // MEDIUM: "True Randomness"
            // Standard Fisher-Yates shuffle. Pure VRF entropy.
            for i in (1..filled).rev() {
                let j = rng.next_bounded(i as u64 + 1) as usize;
                balls.swap(get_real_index(i), get_real_index(j));
            }
        }
        __=> {
            // HARD: Adversarial Disorder
            // Step A: Pure random shuffle first
            for i in (1..filled).rev() {
                let j = rng.next_bounded(i as u64 + 1) as usize;
                balls.swap(get_real_index(i), get_real_index(j));
            }

            // Step B: Aggressive Anti-Clumping Pass (Run 3 sweeps)
            let bpt = balls_per_tube as usize;
            for _ in 0..3 {
                for i in 1..filled {
                    // Make sure we only check balls within the same tube
                    if i % bpt != 0 {
                        let curr = get_real_index(i);
                        let prev = get_real_index(i - 1);

                        // If two balls stacked on each other match...
                        if balls[curr] == balls[prev] {
                            // Banish the top one to a random spot that DOES NOT match
                            for _attempt in 0..7 {
                                let j = rng.next_bounded(filled as u64) as usize;
                                let target = get_real_index(j);
                                // Ensure we aren't just creating a new pair!
                                if balls[target] != balls[curr] {
                                    balls.swap(curr, target);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    board.num_tubes = num_tubes;
    board.num_colors = num_colors;
    board.max_capacity = balls_per_tube;
    board.balls = balls;
    board.tube_lengths = tube_lengths;
    board.vrf_seed = vrf_seed;
    board.has_undo = false;
    board.undo_from = 0;
    board.undo_to = 0;
    board.undo_ball = 0;

    stats.status = 2; // Active
    stats.difficulty = difficulty;
    stats.num_tubes = num_tubes;
    stats.balls_per_tube = balls_per_tube;
    stats.move_count = 0;
    stats.undo_count = 0;
    stats.started_at = clock.unix_timestamp;
    stats.completed_at = 0;
    stats.is_solved = false;
    stats.final_score = 0;

    // Update PlayerAuth status to Started
    let auth = &mut ctx.accounts.player_auth;
    auth.active_puzzle_status = PuzzleStatus::Started as u8;

    emit!(PuzzleStarted {
        puzzle_entity,
        player: auth.wallet,
        started_at: clock.unix_timestamp,
        timestamp: clock.unix_timestamp,
    });

    Ok(())
}

struct SeededRng {
    state: [u8; 32],
    pos: usize,
}

impl SeededRng {
    fn new(seed: [u8; 32]) -> Self {
        Self {
            state: seed,
            pos: 0,
        }
    }

    fn next_u64(&mut self) -> u64 {
        if self.pos + 8 > 32 {
            for i in 0..32 {
                self.state[i] ^= self.state[(i + 7) % 32]
                    .wrapping_add(self.state[(i + 13) % 32])
                    .rotate_left(3);
            }
            self.pos = 0;
        }
        let bytes: [u8; 8] = self.state[self.pos..self.pos + 8].try_into().unwrap();
        self.pos += 8;
        u64::from_le_bytes(bytes)
    }

    fn next_bounded(&mut self, bound: u64) -> u64 {
        if bound == 0 {
            return 0;
        }
        self.next_u64() % bound
    }
}

#[derive(Accounts)]
pub struct StartPuzzle<'info> {
    pub signer: Signer<'info>,

    #[account(mut, seeds=[SEED_PLAYER_AUTH, player_auth.wallet.as_ref()], bump=player_auth.bump,
              constraint = player_auth.wallet == signer.key()
                        || player_auth.session_key == Some(signer.key()) @ GameError::Unauthorized)]
    pub player_auth: Account<'info, PlayerAuth>,

    #[account(seeds=[SEED_GAME_CONFIG], bump=game_config.bump)]
    pub game_config: Account<'info, GameConfig>,

    #[account(
        mut,
        seeds = [SEED_PUZZLE_BOARD, player_auth.key().as_ref(), &player_auth.puzzles_started_nonce.saturating_sub(1).to_le_bytes()],
        bump
    )]
    pub puzzle_board: Account<'info, PuzzleBoard>,

    #[account(
        mut,
        seeds = [SEED_PUZZLE_STATS, player_auth.key().as_ref(), &player_auth.puzzles_started_nonce.saturating_sub(1).to_le_bytes()],
        bump
    )]
    pub puzzle_stats: Account<'info, PuzzleStats>,
}

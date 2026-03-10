# Ball Sort Privacy Game Protocol

## 1. Overview

Ball Sort is a privacy-preserving, fully on-chain puzzle game built on Solana using Anchor, Magicblock VRF, and MagicBlock Private Ephemeral Rollups (ER).

Unlike traditional Web3 games where every move requires a wallet popup and exposes player strategy, Ball Sort utilizes Session Keys and Private TEE (Trusted Execution Environment) execution. Players solve complex generated puzzles instantly and privately.

The protocol separates private gameplay from public settlement:

- Sensitive user actions (moves, undos, puzzle state) happen inside a private ephemeral rollup.
- Only finalized results (completion time, move count) are committed to Solana L1 for tournament settlement.

### Links

Frontend Repo  
https://github.com/samuelhorjet/ball-sort-frontend

Smart Contract Repo  
https://github.com/samuelhorjet/ball-sort

Demo Video  
https://youtu.be/placeholder

---

# 2. Core Architecture

## 2.1 On-chain Program (L1)

Written in Rust using the Anchor framework and deployed on Solana.

Manages:

- Global game configuration and treasury
- Switchboard VRF integration for provably fair puzzle generation
- Tournament lifecycle and parimutuel prize pools
- PDA ownership of player profiles and tournament entries

---

## 2.2 Private Execution Layer (MagicBlock)

Enabled via MagicBlock Ephemeral Rollups and Permission Accounts.

Handles:

- Private `apply_move` and `apply_undo` execution
- Gasless transactions via Session Keys

Prevents:

Strategy Leakage / Copy-playing  
Competitors cannot see how you solved the puzzle on-chain.

RPC Lag  
Provides instant real-time gameplay without waiting for L1 block finality.

---

## 2.3 Public Settlement Layer

Final states are flushed back to Solana via TEE undelegation.

This layer:

- Verifies puzzle completion
- Calculates Parimutuel Weights
- Enables permissionless tournament reward claiming

---

# 3. Protocol Lifecycle (End-to-End Flow)

## 3.1 Protocol Initialization (Admin)

```rust
initialize_game_config(params)
```

Creates the global game config PDA.

Sets treasury wallet, protocol fees (`treasury_fee_bps`), and links the VRF oracle authorities.

---

## 3.2 Tournament Creation (Admin)

```rust
create_tournament(entry_fee, difficulty, duration_secs)
```

Defines a gameplay window:

`start_time → end_time`

Initializes the prize pool and tracks total completers.

---

## 3.3 Player Setup & Session Activation (Users)

```rust
create_player_auth()
open_session(session_key, expires_in_secs)
```

Creates the PlayerAuth PDA and player profile.

Injects a local Session Key authorized to sign gameplay transactions on the player's behalf, eliminating wallet popups.

---

## 3.4 Puzzle Generation (VRF)

```rust
init_puzzle(num_tubes, balls_per_tube, difficulty)
consume_randomness(randomness)
```

Player requests a new puzzle board.

Switchboard VRF securely provides true randomness.

The `start_puzzle` instruction shuffles the balls based on the VRF seed.

---

## 3.5 Privacy Activation (TEE Delegation)

```rust
create_puzzle_permissions()
delegate_puzzle_permissions()
delegate_puzzle()
```

Creates permission accounts locking the `puzzle_board` and `puzzle_stats`.

Transfers PDA authority to the MagicBlock TEE.

From this point:

- Gameplay becomes privately mutable
- No public on-chain writes are emitted for individual moves

---

## 3.6 Private Gameplay

```rust
apply_move(from_tube, to_tube)
apply_undo()
```

Executed securely inside the TEE using the Session Key.

---

# 4. Pool Resolution & Settlement

## 4.1 Flush State Back to L1

```rust
undelegate_puzzle()
```

Called once the puzzle is solved or abandoned.

The TEE commits the final state:

- move_count
- undo_count
- elapsed_secs

This ends the private execution phase.

---

## 4.2 Tournament Finalization

```rust
finalize_puzzle()
record_tournament_result(elapsed_secs, move_count)
```

Computes the player's performance weight based on speed and efficiency.

Adds the player's weight to the tournament's cumulative_weight.

---

# 5. Weight Calculation Model (Core Math)

Ball Sort uses a Parimutuel Distribution System.

Rewards are not "winner takes all". They are distributed proportionally based on a calculated weight of your speed and efficiency.

---

## 5.1 Weight Formula

```rust
pub fn parimutuel_weight(elapsed_secs: u64, move_count: u32) -> u128 {
    let time  = elapsed_secs.max(1) as u128;
    let moves = (move_count as u128).max(1);

    PARIMUTUEL_SCALAR
        .checked_div(time.saturating_mul(moves))
        .unwrap_or(0)
}
```

### Factors

Time Factor  
Lower `elapsed_secs` results in a higher score.

Move Factor  
Lower `move_count` results in a higher score.

Scalar  
Uses a high precision scalar constant to divide the product of time and moves, ensuring granular and competitive weights.

---

# 6. Finalization & Public Settlement

## 6.1 Close Tournament (Admin)

```rust
close_tournament()
```

Requirements:

- Tournament end_time has passed
- Tournament is not already closed

Actions:

- Deduct the protocol/treasury fee
- Lock the `net_prize_pool`
- Open the window for players to claim

---

# 7. Reward Claiming (Users)

## 7.1 Completers (Claim Prize)

```rust
claim_prize()
```

Permissionless claim for users who successfully finished the puzzle.

Reward formula:

```rust
amount = (player_weight * net_prize_pool) / cumulative_weight
```

---

# 8. Key Guarantees

### Privacy

Individual puzzle moves (`apply_move`) are executed in a secure enclave. Competitors cannot scrape RPC data to copy your solution path.

### Fairness

Switchboard VRF guarantees every generated board is uniquely and fairly randomized.

### Gasless UX

Session keys remove the need to sign transactions for every move.

### Verifiability

The final `move_count` and `elapsed_secs` are validated on Solana L1 against the TEE’s cryptographically signed state transition.

---

# 9. Summary

Ball Sort introduces a new standard for fully on-chain gaming by combining:

- Provably fair VRF puzzle generation
- Private, gasless ephemeral execution
- Trustless parimutuel tournament settlement

By leveraging MagicBlock Private Ephemeral Rollups and Solana’s PDA model, Ball Sort delivers a real-time and responsive gaming experience without sacrificing decentralization or privacy.
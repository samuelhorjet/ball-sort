use anchor_lang::prelude::*;

#[error_code]
pub enum GameError {
    #[msg("Unauthorized signer — not the player wallet or a valid session key")]
    Unauthorized,
    #[msg("Session key has expired")]
    SessionExpired,
    #[msg("Only the game authority can call this")]
    NotGameAuthority,
    #[msg("Only the tournament authority can call this")]
    NotTournamentAuthority,
    #[msg("Unauthorized VRF caller — not the whitelisted VRF authority")]
    UnauthorizedVrfCaller,
    #[msg("Game is currently paused")]
    GamePaused,
    #[msg("Player already has an active puzzle")]
    PuzzleAlreadyActive,
    #[msg("Puzzle is not in the expected status")]
    InvalidPuzzleStatus,
    #[msg("Board not ready yet — wait for VRF callback")]
    BoardNotReady,
    #[msg("Puzzle not started — call start_puzzle first")]
    PuzzleNotStarted,
    #[msg("No active puzzle — start one first")]
    NoPuzzleActive,
    #[msg("Puzzle is not solved — cannot finalize")]
    PuzzleNotSolved,
    #[msg("Invalid difficulty level (must be 0, 1, or 2)")]
    InvalidDifficulty,
    #[msg("Invalid game mode")]
    InvalidGameMode,
    #[msg("Delegation to Ephemeral Rollup failed")]
    DelegationFailed,
    #[msg("Undelegation / commit to L1 failed")]
    UndelegationFailed,
    #[msg("Failed to create Private ER permission account")]
    PermissionSetupFailed,
    #[msg("Failed to remove Private ER permission")]
    PermissionRemovalFailed,
    #[msg("No active session key to close")]
    NoActiveSession,
    #[msg("Tournament is not open")]
    TournamentNotOpen,
    #[msg("Tournament window has not expired yet")]
    TournamentStillActive,
    #[msg("Tournament is not closed yet")]
    TournamentNotClosed,
    #[msg("Player has already played in this tournament")]
    AlreadyPlayed,
    #[msg("Player has already claimed")]
    AlreadyClaimed,
    #[msg("Only completers can claim prize — DNF players use claim_refund")]
    NotCompleter,
    #[msg("Arithmetic overflow")]
    ArithmeticOverflow,
    #[msg("Treasury fee exceeds maximum of 2000 bps (20%)")]
    FeeTooHigh,
    #[msg("Session duration out of allowed range (60s - 3600s)")]
    InvalidSessionDuration,
}

using System;
using UnityEngine;

namespace BallSort.Core
{
    /// <summary>
    /// All possible screens in the app.
    /// </summary>
    public enum GameScreen
    {
        Splash,
        Tutorial,
        TutorialSuccess,
        Dashboard,
        GameSetup,
        GameLoading,
        GameActive,
        GameSolved,
        GameFinalizing,
        Leaderboard,
        Tournament,
        Profile
    }

    /// <summary>
    /// All on-chain loading sub-steps shown in GameLoadingScreen.
    /// </summary>
    public enum LoadingStep
    {
        None,
        OpeningSession,
        InitializingPuzzle,
        WaitingForVRF,
        CreatingPermissions,
        DelegatingPermissions,
        DelegatingPuzzle,
        StartingPuzzle,
        Undelegating,
        Finalizing,
        ClosingSession
    }

    /// <summary>
    /// Central singleton managing screen navigation and game-wide state.
    /// All screens subscribe to OnScreenChanged to show/hide themselves.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        // ── Events ──────────────────────────────────────────────────────────
        public event Action<GameScreen, GameScreen> OnScreenChanged;   // (from, to)
        public event Action<LoadingStep, string>    OnLoadingStepChanged;
        public event Action<string>                 OnError;

        // ── State ────────────────────────────────────────────────────────────
        public GameScreen   CurrentScreen  { get; private set; } = GameScreen.Splash;
        public LoadingStep  CurrentStep    { get; private set; } = LoadingStep.None;
        public string       LoadingMessage { get; private set; } = "";
        public string       LastError      { get; private set; } = "";

        // Puzzle setup params (set by GameSetupScreen)
        public int  SelectedTubes      { get; set; } = 6;
        public int  SelectedBalls      { get; set; } = 4;
        public byte SelectedDifficulty { get; set; } = 1; // 1=Easy, 2=Medium, 3=Hard

        // ── Unity lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── Navigation ───────────────────────────────────────────────────────
        public void NavigateTo(GameScreen screen)
        {
            if (screen == CurrentScreen) return;
            var prev = CurrentScreen;
            CurrentScreen = screen;
            Debug.Log($"[GameState] {prev} → {screen}");
            OnScreenChanged?.Invoke(prev, screen);
        }

        public void SetLoadingStep(LoadingStep step, string message = "")
        {
            CurrentStep    = step;
            LoadingMessage = message;
            OnLoadingStepChanged?.Invoke(step, message);
        }

        public void RaiseError(string message)
        {
            LastError = message;
            Debug.LogError($"[GameError] {message}");
            OnError?.Invoke(message);
        }

        // ── Convenience shortcuts ─────────────────────────────────────────────
        public void GoToDashboard()    => NavigateTo(GameScreen.Dashboard);
        public void GoToGameSetup()    => NavigateTo(GameScreen.GameSetup);
        public void GoToGameLoading()  => NavigateTo(GameScreen.GameLoading);
        public void GoToGameActive()   => NavigateTo(GameScreen.GameActive);
        public void GoToGameSolved()   => NavigateTo(GameScreen.GameSolved);
        public void GoToFinalizing()   => NavigateTo(GameScreen.GameFinalizing);
        public void GoToLeaderboard()  => NavigateTo(GameScreen.Leaderboard);
        public void GoToTournament()   => NavigateTo(GameScreen.Tournament);
        public void GoToProfile()      => NavigateTo(GameScreen.Profile);
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using BallSort.Core;

namespace BallSort.UI
{
    /// <summary>
    /// Manages showing/hiding UI Toolkit screens based on GameStateManager events.
    /// Each screen is a VisualElement loaded from a UXML asset.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        [Header("Global Style Sheets — drag USS files here")]
        [SerializeField] private StyleSheet _variablesUss;  // ← drag Variables.uss here
        [SerializeField] private StyleSheet _commonUss;     // ← drag Common.uss here
        [SerializeField] private StyleSheet _gameUss;       // ← drag Game.uss here

        [Header("UXML Assets")]
        [SerializeField] private VisualTreeAsset _splashUxml;
        [SerializeField] private VisualTreeAsset _tutorialUxml;
        [SerializeField] private VisualTreeAsset _tutorialSuccessUxml;
        [SerializeField] private VisualTreeAsset _dashboardUxml;
        [SerializeField] private VisualTreeAsset _gameSetupUxml;
        [SerializeField] private VisualTreeAsset _gameLoadingUxml;
        [SerializeField] private VisualTreeAsset _gameUxml;
        [SerializeField] private VisualTreeAsset _solvedUxml;
        [SerializeField] private VisualTreeAsset _leaderboardUxml;
        [SerializeField] private VisualTreeAsset _tournamentUxml;
        [SerializeField] private VisualTreeAsset _profileUxml;

        private UIDocument _doc;
        private VisualElement _root;

        // Active screen controllers
        private readonly Dictionary<GameScreen, IScreenController> _controllers = new();
        private IScreenController _activeController;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _doc  = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement;

            // ── FIX: Validate root is ready ──────────────────────────────────
            // rootVisualElement can be null if UIDocument has no Source Asset AND
            // the panel hasn't been initialized yet. Force a frame delay if needed.
            if (_root == null)
            {
                Debug.LogError("[ScreenManager] rootVisualElement is null! " +
                    "Assign a Source Asset (even an empty Root.uxml) to the UIDocument on this GameObject.");
            }
            else
            {
                Debug.Log($"[ScreenManager] Awake OK — root size will resolve at Start.");
            }
        }

        private void Start()
        {
            if (_root == null)
            {
                Debug.LogError("[ScreenManager] Start aborted — _root is null.");
                return;
            }

            Debug.Log("[ScreenManager] Start — building screens...");

            // ── MUST happen FIRST before any UXML is instantiated ──────────
            // Adds Variables.uss + Common.uss to the root so every var(--xyz)
            // in inline styles can resolve.
            if (_variablesUss != null) _root.styleSheets.Add(_variablesUss);
            else Debug.LogWarning("[ScreenManager] Variables.uss not assigned!");
            if (_commonUss    != null) _root.styleSheets.Add(_commonUss);
            else Debug.LogWarning("[ScreenManager] Common.uss not assigned!");
            if (_gameUss      != null) _root.styleSheets.Add(_gameUss);
            else Debug.LogWarning("[ScreenManager] Game.uss not assigned!");
            // ─────────────────────────────────────────────────────────────────

            // ── FIX: Make root fill the panel explicitly ──────────────────────
            // Without a Source Asset, the root VE may not auto-fill. This ensures
            // all absolute-positioned child panels have a concrete parent size.
            _root.style.position = Position.Absolute;
            _root.style.left   = 0;
            _root.style.top    = 0;
            _root.style.right  = 0;
            _root.style.bottom = 0;
            _root.AddToClassList("fill");

            RegisterControllers();
            GameStateManager.Instance.OnScreenChanged += OnScreenChanged;

            // GameStateManager.CurrentScreen defaults to GameScreen.Splash, so
            // NavigateTo(Splash) hits the guard and exits — Show() is never called.
            // Fix: show the initial screen directly, bypassing NavigateTo.
            if (_controllers.TryGetValue(GameScreen.Splash, out var initial))
            {
                _activeController = initial;
                initial.Show();
                Debug.Log("[ScreenManager] Splash screen shown.");
            }
            else
            {
                Debug.LogError("[ScreenManager] Splash controller not found!");
            }
        }

        private void RegisterControllers()
        {
            Register(GameScreen.Splash,          new SplashScreen(_splashUxml));
            Register(GameScreen.Tutorial,         new TutorialScreen(_tutorialUxml));
            Register(GameScreen.TutorialSuccess,  new TutorialSuccessScreen(_tutorialSuccessUxml));
            Register(GameScreen.Dashboard,        new DashboardScreen(_dashboardUxml));
            Register(GameScreen.GameSetup,        new GameSetupScreen(_gameSetupUxml));
            Register(GameScreen.GameLoading,      new GameLoadingScreen(_gameLoadingUxml));
            Register(GameScreen.GameActive,       new GameScreen_(_gameUxml));
            Register(GameScreen.GameSolved,       new SolvedScreen(_solvedUxml));
            Register(GameScreen.Leaderboard,      new LeaderboardScreen(_leaderboardUxml));
            Register(GameScreen.Tournament,       new TournamentScreen(_tournamentUxml));
            Register(GameScreen.Profile,          new ProfileScreen(_profileUxml));
        }

        private void Register(GameScreen screen, IScreenController controller)
        {
            _controllers[screen] = controller;
            controller.Build(_root);
            controller.Hide();
        }

        private void OnScreenChanged(GameScreen from, GameScreen to)
        {
            Debug.Log($"[ScreenManager] Transition: {from} → {to}");

            if (_activeController != null)
                _activeController.Hide();

            if (_controllers.TryGetValue(to, out var next))
            {
                _activeController = next;
                next.Show();
            }
            else
            {
                Debug.LogWarning($"[ScreenManager] No controller registered for {to}");
            }
        }

        public T GetController<T>(GameScreen screen) where T : class, IScreenController
            => _controllers.TryGetValue(screen, out var c) ? c as T : null;
    }

    // ── Screen controller interface ───────────────────────────────────────────
    public interface IScreenController
    {
        void Build(VisualElement root);
        void Show();
        void Hide();
    }

    // ── Base screen controller ────────────────────────────────────────────────
    public abstract class BaseScreenController : IScreenController
    {
        protected VisualElement Panel;
        private readonly VisualTreeAsset _uxml;

        protected BaseScreenController(VisualTreeAsset uxml) => _uxml = uxml;

        public virtual void Build(VisualElement root)
        {
            if (_uxml == null)
            {
                Debug.LogWarning($"[{GetType().Name}] UXML is null — panel will be empty.");
                Panel = new VisualElement();
                root.Add(Panel);
                return;
            }

            // ── KEY FIX ────────────────────────────────────────────────────
            // Add Panel to root FIRST so it inherits USS variables,
            // then CloneTree(Panel) so var(--xyz) resolves correctly.
            Panel = new VisualElement();
            Panel.style.position = Position.Absolute;
            Panel.style.left     = 0;
            Panel.style.top      = 0;
            Panel.style.right    = 0;
            Panel.style.bottom   = 0;
            Panel.style.display  = DisplayStyle.None;
            root.Add(Panel);          // attach first → inherits stylesheets
            _uxml.CloneTree(Panel);   // clone into it → var() resolves

            OnBuild();
        }

        protected virtual void OnBuild() { }

        public virtual void Show()
        {
            Panel.style.display = DisplayStyle.Flex;

            // ── FIX: Remove the opacity fade-in ────────────────────────────
            // The original code set opacity=0 then scheduled opacity=1 after 10ms.
            // If the UIDocument panel renderer isn't fully active on that first
            // frame (common when Source Asset is None), the schedule never fires
            // and the panel stays permanently invisible.
            // Solution: set opacity directly to 1 — visually instant but correct.
            Panel.style.opacity = 1;

            OnShow();
        }

        public virtual void Hide()
        {
            Panel.style.display = DisplayStyle.None;
            OnHide();
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
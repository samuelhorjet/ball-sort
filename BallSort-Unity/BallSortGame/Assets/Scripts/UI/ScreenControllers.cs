using System;
using UnityEngine;
using UnityEngine.UIElements;
using BallSort.Core;
using BallSort.Game;

namespace BallSort.UI
{
    // ── SplashScreen ──────────────────────────────────────────────────────────
    public class SplashScreen : BaseScreenController
    {
        public SplashScreen(VisualTreeAsset uxml) : base(uxml) { }

        private VisualElement _progressFill;
        private Label _progressLabel;
        private Label _progressPercent;
        private float _progress;
        private bool _done;   // guard: stop animating once navigation is scheduled

        protected override void OnBuild()
        {
            _progressFill = Panel.Q<VisualElement>("ProgressFill");
            _progressLabel = Panel.Q<Label>("ProgressLabel");
            _progressPercent = Panel.Q<Label>("ProgressPercent");
        }

        protected override void OnShow()
        {
            _progress = 0;
            _done = false;
            Panel.schedule.Execute(AnimateProgress).Every(30);
        }

        protected override void OnHide()
        {
            // Mark done so AnimateProgress no-ops if the schedule fires after hide.
            // Unity UI Toolkit schedules keep ticking on hidden elements, so this
            // guard prevents NavigateTo() being called again from the old schedule.
            _done = true;
        }

        private void AnimateProgress()
        {
            if (_done) return;  // screen already hidden — do nothing

            _progress += UnityEngine.Random.Range(0.8f, 2.2f);
            if (_progress >= 100f) _progress = 100f;

            if (_progressFill != null) _progressFill.style.width = Length.Percent(_progress);
            if (_progressPercent != null) _progressPercent.text = $"{Mathf.FloorToInt(_progress)}%";
            if (_progressLabel != null)
            {
                _progressLabel.text = _progress < 30 ? "Initializing game engine..." :
                                      _progress < 60 ? "Loading puzzle mechanics..." :
                                      _progress < 90 ? "Connecting to Solana..." : "Ready.";
            }

            if (_progress >= 100f)
            {
                _done = true;  // stop future ticks BEFORE scheduling navigation
                Panel.schedule.Execute(() =>
                {
                    var target = WalletManager.Instance.IsConnected
                        ? GameScreen.Dashboard
                        : GameScreen.Tutorial;
                    GameStateManager.Instance.NavigateTo(target);
                }).ExecuteLater(400);
            }
        }
    }

    // ── TutorialScreen ────────────────────────────────────────────────────────
    public class TutorialScreen : BaseScreenController
    {
        public TutorialScreen(VisualTreeAsset uxml) : base(uxml) { }

        private TutorialEngine _engine;
        private TutorialBoardRenderer _boardRenderer;

        protected override void OnBuild()
        {
            _engine = new TutorialEngine();
            _boardRenderer = new TutorialBoardRenderer(Panel, _engine);
            _boardRenderer.OnComplete = () =>
                GameStateManager.Instance.NavigateTo(GameScreen.TutorialSuccess);

            Panel.Q<Button>("UndoBtn")?.RegisterCallback<ClickEvent>(_ =>
            {
                _engine.Undo();
                _boardRenderer.Refresh();
            });

            // Skip button — goes straight to TutorialSuccess without solving
            Panel.Q<Button>("SkipBtn")?.RegisterCallback<ClickEvent>(_ =>
                GameStateManager.Instance.NavigateTo(GameScreen.TutorialSuccess));
        }

        protected override void OnShow()
        {
            _engine.Reset();
            _boardRenderer.StopTimer();

            // Ensure timer label exists in panel
            if (Panel.Q<Label>("TimerLabel") == null)
            {
                // Build HUD bar above board if not already present
                var hud = Panel.Q<VisualElement>("TutorialHUD");
                if (hud == null)
                {
                    hud = new VisualElement();
                    hud.name = "TutorialHUD";
                    hud.style.flexDirection = FlexDirection.Row;
                    hud.style.justifyContent = Justify.SpaceAround;
                    hud.style.alignItems = Align.Center;
                    hud.style.paddingTop = hud.style.paddingBottom = 8;
                    hud.style.borderBottomWidth = 1;
                    hud.style.borderBottomColor = new StyleColor(new Color(1, 1, 1, 0.07f));
                    hud.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.3f));

                    // Timer
                    var timerCol = MakeHudStat(out var timerLbl, "00:00", "TIME");
                    timerLbl.name = "TimerLabel";
                    hud.Add(timerCol);

                    // Moves
                    var movesCol = MakeHudStat(out var movesLbl, "0", "MOVES");
                    movesLbl.name = "MovesLabel";
                    hud.Add(movesCol);

                    // Insert before board content (index 0 in panel)
                    Panel.Insert(0, hud);
                }
            }

            _boardRenderer.Refresh();
            _boardRenderer.StartTimer();
        }

        private static VisualElement MakeHudStat(out Label valLbl, string defaultVal, string caption)
        {
            var col = new VisualElement();
            col.style.flexDirection = FlexDirection.Column;
            col.style.alignItems = Align.Center;

            valLbl = new Label(defaultVal);
            valLbl.style.fontSize = 20;
            valLbl.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            valLbl.style.color = new StyleColor(new Color(0.95f, 0.95f, 0.95f));
            valLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            col.Add(valLbl);

            var cap = new Label(caption);
            cap.style.fontSize = 9;
            cap.style.color = new StyleColor(new Color(1, 1, 1, 0.3f));
            cap.style.unityTextAlign = TextAnchor.MiddleCenter;
            cap.style.letterSpacing = 1.5f;
            col.Add(cap);

            return col;
        }
    }

    // ── TutorialSuccessScreen ─────────────────────────────────────────────────
    public class TutorialSuccessScreen : BaseScreenController
    {
        public TutorialSuccessScreen(VisualTreeAsset uxml) : base(uxml) { }

        private VisualElement _mnemonicOverlay;
        private TextField _mnemonicField;
        private Label _mnemonicError;

        protected override void OnBuild()
        {
            // ── Build mnemonic input overlay (shown when Connect is tapped) ──
            _mnemonicOverlay = new VisualElement();
            _mnemonicOverlay.style.position = Position.Absolute;
            _mnemonicOverlay.style.left = 0;
            _mnemonicOverlay.style.top = 0;
            _mnemonicOverlay.style.right = 0;
            _mnemonicOverlay.style.bottom = 0;
            _mnemonicOverlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.85f));
            _mnemonicOverlay.style.alignItems = Align.Center;
            _mnemonicOverlay.style.justifyContent = Justify.Center;
            _mnemonicOverlay.style.display = DisplayStyle.None;
            _mnemonicOverlay.style.paddingLeft = 20;
            _mnemonicOverlay.style.paddingRight = 20;

            var card = new VisualElement();
            card.style.backgroundColor = new StyleColor(new Color(0.07f, 0.07f, 0.10f, 1f));
            card.style.borderTopLeftRadius =
            card.style.borderTopRightRadius =
            card.style.borderBottomLeftRadius =
            card.style.borderBottomRightRadius = 16;
            card.style.borderTopWidth =
            card.style.borderBottomWidth =
            card.style.borderLeftWidth =
            card.style.borderRightWidth = 1;
            card.style.borderTopColor =
            card.style.borderBottomColor =
            card.style.borderLeftColor =
            card.style.borderRightColor = new StyleColor(new Color(1f, 0.18f, 0.47f, 0.3f));
            card.style.paddingTop =
            card.style.paddingBottom =
            card.style.paddingLeft =
            card.style.paddingRight = 24;
            card.style.width = 340;

            var title = new Label("Enter Wallet Mnemonic");
            title.style.fontSize = 17;
            title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            title.style.color = new StyleColor(new Color(0.95f, 0.95f, 0.95f));
            title.style.marginBottom = 6;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;

            var subtitle = new Label("12 or 24 word seed phrase — devnet only");
            subtitle.style.fontSize = 11;
            subtitle.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f, 0.7f));
            subtitle.style.marginBottom = 16;
            subtitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            subtitle.style.whiteSpace = WhiteSpace.Normal;

            _mnemonicField = new TextField();
            _mnemonicField.multiline = true;
            _mnemonicField.style.whiteSpace = WhiteSpace.Normal;
            _mnemonicField.style.height = 80;
            _mnemonicField.style.marginBottom = 4;
            _mnemonicField.style.fontSize = 13;

            // Word count indicator — shows live count so user knows when ready
            var wordCountLabel = new Label("0 words");
            wordCountLabel.style.fontSize = 11;
            wordCountLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            wordCountLabel.style.marginBottom = 8;
            wordCountLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            _mnemonicField.RegisterValueChangedCallback(evt =>
            {
                int count = string.IsNullOrWhiteSpace(evt.newValue)
                    ? 0
                    : evt.newValue.Trim().Split(new[] { ' ', ' ', '	' },
                        System.StringSplitOptions.RemoveEmptyEntries).Length;
                bool valid = count == 12 || count == 24;
                wordCountLabel.text = $"{count} words {(valid ? "✓" : "(need 12 or 24)")}";
                wordCountLabel.style.color = new StyleColor(valid
                    ? new Color(0.2f, 0.85f, 0.4f)
                    : new Color(0.6f, 0.6f, 0.6f));
            });

            _mnemonicError = new Label("");
            _mnemonicError.style.color = new StyleColor(new Color(1f, 0.35f, 0.35f));
            _mnemonicError.style.fontSize = 12;
            _mnemonicError.style.marginBottom = 12;
            _mnemonicError.style.whiteSpace = WhiteSpace.Normal;
            _mnemonicError.style.display = DisplayStyle.None;

            var confirmBtn = new Button();
            confirmBtn.text = "Connect & Continue";
            confirmBtn.style.backgroundColor = new StyleColor(new Color(1f, 0.18f, 0.47f));
            confirmBtn.style.color = new StyleColor(Color.white);
            confirmBtn.style.fontSize = 15;
            confirmBtn.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            confirmBtn.style.height = 44;
            confirmBtn.style.borderTopLeftRadius =
            confirmBtn.style.borderTopRightRadius =
            confirmBtn.style.borderBottomLeftRadius =
            confirmBtn.style.borderBottomRightRadius = 10;
            confirmBtn.style.borderTopWidth =
            confirmBtn.style.borderBottomWidth =
            confirmBtn.style.borderLeftWidth =
            confirmBtn.style.borderRightWidth = 0;
            confirmBtn.style.marginBottom = 10;

            var cancelBtn = new Button();
            cancelBtn.text = "Cancel";
            cancelBtn.style.backgroundColor = new StyleColor(Color.clear);
            cancelBtn.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            cancelBtn.style.fontSize = 13;
            cancelBtn.style.height = 36;
            cancelBtn.style.borderTopWidth =
            cancelBtn.style.borderBottomWidth =
            cancelBtn.style.borderLeftWidth =
            cancelBtn.style.borderRightWidth = 0;

            card.Add(title);
            card.Add(subtitle);
            card.Add(_mnemonicField);
            card.Add(wordCountLabel);
            card.Add(_mnemonicError);
            card.Add(confirmBtn);
            card.Add(cancelBtn);
            _mnemonicOverlay.Add(card);
            Panel.Add(_mnemonicOverlay);

            // ── Connect button: show mnemonic overlay ────────────────────────
            Panel.Q<Button>("ConnectBtn")?.RegisterCallback<ClickEvent>(_ =>
            {
                _mnemonicField.value = "";
                _mnemonicError.style.display = DisplayStyle.None;
                _mnemonicOverlay.style.display = DisplayStyle.Flex;
            });

            // ── Confirm: connect with mnemonic then go to dashboard ──────────
            confirmBtn.RegisterCallback<ClickEvent>(async _ =>
            {
                var mnemonic = _mnemonicField.value.Trim();
                if (string.IsNullOrEmpty(mnemonic))
                {
                    ShowMnemonicError("Please enter your seed phrase.");
                    return;
                }
                try
                {
                    _mnemonicError.style.display = DisplayStyle.None;
                    confirmBtn.SetEnabled(false);
                    confirmBtn.text = "Connecting...";

                    await WalletManager.Instance.ConnectAsync(mnemonic);

                    // PuzzleOrchestrator is a MonoBehaviour — must be on the
                    // Bootstrap GameObject in the scene. Guard against null so
                    // the UI doesn't crash if it's missing from the scene.
                    if (PuzzleOrchestrator.Instance != null)
                        await PuzzleOrchestrator.Instance.EnsurePlayerAuthAsync();
                    else
                        Debug.LogWarning("[Connect] PuzzleOrchestrator not in scene — add it to Bootstrap.");

                    _mnemonicOverlay.style.display = DisplayStyle.None;
                    GameStateManager.Instance.GoToDashboard();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (msg.Contains("not in the wordlist"))
                    {
                        var parts = msg.Split(' ');
                        string badWord = parts.Length > 1 ? $"\"{parts[1]}\"" : "a word";
                        msg = badWord + " is not a valid BIP39 word. Check for typos or autocorrect changes.";
                    }
                    else if (msg.Contains("entropy"))
                        msg = "Invalid seed phrase. Check word count (need exactly 12 or 24) and spelling.";
                    else if (e is NullReferenceException || msg.Contains("Object reference"))
                        msg = "Scene setup error: add PuzzleOrchestrator to the Bootstrap GameObject.";
                    ShowMnemonicError(msg);
                    confirmBtn.SetEnabled(true);
                    confirmBtn.text = "Connect & Continue";
                }
            });

            cancelBtn.RegisterCallback<ClickEvent>(_ =>
                _mnemonicOverlay.style.display = DisplayStyle.None);

            Panel.Q<Button>("TryAgainBtn")?.RegisterCallback<ClickEvent>(_ =>
                GameStateManager.Instance.NavigateTo(GameScreen.Tutorial));
        }

        private void ShowMnemonicError(string msg)
        {
            _mnemonicError.text = msg;
            _mnemonicError.style.display = DisplayStyle.Flex;
        }

        protected override void OnShow()
        {
            // If wallet already connected from a previous session, skip overlay
            if (WalletManager.Instance.IsConnected)
            {
                Panel.schedule.Execute(async () =>
                {
                    try
                    {
                        await PuzzleOrchestrator.Instance.EnsurePlayerAuthAsync();
                        GameStateManager.Instance.GoToDashboard();
                    }
                    catch (Exception e)
                    {
                        GameStateManager.Instance.RaiseError(e.Message);
                    }
                }).ExecuteLater(300);
            }
        }
    }

    // ── DashboardScreen — 100% C#, fixed layout, no scroll ──────────────────
    public class DashboardScreen : BaseScreenController
    {
        public DashboardScreen(VisualTreeAsset uxml) : base(uxml) { }

        // Updated in OnShow — refs survive Panel.Clear() in OnBuild
        private Label _walletChipLbl, _walletAddrLbl, _solvedLbl, _bestLbl;

        // Compact palette
        private static readonly Color Pink = new Color(1.00f, 0.18f, 0.47f);
        private static readonly Color Card = new Color(1f, 1f, 1f, 0.045f);
        private static readonly Color Border = new Color(1f, 1f, 1f, 0.08f);
        private static readonly Color Bg = new Color(0.020f, 0.020f, 0.031f);
        private static readonly Color Dim = new Color(1f, 1f, 1f, 0.30f);

        protected override void OnBuild()
        {
            Panel.Clear();

            var root = new VisualElement();
            root.style.position = Position.Absolute;
            root.style.left = root.style.top = root.style.right = root.style.bottom = 0;
            root.style.flexDirection = FlexDirection.Column;
            root.style.backgroundColor = new StyleColor(Bg);
            Panel.Add(root);

            // ── TOP BAR ──────────────────────────────────────────────────
            var bar = Row(Justify.SpaceBetween, Align.Center);
            bar.style.paddingTop = bar.style.paddingBottom = 10;
            bar.style.paddingLeft = bar.style.paddingRight = 14;
            bar.style.borderBottomWidth = 1;
            bar.style.borderBottomColor = new StyleColor(Border);
            root.Add(bar);

            // Logo
            var logo = Row(Justify.FlexStart, Align.Center);
            var dot = new VisualElement();
            dot.style.width = dot.style.height = 18;
            dot.style.borderTopLeftRadius = dot.style.borderTopRightRadius =
            dot.style.borderBottomLeftRadius = dot.style.borderBottomRightRadius = 9;
            dot.style.backgroundColor = new StyleColor(Pink);
            dot.style.marginRight = 7;
            logo.Add(dot);
            var logotxt = Lbl("BALLSORT", 12, new Color(0.95f, 0.95f, 0.95f), bold: true);
            logo.Add(logotxt);
            bar.Add(logo);

            // Wallet chip
            var chip = new Button();
            chip.style.flexDirection = FlexDirection.Row;
            chip.style.alignItems = Align.Center;
            chip.style.paddingTop = chip.style.paddingBottom = 5;
            chip.style.paddingLeft = chip.style.paddingRight = 10;
            chip.style.backgroundColor = new StyleColor(Card);
            chip.style.borderTopLeftRadius = chip.style.borderTopRightRadius =
            chip.style.borderBottomLeftRadius = chip.style.borderBottomRightRadius = 999;
            chip.style.borderTopWidth = chip.style.borderBottomWidth =
            chip.style.borderLeftWidth = chip.style.borderRightWidth = 1;
            chip.style.borderTopColor = chip.style.borderBottomColor =
            chip.style.borderLeftColor = chip.style.borderRightColor = new StyleColor(Border);
            var gd = new VisualElement();
            gd.style.width = gd.style.height = 5;
            gd.style.borderTopLeftRadius = gd.style.borderTopRightRadius =
            gd.style.borderBottomLeftRadius = gd.style.borderBottomRightRadius = 3;
            gd.style.backgroundColor = new StyleColor(new Color(0.13f, 0.77f, 0.37f));
            gd.style.marginRight = 5;
            chip.Add(gd);
            _walletChipLbl = Lbl("—", 10, new Color(0.92f, 0.92f, 0.92f), bold: true);
            chip.Add(_walletChipLbl);
            bar.Add(chip);

            // ── STATS ROW ─────────────────────────────────────────────────
            var stats = Row(Justify.SpaceBetween, Align.Stretch);
            stats.style.marginTop = stats.style.marginBottom = 8;
            stats.style.marginLeft = stats.style.marginRight = 12;
            root.Add(stats);

            _solvedLbl = AddStatCard(stats, "0", "SOLVED", Pink, marginR: 4);
            _bestLbl = AddStatCard(stats, "—", "BEST", new Color(1f, 0.43f, 0.71f), marginL: 4, marginR: 4);
            _walletAddrLbl = AddStatCard(stats, "—", "WALLET", Dim, marginL: 4);

            // ── PLAY SECTION ──────────────────────────────────────────────
            var menu = new VisualElement();
            menu.style.flexGrow = 1;
            menu.style.flexDirection = FlexDirection.Column;
            menu.style.paddingLeft = menu.style.paddingRight = 12;
            root.Add(menu);

            menu.Add(SectionLbl("PLAY"));

            // Primary CTA
            var startBtn = new Button();
            startBtn.style.flexDirection = FlexDirection.Row;
            startBtn.style.alignItems = Align.Center;
            startBtn.style.paddingTop = startBtn.style.paddingBottom = 14;
            startBtn.style.paddingLeft = startBtn.style.paddingRight = 14;
            startBtn.style.backgroundColor = new StyleColor(Pink);
            startBtn.style.borderTopLeftRadius = startBtn.style.borderTopRightRadius =
            startBtn.style.borderBottomLeftRadius = startBtn.style.borderBottomRightRadius = 14;
            startBtn.style.borderTopWidth = startBtn.style.borderBottomWidth =
            startBtn.style.borderLeftWidth = startBtn.style.borderRightWidth = 0;
            startBtn.style.marginBottom = 8;
            startBtn.RegisterCallback<ClickEvent>(_ => GameStateManager.Instance.GoToGameSetup());

            var startIcon = new VisualElement();
            startIcon.style.width = startIcon.style.height = 38;
            startIcon.style.borderTopLeftRadius = startIcon.style.borderTopRightRadius =
            startIcon.style.borderBottomLeftRadius = startIcon.style.borderBottomRightRadius = 10;
            startIcon.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 0.18f));
            startIcon.style.alignItems = Align.Center; startIcon.style.justifyContent = Justify.Center;
            startIcon.style.marginRight = 12;
            startIcon.Add(Lbl("▶", 16, Color.white));
            startBtn.Add(startIcon);

            var startText = new VisualElement();
            startText.style.flexGrow = 1;
            startText.Add(Lbl("Start Game", 15, Color.white, bold: true));
            startText.Add(Lbl("New on-chain puzzle", 11, new Color(1f, 1f, 1f, 0.65f)));
            startBtn.Add(startText);
            startBtn.Add(Lbl("→", 16, new Color(1f, 1f, 1f, 0.5f)));
            menu.Add(startBtn);

            menu.Add(SectionLbl("COMPETE"));

            AddMenuRow(menu, "🏆", "Tournament", "Win prizes on-chain",
                new Color(0.92f, 0.70f, 0.03f), _ => GameStateManager.Instance.GoToTournament());
            AddMenuRow(menu, "📊", "Leaderboard", "Top solvers globally",
                new Color(0.23f, 0.51f, 0.96f), _ => GameStateManager.Instance.GoToLeaderboard());
            AddMenuRow(menu, "👤", "Profile", "Stats & achievements",
                new Color(0.06f, 0.73f, 0.51f), _ => GameStateManager.Instance.GoToProfile());

            // Footer
            var foot = Row(Justify.FlexStart, Align.Center);
            foot.style.paddingTop = foot.style.paddingBottom = 6;
            foot.style.paddingLeft = foot.style.paddingRight = 10;
            foot.style.marginTop = 4; foot.style.marginBottom = 8;
            foot.style.backgroundColor = new StyleColor(new Color(1f, 0.18f, 0.47f, 0.04f));
            foot.style.borderTopLeftRadius = foot.style.borderTopRightRadius =
            foot.style.borderBottomLeftRadius = foot.style.borderBottomRightRadius = 8;
            foot.style.borderTopWidth = foot.style.borderBottomWidth =
            foot.style.borderLeftWidth = foot.style.borderRightWidth = 1;
            foot.style.borderTopColor = foot.style.borderBottomColor =
            foot.style.borderLeftColor = foot.style.borderRightColor =
                new StyleColor(new Color(1f, 0.18f, 0.47f, 0.12f));
            foot.Add(Lbl("⚡", 10, new Color(1f, 1f, 1f, 0.5f)));
            var fl = Lbl("Session key signs moves — no wallet pop-ups during play", 10, Dim);
            fl.style.flexGrow = 1; fl.style.whiteSpace = WhiteSpace.Normal;
            fl.style.marginLeft = 5;
            foot.Add(fl);
            menu.Add(foot);
        }

        protected override void OnShow()
        {
            // Fully defensive — null-check every label before touching it
            try
            {
                var addr = WalletManager.Instance?.ShortAddress ?? "—";
                if (_walletChipLbl != null) _walletChipLbl.text = addr;
                if (_walletAddrLbl != null) _walletAddrLbl.text = addr;
            }
            catch { /* wallet not ready */ }

            try
            {
                ulong solved = PuzzleOrchestrator.Instance?.CurrentPlayerAuth?.TotalPuzzlesSolved ?? 0;
                if (_solvedLbl != null) _solvedLbl.text = solved.ToString();
            }
            catch { /* orchestrator not ready */ }

            if (_bestLbl != null) _bestLbl.text = "—";
        }

        // ── Compact helpers ───────────────────────────────────────────────
        private static VisualElement Row(Justify justify, Align align)
        {
            var v = new VisualElement();
            v.style.flexDirection = FlexDirection.Row;
            v.style.justifyContent = justify;
            v.style.alignItems = align;
            return v;
        }

        private static Label Lbl(string text, float size, Color color, bool bold = false)
        {
            var l = new Label(text);
            l.style.fontSize = size;
            l.style.color = new StyleColor(color);
            if (bold) l.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            return l;
        }

        private Label AddStatCard(VisualElement parent, string val, string cap,
            Color valColor, float marginL = 0, float marginR = 0)
        {
            var card = new VisualElement();
            card.style.flexGrow = 1;
            card.style.backgroundColor = new StyleColor(Card);
            card.style.borderTopLeftRadius = card.style.borderTopRightRadius =
            card.style.borderBottomLeftRadius = card.style.borderBottomRightRadius = 10;
            card.style.borderTopWidth = card.style.borderBottomWidth =
            card.style.borderLeftWidth = card.style.borderRightWidth = 1;
            card.style.borderTopColor = card.style.borderBottomColor =
            card.style.borderLeftColor = card.style.borderRightColor = new StyleColor(Border);
            card.style.paddingTop = card.style.paddingBottom = 10;
            card.style.paddingLeft = card.style.paddingRight = 6;
            card.style.alignItems = Align.Center;
            card.style.marginLeft = marginL; card.style.marginRight = marginR;
            parent.Add(card);

            var vl = Lbl(val, 22, valColor, bold: true);
            vl.style.unityTextAlign = TextAnchor.MiddleCenter;
            card.Add(vl);

            var cl = Lbl(cap, 9, new Color(1f, 1f, 1f, 0.3f));
            cl.style.unityTextAlign = TextAnchor.MiddleCenter;
            cl.style.marginTop = 2;
            card.Add(cl);

            return vl; // return value label so caller can update it
        }

        private static VisualElement SectionLbl(string text)
        {
            var l = Lbl(text, 9, new Color(1f, 1f, 1f, 0.22f));
            l.style.letterSpacing = 2;
            l.style.marginBottom = 5;
            l.style.marginTop = 6;
            return l;
        }

        private static void AddMenuRow(VisualElement parent, string emoji,
            string title, string sub, Color iconColor,
            EventCallback<ClickEvent> onClick)
        {
            var btn = new Button();
            btn.style.flexDirection = FlexDirection.Row;
            btn.style.alignItems = Align.Center;
            btn.style.paddingTop = btn.style.paddingBottom = 10;
            btn.style.paddingLeft = btn.style.paddingRight = 12;
            btn.style.backgroundColor = new StyleColor(Card);
            btn.style.borderTopLeftRadius = btn.style.borderTopRightRadius =
            btn.style.borderBottomLeftRadius = btn.style.borderBottomRightRadius = 12;
            btn.style.borderTopWidth = btn.style.borderBottomWidth =
            btn.style.borderLeftWidth = btn.style.borderRightWidth = 1;
            btn.style.borderTopColor = btn.style.borderBottomColor =
            btn.style.borderLeftColor = btn.style.borderRightColor = new StyleColor(Border);
            btn.style.marginBottom = 6;
            btn.RegisterCallback<ClickEvent>(onClick);

            var ico = new VisualElement();
            ico.style.width = ico.style.height = 34;
            ico.style.borderTopLeftRadius = ico.style.borderTopRightRadius =
            ico.style.borderBottomLeftRadius = ico.style.borderBottomRightRadius = 9;
            ico.style.backgroundColor = new StyleColor(new Color(iconColor.r, iconColor.g, iconColor.b, 0.12f));
            ico.style.borderTopWidth = ico.style.borderBottomWidth =
            ico.style.borderLeftWidth = ico.style.borderRightWidth = 1;
            ico.style.borderTopColor = ico.style.borderBottomColor =
            ico.style.borderLeftColor = ico.style.borderRightColor =
                new StyleColor(new Color(iconColor.r, iconColor.g, iconColor.b, 0.25f));
            ico.style.alignItems = Align.Center; ico.style.justifyContent = Justify.Center;
            ico.style.marginRight = 10;
            ico.Add(Lbl(emoji, 16, Color.white));
            btn.Add(ico);

            var txt = new VisualElement();
            txt.style.flexGrow = 1;
            txt.Add(Lbl(title, 14, new Color(0.95f, 0.95f, 0.95f), bold: true));
            txt.Add(Lbl(sub, 11, new Color(1f, 1f, 1f, 0.35f)));
            btn.Add(txt);
            btn.Add(Lbl("›", 18, new Color(1f, 1f, 1f, 0.18f)));
            parent.Add(btn);
        }
    }


    // ── GameSetupScreen — step-by-step wizard ──────────────────────────────
    public class GameSetupScreen : BaseScreenController
    {
        public GameSetupScreen(VisualTreeAsset uxml) : base(uxml) { }

        private int  _tubes = 6, _balls = 4;
        private byte _difficulty = 1; // 1=Easy, 2=Medium, 3=Hard
        private const int MIN = 4, MAX = 10;
        private int _step = 0; // 0=tubes, 1=balls, 2=difficulty, 3=preview

        // Step panels
        private VisualElement _stepTubes, _stepBalls, _stepDifficulty, _stepPreview;

        // Labels updated per step
        private Label _tubesValueLbl, _tubesSubLbl;
        private Label _ballsValueLbl, _ballsSubLbl;
        private Label _previewTubesLbl, _previewBallsLbl, _previewDiffLbl;
        private VisualElement _boardPreviewEl;
        private VisualElement[] _diffBtns; // [3] Easy/Medium/Hard

        protected override void OnBuild()
        {
            Panel.Clear();

            // ── Root layout fills screen ──────────────────────────────────
            var root = new VisualElement();
            root.style.position = Position.Absolute;
            root.style.left = root.style.top = root.style.right = root.style.bottom = 0;
            root.style.flexDirection = FlexDirection.Column;
            root.style.backgroundColor = new StyleColor(new Color(0.020f, 0.020f, 0.031f));
            Panel.Add(root);

            // ── Header ────────────────────────────────────────────────────
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.paddingTop = 18; header.style.paddingBottom = 14;
            header.style.paddingLeft = 20; header.style.paddingRight = 20;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new StyleColor(new Color(1, 1, 1, 0.07f));
            root.Add(header);

            var backBtn = new Button(() => GameStateManager.Instance.GoToDashboard());
            backBtn.text = "← Back";
            backBtn.style.backgroundColor = StyleKeyword.None;
            backBtn.style.color = new StyleColor(new Color(1, 1, 1, 0.35f));
            backBtn.style.fontSize = 13;
            backBtn.style.borderTopWidth = backBtn.style.borderBottomWidth =
            backBtn.style.borderLeftWidth = backBtn.style.borderRightWidth = 0;
            backBtn.style.paddingLeft = backBtn.style.paddingRight = 0;
            header.Add(backBtn);

            var titleLbl = new Label("New Puzzle");
            titleLbl.style.flexGrow = 1;
            titleLbl.style.fontSize = 16;
            titleLbl.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            titleLbl.style.color = new StyleColor(new Color(0.95f, 0.95f, 0.95f));
            titleLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.Add(titleLbl);

            // Invisible spacer to balance back button
            var spacer = new VisualElement();
            spacer.style.width = 60;
            header.Add(spacer);

            // ── Step indicator dots ────────────────────────────────────────
            var dots = new VisualElement();
            dots.style.flexDirection = FlexDirection.Row;
            dots.style.justifyContent = Justify.Center;
            dots.style.paddingTop = dots.style.paddingBottom = 14;
            root.Add(dots);

            var dotEls = new VisualElement[4];
            string[] dotLabels = { "Tubes", "Balls", "Difficulty", "Start" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var col = new VisualElement();
                col.style.flexDirection = FlexDirection.Column;
                col.style.alignItems = Align.Center;
                col.style.marginLeft = col.style.marginRight = 16;

                var dot = new VisualElement();
                dot.style.width = dot.style.height = 10;
                dot.style.borderTopLeftRadius = dot.style.borderTopRightRadius =
                dot.style.borderBottomLeftRadius = dot.style.borderBottomRightRadius = 5;
                dot.style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.15f));
                col.Add(dot);
                dotEls[i] = dot;

                var lbl = new Label(dotLabels[i]);
                lbl.style.fontSize = 10;
                lbl.style.color = new StyleColor(new Color(1, 1, 1, 0.25f));
                lbl.style.marginTop = 4;
                col.Add(lbl);
                dots.Add(col);
            }

            // ── Step content area ─────────────────────────────────────────
            var content = new VisualElement();
            content.style.flexGrow = 1;
            content.style.flexDirection = FlexDirection.Column;
            content.style.paddingLeft = content.style.paddingRight = 24;
            content.style.paddingTop = 8;
            root.Add(content);

            // ── STEP 0: Tubes ─────────────────────────────────────────────
            _stepTubes = BuildStepPanel(content,
                "How many tubes?",
                "More tubes = more working space",
                out _tubesValueLbl, out _tubesSubLbl);

            // ── STEP 1: Balls ─────────────────────────────────────────────
            _stepBalls = BuildStepPanel(content,
                "Balls per tube?",
                "More balls = harder puzzle",
                out _ballsValueLbl, out _ballsSubLbl);
            _stepBalls.style.display = DisplayStyle.None;

            // ── STEP 2: Difficulty ────────────────────────────────────────
            _stepDifficulty = new VisualElement();
            _stepDifficulty.style.flexGrow = 1;
            _stepDifficulty.style.flexDirection = FlexDirection.Column;
            _stepDifficulty.style.alignItems = Align.Center;
            _stepDifficulty.style.justifyContent = Justify.Center;
            _stepDifficulty.style.display = DisplayStyle.None;
            content.Add(_stepDifficulty);

            var diffTitle = new Label("Difficulty");
            diffTitle.style.fontSize = 26;
            diffTitle.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            diffTitle.style.color = new StyleColor(new Color(0.95f, 0.95f, 0.95f));
            diffTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            diffTitle.style.marginBottom = 6;
            _stepDifficulty.Add(diffTitle);

            var diffSub = new Label("How hard should the shuffle be?");
            diffSub.style.fontSize = 13;
            diffSub.style.color = new StyleColor(new Color(1, 1, 1, 0.35f));
            diffSub.style.unityTextAlign = TextAnchor.MiddleCenter;
            diffSub.style.marginBottom = 36;
            _stepDifficulty.Add(diffSub);

            var diffOptions = new (string emoji, string label, string sub, byte val)[]
            {
                ("🟢", "Easy",   "Light shuffle — solvable quickly", 1),
                ("🟡", "Medium", "Good mix — takes some thought",     2),
                ("🔴", "Hard",   "Deep shuffle — serious challenge",  3),
            };

            _diffBtns = new VisualElement[3];
            for (int di = 0; di < 3; di++)
            {
                int capturedDi = di;
                byte capturedVal = diffOptions[di].val;

                var diffBtn = new Button();
                diffBtn.style.flexDirection = FlexDirection.Row;
                diffBtn.style.alignItems = Align.Center;
                diffBtn.style.width = 300;
                diffBtn.style.paddingTop = diffBtn.style.paddingBottom = 14;
                diffBtn.style.paddingLeft = diffBtn.style.paddingRight = 18;
                diffBtn.style.marginBottom = 10;
                diffBtn.style.borderTopLeftRadius = diffBtn.style.borderTopRightRadius =
                diffBtn.style.borderBottomLeftRadius = diffBtn.style.borderBottomRightRadius = 14;
                diffBtn.style.borderTopWidth = diffBtn.style.borderBottomWidth =
                diffBtn.style.borderLeftWidth = diffBtn.style.borderRightWidth = 1.5f;
                diffBtn.style.borderTopColor = diffBtn.style.borderBottomColor =
                diffBtn.style.borderLeftColor = diffBtn.style.borderRightColor =
                    new StyleColor(new Color(1, 1, 1, 0.08f));
                diffBtn.style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.03f));
                _stepDifficulty.Add(diffBtn);
                _diffBtns[di] = diffBtn;

                var emLbl = new Label(diffOptions[di].emoji);
                emLbl.style.fontSize = 22;
                emLbl.style.marginRight = 14;
                emLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
                diffBtn.Add(emLbl);

                var txtCol = new VisualElement();
                txtCol.style.flexGrow = 1;
                txtCol.style.flexDirection = FlexDirection.Column;
                var nameLbl = new Label(diffOptions[di].label);
                nameLbl.style.fontSize = 15;
                nameLbl.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                nameLbl.style.color = new StyleColor(new Color(0.95f, 0.95f, 0.95f));
                nameLbl.style.marginBottom = 2;
                txtCol.Add(nameLbl);
                var subLbl2 = new Label(diffOptions[di].sub);
                subLbl2.style.fontSize = 11;
                subLbl2.style.color = new StyleColor(new Color(1, 1, 1, 0.35f));
                txtCol.Add(subLbl2);
                diffBtn.Add(txtCol);

                var checkLbl = new Label("✓");
                checkLbl.name = $"DiffCheck_{di}";
                checkLbl.style.fontSize = 18;
                checkLbl.style.color = new StyleColor(new Color(1f, 0.18f, 0.47f));
                checkLbl.style.display = DisplayStyle.None;
                diffBtn.Add(checkLbl);

                diffBtn.RegisterCallback<ClickEvent>(_ =>
                {
                    _difficulty = capturedVal;
                    RefreshDifficultyStep();
                });
            }

            RefreshDifficultyStep();

            // ── STEP 3: Preview + Start ───────────────────────────────────
            _stepPreview = new VisualElement();
            _stepPreview.style.flexGrow = 1;
            _stepPreview.style.flexDirection = FlexDirection.Column;
            _stepPreview.style.display = DisplayStyle.None;
            content.Add(_stepPreview);

            var prevTitle = new Label("Ready to play?");
            prevTitle.style.fontSize = 26;
            prevTitle.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            prevTitle.style.color = new StyleColor(new Color(0.95f, 0.95f, 0.95f));
            prevTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            prevTitle.style.marginBottom = 6;
            _stepPreview.Add(prevTitle);

            _previewTubesLbl = new Label();
            _previewTubesLbl.style.fontSize = 14;
            _previewTubesLbl.style.color = new StyleColor(new Color(1, 1, 1, 0.5f));
            _previewTubesLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            _previewTubesLbl.style.marginBottom = 2;
            _stepPreview.Add(_previewTubesLbl);

            _previewBallsLbl = new Label();
            _previewBallsLbl.style.fontSize = 14;
            _previewBallsLbl.style.color = new StyleColor(new Color(1, 1, 1, 0.5f));
            _previewBallsLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            _previewBallsLbl.style.marginBottom = 4;
            _stepPreview.Add(_previewBallsLbl);

            _previewDiffLbl = new Label();
            _previewDiffLbl.style.fontSize = 14;
            _previewDiffLbl.style.color = new StyleColor(new Color(1f, 0.18f, 0.47f));
            _previewDiffLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            _previewDiffLbl.style.marginBottom = 20;
            _stepPreview.Add(_previewDiffLbl);

            // Mini board preview
            _boardPreviewEl = new VisualElement();
            _boardPreviewEl.style.flexDirection = FlexDirection.Row;
            _boardPreviewEl.style.flexWrap = Wrap.Wrap;
            _boardPreviewEl.style.justifyContent = Justify.Center;
            _boardPreviewEl.style.alignItems = Align.FlexEnd;
            _boardPreviewEl.style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.02f));
            _boardPreviewEl.style.borderTopLeftRadius = _boardPreviewEl.style.borderTopRightRadius =
            _boardPreviewEl.style.borderBottomLeftRadius = _boardPreviewEl.style.borderBottomRightRadius = 16;
            _boardPreviewEl.style.borderTopWidth = _boardPreviewEl.style.borderBottomWidth =
            _boardPreviewEl.style.borderLeftWidth = _boardPreviewEl.style.borderRightWidth = 1;
            _boardPreviewEl.style.borderTopColor = _boardPreviewEl.style.borderBottomColor =
            _boardPreviewEl.style.borderLeftColor = _boardPreviewEl.style.borderRightColor =
                new StyleColor(new Color(1, 1, 1, 0.07f));
            _boardPreviewEl.style.paddingTop = _boardPreviewEl.style.paddingBottom =
            _boardPreviewEl.style.paddingLeft = _boardPreviewEl.style.paddingRight = 16;
            _boardPreviewEl.style.minHeight = 100;
            _boardPreviewEl.style.flexGrow = 1;
            _stepPreview.Add(_boardPreviewEl);

            // ── Bottom nav bar ─────────────────────────────────────────────
            var navBar = new VisualElement();
            navBar.style.flexDirection = FlexDirection.Row;
            navBar.style.paddingTop = 14;
            navBar.style.paddingBottom = 20;
            navBar.style.paddingLeft = navBar.style.paddingRight = 24;
            navBar.style.borderTopWidth = 1;
            navBar.style.borderTopColor = new StyleColor(new Color(1, 1, 1, 0.07f));
            root.Add(navBar);

            // Prev button (hidden on step 0)
            var prevBtn = new Button();
            prevBtn.text = "← Back";
            prevBtn.style.flexGrow = 1;
            prevBtn.style.height = 48;
            prevBtn.style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.05f));
            prevBtn.style.color = new StyleColor(new Color(1, 1, 1, 0.6f));
            prevBtn.style.fontSize = 15;
            prevBtn.style.borderTopLeftRadius = prevBtn.style.borderTopRightRadius =
            prevBtn.style.borderBottomLeftRadius = prevBtn.style.borderBottomRightRadius = 12;
            prevBtn.style.borderTopWidth = prevBtn.style.borderBottomWidth =
            prevBtn.style.borderLeftWidth = prevBtn.style.borderRightWidth = 0;
            prevBtn.style.marginRight = 10;
            prevBtn.style.display = DisplayStyle.None;
            navBar.Add(prevBtn);

            // Next/Start button
            var nextBtn = new Button();
            nextBtn.text = "Next →";
            nextBtn.style.flexGrow = 1;
            nextBtn.style.height = 48;
            nextBtn.style.backgroundColor = new StyleColor(new Color(1f, 0.18f, 0.47f));
            nextBtn.style.color = new StyleColor(Color.white);
            nextBtn.style.fontSize = 16;
            nextBtn.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            nextBtn.style.borderTopLeftRadius = nextBtn.style.borderTopRightRadius =
            nextBtn.style.borderBottomLeftRadius = nextBtn.style.borderBottomRightRadius = 12;
            nextBtn.style.borderTopWidth = nextBtn.style.borderBottomWidth =
            nextBtn.style.borderLeftWidth = nextBtn.style.borderRightWidth = 0;
            navBar.Add(nextBtn);

            // ── Step +/- controls (attached to step panels) ───────────────
            AttachStepper(_stepTubes, () => _tubes, v => { _tubes = v; RefreshTubesStep(); },
                MIN, MAX);
            AttachStepper(_stepBalls, () => _balls, v => { _balls = v; RefreshBallsStep(); },
                MIN, MAX);

            // ── Navigation logic ──────────────────────────────────────────
            void ShowStep(int s)
            {
                _step = s;
                _stepTubes.style.display       = s == 0 ? DisplayStyle.Flex : DisplayStyle.None;
                _stepBalls.style.display        = s == 1 ? DisplayStyle.Flex : DisplayStyle.None;
                _stepDifficulty.style.display   = s == 2 ? DisplayStyle.Flex : DisplayStyle.None;
                _stepPreview.style.display      = s == 3 ? DisplayStyle.Flex : DisplayStyle.None;
                prevBtn.style.display = s > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                nextBtn.text = s == 3 ? "🚀  Start Puzzle" : "Next →";

                for (int i2 = 0; i2 < dotEls.Length; i2++)
                    dotEls[i2].style.backgroundColor = new StyleColor(
                        i2 == s ? new Color(1f, 0.18f, 0.47f) :
                        i2 < s  ? new Color(1f, 0.18f, 0.47f, 0.4f) :
                                   new Color(1f, 1f, 1f, 0.15f));

                if (s == 3) RefreshPreview();
            }

            prevBtn.RegisterCallback<ClickEvent>(_ => { if (_step > 0) ShowStep(_step - 1); });

            nextBtn.RegisterCallback<ClickEvent>(async _ =>
            {
                if (_step < 3) { ShowStep(_step + 1); return; }

                // Step 3 = Start Puzzle
                GameStateManager.Instance.SelectedTubes       = _tubes;
                GameStateManager.Instance.SelectedBalls        = _balls;
                GameStateManager.Instance.SelectedDifficulty  = _difficulty;
                nextBtn.SetEnabled(false);
                nextBtn.text = "Opening session...";

                if (PuzzleOrchestrator.Instance == null)
                {
                    GameStateManager.Instance.RaiseError(
                        "PuzzleOrchestrator missing — add it to Bootstrap.");
                    nextBtn.SetEnabled(true);
                    nextBtn.text = "🚀  Start Puzzle";
                    return;
                }
                try
                {
                    await PuzzleOrchestrator.Instance.StartNewPuzzleAsync(
                        (byte)_tubes, (byte)_balls);
                }
                catch (Exception ex)
                {
                    GameStateManager.Instance.RaiseError($"Failed to start: {ex.Message}");
                    nextBtn.SetEnabled(true);
                    nextBtn.text = "🚀  Start Puzzle";
                }
            });

            RefreshTubesStep();
            RefreshBallsStep();
            ShowStep(0);
        }

        protected override void OnShow()
        {
            _step = 0;
            _tubes = 6; _balls = 4; _difficulty = 1;
            if (_stepTubes != null)
            {
                _stepTubes.style.display      = DisplayStyle.Flex;
                _stepBalls.style.display      = DisplayStyle.None;
                _stepDifficulty.style.display = DisplayStyle.None;
                _stepPreview.style.display    = DisplayStyle.None;
            }
            RefreshTubesStep();
        }

        // ── Builds a step panel with title/subtitle + big number display ──
        private VisualElement BuildStepPanel(VisualElement parent,
            string title, string subtitle,
            out Label valueLbl, out Label subLbl)
        {
            var panel = new VisualElement();
            panel.style.flexGrow = 1;
            panel.style.flexDirection = FlexDirection.Column;
            panel.style.alignItems = Align.Center;
            panel.style.justifyContent = Justify.Center;
            parent.Add(panel);

            var t = new Label(title);
            t.style.fontSize = 26;
            t.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            t.style.color = new StyleColor(new Color(0.95f, 0.95f, 0.95f));
            t.style.unityTextAlign = TextAnchor.MiddleCenter;
            t.style.marginBottom = 6;
            panel.Add(t);

            var s = new Label(subtitle);
            s.style.fontSize = 13;
            s.style.color = new StyleColor(new Color(1, 1, 1, 0.35f));
            s.style.unityTextAlign = TextAnchor.MiddleCenter;
            s.style.marginBottom = 40;
            panel.Add(s);

            // Big number
            valueLbl = new Label("6");
            valueLbl.style.fontSize = 72;
            valueLbl.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            valueLbl.style.color = new StyleColor(new Color(1f, 0.18f, 0.47f));
            valueLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueLbl.style.marginBottom = 8;
            panel.Add(valueLbl);

            subLbl = new Label();
            subLbl.style.fontSize = 14;
            subLbl.style.color = new StyleColor(new Color(1, 1, 1, 0.40f));
            subLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            subLbl.style.marginBottom = 32;
            panel.Add(subLbl);

            // Stepper row placeholder (filled by AttachStepper)
            return panel;
        }

        // ── Attaches − + stepper row to a step panel ─────────────────────
        private void AttachStepper(VisualElement panel,
            System.Func<int> get, System.Action<int> set, int min, int max)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.Center;
            panel.Add(row);

            var minus = MakeStepperBtn("−");
            minus.RegisterCallback<ClickEvent>(_ =>
            {
                int v = Mathf.Clamp(get() - 1, min, max);
                set(v);
            });
            row.Add(minus);

            // Range bar
            var track = new VisualElement();
            track.style.width = 120;
            track.style.height = 4;
            track.style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.08f));
            track.style.borderTopLeftRadius = track.style.borderTopRightRadius =
            track.style.borderBottomLeftRadius = track.style.borderBottomRightRadius = 2;
            track.style.marginLeft = track.style.marginRight = 20;
            track.style.overflow = Overflow.Hidden;
            row.Add(track);

            var fill = new VisualElement();
            fill.style.height = 4;
            fill.style.backgroundColor = new StyleColor(new Color(1f, 0.18f, 0.47f));
            fill.style.borderTopLeftRadius = fill.style.borderTopRightRadius =
            fill.style.borderBottomLeftRadius = fill.style.borderBottomRightRadius = 2;
            track.Add(fill);

            var plus = MakeStepperBtn("+");
            plus.RegisterCallback<ClickEvent>(_ =>
            {
                int v = Mathf.Clamp(get() + 1, min, max);
                set(v);
            });
            row.Add(plus);

            // Store fill ref for update
            panel.userData = fill;

            // Min/max labels
            var rangeRow = new VisualElement();
            rangeRow.style.flexDirection = FlexDirection.Row;
            rangeRow.style.justifyContent = Justify.Center;
            rangeRow.style.marginTop = 10;
            panel.Add(rangeRow);

            var minLbl = new Label($"min {min}");
            minLbl.style.fontSize = 11;
            minLbl.style.color = new StyleColor(new Color(1, 1, 1, 0.2f));
            minLbl.style.marginRight = 80;
            rangeRow.Add(minLbl);

            var maxLbl = new Label($"max {max}");
            maxLbl.style.fontSize = 11;
            maxLbl.style.color = new StyleColor(new Color(1, 1, 1, 0.2f));
            rangeRow.Add(maxLbl);
        }

        private Button MakeStepperBtn(string text)
        {
            var btn = new Button();
            btn.text = text;
            btn.style.width = btn.style.height = 52;
            btn.style.fontSize = 28;
            btn.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            btn.style.color = new StyleColor(Color.white);
            btn.style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.08f));
            btn.style.borderTopLeftRadius = btn.style.borderTopRightRadius =
            btn.style.borderBottomLeftRadius = btn.style.borderBottomRightRadius = 999;
            btn.style.borderTopWidth = btn.style.borderBottomWidth =
            btn.style.borderLeftWidth = btn.style.borderRightWidth = 0;
            btn.style.unityTextAlign = TextAnchor.MiddleCenter;
            return btn;
        }

        private void RefreshTubesStep()
        {
            if (_tubesValueLbl == null) return;
            _tubesValueLbl.text = _tubes.ToString();
            _tubesSubLbl.text = $"{_tubes - 1} filled + 1 empty";
            UpdateFill(_stepTubes, _tubes, MIN, MAX);
        }

        private void RefreshBallsStep()
        {
            if (_ballsValueLbl == null) return;
            _ballsValueLbl.text = _balls.ToString();
            _ballsSubLbl.text = $"{_balls} balls per tube";
            UpdateFill(_stepBalls, _balls, MIN, MAX);
        }

        private void RefreshDifficultyStep()
        {
            if (_diffBtns == null) return;
            Color pink    = new Color(1f, 0.18f, 0.47f);
            Color pinkDim = new Color(1f, 0.18f, 0.47f, 0.12f);
            for (int di = 0; di < 3; di++)
            {
                bool selected = (_difficulty == di + 1);
                _diffBtns[di].style.borderTopColor =
                _diffBtns[di].style.borderBottomColor =
                _diffBtns[di].style.borderLeftColor =
                _diffBtns[di].style.borderRightColor =
                    new StyleColor(selected ? pink : new Color(1f, 1f, 1f, 0.08f));
                _diffBtns[di].style.backgroundColor =
                    new StyleColor(selected ? pinkDim : new Color(1f, 1f, 1f, 0.03f));
                var check = _diffBtns[di].Q<Label>($"DiffCheck_{di}");
                if (check != null)
                    check.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdateFill(VisualElement panel, int val, int min, int max)
        {
            if (panel?.userData is VisualElement fill)
                fill.style.width = Length.Percent((val - min) * 100f / (max - min));
        }

        private void RefreshPreview()
        {
            if (_previewTubesLbl == null) return;
            _previewTubesLbl.text = $"{_tubes} tubes  ({_tubes - 1} filled + 1 empty)";
            _previewBallsLbl.text = $"{_balls} balls per tube";
            _previewDiffLbl.text  = _difficulty switch { 1 => "🟢 Easy", 2 => "🟡 Medium", _ => "🔴 Hard" };
            BuildBoardPreview();
        }

        private void BuildBoardPreview()
        {
            _boardPreviewEl.Clear();
            int ballPx = Mathf.Clamp(200 / _tubes, 14, 22);
            for (int t = 0; t < _tubes; t++)
            {
                var tube = new VisualElement();
                tube.style.flexDirection = FlexDirection.Column;
                tube.style.alignItems = Align.Center;
                tube.style.justifyContent = Justify.FlexEnd;
                tube.style.width = ballPx + 10;
                tube.style.height = _balls * (ballPx + 2) + 10;
                tube.style.borderBottomLeftRadius = tube.style.borderBottomRightRadius =
                    (ballPx + 10) / 2;
                tube.style.borderTopLeftRadius = tube.style.borderTopRightRadius = 3;
                tube.style.borderBottomWidth = tube.style.borderLeftWidth =
                    tube.style.borderRightWidth = 1.5f;
                tube.style.borderTopWidth = 0;
                tube.style.borderTopColor = tube.style.borderBottomColor =
                tube.style.borderLeftColor = tube.style.borderRightColor =
                    new StyleColor(new Color(1, 1, 1, 0.15f));
                tube.style.backgroundColor = new StyleColor(new Color(1, 1, 1, 0.02f));
                tube.style.paddingBottom = 3;
                tube.style.marginLeft = tube.style.marginRight = 3;
                tube.style.marginBottom = 4;

                if (t < _tubes - 1)
                {
                    for (int b = 0; b < _balls; b++)
                    {
                        var ball = new VisualElement();
                        ball.style.width = ball.style.height = ballPx;
                        ball.style.borderTopLeftRadius = ball.style.borderTopRightRadius =
                        ball.style.borderBottomLeftRadius = ball.style.borderBottomRightRadius = ballPx / 2;
                        ball.style.marginTop = ball.style.marginBottom = 1;
                        ball.style.backgroundColor = new StyleColor(GetColor((t % 10) + 1));
                        tube.Add(ball);
                    }
                }
                _boardPreviewEl.Add(tube);
            }
        }

        private static Color GetColor(int i) => i switch
        {
            1 => new Color(1.00f, 0.18f, 0.47f),
            2 => new Color(0.00f, 0.83f, 1.00f),
            3 => new Color(1.00f, 0.84f, 0.00f),
            4 => new Color(0.49f, 0.23f, 0.93f),
            5 => new Color(0.06f, 0.73f, 0.51f),
            6 => new Color(0.98f, 0.45f, 0.09f),
            7 => new Color(0.23f, 0.51f, 0.96f),
            8 => new Color(0.93f, 0.28f, 0.60f),
            9 => new Color(0.52f, 0.80f, 0.09f),
            10 => new Color(0.08f, 0.72f, 0.65f),
            _ => Color.gray
        };
    }


    // ── GameLoadingScreen ─────────────────────────────────────────────────────
    public class GameLoadingScreen : BaseScreenController
    {
        public GameLoadingScreen(VisualTreeAsset uxml) : base(uxml) { }

        private static readonly (string id, string key)[] STEPS =
        {
            ("Step_Init",       "initializing"),
            ("Step_VRF",        "waiting-vrf"),
            ("Step_Perms",      "creating-permissions"),
            ("Step_DelegPerms", "delegating-permissions"),
            ("Step_Deleg",      "delegating-puzzle"),
            ("Step_Start",      "starting"),
        };

        private readonly LoadingStep[] STEP_ENUM =
        {
            LoadingStep.InitializingPuzzle,
            LoadingStep.WaitingForVRF,
            LoadingStep.CreatingPermissions,
            LoadingStep.DelegatingPermissions,
            LoadingStep.DelegatingPuzzle,
            LoadingStep.StartingPuzzle,
        };

        private Label _subLabel;
        private VisualElement _overallFill;

        protected override void OnBuild()
        {
            _subLabel = Panel.Q<Label>("LoadingSubLabel");
            _overallFill = Panel.Q<VisualElement>("OverallFill");

            GameStateManager.Instance.OnLoadingStepChanged += OnStepChanged;
        }

        private void OnStepChanged(LoadingStep step, string message)
        {
            if (_subLabel != null) _subLabel.text = message;

            int currentIdx = Array.IndexOf(STEP_ENUM, step);

            for (int i = 0; i < STEPS.Length; i++)
            {
                var stepEl = Panel.Q<VisualElement>(STEPS[i].id);
                var iconEl = Panel.Q<VisualElement>($"StepIcon_{STEPS[i].id.Replace("Step_", "")}");
                var checkEl = Panel.Q<Label>($"StepCheck_{STEPS[i].id.Replace("Step_", "")}");

                if (stepEl == null) continue;

                bool done = i < currentIdx;
                bool active = i == currentIdx;
                bool pending = i > currentIdx;

                stepEl.style.opacity = pending ? 0.3f : 1f;

                if (iconEl != null)
                {
                    iconEl.RemoveFromClassList("step-icon--done");
                    iconEl.RemoveFromClassList("step-icon--active");
                    iconEl.RemoveFromClassList("step-icon--pending");

                    if (done) iconEl.AddToClassList("step-icon--done");
                    else if (active) iconEl.AddToClassList("step-icon--active");
                    else iconEl.AddToClassList("step-icon--pending");
                }

                if (checkEl != null)
                    checkEl.text = done ? "✓" : "";
            }

            float progress = currentIdx < 0 ? 5f :
                5f + (currentIdx / (float)(STEPS.Length - 1)) * 95f;
            if (_overallFill != null)
                _overallFill.style.width = Length.Percent(progress);
        }
    }

    // ── GameScreen_ (active game) ─────────────────────────────────────────────
    // Renamed to avoid conflict with Unity's GameScreen type
    public class GameScreen_ : BaseScreenController
    {
        public GameScreen_(VisualTreeAsset uxml) : base(uxml) { }

        private Label _timerLabel, _movesLabel, _undosLabel;
        private System.DateTime _startTime;
        private bool _timerRunning;

        protected override void OnBuild()
        {
            _timerLabel = Panel.Q<Label>("TimerLabel");
            _movesLabel = Panel.Q<Label>("MovesLabel");
            _undosLabel = Panel.Q<Label>("UndosLabel");

            Panel.Q<Button>("UndoBtn")?.RegisterCallback<ClickEvent>(async _ =>
            {
                if (PuzzleOrchestrator.Instance.CurrentPuzzleBoard?.HasUndo == true)
                    await PuzzleOrchestrator.Instance.ApplyUndoAsync();
            });

            Panel.Q<Button>("ExitBtn")?.RegisterCallback<ClickEvent>(_ =>
            {
                Panel.Q<VisualElement>("AbandonModal").style.display = DisplayStyle.Flex;
            });

            Panel.Q<Button>("AbandonBtn")?.RegisterCallback<ClickEvent>(_ =>
            {
                Panel.Q<VisualElement>("AbandonModal").style.display = DisplayStyle.Flex;
            });

            Panel.Q<Button>("AbandonCancelBtn")?.RegisterCallback<ClickEvent>(_ =>
            {
                Panel.Q<VisualElement>("AbandonModal").style.display = DisplayStyle.None;
            });

            Panel.Q<Button>("AbandonConfirmBtn")?.RegisterCallback<ClickEvent>(async _ =>
            {
                Panel.Q<VisualElement>("AbandonModal").style.display = DisplayStyle.None;
                await PuzzleOrchestrator.Instance.AbandonPuzzleAsync();
            });

            // Initialize BoardRenderer
            var top = Panel.Q<VisualElement>("BoardRowTop");
            var bottom = Panel.Q<VisualElement>("BoardRowBottom");
            BoardRenderer.Instance?.Initialize(top, bottom);
        }

        protected override void OnShow()
        {
            _startTime = System.DateTime.UtcNow;
            _timerRunning = true;
            Panel.schedule.Execute(UpdateHUD).Every(1000);

            // Build board
            var board = PuzzleOrchestrator.Instance.CurrentPuzzleBoard;
            var stats = PuzzleOrchestrator.Instance.CurrentPuzzleStats;
            if (board != null)
                BoardRenderer.Instance?.BuildBoard(board, stats);
        }

        protected override void OnHide()
        {
            _timerRunning = false;
        }

        private void UpdateHUD()
        {
            if (!_timerRunning) return;
            var elapsed = System.DateTime.UtcNow - _startTime;
            int secs = (int)elapsed.TotalSeconds;
            _timerLabel.text = $"{secs / 60:D2}:{secs % 60:D2}";

            var stats = PuzzleOrchestrator.Instance.CurrentPuzzleStats;
            if (stats != null)
            {
                _movesLabel.text = stats.MoveCount.ToString();
                _undosLabel.text = stats.UndoCount.ToString();
            }
        }
    }

    // ── SolvedScreen ──────────────────────────────────────────────────────────
    public class SolvedScreen : BaseScreenController
    {
        public SolvedScreen(VisualTreeAsset uxml) : base(uxml) { }

        protected override void OnBuild()
        {
            Panel.Q<Button>("FinalizeBtn")?.RegisterCallback<ClickEvent>(async _ =>
                await PuzzleOrchestrator.Instance.FinalizePuzzleAsync());
        }

        protected override void OnShow()
        {
            var stats = PuzzleOrchestrator.Instance.CurrentPuzzleStats;
            if (stats == null) return;

            Panel.Q<Label>("ScoreLabel").text = stats.FinalScore.ToString("N0");
            Panel.Q<Label>("MovesStatLabel").text = stats.MoveCount.ToString();
            Panel.Q<Label>("UndosStatLabel").text = stats.UndoCount.ToString();

            long elapsed = stats.CompletedAt > 0 ? stats.CompletedAt - stats.StartedAt : 0;
            Panel.Q<Label>("TimeStatLabel").text = FormatTime(elapsed);
        }

        private static string FormatTime(long secs)
        {
            long m = secs / 60, s = secs % 60;
            return m > 0 ? $"{m}m {s}s" : $"{s}s";
        }
    }

    // ── Stub screens (Leaderboard, Tournament, Profile) ───────────────────────
    public class LeaderboardScreen : BaseScreenController
    {
        public LeaderboardScreen(VisualTreeAsset uxml) : base(uxml) { }
        protected override void OnBuild()
        {
            Panel.Q<Button>("BackBtn")?.RegisterCallback<ClickEvent>(_ =>
                GameStateManager.Instance.GoToDashboard());
        }
    }

    public class TournamentScreen : BaseScreenController
    {
        public TournamentScreen(VisualTreeAsset uxml) : base(uxml) { }
        protected override void OnBuild()
        {
            Panel.Q<Button>("BackBtn")?.RegisterCallback<ClickEvent>(_ =>
                GameStateManager.Instance.GoToDashboard());
        }
    }

    public class ProfileScreen : BaseScreenController
    {
        public ProfileScreen(VisualTreeAsset uxml) : base(uxml) { }
        protected override void OnBuild()
        {
            Panel.Q<Button>("BackBtn")?.RegisterCallback<ClickEvent>(_ =>
                GameStateManager.Instance.GoToDashboard());
        }
    }
}
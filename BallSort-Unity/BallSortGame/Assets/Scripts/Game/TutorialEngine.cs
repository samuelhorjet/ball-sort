using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BallSort.Game
{
    /// <summary>
    /// Pure client-side tutorial puzzle — no blockchain.
    /// 4 colors, 4 balls each, 5 tubes (1 empty).
    /// Mirrors TypeScript tutorialEngine.ts exactly.
    /// </summary>
    public class TutorialEngine
    {
        public const int MAX_CAPACITY = 4;
        public const int NUM_TUBES    = 4;  // 3 filled + 1 empty
        public const int NUM_COLORS   = 3;

        public List<List<int>> Tubes { get; private set; }
        public int SelectedTube      { get; private set; } = -1;
        public int MoveCount         { get; private set; }

        private readonly List<(int from, int to)> _history = new();

        public bool IsSolved { get; private set; }

        public void Reset()
        {
            // 3 colors (1=Pink, 2=Cyan, 3=Gold), 4 balls each, mixed across 3 tubes + 1 empty
            Tubes = new List<List<int>>
            {
                new() { 1, 2, 3, 1 },  // tube 1: mixed
                new() { 2, 3, 1, 2 },  // tube 2: mixed
                new() { 3, 1, 2, 3 },  // tube 3: mixed
                new()                  // tube 4: empty (working space)
            };
            SelectedTube = -1;
            MoveCount    = 0;
            IsSolved     = false;
            _history.Clear();
        }

        /// <summary>Handle a tube click. Returns true if a move was made.</summary>
        public bool SelectTube(int index)
        {
            if (IsSolved) return false;

            if (SelectedTube < 0)
            {
                if (Tubes[index].Count == 0) return false;
                SelectedTube = index;
                return false;
            }

            if (SelectedTube == index)
            {
                SelectedTube = -1;
                return false;
            }

            if (CanMove(SelectedTube, index))
            {
                int from = SelectedTube;
                int to   = index;
                int ball = Tubes[from][^1];
                Tubes[from].RemoveAt(Tubes[from].Count - 1);
                Tubes[to].Add(ball);
                _history.Add((from, to));
                MoveCount++;
                SelectedTube = -1;
                CheckSolved();
                return true;
            }
            else
            {
                SelectedTube = index;
                return false;
            }
        }

        public void Undo()
        {
            if (_history.Count == 0) return;
            var (from, to) = _history[^1];
            _history.RemoveAt(_history.Count - 1);
            int ball = Tubes[to][^1];
            Tubes[to].RemoveAt(Tubes[to].Count - 1);
            Tubes[from].Add(ball);
            SelectedTube = -1;
            IsSolved     = false;
        }

        public bool CanMove(int from, int to)
        {
            if (from == to)                      return false;
            if (Tubes[from].Count == 0)          return false;
            if (Tubes[to].Count >= MAX_CAPACITY) return false;
            return true; // any ball can go into any non-full tube
        }

        private void CheckSolved()
        {
            foreach (var tube in Tubes)
            {
                if (tube.Count == 0) continue;
                if (tube.Count != MAX_CAPACITY) return;
                int first = tube[0];
                foreach (var b in tube)
                    if (b != first) return;
            }
            IsSolved = true;
        }
    }

    /// <summary>
    /// Renders the tutorial board using UI Toolkit elements.
    /// </summary>
    public class TutorialBoardRenderer
    {
        private readonly VisualElement _panel;
        private readonly TutorialEngine _engine;
        private VisualElement _boardRow;
        private Label _movesLabel;
        private Label _timerLabel;
        private float _elapsed;
        private bool  _running;

        public Action OnComplete;

        private static readonly Color[] COLORS = {
            Color.clear,
            new Color(1.00f, 0.18f, 0.47f),
            new Color(0.00f, 0.83f, 1.00f),
            new Color(1.00f, 0.84f, 0.00f),
            new Color(0.49f, 0.23f, 0.93f),
        };

        private const int BALL_SIZE = 28;
        private const int GAP       = 3;

        public TutorialBoardRenderer(VisualElement panel, TutorialEngine engine)
        {
            _panel  = panel;
            _engine = engine;
            _elapsed = 0f;
            _running = false;
        }

        public void StartTimer()
        {
            _elapsed = 0f;
            _running = true;
            _panel.schedule.Execute(TickTimer).Every(1000);
        }

        public void StopTimer() => _running = false;

        private void TickTimer()
        {
            if (!_running) return;
            _elapsed += 1f;
            if (_timerLabel == null)
                _timerLabel = _panel.Q<Label>("TimerLabel");
            if (_timerLabel != null)
            {
                int m = (int)_elapsed / 60;
                int s = (int)_elapsed % 60;
                _timerLabel.text = $"{m:D2}:{s:D2}";
            }
        }

        public void Refresh()
        {
            if (_timerLabel == null)
                _timerLabel = _panel.Q<Label>("TimerLabel");

            if (_boardRow == null)
            {
                _boardRow = _panel.Q<VisualElement>("BoardRow");
                if (_boardRow == null)
                {
                    _boardRow = new VisualElement();
                    _boardRow.name = "BoardRow";
                    _boardRow.style.flexDirection  = FlexDirection.Row;
                    _boardRow.style.alignItems     = Align.FlexEnd;
                    _boardRow.style.justifyContent = Justify.Center;
                    _boardRow.style.flexWrap       = Wrap.Wrap;
                    _boardRow.style.marginTop      = BALL_SIZE + 12;
                    _boardRow.style.marginBottom   = 12;
                    _panel.Add(_boardRow);
                }
            }

            _boardRow.Clear();

            int tubeWidth = BALL_SIZE + 14;
            // Proper test-tube bottom: semicircle = half the tube width
            float bottomRadius = tubeWidth / 2f;

            for (int t = 0; t < TutorialEngine.NUM_TUBES; t++)
            {
                var tube    = _engine.Tubes[t];
                bool sel    = _engine.SelectedTube == t;
                bool canRcv = _engine.SelectedTube >= 0 && _engine.CanMove(_engine.SelectedTube, t);

                // Wrapper uses relative positioning so the absolutely-positioned
                // float ball can sit above the tube without affecting row layout.
                var wrapper = new VisualElement();
                wrapper.style.flexDirection = FlexDirection.Column;
                wrapper.style.alignItems    = Align.Center;
                wrapper.style.marginLeft    = wrapper.style.marginRight = 5;
                wrapper.style.position      = Position.Relative;

                // ── Tube body ────────────────────────────────────────────────
                int idx = t;
                var body = new VisualElement();
                body.style.width  = tubeWidth;
                body.style.height = TutorialEngine.MAX_CAPACITY * (BALL_SIZE + GAP) + 10;

                // Proper test-tube shape: open top, semicircular bottom
                body.style.borderBottomLeftRadius  = bottomRadius;
                body.style.borderBottomRightRadius = bottomRadius;
                body.style.borderTopLeftRadius     = 3;
                body.style.borderTopRightRadius    = 3;

                body.style.borderBottomWidth = 2f;
                body.style.borderLeftWidth   = 2f;
                body.style.borderRightWidth  = 2f;
                body.style.borderTopWidth    = 0;

                var borderColor = new StyleColor(
                    sel    ? new Color(1f, 0.18f, 0.47f) :
                    canRcv ? new Color(1f, 0.43f, 0.71f, 0.5f) :
                             new Color(1f, 1f,    1f,    0.12f));

                body.style.borderTopColor    = borderColor;
                body.style.borderBottomColor = borderColor;
                body.style.borderLeftColor   = borderColor;
                body.style.borderRightColor  = borderColor;

                body.style.backgroundColor = new StyleColor(
                    sel ? new Color(1f, 0.18f, 0.47f, 0.05f) : new Color(1f, 1f, 1f, 0.02f));

                body.style.overflow       = Overflow.Hidden;
                body.style.alignItems     = Align.Center;
                body.style.justifyContent = Justify.FlexEnd; // gravity: pack balls to bottom
                body.style.paddingBottom  = 5;
                body.style.flexDirection  = FlexDirection.Column; // normal top-to-bottom

                // Add balls top-to-bottom so the LAST in list = BOTTOM ball
                // (FlexEnd with Column packs toward the bottom, first item is highest)
                for (int b = tube.Count - 1; b >= 0; b--)
                {
                    // The top ball (b == tube.Count-1) is dimmed while floating
                    bool isTopBall = b == tube.Count - 1;
                    bool dimmed    = sel && isTopBall;
                    body.Add(MakeBall(tube[b], dimmed: dimmed));
                }

                body.RegisterCallback<ClickEvent>(_ =>
                {
                    _engine.SelectTube(idx);
                    if (_engine.IsSolved)
                    {
                        Refresh();
                        OnComplete?.Invoke();
                    }
                    else
                    {
                        Refresh();
                    }
                });

                wrapper.Add(body);

                // ── Float ball (selected top ball) ────────────────────────────
                // Absolutely positioned above the tube body so it never takes
                // up layout space — no empty gap above unselected tubes.
                if (sel && tube.Count > 0)
                {
                    var floatBall = MakeBall(tube[^1], floating: true);
                    floatBall.style.position = Position.Absolute;
                    // Center horizontally over the tube, sit just above it
                    floatBall.style.left = (tubeWidth - BALL_SIZE) / 2;
                    floatBall.style.top  = -(BALL_SIZE + 6);
                    wrapper.Add(floatBall);
                }

                // ── Index label ───────────────────────────────────────────────
                var idxLabel = new Label((t + 1).ToString());
                idxLabel.style.fontSize  = 10;
                idxLabel.style.color     = new StyleColor(new Color(1f, 1f, 1f, 0.2f));
                idxLabel.style.marginTop = 4;
                wrapper.Add(idxLabel);

                _boardRow.Add(wrapper);
            }

            if (_movesLabel == null)
                _movesLabel = _panel.Q<Label>("MovesLabel");
            if (_movesLabel != null)
                _movesLabel.text = $"MOVES: {_engine.MoveCount}";
            // keep timer label synced
            if (_timerLabel == null)
                _timerLabel = _panel.Q<Label>("TimerLabel");
        }

        private static VisualElement MakeBall(int color, bool floating = false, bool dimmed = false)
        {
            var el = new VisualElement();
            el.style.width  = BALL_SIZE;
            el.style.height = BALL_SIZE;
            // Full circle
            el.style.borderTopLeftRadius     =
            el.style.borderTopRightRadius    =
            el.style.borderBottomLeftRadius  =
            el.style.borderBottomRightRadius = BALL_SIZE / 2;
            el.style.marginTop    = GAP / 2;
            el.style.marginBottom = GAP / 2;
            el.style.backgroundColor = new StyleColor(
                color > 0 && color < COLORS.Length ? COLORS[color] : Color.gray);
            if (dimmed)   el.style.opacity = 0.20f;
            if (floating) el.style.scale   = new Scale(new Vector3(1.15f, 1.15f, 1f));
            return el;
        }
    }
}
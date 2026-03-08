using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using BallSort.Blockchain;

namespace BallSort.Game
{
    /// <summary>
    /// Renders the puzzle board. Tubes are compact and centered.
    /// Size is computed once when geometry resolves and on rebuild.
    ///
    /// KEY FIX: Balls are now added directly to the tube body using
    /// Column + justifyContent:FlexEnd, matching TutorialBoardRenderer.
    /// The old approach (ColumnReverse + position:absolute + bottom:5)
    /// was broken in Unity's Yoga engine — balls appeared at the top of
    /// the tube instead of the bottom for top-row tubes.
    /// </summary>
    public class BoardRenderer : MonoBehaviour
    {
        public static BoardRenderer Instance { get; private set; }

        private VisualElement _boardArea;
        private VisualElement _boardRowTop;
        private VisualElement _boardRowBottom;
        private List<TubeView> _tubeViews = new();
        private int _selectedTubeIndex = -1;

        private PuzzleBoard _lastBoard;
        private PuzzleStats _lastStats;
        private bool        _built;

        // ── Wait for geometry before first build ─────────────────────────────
        // BuildBoard() is called from GameScreen_.OnShow() while the panel
        // may still have resolvedStyle.width/height == 0. We store the data
        // and defer the actual draw until GeometryChangedEvent fires.
        private bool _pendingBuild;

        private static readonly Color[] BALL_COLORS = new Color[]
        {
            Color.clear,
            new Color(1.00f, 0.18f, 0.47f),  // 1 Pink
            new Color(0.00f, 0.83f, 1.00f),  // 2 Cyan
            new Color(1.00f, 0.84f, 0.00f),  // 3 Gold
            new Color(0.49f, 0.23f, 0.93f),  // 4 Violet
            new Color(0.06f, 0.73f, 0.51f),  // 5 Emerald
            new Color(0.98f, 0.45f, 0.09f),  // 6 Orange
            new Color(0.23f, 0.51f, 0.96f),  // 7 Blue
            new Color(0.93f, 0.28f, 0.60f),  // 8 Rose
            new Color(0.52f, 0.80f, 0.09f),  // 9 Lime
            new Color(0.08f, 0.72f, 0.65f),  // 10 Teal
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize(VisualElement boardRowTop, VisualElement boardRowBottom)
        {
            _boardRowTop    = boardRowTop;
            _boardRowBottom = boardRowBottom;
            _boardArea      = boardRowTop.parent;

            // Rebuild when layout resolves (first frame, screen resize, orientation change)
            _boardArea.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (_built || _pendingBuild)
                {
                    _pendingBuild = false;
                    Rebuild();
                }
            });
        }

        public void BuildBoard(PuzzleBoard board, PuzzleStats stats)
        {
            _lastBoard = board;
            _lastStats = stats;

            // ── Wait for geometry to resolve before drawing ─────────────────
            // resolvedStyle.width/height are 0 on the same frame the panel
            // becomes visible. Defer the draw to the GeometryChangedEvent.
            float areaW = _boardArea?.resolvedStyle.width  ?? 0;
            float areaH = _boardArea?.resolvedStyle.height ?? 0;

            if (areaW < 10 || areaH < 10)
            {
                // Geometry not ready yet — set flags and let the callback draw
                _built        = false;
                _pendingBuild = true;
                return;
            }

            _built        = true;
            _pendingBuild = false;
            Rebuild();
        }

        private void Rebuild()
        {
            if (_lastBoard == null) return;

            _tubeViews.Clear();
            _selectedTubeIndex = -1;
            _boardRowTop.Clear();
            _boardRowBottom.Clear();

            int numTubes  = _lastBoard.NumTubes;
            int capacity  = _lastBoard.MaxCapacity;
            int halfPoint = Mathf.CeilToInt(numTubes / 2f);
            int tubesTop  = halfPoint;
            int tubesBot  = numTubes - halfPoint;
            int maxPerRow = Mathf.Max(tubesTop, tubesBot);

            // ── Compute tube size from available board area ─────────────────
            float areaW = _boardArea.resolvedStyle.width;
            float areaH = _boardArea.resolvedStyle.height;

            // Safer fallbacks: use Screen dimensions minus estimated chrome heights
            // HUD (~60px) + Controls (~72px) + padding (~30px) ≈ 162px overhead
            if (areaW < 10) areaW = Screen.width  * 0.92f;
            if (areaH < 10) areaH = Screen.height * 0.92f - 162f;

            // ── Ball size formula (matches TutorialBoardRenderer scale) ─────
            // Width: fit maxPerRow tubes with margins into areaW
            float maxBallByWidth  = (areaW * 0.88f / maxPerRow) - 18f;

            // Height: two rows, each row = floatSlot + tubeBody.
            // floatSlot ≈ ballSize + 10 ≈ 0.12 of row height
            // tubeBody  = capacity × (ballSize + 4) + 12
            // rowHeight ≈ (ballSize + 10) + (capacity × (ballSize+4) + 12)
            //           = ballSize × (1 + capacity) + (capacity×4 + 22)
            // 2 rows fit in areaH * 0.92 (leave some breathing room)
            // Solving for ballSize:
            // ballSize = (areaH×0.46 - capacity×4 - 22) / (1 + capacity)
            float maxBallByHeight = (areaH * 0.46f - capacity * 4f - 22f) / (1f + capacity);

            // Cap at 44px — beyond this tubes become unwieldy on any screen size
            int ballSize = (int)Mathf.Clamp(Mathf.Min(maxBallByWidth, maxBallByHeight), 16, 44);
            int tubeW    = ballSize + 14;
            int tubeH    = capacity * (ballSize + 4) + 12;
            int floatH   = ballSize + 10;

            Debug.Log($"[Board] area={areaW:F0}×{areaH:F0} tubes={numTubes} cap={capacity} " +
                      $"maxPerRow={maxPerRow} ballSize={ballSize} tubeW={tubeW} tubeH={tubeH}");

            for (int i = 0; i < numTubes; i++)
            {
                var tv = new TubeView(i, _lastBoard.GetTubeBalls(i), (byte)capacity,
                                      ballSize, tubeW, tubeH, floatH, BALL_COLORS);
                tv.OnTubeClicked += HandleTubeClicked;
                _tubeViews.Add(tv);

                (i < halfPoint ? _boardRowTop : _boardRowBottom).Add(tv.Root);
            }
        }

        public void RefreshFromData(PuzzleBoard board, PuzzleStats stats)
        {
            _lastBoard = board;
            _lastStats = stats;
            if (board == null || _tubeViews.Count != board.NumTubes) return;

            for (int i = 0; i < board.NumTubes; i++)
            {
                bool sel    = i == _selectedTubeIndex;
                bool canGet = _selectedTubeIndex >= 0 && board.CanMove(_selectedTubeIndex, i);
                _tubeViews[i].UpdateBalls(board.GetTubeBalls(i), board.MaxCapacity, sel, canGet);
            }
        }

        private void HandleTubeClicked(int tubeIndex)
        {
            var board = PuzzleOrchestrator.Instance.CurrentPuzzleBoard;
            if (board == null) return;

            if (_selectedTubeIndex < 0)
            {
                if (board.IsTubeEmpty(tubeIndex)) return;
                _selectedTubeIndex = tubeIndex;
                HighlightSelection(board);
            }
            else if (_selectedTubeIndex == tubeIndex)
            {
                _selectedTubeIndex = -1;
                HighlightSelection(board);
            }
            else if (board.CanMove(_selectedTubeIndex, tubeIndex))
            {
                int from = _selectedTubeIndex;
                int to   = tubeIndex;
                _selectedTubeIndex = -1;
                HighlightSelection(board);
                _ = PuzzleOrchestrator.Instance.ApplyMoveAsync((byte)from, (byte)to);
            }
            else
            {
                _tubeViews[tubeIndex].PlayShakeAnim();
                _selectedTubeIndex = board.IsTubeEmpty(tubeIndex) ? -1 : tubeIndex;
                HighlightSelection(board);
            }
        }

        private void HighlightSelection(PuzzleBoard board)
        {
            for (int i = 0; i < _tubeViews.Count; i++)
            {
                bool sel = i == _selectedTubeIndex;
                bool can = _selectedTubeIndex >= 0 && board.CanMove(_selectedTubeIndex, i);
                _tubeViews[i].SetHighlight(sel, can);
            }
        }
    }

    // ── TubeView ──────────────────────────────────────────────────────────────
    public class TubeView
    {
        public VisualElement Root { get; }

        private readonly VisualElement _floatSlot;
        private readonly VisualElement _tubeBody;
        private readonly int           _index;
        private readonly Color[]       _colors;
        private readonly int           _ballSize;

        public event System.Action<int> OnTubeClicked;

        public TubeView(int index, byte[] balls, byte maxCapacity,
                        int ballSize, int tubeW, int tubeH, int floatH, Color[] colors)
        {
            _index    = index;
            _colors   = colors;
            _ballSize = ballSize;

            // ── Outer wrapper ────────────────────────────────────────────────
            Root = new VisualElement();
            Root.style.flexDirection  = FlexDirection.Column;
            Root.style.alignItems     = Align.Center;
            Root.style.width          = tubeW + 16;
            Root.style.marginLeft     = 4;
            Root.style.marginRight    = 4;

            // ── Float slot (shows the lifted top ball when selected) ──────────
            _floatSlot = new VisualElement();
            _floatSlot.style.height         = floatH;
            _floatSlot.style.width          = tubeW + 16;
            _floatSlot.style.alignItems     = Align.Center;
            _floatSlot.style.justifyContent = Justify.FlexEnd;  // ball sits at bottom of slot

            // ── Tube body ────────────────────────────────────────────────────
            _tubeBody = new VisualElement();
            _tubeBody.style.width   = tubeW;
            _tubeBody.style.height  = tubeH;

            // Test-tube shape: rounded bottom, slight top rounding
            _tubeBody.style.borderBottomLeftRadius  = tubeW / 2f;
            _tubeBody.style.borderBottomRightRadius = tubeW / 2f;
            _tubeBody.style.borderTopLeftRadius     = 5;
            _tubeBody.style.borderTopRightRadius    = 5;
            _tubeBody.style.borderBottomWidth = 2;
            _tubeBody.style.borderLeftWidth   = 2;
            _tubeBody.style.borderRightWidth  = 2;
            _tubeBody.style.borderTopWidth    = 0;
            SetBorderColor(new Color(1f, 1f, 1f, 0.12f));
            _tubeBody.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 0.03f));
            _tubeBody.style.overflow        = Overflow.Hidden;
            _tubeBody.style.alignItems      = Align.Center;

            // ── KEY FIX: gravity ─────────────────────────────────────────────
            // Use Column + justifyContent:FlexEnd (packs children to the bottom).
            // This mirrors TutorialBoardRenderer which is proven to work.
            // The old code used ColumnReverse + position:absolute + bottom:5
            // which is broken in Unity's Yoga: balls appeared at the TOP of the
            // tube instead of the bottom for top-row tubes.
            _tubeBody.style.flexDirection  = FlexDirection.Column;
            _tubeBody.style.justifyContent = Justify.FlexEnd;
            _tubeBody.style.paddingBottom  = 4;
            // ─────────────────────────────────────────────────────────────────

            // ── Index label ───────────────────────────────────────────────────
            var lbl = new Label((index + 1).ToString());
            lbl.style.fontSize       = 9;
            lbl.style.color          = new StyleColor(new Color(1f, 1f, 1f, 0.2f));
            lbl.style.marginTop      = 4;
            lbl.style.unityTextAlign = TextAnchor.MiddleCenter;

            Root.Add(_floatSlot);
            Root.Add(_tubeBody);
            Root.Add(lbl);
            Root.RegisterCallback<ClickEvent>(_ => OnTubeClicked?.Invoke(_index));

            UpdateBalls(balls, maxCapacity, false, false);
        }

        private void SetBorderColor(Color c)
        {
            _tubeBody.style.borderBottomColor =
            _tubeBody.style.borderLeftColor   =
            _tubeBody.style.borderRightColor  = new StyleColor(c);
        }

        private void SetBg(Color c)
            => _tubeBody.style.backgroundColor = new StyleColor(c);

        public void SetHighlight(bool selected, bool canReceive)
        {
            if (selected)
            { SetBorderColor(new Color(1f, 0.18f, 0.47f)); SetBg(new Color(1f, 0.18f, 0.47f, 0.07f)); }
            else if (canReceive)
            { SetBorderColor(new Color(1f, 0.55f, 0.78f, 0.6f)); SetBg(new Color(1f, 0.55f, 0.78f, 0.04f)); }
            else
            { SetBorderColor(new Color(1f, 1f, 1f, 0.12f)); SetBg(new Color(1f, 1f, 1f, 0.03f)); }
        }

        public void UpdateBalls(byte[] balls, byte maxCapacity, bool isSelected, bool canReceive)
        {
            _tubeBody.Clear();
            _floatSlot.Clear();
            SetHighlight(isSelected, canReceive);

            // Float ball: lifted top ball shown above the tube when selected
            if (isSelected && balls.Length > 0)
                _floatSlot.Add(MakeBall(balls[^1], floating: true));

            // ── KEY FIX: add balls directly to tube body ─────────────────────
            // Iterate from the TOP ball (index n-1) DOWN to the BOTTOM ball (index 0).
            // With flexDirection:Column + justifyContent:FlexEnd, the last item added
            // sits at the visual bottom, so:
            //   - balls[0]   → visual bottom  ✓
            //   - balls[n-1] → visual top     ✓
            //
            // This is identical to TutorialBoardRenderer (which is proven correct).
            // The old inner absolute-positioned ColumnReverse stack was causing
            // balls to appear at the top of the tube in top-row tubes.
            for (int i = balls.Length - 1; i >= 0; i--)
            {
                bool dimmed = isSelected && i == balls.Length - 1;
                _tubeBody.Add(MakeBall(balls[i], dimmed: dimmed));
            }
        }

        public void PlayShakeAnim()
        {
            Root.schedule.Execute(() => Root.style.translate = new Translate(-6, 0)).ExecuteLater(0);
            Root.schedule.Execute(() => Root.style.translate = new Translate(6, 0)).ExecuteLater(60);
            Root.schedule.Execute(() => Root.style.translate = new Translate(-4, 0)).ExecuteLater(120);
            Root.schedule.Execute(() => Root.style.translate = new Translate(4, 0)).ExecuteLater(180);
            Root.schedule.Execute(() => Root.style.translate = new Translate(0, 0)).ExecuteLater(240);
        }

        private VisualElement MakeBall(byte color, bool floating = false, bool dimmed = false)
        {
            var el = new VisualElement();
            el.style.width  = _ballSize;
            el.style.height = _ballSize;
            el.style.borderTopLeftRadius     =
            el.style.borderTopRightRadius    =
            el.style.borderBottomLeftRadius  =
            el.style.borderBottomRightRadius = _ballSize / 2f;
            el.style.marginTop    = 2;
            el.style.marginBottom = 2;
            el.style.flexShrink   = 0;

            if (color > 0 && color < _colors.Length)
                el.style.backgroundColor = new StyleColor(_colors[color]);
            if (dimmed)   el.style.opacity = 0.25f;
            if (floating) el.style.scale   = new Scale(new Vector3(1.15f, 1.15f, 1f));

            return el;
        }
    }
}
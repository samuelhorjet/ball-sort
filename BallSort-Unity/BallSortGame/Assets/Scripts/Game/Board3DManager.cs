using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BallSort.Core;
using BallSort.Blockchain;
using UnityEngine.InputSystem;

namespace BallSort.Game
{
    public class Board3DManager : MonoBehaviour
    {
        public static Board3DManager Instance { get; private set; }

        [Header("Prefabs")]
        public GameObject TubePrefab;
        public GameObject BallPrefab;

        [Header("Dynamic Spacing")]
        private float _tubeSpacing;
        private float _ballYSpacing    = 1.1f;
        private float _ballDiameter    = 0.85f;
        private float _tubeBottomY     = -2.2f;   // world Y of the CENTRE of the bottom ball slot
        private float _hoverYOffset    = 1.2f;

        // Padding added below / above the ball stack to form the visible tube walls.
        // _tubeTopPad is intentionally larger so the open tube rim is clearly visible.
        private const float _tubeBottomPad = 0.30f;
        private const float _tubeTopPad    = 0.85f;

        // --- Add this missing method ---
        private int GetTubeIndexFromHit(Transform hitTransform)
        {
            Transform current = hitTransform;
            while (current != null)
            {
                // Check if the object or its parent is named "Tube_X"
                if (current.name.StartsWith("Tube_"))
                {
                    string[] parts = current.name.Split('_');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int index))
                    {
                        return index;
                    }
                }
                // If not, look at the parent (to handle clicking the cylinder inside the prefab)
                current = current.parent;
            }
            return -1;
        }

        [Header("Colors")]
        public Color[] BallColors;

        private List<GameObject> _spawnedTubes = new List<GameObject>();
        private List<GameObject>[] _tubeBalls;
        private int _selectedTubeIndex = -1;
        private bool _isAnimating = false;

        // Cache the current board settings
        private int _currentMaxCapacity;
        private int _currentNumTubes;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            SetupColors();
        }

        private void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !_isAnimating)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    int clickedTube = GetTubeIndexFromHit(hit.transform);
                    if (clickedTube != -1) HandleTubeClicked(clickedTube);
                }
            }
        }

        public void DrawBoard(PuzzleBoard board)
        {
            if (_isAnimating) return;
            ClearBoard();
            if (board == null) return;

            _currentMaxCapacity = board.MaxCapacity;
            _currentNumTubes    = board.NumTubes;
            _tubeBalls          = new List<GameObject>[_currentNumTubes];

            // ── 1. Tube spacing ───────────────────────────────────────────────
            // Tighter spacing for larger boards so everything stays on screen.
            _tubeSpacing = _currentNumTubes > 8 ? 2.0f :
                           _currentNumTubes > 6 ? 2.3f : 2.8f;

            float totalWidth = (_currentNumTubes - 1) * _tubeSpacing;
            float startX     = -totalWidth / 2f;

            // ── 2. Tube geometry (the same for every tube on the board) ───────
            //
            // The ball stack occupies world-Y from:
            //   bottom ball centre  = _tubeBottomY
            //   top    ball centre  = _tubeBottomY + (maxCapacity-1) * _ballYSpacing
            //
            // We pad below and above to form the tube walls:
            //   tube world bottom   = _tubeBottomY - _tubeBottomPad
            //   tube world top      = _tubeBottomY + (maxCapacity-1)*_ballYSpacing + _tubeTopPad
            //
            // Unity's default Cylinder prefab has a LOCAL height of 2 units (−1 → +1 on Y).
            // So  world height = 2 × scaleY  →  scaleY = worldHeight / 2.
            // The tube GameObject must be placed at the MID-POINT of [bottom, top].

            float tubeWorldBottom = _tubeBottomY - _tubeBottomPad;
            float tubeWorldTop    = _tubeBottomY + (_currentMaxCapacity - 1) * _ballYSpacing
                                    + _tubeTopPad;
            float tubeWorldHeight = tubeWorldTop - tubeWorldBottom;   // exact, not guessed
            float tubeScaleY      = tubeWorldHeight * 0.5f;           // ÷2 because cylinder local h=2
            float tubeCenterY     = (tubeWorldBottom + tubeWorldTop) * 0.5f;

            // ── 3. Spawn tubes & balls ────────────────────────────────────────
            for (int t = 0; t < _currentNumTubes; t++)
            {
                _tubeBalls[t] = new List<GameObject>();

                // Place tube centred on its computed Y mid-point
                Vector3 tubePos = new Vector3(startX + t * _tubeSpacing, tubeCenterY, 0f);
                GameObject tube = Instantiate(TubePrefab, tubePos, Quaternion.identity, transform);
                tube.name = $"Tube_{t}";
                tube.transform.localScale = new Vector3(1.1f, tubeScaleY, 1.1f);
                _spawnedTubes.Add(tube);

                int ballCount = board.TubeLengths[t];
                int baseIndex = t * Blockchain.PuzzleConstants.MAX_CAPACITY;

                for (int b = 0; b < ballCount; b++)
                {
                    byte colorIndex = board.Balls[baseIndex + b];
                    Vector3 targetPos = GetBallPosition(t, b);
                    GameObject ball = Instantiate(BallPrefab, targetPos, Quaternion.identity, transform);
                    ball.transform.localScale = Vector3.one * _ballDiameter;

                    if (colorIndex < BallColors.Length)
                        ball.GetComponent<MeshRenderer>().material.color = BallColors[colorIndex];

                    _tubeBalls[t].Add(ball);
                }
            }

            // ── 4. Auto-zoom camera — centres on the board both X and Y ──────
            //
            // The board's visual centre sits at (0, tubeCenterY).
            // We use the camera's actual FOV and aspect ratio so the calculation
            // is correct on every screen size and orientation:
            //
            //   tan(halfFov) = halfBoardDimension / distance
            //   → distance   = halfBoardDimension / tan(halfFov)
            //
            // We compute required distance for both height and width, then take
            // the larger (so the board always fits), add a small padding factor.

            float camFov      = Camera.main.fieldOfView;
            float halfFovRad  = camFov * 0.5f * Mathf.Deg2Rad;
            float aspect      = Camera.main.aspect;

            float halfBoardH  = tubeWorldHeight * 0.5f;
            float halfBoardW  = totalWidth      * 0.5f;

            // Distance needed to fit the height / width (with 15 % breathing room)
            const float PAD = 1.18f;
            float distForH = (halfBoardH * PAD) / Mathf.Tan(halfFovRad);
            float distForW = (halfBoardW * PAD) / (Mathf.Tan(halfFovRad) * aspect);

            float targetZ = -Mathf.Clamp(Mathf.Max(distForH, distForW), 8f, 40f);

            Camera.main.transform.position = new Vector3(0f, tubeCenterY, targetZ);
        }

        private Vector3 GetBallPosition(int tubeIndex, int ballIndexInTube)
        {
            float totalWidth = (_currentNumTubes - 1) * _tubeSpacing;
            float startX     = -totalWidth / 2f;
            float xPos = startX + tubeIndex * _tubeSpacing;
            float yPos = _tubeBottomY + ballIndexInTube * _ballYSpacing;
            return new Vector3(xPos, yPos, 0f);
        }

        private void HandleTubeClicked(int tubeIndex)
        {
            // We use the Local Authoritative state from PuzzleOrchestrator
            var board = PuzzleOrchestrator.Instance.CurrentPuzzleBoard;
            if (board == null) return;

            if (_selectedTubeIndex < 0)
            {
                if (_tubeBalls[tubeIndex].Count == 0) return;
                _selectedTubeIndex = tubeIndex;
                GameObject topBall = _tubeBalls[tubeIndex][_tubeBalls[tubeIndex].Count - 1];

                // Lift ball to exactly above the tube rim
                Vector3 hoverPos = GetBallPosition(tubeIndex, _currentMaxCapacity) + new Vector3(0, _hoverYOffset, 0);
                StartCoroutine(AnimateBallMovement(topBall, topBall.transform.position, hoverPos, 0.15f));
            }
            else if (_selectedTubeIndex == tubeIndex)
            {
                GameObject topBall = _tubeBalls[tubeIndex][_tubeBalls[tubeIndex].Count - 1];
                Vector3 originalPos = GetBallPosition(tubeIndex, _tubeBalls[tubeIndex].Count - 1);
                StartCoroutine(AnimateBallMovement(topBall, topBall.transform.position, originalPos, 0.15f));
                _selectedTubeIndex = -1;
            }
            else if (board.CanMove(_selectedTubeIndex, tubeIndex))
            {
                int from = _selectedTubeIndex;
                int to = tubeIndex;
                _selectedTubeIndex = -1;

                GameObject ballToMove = _tubeBalls[from][_tubeBalls[from].Count - 1];
                _tubeBalls[from].RemoveAt(_tubeBalls[from].Count - 1);
                _tubeBalls[to].Add(ballToMove);

                // Start the Realistic 3D Arc Animation
                StartCoroutine(AnimateArcMove(ballToMove, from, to, _tubeBalls[to].Count - 1));

                // TRIGGER MOVE IN ORCHESTRATOR (It will update local state + send TEE tx)
                _ = PuzzleOrchestrator.Instance.ApplyMoveAsync((byte)from, (byte)to);
            }
        }

        private IEnumerator AnimateBallMovement(GameObject ball, Vector3 start, Vector3 end, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (ball == null) yield break;
                ball.transform.position = Vector3.Lerp(start, end, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (ball != null) ball.transform.position = end;
        }

        private IEnumerator AnimateArcMove(GameObject ball, int fromTube, int toTube, int targetIndexInTube)
        {
            _isAnimating = true;

            // 1. Lift Up high enough to clear the tube
            Vector3 upPos = GetBallPosition(fromTube, _currentMaxCapacity) + new Vector3(0, _hoverYOffset, 0);
            yield return StartCoroutine(AnimateBallMovement(ball, ball.transform.position, upPos, 0.12f));

            // 2. Move horizontally to target tube
            Vector3 overPos = GetBallPosition(toTube, _currentMaxCapacity) + new Vector3(0, _hoverYOffset, 0);
            yield return StartCoroutine(AnimateBallMovement(ball, upPos, overPos, 0.2f));

            // 3. Drop down into the tube
            Vector3 dropPos = GetBallPosition(toTube, targetIndexInTube);
            yield return StartCoroutine(AnimateBallMovement(ball, overPos, dropPos, 0.15f));

            _isAnimating = false;

            // AFTER animation finishes, check if the game is solved locally
            CheckLocalWinCondition();
        }

        private void CheckLocalWinCondition()
        {
            bool allSolved = true;
            foreach (var list in _tubeBalls)
            {
                if (list.Count == 0) continue;
                if (list.Count != _currentMaxCapacity) { allSolved = false; break; }

                Color firstColor = list[0].GetComponent<MeshRenderer>().material.color;
                foreach (var ball in list)
                {
                    if (ball.GetComponent<MeshRenderer>().material.color != firstColor) { allSolved = false; break; }
                }
                if (!allSolved) break;
            }

            if (allSolved)
            {
                Debug.Log("Local Win Detected!");
                GameStateManager.Instance.GoToGameSolved();
            }
        }

        public void ClearBoard()
        {
            foreach (var t in _spawnedTubes) if (t != null) Destroy(t);
            if (_tubeBalls != null)
            {
                foreach (var list in _tubeBalls)
                    if (list != null) foreach (var b in list) if (b != null) Destroy(b);
            }
            _spawnedTubes.Clear();
            _selectedTubeIndex = -1;
        }

        private void SetupColors()
        {
            if (BallColors == null || BallColors.Length == 0)
            {
                BallColors = new Color[] {
                    Color.clear,
                    new Color(1f, 0.17f, 0.47f), new Color(0f, 0.83f, 1f),
                    new Color(1f, 0.84f, 0f), new Color(0.48f, 0.22f, 0.92f),
                    new Color(0.06f, 0.72f, 0.5f), new Color(0.97f, 0.45f, 0.08f)
                };
            }
        }
    }
}
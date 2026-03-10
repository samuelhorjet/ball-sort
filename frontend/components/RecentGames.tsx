const PUZZLES = [
  {
    id: "#P-0041",
    difficulty: "Hard",
    diffColor: "#ff6b6b",
    moves: 74,
    time: "4m 12s",
    score: 87500,
    status: "Solved",
    statusColor: "#3dd9a0",
    tubes: 9,
  },
  {
    id: "#P-0040",
    difficulty: "Medium",
    diffColor: "#ffd93d",
    moves: 48,
    time: "2m 55s",
    score: 54200,
    status: "Solved",
    statusColor: "#3dd9a0",
    tubes: 7,
  },
  {
    id: "#P-0039",
    difficulty: "Hard",
    diffColor: "#ff6b6b",
    moves: 102,
    time: "8m 30s",
    score: 31000,
    status: "Abandoned",
    statusColor: "#ff6b6b",
    tubes: 10,
  },
  {
    id: "#P-0038",
    difficulty: "Easy",
    diffColor: "#3dd9a0",
    moves: 22,
    time: "1m 08s",
    score: 62100,
    status: "Solved",
    statusColor: "#3dd9a0",
    tubes: 5,
  },
  {
    id: "#P-0037",
    difficulty: "Medium",
    diffColor: "#ffd93d",
    moves: 55,
    time: "3m 44s",
    score: 48800,
    status: "Solved",
    statusColor: "#3dd9a0",
    tubes: 7,
  },
];

const BALL_COLORS = ["#ff2d8b", "#4d9fff", "#3dd9a0", "#ffd93d", "#b44dff", "#ff6b6b", "#ff9f43", "#2dd4bf"];

function MiniTubes({ count }: { count: number }) {
  const colors = BALL_COLORS.slice(0, Math.min(count, 8));
  return (
    <div style={{ display: "flex", gap: "3px" }}>
      {colors.map((color, i) => (
        <div
          key={i}
          style={{
            width: "8px",
            height: "18px",
            borderRadius: "4px",
            background: `linear-gradient(to bottom, ${color}cc, ${color}66)`,
            border: `1px solid ${color}44`,
          }}
        />
      ))}
    </div>
  );
}

export default function RecentGames() {
  return (
    <div
      style={{
        background: "#0d0d1e",
        border: "1px solid #1c1c38",
        borderRadius: "1.25rem",
        overflow: "hidden",
      }}
    >
      {/* Header */}
      <div
        style={{
          padding: "1.25rem 1.5rem",
          borderBottom: "1px solid #1c1c38",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
      >
        <div>
          <h3
            style={{
              fontFamily: "var(--font-syne)",
              fontSize: "1rem",
              fontWeight: 700,
              color: "#f0f0fa",
              letterSpacing: "-0.01em",
            }}
          >
            Recent Puzzles
          </h3>
          <p style={{ fontFamily: "var(--font-outfit)", fontSize: "0.75rem", color: "#555577", marginTop: "0.15rem" }}>
            Your last 5 puzzle attempts
          </p>
        </div>
        <button
          style={{
            fontFamily: "var(--font-outfit)",
            fontSize: "0.8rem",
            color: "#ff79c6",
            background: "none",
            border: "none",
            cursor: "pointer",
            fontWeight: 500,
          }}
        >
          View All →
        </button>
      </div>

      {/* Table */}
      <div style={{ overflowX: "auto" }}>
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead>
            <tr style={{ borderBottom: "1px solid #1c1c38" }}>
              {["Puzzle", "Board", "Difficulty", "Moves", "Time", "Score", "Status"].map((h) => (
                <th
                  key={h}
                  style={{
                    fontFamily: "var(--font-syne)",
                    fontSize: "0.68rem",
                    fontWeight: 600,
                    color: "#444466",
                    textTransform: "uppercase",
                    letterSpacing: "0.08em",
                    padding: "0.75rem 1.5rem",
                    textAlign: "left",
                    whiteSpace: "nowrap",
                  }}
                >
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {PUZZLES.map((p, idx) => (
              <tr
                key={p.id}
                style={{
                  borderBottom: idx < PUZZLES.length - 1 ? "1px solid #12122a" : "none",
                  transition: "background 0.15s",
                }}
                onMouseEnter={(e) => (e.currentTarget.style.background = "rgba(255,255,255,0.02)")}
                onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
              >
                <td style={{ padding: "0.875rem 1.5rem" }}>
                  <span style={{ fontFamily: "var(--font-syne)", fontSize: "0.8rem", fontWeight: 600, color: "#8888aa" }}>
                    {p.id}
                  </span>
                </td>
                <td style={{ padding: "0.875rem 1.5rem" }}>
                  <MiniTubes count={p.tubes} />
                </td>
                <td style={{ padding: "0.875rem 1.5rem" }}>
                  <span
                    style={{
                      fontFamily: "var(--font-syne)",
                      fontSize: "0.72rem",
                      fontWeight: 600,
                      color: p.diffColor,
                      background: `${p.diffColor}15`,
                      border: `1px solid ${p.diffColor}25`,
                      borderRadius: "0.4rem",
                      padding: "0.15rem 0.5rem",
                    }}
                  >
                    {p.difficulty}
                  </span>
                </td>
                <td style={{ padding: "0.875rem 1.5rem" }}>
                  <span style={{ fontFamily: "var(--font-outfit)", fontSize: "0.85rem", color: "#9999bb", fontWeight: 500 }}>
                    {p.moves}
                  </span>
                </td>
                <td style={{ padding: "0.875rem 1.5rem" }}>
                  <span style={{ fontFamily: "var(--font-outfit)", fontSize: "0.85rem", color: "#9999bb" }}>
                    {p.time}
                  </span>
                </td>
                <td style={{ padding: "0.875rem 1.5rem" }}>
                  <span
                    style={{
                      fontFamily: "var(--font-syne)",
                      fontSize: "0.85rem",
                      fontWeight: 700,
                      color: p.status === "Solved" ? "#f0f0fa" : "#555577",
                    }}
                  >
                    {p.status === "Solved" ? p.score.toLocaleString() : "—"}
                  </span>
                </td>
                <td style={{ padding: "0.875rem 1.5rem" }}>
                  <div
                    style={{
                      display: "inline-flex",
                      alignItems: "center",
                      gap: "0.3rem",
                      background: `${p.statusColor}10`,
                      border: `1px solid ${p.statusColor}20`,
                      borderRadius: "0.5rem",
                      padding: "0.2rem 0.55rem",
                    }}
                  >
                    <div
                      style={{
                        width: "5px",
                        height: "5px",
                        borderRadius: "50%",
                        background: p.statusColor,
                      }}
                    />
                    <span
                      style={{
                        fontFamily: "var(--font-outfit)",
                        fontSize: "0.72rem",
                        fontWeight: 600,
                        color: p.statusColor,
                      }}
                    >
                      {p.status}
                    </span>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

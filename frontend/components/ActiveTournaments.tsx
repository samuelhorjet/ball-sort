const TOURNAMENTS = [
  {
    id: "T-042",
    name: "Weekend Blitz",
    prizePool: "4.2 SOL",
    difficulty: "Hard",
    diffColor: "#ff6b6b",
    myRank: 4,
    totalPlayers: 38,
    endsIn: "14h 22m",
    status: "active",
    myScore: 72400,
  },
  {
    id: "T-039",
    name: "April Classic",
    prizePool: "1.5 SOL",
    difficulty: "Medium",
    diffColor: "#ffd93d",
    myRank: 2,
    totalPlayers: 25,
    endsIn: "Ended",
    status: "ended",
    myScore: 81200,
  },
];

export default function ActiveTournaments() {
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
            My Tournaments
          </h3>
          <p style={{ fontFamily: "var(--font-outfit)", fontSize: "0.75rem", color: "#555577", marginTop: "0.15rem" }}>
            Active &amp; recent
          </p>
        </div>
        <a
          href="#"
          style={{
            fontFamily: "var(--font-outfit)",
            fontSize: "0.8rem",
            color: "#ff79c6",
            textDecoration: "none",
            fontWeight: 500,
          }}
        >
          Browse all →
        </a>
      </div>

      {/* Tournament entries */}
      <div style={{ padding: "0.75rem" }}>
        {TOURNAMENTS.map((t) => (
          <div
            key={t.id}
            style={{
              background: "#12122a",
              border: "1px solid #1c1c38",
              borderRadius: "0.875rem",
              padding: "1rem",
              marginBottom: "0.75rem",
              position: "relative",
              overflow: "hidden",
            }}
          >
            {/* Status dot */}
            {t.status === "active" && (
              <div
                style={{
                  position: "absolute",
                  top: "1rem",
                  right: "1rem",
                  display: "flex",
                  alignItems: "center",
                  gap: "0.3rem",
                  background: "rgba(61,217,160,0.1)",
                  border: "1px solid rgba(61,217,160,0.2)",
                  borderRadius: "2rem",
                  padding: "0.15rem 0.5rem",
                }}
              >
                <div
                  style={{
                    width: "5px",
                    height: "5px",
                    borderRadius: "50%",
                    background: "#3dd9a0",
                    boxShadow: "0 0 6px #3dd9a033",
                    animation: "pulseGlow 2s ease-in-out infinite",
                  }}
                />
                <span style={{ fontFamily: "var(--font-outfit)", fontSize: "0.65rem", color: "#3dd9a0", fontWeight: 600 }}>
                  Live
                </span>
              </div>
            )}

            {/* Title row */}
            <div style={{ marginBottom: "0.75rem", paddingRight: "4rem" }}>
              <div style={{ fontFamily: "var(--font-outfit)", fontSize: "0.7rem", color: "#444466", marginBottom: "0.15rem" }}>
                {t.id}
              </div>
              <div
                style={{
                  fontFamily: "var(--font-syne)",
                  fontSize: "0.95rem",
                  fontWeight: 700,
                  color: "#f0f0fa",
                  letterSpacing: "-0.01em",
                }}
              >
                {t.name}
              </div>
            </div>

            {/* Stats row */}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: "0.5rem", marginBottom: "0.875rem" }}>
              {[
                { label: "Prize Pool", value: t.prizePool, highlight: true },
                { label: "My Rank", value: `#${t.myRank}`, highlight: false },
                { label: "Ends In", value: t.endsIn, highlight: false },
              ].map((s) => (
                <div
                  key={s.label}
                  style={{
                    background: s.highlight ? "rgba(255,45,139,0.06)" : "#0d0d1e",
                    border: `1px solid ${s.highlight ? "rgba(255,45,139,0.15)" : "#1c1c38"}`,
                    borderRadius: "0.6rem",
                    padding: "0.5rem",
                    textAlign: "center",
                  }}
                >
                  <div
                    style={{
                      fontFamily: "var(--font-syne)",
                      fontSize: "0.85rem",
                      fontWeight: 700,
                      color: s.highlight ? "#ff2d8b" : "#f0f0fa",
                      marginBottom: "0.1rem",
                    }}
                  >
                    {s.value}
                  </div>
                  <div style={{ fontFamily: "var(--font-outfit)", fontSize: "0.62rem", color: "#444466" }}>
                    {s.label}
                  </div>
                </div>
              ))}
            </div>

            {/* Difficulty + action */}
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <span
                style={{
                  fontFamily: "var(--font-syne)",
                  fontSize: "0.68rem",
                  fontWeight: 700,
                  color: t.diffColor,
                  background: `${t.diffColor}15`,
                  border: `1px solid ${t.diffColor}25`,
                  borderRadius: "0.4rem",
                  padding: "0.15rem 0.5rem",
                }}
              >
                {t.difficulty}
              </span>
              {t.status === "active" ? (
                <button className="btn-primary" style={{ animation: "none", padding: "0.4rem 0.875rem", fontSize: "0.75rem" }}>
                  Play Now ↗
                </button>
              ) : (
                <button
                  className="btn-secondary"
                  style={{ padding: "0.4rem 0.875rem", fontSize: "0.75rem" }}
                >
                  Claim Prize
                </button>
              )}
            </div>
          </div>
        ))}

        {/* Explore more */}
        <button
          style={{
            width: "100%",
            background: "rgba(255,45,139,0.05)",
            border: "1px dashed rgba(255,45,139,0.2)",
            borderRadius: "0.875rem",
            padding: "0.875rem",
            color: "#ff79c6",
            fontFamily: "var(--font-outfit)",
            fontSize: "0.825rem",
            fontWeight: 500,
            cursor: "pointer",
            transition: "all 0.2s",
          }}
          onMouseEnter={(e) => (e.currentTarget.style.background = "rgba(255,45,139,0.1)")}
          onMouseLeave={(e) => (e.currentTarget.style.background = "rgba(255,45,139,0.05)")}
        >
          + Join a Tournament
        </button>
      </div>
    </div>
  );
}

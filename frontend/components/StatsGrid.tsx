const STATS = [
  {
    label: "Puzzles Solved",
    value: "42",
    change: "+5 this week",
    changePositive: true,
    icon: "🔮",
    accent: "#ff2d8b",
    sub: "All-time",
  },
  {
    label: "Best Score",
    value: "87,500",
    change: "Personal best",
    changePositive: true,
    icon: "⭐",
    accent: "#ffd93d",
    sub: "Points",
  },
  {
    label: "Win Rate",
    value: "67%",
    change: "+3% vs last month",
    changePositive: true,
    icon: "📈",
    accent: "#3dd9a0",
    sub: "Tournaments",
  },
  {
    label: "Active Streak",
    value: "5 Days",
    change: "Keep it going!",
    changePositive: true,
    icon: "🔥",
    accent: "#ff9f43",
    sub: "Current",
  },
];

export default function StatsGrid() {
  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
        gap: "1rem",
        marginBottom: "1.5rem",
      }}
    >
      {STATS.map((s) => (
        <div
          key={s.label}
          className="card-glow"
          style={{
            background: "#0d0d1e",
            border: "1px solid #1c1c38",
            borderRadius: "1.25rem",
            padding: "1.25rem",
            position: "relative",
            overflow: "hidden",
          }}
        >
          {/* Corner glow */}
          <div
            style={{
              position: "absolute",
              top: 0,
              right: 0,
              width: "80px",
              height: "80px",
              background: `radial-gradient(circle at 100% 0%, ${s.accent}15, transparent 70%)`,
              pointerEvents: "none",
            }}
          />

          {/* Top row */}
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "flex-start",
              marginBottom: "0.875rem",
            }}
          >
            <span
              style={{
                fontFamily: "var(--font-outfit)",
                fontSize: "0.8rem",
                color: "#666688",
                fontWeight: 500,
              }}
            >
              {s.label}
            </span>
            <div
              style={{
                width: "34px",
                height: "34px",
                borderRadius: "9px",
                background: `${s.accent}18`,
                border: `1px solid ${s.accent}25`,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                fontSize: "1rem",
              }}
            >
              {s.icon}
            </div>
          </div>

          {/* Value */}
          <div
            style={{
              fontFamily: "var(--font-syne)",
              fontSize: "2rem",
              fontWeight: 800,
              color: "#f0f0fa",
              letterSpacing: "-0.03em",
              lineHeight: 1,
              marginBottom: "0.5rem",
            }}
          >
            {s.value}
          </div>

          {/* Sub label */}
          <div
            style={{
              fontFamily: "var(--font-outfit)",
              fontSize: "0.7rem",
              color: "#444466",
              marginBottom: "0.6rem",
            }}
          >
            {s.sub}
          </div>

          {/* Change */}
          <div
            style={{
              display: "inline-flex",
              alignItems: "center",
              gap: "0.3rem",
              background: s.changePositive ? "rgba(61,217,160,0.08)" : "rgba(255,107,107,0.08)",
              border: `1px solid ${s.changePositive ? "rgba(61,217,160,0.18)" : "rgba(255,107,107,0.18)"}`,
              borderRadius: "0.5rem",
              padding: "0.15rem 0.5rem",
            }}
          >
            <span
              style={{
                fontFamily: "var(--font-outfit)",
                fontSize: "0.7rem",
                fontWeight: 500,
                color: s.changePositive ? "#3dd9a0" : "#ff6b6b",
              }}
            >
              {s.changePositive ? "↑" : "↓"} {s.change}
            </span>
          </div>
        </div>
      ))}
    </div>
  );
}

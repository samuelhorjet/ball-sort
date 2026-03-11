import PuzzleDemo from "./PuzzleDemo";

const STEPS = [
  {
    num: "01",
    title: "Connect Your Wallet",
    desc: "Link your Solana wallet and create your player account. No email, no passwords — your identity lives on-chain.",
    icon: "👛",
    color: "#ff2d8b",
  },
  {
    num: "02",
    title: "Start a Puzzle",
    desc: "Pick your difficulty. VRF generates a provably random board just for you — nobody can predict or manipulate it.",
    icon: "🎲",
    color: "#ffd93d",
  },
  {
    num: "03",
    title: "Sort the Balls",
    desc: "Move balls between tubes until every tube holds one color. Each move is recorded on-chain via MagicBlock rollups.",
    icon: "🔮",
    color: "#4d9fff",
  },
  {
    num: "04",
    title: "Earn & Compete",
    desc: "Finish puzzles to earn scores. Join tournaments, climb the leaderboard, and claim real SOL from the prize pool.",
    icon: "🏆",
    color: "#3dd9a0",
  },
];

export default function HowItWorks() {
  return (
    <section
      id="how-it-works"
      style={{
        padding: "6rem 1.5rem",
        position: "relative",
        overflow: "hidden",
        background:
          "radial-gradient(ellipse 80% 60% at 50% 50%, rgba(255,45,139,0.03) 0%, transparent 70%)",
      }}
    >
      <div style={{ maxWidth: "1100px", margin: "0 auto" }}>
        {/* Section header */}
        <div style={{ textAlign: "center", marginBottom: "4rem" }}>
          <div
            style={{
              display: "inline-block",
              background: "rgba(255,45,139,0.08)",
              border: "1px solid rgba(255,45,139,0.2)",
              borderRadius: "2rem",
              padding: "0.3rem 0.9rem",
              marginBottom: "1rem",
            }}
          >
            <span
              style={{
                fontFamily: "var(--font-syne)",
                fontSize: "0.72rem",
                fontWeight: 600,
                color: "#ff79c6",
                textTransform: "uppercase",
                letterSpacing: "0.1em",
              }}
            >
              How It Works
            </span>
          </div>
          <h2
            style={{
              fontFamily: "var(--font-syne)",
              fontSize: "clamp(2rem, 5vw, 3rem)",
              fontWeight: 800,
              color: "#f0f0fa",
              letterSpacing: "-0.03em",
              lineHeight: 1.1,
            }}
          >
            Get started in{" "}
            <span className="gradient-text">minutes</span>
          </h2>
          <p
            style={{
              fontFamily: "var(--font-outfit)",
              color: "#666688",
              fontSize: "0.95rem",
              maxWidth: "440px",
              margin: "0.75rem auto 0",
              lineHeight: 1.6,
            }}
          >
            Watch the AI solve a live puzzle below — this is exactly what you&apos;ll
            experience, in real-time, on-chain.
          </p>
        </div>

        {/* Two-column: steps left, demo right */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr",
            gap: "4rem",
            alignItems: "center",
          }}
          className="hiw-grid"
        >
          {/* Steps */}
          <div style={{ display: "flex", flexDirection: "column", gap: "1.75rem", overflow: "hidden" }}>
            {STEPS.map((step, idx) => (
              <div
                key={step.num}
                style={{
                  display: "flex",
                  gap: "1rem",
                  alignItems: "flex-start",
                  position: "relative",
                }}
              >
                {/* Vertical connector */}
                {idx < STEPS.length - 1 && (
                  <div
                    style={{
                      position: "absolute",
                      left: "22px",
                      top: "48px",
                      width: "1px",
                      height: "calc(100% + 0.75rem)",
                      background: "linear-gradient(to bottom, #1c1c38, transparent)",
                    }}
                  />
                )}

                {/* Icon bubble */}
                <div
                  style={{
                    width: "46px",
                    height: "46px",
                    borderRadius: "50%",
                    background: `radial-gradient(circle at 40% 35%, ${step.color}28, ${step.color}10)`,
                    border: `2px solid ${step.color}30`,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontSize: "1.25rem",
                    flexShrink: 0,
                    boxShadow: `0 0 16px ${step.color}20`,
                    zIndex: 1,
                    position: "relative",
                  }}
                >
                  {step.icon}
                </div>

                {/* Text */}
                <div style={{ paddingTop: "0.1rem", minWidth: 0, flex: 1 }}>
                  <div
                    style={{
                      fontFamily: "var(--font-syne)",
                      fontSize: "0.65rem",
                      fontWeight: 700,
                      color: step.color,
                      textTransform: "uppercase",
                      letterSpacing: "0.12em",
                      marginBottom: "0.25rem",
                      opacity: 0.8,
                    }}
                  >
                    Step {step.num}
                  </div>
                  <h3
                    style={{
                      fontFamily: "var(--font-syne)",
                      fontSize: "1rem",
                      fontWeight: 700,
                      color: "#f0f0fa",
                      letterSpacing: "-0.01em",
                      marginBottom: "0.35rem",
                    }}
                  >
                    {step.title}
                  </h3>
                  <p
                    style={{
                      fontFamily: "var(--font-outfit)",
                      fontSize: "0.85rem",
                      color: "#666688",
                      lineHeight: 1.6,
                      wordBreak: "break-word" as const,
                      overflowWrap: "break-word" as const,
                    }}
                  >
                    {step.desc}
                  </p>
                </div>
              </div>
            ))}

            {/* CTA */}
            <a
              href="#"
              className="btn-primary"
              style={{ marginTop: "0.5rem", alignSelf: "center", fontSize: "0.9rem" }}
            >
              Start Playing — It&apos;s Free ↗
            </a>
          </div>

          {/* Live puzzle demo */}
          <div
            className="hiw-demo-col"
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              gap: "1rem",
              overflow: "hidden",
            }}
          >
            {/* Demo label */}
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "0.6rem",
                padding: "0.5rem 1rem",
                background: "#0d0d1e",
                border: "1px solid #1c1c38",
                borderRadius: "0.875rem",
                width: "100%",
                maxWidth: "380px",
              }}
            >
              <div
                style={{
                  width: "8px",
                  height: "8px",
                  borderRadius: "50%",
                  background: "#ff2d8b",
                  boxShadow: "0 0 8px rgba(255,45,139,0.6)",
                  animation: "pulseGlow 2s ease-in-out infinite",
                  flexShrink: 0,
                }}
              />
              <span
                style={{
                  fontFamily: "var(--font-syne)",
                  fontSize: "0.72rem",
                  fontWeight: 700,
                  color: "#f0f0fa",
                  letterSpacing: "0.02em",
                }}
              >
                Live Demo — Puzzle being Solved in Real Time
              </span>
            </div>

            {/* The actual animated puzzle */}
            <div
              style={{
                background: "#09091a",
                border: "1px solid #1c1c38",
                borderRadius: "1.5rem",
                padding: "1.5rem",
                width: "100%",
                maxWidth: "380px",
              }}
            >
              <PuzzleDemo />
            </div>

            {/* Caption */}
            <p
              style={{
                fontFamily: "var(--font-outfit)",
                fontSize: "0.75rem",
                color: "#444466",
                textAlign: "center",
                maxWidth: "300px",
                lineHeight: 1.5,
              }}
            >
              4 colors × 4 balls. 6 tubes. 20 optimal moves.
              Watch the cursor solve it — then it resets and goes again.
            </p>
          </div>
        </div>
      </div>

      <style>{`
        @media (max-width: 768px) {
          .hiw-grid {
            grid-template-columns: 1fr !important;
            gap: 3rem !important;
          }
          .hiw-demo-col > div {
            max-width: 100% !important;
          }
        }
        @media (max-width: 400px) {
          .hiw-grid {
            gap: 2rem !important;
          }
        }
      `}</style>
    </section>
  );
}

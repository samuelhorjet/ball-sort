"use client";

import DashboardSidebar from "@/components/Sidebar";
import StatsGrid from "@/components/StatsGrid";
import RecentGames from "@/components/RecentGames";
import ActiveTournaments from "@/components/ActiveTournaments";

const BALL_COLORS = ["#ff2d8b", "#4d9fff", "#3dd9a0", "#ffd93d", "#b44dff", "#ff6b6b"];

export default function DashboardPage() {
  return (
    <div style={{ display: "flex", minHeight: "100vh", background: "#07070f" }}>
      <DashboardSidebar />
      <main style={{ flex: 1, overflow: "auto", minWidth: 0 }}>
        {/* Top bar */}
        <div
          style={{
            position: "sticky", top: 0, zIndex: 10,
            background: "rgba(7,7,15,0.9)", backdropFilter: "blur(16px)",
            borderBottom: "1px solid #1c1c38", padding: "0 2rem",
            height: "64px", display: "flex", alignItems: "center", justifyContent: "space-between",
          }}
        >
          <h1 style={{ fontFamily: "var(--font-syne)", fontSize: "1.1rem", fontWeight: 700, color: "#f0f0fa", letterSpacing: "-0.02em" }}>
            Overview
          </h1>
          <a href="#" className="btn-primary" style={{ padding: "0.5rem 1rem", fontSize: "0.825rem", animation: "none" }}>
            + New Puzzle
          </a>
        </div>

        <div style={{ padding: "2rem" }}>
          {/* Welcome banner */}
          <div
            style={{
              background: "linear-gradient(135deg, rgba(255,45,139,0.1), rgba(180,77,255,0.06))",
              border: "1px solid rgba(255,45,139,0.18)", borderRadius: "1.25rem",
              padding: "1.5rem 2rem", marginBottom: "1.5rem",
              position: "relative", overflow: "hidden",
              display: "flex", justifyContent: "space-between", alignItems: "center",
              gap: "1rem", flexWrap: "wrap",
            }}
          >
            {BALL_COLORS.map((color, i) => (
              <div
                key={i}
                style={{
                  position: "absolute", right: `${8 + i * 28}px`, top: "50%",
                  transform: "translateY(-50%)", width: "36px", height: "36px",
                  borderRadius: "50%",
                  background: `radial-gradient(circle at 35% 35%, ${color}cc, ${color}66)`,
                  boxShadow: `0 0 12px ${color}44`, opacity: 0.6, pointerEvents: "none",
                }}
              />
            ))}
            <div style={{ position: "relative", zIndex: 1 }}>
              <div style={{ fontFamily: "var(--font-outfit)", fontSize: "0.8rem", color: "#ff79c6", fontWeight: 500, marginBottom: "0.25rem" }}>
                Welcome back, Player 👋
              </div>
              <h2 style={{ fontFamily: "var(--font-syne)", fontSize: "1.3rem", fontWeight: 800, color: "#f0f0fa", letterSpacing: "-0.02em" }}>
                You&apos;re on a <span className="gradient-text">5-day streak!</span>
              </h2>
              <p style={{ fontFamily: "var(--font-outfit)", fontSize: "0.825rem", color: "#666688", marginTop: "0.3rem" }}>
                Solve a puzzle today to keep your streak alive.
              </p>
            </div>
            <a href="#" className="btn-primary" style={{ position: "relative", zIndex: 1, animation: "none", flexShrink: 0 }}>
              Play Now ↗
            </a>
          </div>

          <StatsGrid />

          <div style={{ display: "grid", gridTemplateColumns: "1fr 360px", gap: "1.25rem", alignItems: "start" }}>
            <RecentGames />
            <ActiveTournaments />
          </div>
        </div>
      </main>

      <style>{`
        @media (max-width: 1024px) {
          .dash-grid { grid-template-columns: 1fr !important; }
        }
        @media (max-width: 768px) {
          aside { display: none !important; }
        }
      `}</style>
    </div>
  );
}

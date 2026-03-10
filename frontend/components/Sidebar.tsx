"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const NAV_ITEMS = [
  { icon: "◈", label: "Overview", href: "/dashboard" },
  { icon: "🔮", label: "My Puzzles", href: "/dashboard/puzzles" },
  { icon: "🏆", label: "Tournaments", href: "/dashboard/tournaments" },
  { icon: "📊", label: "Stats", href: "/dashboard/stats" },
  { icon: "⚙️", label: "Settings", href: "/dashboard/settings" },
];

const BALL_COLORS = ["#ff2d8b", "#4d9fff", "#3dd9a0", "#ffd93d", "#b44dff"];

export default function DashboardSidebar() {
  const pathname = usePathname();

  return (
    <aside
      style={{
        width: "240px",
        minHeight: "100vh",
        background: "#0a0a18",
        borderRight: "1px solid #1c1c38",
        display: "flex",
        flexDirection: "column",
        padding: "1.5rem 1rem",
        position: "sticky",
        top: 0,
        flexShrink: 0,
      }}
    >
      {/* Logo */}
      <Link
        href="/"
        style={{ textDecoration: "none", display: "flex", alignItems: "center", gap: "0.6rem", marginBottom: "2rem", padding: "0 0.5rem" }}
      >
        <div
          style={{
            width: "32px",
            height: "32px",
            borderRadius: "9px",
            background: "linear-gradient(135deg, #ff2d8b, #b44dff)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            fontSize: "14px",
            boxShadow: "0 0 12px rgba(255,45,139,0.35)",
            flexShrink: 0,
          }}
        >
          🔮
        </div>
        <span
          style={{
            fontFamily: "var(--font-syne)",
            fontWeight: 800,
            fontSize: "1.1rem",
            color: "#f0f0fa",
            letterSpacing: "-0.02em",
          }}
        >
          Ball<span style={{ color: "#ff2d8b" }}>Sort</span>
        </span>
      </Link>

      {/* Wallet badge */}
      <div
        style={{
          background: "#12122a",
          border: "1px solid #1c1c38",
          borderRadius: "0.875rem",
          padding: "0.75rem",
          marginBottom: "1.75rem",
          display: "flex",
          alignItems: "center",
          gap: "0.6rem",
        }}
      >
        <div
          style={{
            width: "32px",
            height: "32px",
            borderRadius: "8px",
            background: "linear-gradient(135deg, rgba(255,45,139,0.3), rgba(180,77,255,0.3))",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            fontSize: "14px",
            flexShrink: 0,
          }}
        >
          👛
        </div>
        <div style={{ overflow: "hidden" }}>
          <div style={{ fontFamily: "var(--font-syne)", fontSize: "0.7rem", fontWeight: 600, color: "#ff79c6", marginBottom: "0.1rem" }}>
            Connected
          </div>
          <div
            style={{
              fontFamily: "var(--font-outfit)",
              fontSize: "0.7rem",
              color: "#555577",
              overflow: "hidden",
              textOverflow: "ellipsis",
              whiteSpace: "nowrap",
            }}
          >
            7xKp...m3Rq
          </div>
        </div>
      </div>

      {/* Nav */}
      <nav style={{ display: "flex", flexDirection: "column", gap: "0.25rem", flex: 1 }}>
        <div
          style={{
            fontFamily: "var(--font-syne)",
            fontSize: "0.65rem",
            fontWeight: 700,
            color: "#333355",
            textTransform: "uppercase",
            letterSpacing: "0.12em",
            padding: "0 0.5rem",
            marginBottom: "0.5rem",
          }}
        >
          Menu
        </div>

        {NAV_ITEMS.map((item) => {
          const active = pathname === item.href;
          return (
            <Link
              key={item.label}
              href={item.href}
              style={{
                display: "flex",
                alignItems: "center",
                gap: "0.75rem",
                padding: "0.65rem 0.75rem",
                borderRadius: "0.75rem",
                textDecoration: "none",
                transition: "all 0.2s",
                background: active ? "rgba(255,45,139,0.1)" : "transparent",
                border: `1px solid ${active ? "rgba(255,45,139,0.2)" : "transparent"}`,
                position: "relative",
              }}
              onMouseEnter={(e) => {
                if (!active) {
                  e.currentTarget.style.background = "rgba(255,255,255,0.03)";
                }
              }}
              onMouseLeave={(e) => {
                if (!active) {
                  e.currentTarget.style.background = "transparent";
                }
              }}
            >
              {active && (
                <div
                  style={{
                    position: "absolute",
                    left: 0,
                    top: "50%",
                    transform: "translateY(-50%)",
                    width: "3px",
                    height: "60%",
                    background: "#ff2d8b",
                    borderRadius: "0 2px 2px 0",
                  }}
                />
              )}
              <span style={{ fontSize: "0.95rem" }}>{item.icon}</span>
              <span
                style={{
                  fontFamily: "var(--font-outfit)",
                  fontSize: "0.875rem",
                  fontWeight: active ? 600 : 400,
                  color: active ? "#ff79c6" : "#666688",
                }}
              >
                {item.label}
              </span>
            </Link>
          );
        })}
      </nav>

      {/* Bottom — ball collection */}
      <div
        style={{
          background: "#12122a",
          border: "1px solid #1c1c38",
          borderRadius: "0.875rem",
          padding: "1rem",
          marginTop: "auto",
        }}
      >
        <div
          style={{
            fontFamily: "var(--font-syne)",
            fontSize: "0.7rem",
            fontWeight: 600,
            color: "#555577",
            marginBottom: "0.75rem",
            textTransform: "uppercase",
            letterSpacing: "0.08em",
          }}
        >
          Ball Collection
        </div>
        <div style={{ display: "flex", gap: "0.4rem", flexWrap: "wrap" }}>
          {BALL_COLORS.map((color, i) => (
            <div
              key={i}
              style={{
                width: "22px",
                height: "22px",
                borderRadius: "50%",
                background: `radial-gradient(circle at 35% 35%, ${color}dd, ${color}77)`,
                boxShadow: `0 0 8px ${color}44`,
              }}
            />
          ))}
          <div
            style={{
              width: "22px",
              height: "22px",
              borderRadius: "50%",
              background: "#1c1c38",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              fontSize: "0.6rem",
              color: "#555577",
            }}
          >
            +3
          </div>
        </div>
      </div>
    </aside>
  );
}

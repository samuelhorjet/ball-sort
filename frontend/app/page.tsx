"use client";

import Link from "next/link";
import { useState } from "react";

const FEATURES = [
  {
    icon: "⛓️",
    title: "Fully On-Chain",
    desc: "Every move, every result — recorded on Solana. No hidden servers, no trust required. Your puzzle history lives forever on the blockchain.",
    accent: "#ff2d8b",
    tag: "Solana",
  },
  {
    icon: "⚡",
    title: "Lightning Fast",
    desc: "MagicBlock ephemeral rollups let you make moves in under a second. Real-time gameplay with on-chain security — the best of both worlds.",
    accent: "#ffd93d",
    tag: "MagicBlock ER",
  },
  {
    icon: "🏆",
    title: "Win Real Prizes",
    desc: "Enter tournaments, solve puzzles faster than everyone else, and claim your share of the prize pool via a provably fair parimutuel system.",
    accent: "#3dd9a0",
    tag: "Tournaments",
  },
  {
    icon: "🎲",
    title: "Provably Fair",
    desc: "Every puzzle is generated using Verifiable Random Functions (VRF). Nobody — not even us — can predict or manipulate your puzzle layout.",
    accent: "#b44dff",
    tag: "VRF",
  },
];

const BALLS = [
  {
    color: "#ff2d8b",
    size: 72,
    top: "12%",
    left: "58%",
    anim: "floatA",
    dur: "9s",
    delay: "0s",
    blur: 0,
  },
  {
    color: "#4d9fff",
    size: 52,
    top: "28%",
    left: "72%",
    anim: "floatB",
    dur: "13s",
    delay: "1s",
    blur: 0,
  },
  {
    color: "#3dd9a0",
    size: 44,
    top: "55%",
    left: "64%",
    anim: "floatC",
    dur: "11s",
    delay: "2s",
    blur: 0,
  },
  {
    color: "#ffd93d",
    size: 60,
    top: "70%",
    left: "78%",
    anim: "floatA",
    dur: "15s",
    delay: "0.5s",
    blur: 0,
  },
  {
    color: "#b44dff",
    size: 38,
    top: "18%",
    left: "82%",
    anim: "floatD",
    dur: "10s",
    delay: "3s",
    blur: 0,
  },
  {
    color: "#ff6b6b",
    size: 56,
    top: "40%",
    left: "88%",
    anim: "floatB",
    dur: "12s",
    delay: "1.5s",
    blur: 0,
  },
  {
    color: "#2dd4bf",
    size: 32,
    top: "80%",
    left: "58%",
    anim: "floatC",
    dur: "14s",
    delay: "4s",
    blur: 0,
  },
  {
    color: "#ff9f43",
    size: 46,
    top: "8%",
    left: "68%",
    anim: "floatD",
    dur: "16s",
    delay: "2.5s",
    blur: 0,
  },
  {
    color: "#ff2d8b",
    size: 180,
    top: "5%",
    left: "55%",
    anim: "floatA",
    dur: "20s",
    delay: "0s",
    blur: 60,
  },
  {
    color: "#b44dff",
    size: 140,
    top: "60%",
    left: "75%",
    anim: "floatB",
    dur: "18s",
    delay: "2s",
    blur: 50,
  },
];

const STATS = [
  { value: "10K+", label: "Puzzles Solved" },
  { value: "500+", label: "Active Players" },
  { value: "2.4 SOL", label: "Prize Pool" },
  { value: "<1s", label: "Move Latency" },
];

const STEPS = [
  {
    num: "01",
    title: "Connect Your Wallet",
    desc: "Link your Solana wallet and create your player account. Your identity lives on-chain — no email, no passwords.",
    icon: "👛",
    color: "#ff2d8b",
  },
  {
    num: "02",
    title: "Start a Puzzle",
    desc: "Choose your difficulty and let VRF generate a provably random puzzle just for you. Easy, medium, or hard — your call.",
    icon: "🎲",
    color: "#ffd93d",
  },
  {
    num: "03",
    title: "Sort the Balls",
    desc: "Move balls between tubes to get each tube filled with a single color. Think ahead — every move counts toward your score.",
    icon: "🔮",
    color: "#4d9fff",
  },
  {
    num: "04",
    title: "Earn & Compete",
    desc: "Complete puzzles to earn scores. Join tournaments, beat the leaderboard, and claim real SOL prizes from the prize pool.",
    icon: "🏆",
    color: "#3dd9a0",
  },
];

const MOCK_TOURNAMENTS = [
  {
    id: "T-042",
    name: "Weekend Blitz",
    prizePool: "4.2 SOL",
    entryFee: "0.1 SOL",
    difficulty: "Hard",
    diffColor: "#ff6b6b",
    players: 38,
    maxPlayers: 50,
    endsIn: "14h 22m",
    status: "open",
  },
  {
    id: "T-041",
    name: "Speed Run Classic",
    prizePool: "2.0 SOL",
    entryFee: "0.05 SOL",
    difficulty: "Medium",
    diffColor: "#ffd93d",
    players: 24,
    maxPlayers: 30,
    endsIn: "2d 6h",
    status: "open",
  },
  {
    id: "T-040",
    name: "Beginner's Cup",
    prizePool: "0.8 SOL",
    entryFee: "0.02 SOL",
    difficulty: "Easy",
    diffColor: "#3dd9a0",
    players: 12,
    maxPlayers: 20,
    endsIn: "5d 0h",
    status: "open",
  },
];

const LINKS = {
  Game: [
    { label: "Play Now", href: "#" },
    { label: "Tournaments", href: "#tournaments" },
    { label: "Leaderboard", href: "#" },
    { label: "How It Works", href: "#how-it-works" },
  ],
  Resources: [
    { label: "Documentation", href: "#" },
    { label: "GitHub", href: "#" },
    { label: "Audit Report", href: "#" },
    { label: "FAQ", href: "#" },
  ],
  Account: [
    { label: "My Puzzles", href: "/dashboard" },
    { label: "Wallet Settings", href: "/dashboard" },
  ],
};

const BALL_COLORS = [
  "#ff2d8b",
  "#4d9fff",
  "#3dd9a0",
  "#ffd93d",
  "#b44dff",
  "#ff6b6b",
];

/* ─── Navbar ──────────────────────────────────────────── */
function Navbar() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [walletConnected] = useState(false); // swap to true once wallet is linked

  const NAV_ITEMS = [
    { label: "Features", href: "#features" },
    { label: "How It Works", href: "#how-it-works" },
    { label: "Tournaments", href: "#tournaments" },
    // Dashboard only appears after wallet is connected:
    ...(walletConnected ? [{ label: "Dashboard", href: "/dashboard" }] : []),
  ];

  return (
    <nav
      style={{
        position: "fixed",
        top: 0,
        left: 0,
        right: 0,
        zIndex: 100,
        padding: "0 24px",
        height: "68px",
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        background: "rgba(7,7,26,0.85)",
        backdropFilter: "blur(20px)",
        WebkitBackdropFilter: "blur(20px)",
        borderBottom: "1px solid rgba(255,255,255,0.07)",
      }}
    >
      {/* Logo */}
      <Link
        href="/"
        style={{
          display: "flex",
          alignItems: "center",
          gap: "10px",
          textDecoration: "none",
        }}
      >
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(3, 1fr)",
            gridTemplateRows: "repeat(3, 1fr)",
            gap: "2px",
            alignItems: "center",
            justifyItems: "center",
            width: "45px",
            height: "45px",
            position: "relative",
          }}
        >
          {/* The 4 Outer Points */}
          {BALL_COLORS.slice(0, 4).map((c, i) => {
            const positions = [
              { gridArea: "1 / 2" }, // Top (Row 1, Col 2)
              { gridArea: "2 / 1" }, // Left (Row 2, Col 1)
              { gridArea: "2 / 3" }, // Right (Row 2, Col 3)
              { gridArea: "3 / 2" }, // Bottom (Row 3, Col 2)
            ];

            return (
              <div
                key={i}
                style={{
                  ...positions[i],
                  width: "11px",
                  height: "11px",
                  borderRadius: "50%",
                  background: `radial-gradient(circle at 35% 35%, ${c}ee, ${c}88)`,
                  boxShadow: `0 0 8px ${c}aa`,
                }}
              />
            );
          })}

          {/* The Center Core (Using index 4 or a bright white/yellow) */}
          <div
            style={{
              gridArea: "2 / 2", // Dead Center
              width: "16px",
              height: "16px",
              borderRadius: "50%",
              background: `radial-gradient(circle at 35% 35%, #ffffff, ${
                BALL_COLORS[4] || "#fff"
              }dd)`,
              boxShadow: `0 0 12px white`,
              zIndex: 2,
            }}
          />
        </div>
        <span
          style={{
            fontFamily: "var(--font-syne)",
            fontWeight: 800,
            fontSize: "1.3rem",
            color: "#f0f0fa",
          }}
        >
          Ball<span style={{ color: "#ff2d8b" }}>Sort</span>
        </span>
      </Link>

      {/* Desktop links */}
      <div
        style={{ display: "flex", alignItems: "center", gap: "28px" }}
        className="nav-links"
      >
        {NAV_ITEMS.map((item) => (
          <Link
            key={item.label}
            href={item.href}
            style={{
              color: "#666688",
              textDecoration: "none",
              fontWeight: 500,
              fontSize: "0.93rem",
              transition: "color 0.2s",
            }}
            onMouseEnter={(e) => (e.currentTarget.style.color = "#f0f0fa")}
            onMouseLeave={(e) => (e.currentTarget.style.color = "#666688")}
          >
            {item.label}
          </Link>
        ))}
        <button
          className="btn-primary"
          style={{ padding: "9px 22px", fontSize: "0.88rem" }}
        >
          Connect Wallet
        </button>
      </div>

      {/* Hamburger */}
      <button
        onClick={() => setMenuOpen(!menuOpen)}
        style={{
          background: "none",
          border: "none",
          cursor: "pointer",
          flexDirection: "column",
          gap: "5px",
          padding: "4px",
          display: "none",
        }}
        className="hamburger"
        aria-label="Toggle menu"
      >
        {[0, 1, 2].map((i) => (
          <span
            key={i}
            style={{
              display: "block",
              width: "24px",
              height: "2px",
              background: "#f0f0fa",
              borderRadius: "2px",
              transition: "all 0.3s",
              transform:
                menuOpen && i === 0
                  ? "rotate(45deg) translate(5px,5px)"
                  : menuOpen && i === 2
                  ? "rotate(-45deg) translate(5px,-5px)"
                  : menuOpen && i === 1
                  ? "scaleX(0)"
                  : "none",
            }}
          />
        ))}
      </button>

      {/* Mobile menu */}
      {menuOpen && (
        <div
          style={{
            position: "absolute",
            top: "68px",
            left: 0,
            right: 0,
            background: "rgba(7,7,26,0.98)",
            backdropFilter: "blur(20px)",
            padding: "20px 24px",
            display: "flex",
            flexDirection: "column",
            gap: "16px",
            borderBottom: "1px solid #1c1c38",
          }}
        >
          {NAV_ITEMS.map((item) => (
            <Link
              key={item.label}
              href={item.href}
              onClick={() => setMenuOpen(false)}
              style={{
                color: "#f0f0fa",
                textDecoration: "none",
                fontWeight: 500,
                fontSize: "1rem",
                padding: "8px 0",
                borderBottom: "1px solid #1c1c38",
              }}
            >
              {item.label}
            </Link>
          ))}
          <button className="btn-primary" style={{ marginTop: "8px" }}>
            Connect Wallet
          </button>
        </div>
      )}

      <style>{`
        @media (max-width: 768px) {
          .nav-links { display: none !important; }
          .hamburger { display: flex !important; }
        }
      `}</style>
    </nav>
  );
}

/* ─── Hero ────────────────────────────────────────────── */
function Hero() {
  return (
    <section
      style={{
        position: "relative",
        minHeight: "100vh",
        paddingTop: "68px",
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        overflow: "hidden",
      }}
    >
      {/* Radial gradient background */}
      <div
        style={{
          position: "absolute",
          inset: 0,
          background:
            "radial-gradient(ellipse 80% 60% at 70% 50%, rgba(255,45,139,0.08) 0%, transparent 70%), radial-gradient(ellipse 60% 50% at 85% 80%, rgba(180,77,255,0.06) 0%, transparent 60%)",
          pointerEvents: "none",
        }}
      />

      {/* Grid pattern */}
      <div
        style={{
          position: "absolute",
          inset: 0,
          backgroundImage:
            "linear-gradient(rgba(28,28,56,0.3) 1px, transparent 1px), linear-gradient(90deg, rgba(28,28,56,0.3) 1px, transparent 1px)",
          backgroundSize: "60px 60px",
          pointerEvents: "none",
          maskImage:
            "radial-gradient(ellipse 90% 90% at 50% 50%, black 40%, transparent 100%)",
        }}
      />

      {/* Floating balls — right side of viewport */}
      {BALLS.map((ball, i) => (
        <div
          key={i}
          style={{
            position: "absolute",
            top: ball.top,
            left: ball.left,
            width: ball.size,
            height: ball.size,
            borderRadius: "50%",
            background: `radial-gradient(circle at 35% 35%, ${ball.color}ee, ${ball.color}88)`,
            boxShadow: `0 0 ${ball.size * 0.4}px ${
              ball.color
            }66, inset 0 -4px 8px rgba(0,0,0,0.3)`,
            filter: ball.blur ? `blur(${ball.blur}px)` : undefined,
            opacity: ball.blur ? 0.3 : 0.9,
            animation: `${ball.anim} ${ball.dur} ease-in-out ${ball.delay} infinite`,
            pointerEvents: "none",
            zIndex: ball.blur ? 0 : 1,
          }}
        />
      ))}

      {/* ── Two-column hero layout ── */}
      <div
        className="hero-layout"
        style={{
          position: "relative",
          zIndex: 2,
          maxWidth: "1100px",
          width: "100%",
          margin: "0 auto",
          padding: "4rem 2rem 2rem",
          display: "grid",
          gridTemplateColumns: "1fr 1fr",
          alignItems: "center",
          gap: "2rem",
          minHeight: "calc(100vh - 68px)",
        }}
      >
        {/* LEFT — text content */}
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "flex-start",
          }}
        >
          {/* Badge */}
          <div
            style={{
              display: "inline-flex",
              alignItems: "center",
              gap: "0.5rem",
              background: "rgba(255,45,139,0.1)",
              border: "1px solid rgba(255,45,139,0.25)",
              borderRadius: "2rem",
              padding: "0.35rem 0.9rem",
              marginBottom: "1.75rem",
              animation: "fadeIn 0.6s ease",
            }}
          >
            <span
              style={{
                fontSize: "0.75rem",
                color: "#ff79c6",
                fontFamily: "var(--font-syne)",
                fontWeight: 600,
                letterSpacing: "0.08em",
                textTransform: "uppercase",
              }}
            >
              ⚡ Built on Solana × MagicBlock
            </span>
          </div>

          {/* Headline */}
          <h1
            style={{
              fontFamily: "var(--font-syne)",
              fontSize: "clamp(3rem, 7vw, 5.5rem)",
              fontWeight: 800,
              lineHeight: 1.0,
              letterSpacing: "-0.04em",
              marginBottom: "1.5rem",
              animation: "fadeInUp 0.7s ease 0.1s both",
            }}
          >
            <span style={{ color: "#f0f0fa", display: "block" }}>Sort.</span>
            <span style={{ color: "#f0f0fa", display: "block" }}>Solve.</span>
            <span className="gradient-text" style={{ display: "block" }}>
              Win.
            </span>
          </h1>

          {/* Subtitle */}
          <p
            style={{
              fontFamily: "var(--font-outfit)",
              fontSize: "clamp(1rem, 2vw, 1.15rem)",
              fontWeight: 400,
              color: "#9999bb",
              lineHeight: 1.7,
              maxWidth: "460px",
              marginBottom: "2.5rem",
              animation: "fadeInUp 0.7s ease 0.2s both",
            }}
          >
            The addictive color-sorting puzzle game — fully on-chain, lightning
            fast with ephemeral rollups, and loaded with real prize pools.
          </p>

          {/* CTA Buttons */}
          <div
            style={{
              display: "flex",
              gap: "1rem",
              flexWrap: "wrap",
              animation: "fadeInUp 0.7s ease 0.3s both",
            }}
          >
            <a
              href="#"
              className="btn-primary"
              style={{ fontSize: "1rem", padding: "0.85rem 2rem" }}
            >
              Play Now ↗
            </a>
            <Link
              href="/dashboard"
              className="btn-secondary"
              style={{ fontSize: "1rem", padding: "0.85rem 2rem" }}
            >
              My Dashboard
            </Link>
          </div>

          {/* Trust badges */}
          <div
            style={{
              display: "flex",
              gap: "1.5rem",
              marginTop: "2rem",
              flexWrap: "wrap",
              animation: "fadeIn 0.7s ease 0.5s both",
            }}
          >
            {["On-Chain Verified", "VRF Randomness", "Open Source"].map(
              (badge) => (
                <div
                  key={badge}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "0.35rem",
                  }}
                >
                  <span style={{ color: "#3dd9a0", fontSize: "0.8rem" }}>
                    ✓
                  </span>
                  <span
                    style={{
                      fontFamily: "var(--font-outfit)",
                      fontSize: "0.8rem",
                      color: "#666688",
                      fontWeight: 500,
                    }}
                  >
                    {badge}
                  </span>
                </div>
              ),
            )}
          </div>

          {/* Stats bar — under text content */}
          <div
            style={{
              marginTop: "3rem",
              display: "grid",
              gridTemplateColumns: "repeat(4, 1fr)",
              gap: "1px",
              background: "#1c1c38",
              borderRadius: "1rem",
              overflow: "hidden",
              border: "1px solid #1c1c38",
              width: "100%",
              maxWidth: "520px",
              animation: "fadeInUp 0.7s ease 0.5s both",
            }}
          >
            {STATS.map((s) => (
              <div
                key={s.label}
                style={{
                  background: "#0d0d1e",
                  padding: "1.1rem 0.75rem",
                  textAlign: "center",
                }}
              >
                <div
                  style={{
                    fontFamily: "var(--font-syne)",
                    fontSize: "1.4rem",
                    fontWeight: 800,
                    color: "#ff2d8b",
                    marginBottom: "0.15rem",
                  }}
                >
                  {s.value}
                </div>
                <div
                  style={{
                    fontFamily: "var(--font-outfit)",
                    fontSize: "0.7rem",
                    color: "#666688",
                    fontWeight: 500,
                  }}
                >
                  {s.label}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* RIGHT — empty; balls float here via absolute positioning */}
        <div aria-hidden="true" />
      </div>

      {/* Scroll indicator */}
      <div
        style={{
          position: "absolute",
          bottom: "2rem",
          left: "50%",
          transform: "translateX(-50%)",
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          gap: "0.4rem",
          opacity: 0.4,
          animation: "fadeIn 1s ease 1s both",
        }}
      >
        <span
          style={{
            fontSize: "0.7rem",
            fontFamily: "var(--font-outfit)",
            color: "#666688",
            textTransform: "uppercase",
            letterSpacing: "0.1em",
          }}
        >
          scroll
        </span>
        <div
          style={{
            width: "1px",
            height: "40px",
            background: "linear-gradient(to bottom, #ff2d8b, transparent)",
          }}
        />
      </div>

      {/* Responsive: stack on mobile */}
      <style>{`
        @media (max-width: 768px) {
          .hero-layout {
            grid-template-columns: 1fr !important;
            min-height: auto !important;
            padding-top: 3rem !important;
            padding-bottom: 4rem !important;
          }
          .hero-layout > div:last-child { display: none !important; }
        }
      `}</style>
    </section>
  );
}

/* ─── Features ────────────────────────────────────────── */
function Features() {
  return (
    <section
      id="features"
      style={{ padding: "6rem 1.5rem", maxWidth: "1200px", margin: "0 auto" }}
    >
      {/* Section header */}
      <div style={{ textAlign: "center", marginBottom: "3.5rem" }}>
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
            Why Ball Sort?
          </span>
        </div>
        <h2
          style={{
            fontFamily: "var(--font-syne)",
            fontSize: "clamp(2rem, 5vw, 3rem)",
            fontWeight: 800,
            color: "#f0f0fa",
            letterSpacing: "-0.03em",
            marginBottom: "0.75rem",
            lineHeight: 1.1,
          }}
        >
          Built different.{" "}
          <span className="gradient-text">Plays different.</span>
        </h2>
        <p
          style={{
            fontFamily: "var(--font-outfit)",
            color: "#666688",
            fontSize: "1rem",
            maxWidth: "480px",
            margin: "0 auto",
            lineHeight: 1.6,
          }}
        >
          Not just another puzzle game. Ball Sort is the first color-sorting
          puzzle with fully verifiable on-chain gameplay.
        </p>
      </div>

      {/* 4-column features grid — all boxes in one row on desktop */}
      <div
        className="features-grid"
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(4, 1fr)",
          gap: "1.25rem",
        }}
      >
        {FEATURES.map((f) => (
          <div
            key={f.title}
            className="card-glow"
            style={{
              background: "#0d0d1e",
              border: "1px solid #1c1c38",
              borderRadius: "1.25rem",
              padding: "1.75rem",
              position: "relative",
              overflow: "hidden",
            }}
          >
            {/* Top accent line */}
            <div
              style={{
                position: "absolute",
                top: 0,
                left: "1.75rem",
                right: "1.75rem",
                height: "2px",
                background: `linear-gradient(90deg, ${f.accent}88, transparent)`,
              }}
            />

            {/* Tag */}
            <div
              style={{
                display: "inline-block",
                background: `${f.accent}15`,
                border: `1px solid ${f.accent}30`,
                borderRadius: "0.5rem",
                padding: "0.2rem 0.6rem",
                marginBottom: "1.25rem",
              }}
            >
              <span
                style={{
                  fontFamily: "var(--font-syne)",
                  fontSize: "0.7rem",
                  fontWeight: 600,
                  color: f.accent,
                  textTransform: "uppercase",
                  letterSpacing: "0.08em",
                }}
              >
                {f.tag}
              </span>
            </div>

            {/* Icon */}
            <div
              style={{
                width: "52px",
                height: "52px",
                borderRadius: "14px",
                background: `${f.accent}18`,
                border: `1px solid ${f.accent}25`,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                fontSize: "1.6rem",
                marginBottom: "1.25rem",
              }}
            >
              {f.icon}
            </div>

            <h3
              style={{
                fontFamily: "var(--font-syne)",
                fontSize: "1.1rem",
                fontWeight: 700,
                color: "#f0f0fa",
                marginBottom: "0.6rem",
                letterSpacing: "-0.01em",
              }}
            >
              {f.title}
            </h3>
            <p
              style={{
                fontFamily: "var(--font-outfit)",
                fontSize: "0.85rem",
                color: "#777799",
                lineHeight: 1.65,
              }}
            >
              {f.desc}
            </p>
          </div>
        ))}
      </div>

      <style>{`
        @media (max-width: 1024px) { .features-grid { grid-template-columns: repeat(2, 1fr) !important; } }
        @media (max-width: 540px)  { .features-grid { grid-template-columns: 1fr !important; } }
      `}</style>
    </section>
  );
}

/* ─── How It Works ────────────────────────────────────── */
function HowItWorks() {
  return (
    <section
      id="how-it-works"
      style={{
        padding: "6rem 1.5rem",
        background:
          "radial-gradient(ellipse 70% 50% at 50% 50%, rgba(255,45,139,0.04) 0%, transparent 70%)",
        position: "relative",
        overflow: "hidden",
      }}
    >
      {/* Decorative vertical line */}
      <div
        style={{
          position: "absolute",
          top: 0,
          left: "50%",
          transform: "translateX(-50%)",
          width: "1px",
          height: "100%",
          background:
            "linear-gradient(to bottom, transparent, #1c1c38 20%, #1c1c38 80%, transparent)",
          pointerEvents: "none",
        }}
      />

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
              Simple as 1-2-3
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
            Get started in <span className="gradient-text">minutes</span>
          </h2>
        </div>

        {/* Steps */}
        <div
          className="steps-grid"
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(4, 1fr)",
            gap: "1.5rem",
            position: "relative",
            zIndex: 1,
          }}
        >
          {STEPS.map((step, idx) => (
            <div
              key={step.num}
              style={{
                position: "relative",
                textAlign: "center",
                padding: "2rem 1.5rem",
              }}
            >
              {/* Connector */}
              {idx < STEPS.length - 1 && (
                <div
                  style={{
                    position: "absolute",
                    top: "3.5rem",
                    right: "-0.75rem",
                    width: "1.5rem",
                    height: "1px",
                    background:
                      "linear-gradient(90deg, #1c1c38, rgba(28,28,56,0))",
                    zIndex: -1,
                  }}
                />
              )}

              <div
                style={{
                  fontFamily: "var(--font-syne)",
                  fontSize: "0.7rem",
                  fontWeight: 700,
                  color: step.color,
                  letterSpacing: "0.15em",
                  textTransform: "uppercase",
                  marginBottom: "1.25rem",
                  opacity: 0.7,
                }}
              >
                Step {step.num}
              </div>

              <div
                style={{
                  width: "72px",
                  height: "72px",
                  borderRadius: "50%",
                  background: `radial-gradient(circle at 35% 35%, ${step.color}33, ${step.color}11)`,
                  border: `2px solid ${step.color}35`,
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  fontSize: "2rem",
                  margin: "0 auto 1.25rem",
                  boxShadow: `0 0 24px ${step.color}25`,
                  position: "relative",
                }}
              >
                {step.icon}
                <div
                  style={{
                    position: "absolute",
                    inset: "-6px",
                    borderRadius: "50%",
                    border: `1px solid ${step.color}18`,
                  }}
                />
              </div>

              <h3
                style={{
                  fontFamily: "var(--font-syne)",
                  fontSize: "1.05rem",
                  fontWeight: 700,
                  color: "#f0f0fa",
                  marginBottom: "0.6rem",
                  letterSpacing: "-0.01em",
                }}
              >
                {step.title}
              </h3>
              <p
                style={{
                  fontFamily: "var(--font-outfit)",
                  fontSize: "0.875rem",
                  color: "#666688",
                  lineHeight: 1.6,
                }}
              >
                {step.desc}
              </p>
            </div>
          ))}
        </div>

        {/* CTA */}
        <div style={{ textAlign: "center", marginTop: "3rem" }}>
          <a
            href="#"
            className="btn-primary"
            style={{ fontSize: "1rem", padding: "0.875rem 2.25rem" }}
          >
            Start Playing — It&apos;s Free ↗
          </a>
        </div>
      </div>

      <style>{`
        @media (max-width: 900px)  { .steps-grid { grid-template-columns: repeat(2, 1fr) !important; } }
        @media (max-width: 540px)  { .steps-grid { grid-template-columns: 1fr !important; } }
      `}</style>
    </section>
  );
}

/* ─── Tournament Preview ──────────────────────────────── */
function TournamentPreview() {
  return (
    <section
      id="tournaments"
      style={{ padding: "6rem 1.5rem", maxWidth: "1100px", margin: "0 auto" }}
    >
      {/* Header */}
      <div
        style={{
          display: "flex",
          alignItems: "flex-end",
          justifyContent: "space-between",
          gap: "1rem",
          marginBottom: "2.5rem",
          flexWrap: "wrap",
        }}
      >
        <div>
          <div
            style={{
              display: "inline-block",
              background: "rgba(255,45,139,0.08)",
              border: "1px solid rgba(255,45,139,0.2)",
              borderRadius: "2rem",
              padding: "0.3rem 0.9rem",
              marginBottom: "0.75rem",
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
              Live Now
            </span>
          </div>
          <h2
            style={{
              fontFamily: "var(--font-syne)",
              fontSize: "clamp(1.75rem, 4vw, 2.5rem)",
              fontWeight: 800,
              color: "#f0f0fa",
              letterSpacing: "-0.03em",
              lineHeight: 1.1,
            }}
          >
            Active <span className="gradient-text">Tournaments</span>
          </h2>
        </div>
        <a
          href="#"
          className="btn-secondary"
          style={{ fontSize: "0.875rem", padding: "0.6rem 1.2rem" }}
        >
          View All →
        </a>
      </div>

      {/* Cards */}
      <div
        className="tournament-grid"
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(3, 1fr)",
          gap: "1.25rem",
        }}
      >
        {MOCK_TOURNAMENTS.map((t) => (
          <div
            key={t.id}
            className="card-glow"
            style={{
              background: "#0d0d1e",
              border: "1px solid #1c1c38",
              borderRadius: "1.25rem",
              padding: "1.5rem",
              position: "relative",
              overflow: "hidden",
            }}
          >
            <div
              style={{
                position: "absolute",
                top: "-20px",
                right: "-20px",
                width: "100px",
                height: "100px",
                borderRadius: "50%",
                background:
                  "radial-gradient(circle, rgba(255,45,139,0.08), transparent 70%)",
                pointerEvents: "none",
              }}
            />

            <div
              style={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "flex-start",
                marginBottom: "1.25rem",
              }}
            >
              <div>
                <div
                  style={{
                    fontFamily: "var(--font-outfit)",
                    fontSize: "0.72rem",
                    color: "#666688",
                    marginBottom: "0.25rem",
                    fontWeight: 500,
                  }}
                >
                  {t.id}
                </div>
                <h3
                  style={{
                    fontFamily: "var(--font-syne)",
                    fontSize: "1.05rem",
                    fontWeight: 700,
                    color: "#f0f0fa",
                    letterSpacing: "-0.01em",
                  }}
                >
                  {t.name}
                </h3>
              </div>
              <div
                style={{
                  background: `${t.diffColor}18`,
                  border: `1px solid ${t.diffColor}30`,
                  borderRadius: "0.5rem",
                  padding: "0.25rem 0.6rem",
                }}
              >
                <span
                  style={{
                    fontFamily: "var(--font-syne)",
                    fontSize: "0.7rem",
                    fontWeight: 700,
                    color: t.diffColor,
                  }}
                >
                  {t.difficulty}
                </span>
              </div>
            </div>

            <div
              style={{
                background: "rgba(255,45,139,0.06)",
                border: "1px solid rgba(255,45,139,0.15)",
                borderRadius: "0.875rem",
                padding: "1rem",
                marginBottom: "1.25rem",
                textAlign: "center",
              }}
            >
              <div
                style={{
                  fontFamily: "var(--font-outfit)",
                  fontSize: "0.72rem",
                  color: "#ff79c6",
                  fontWeight: 500,
                  marginBottom: "0.2rem",
                  textTransform: "uppercase",
                  letterSpacing: "0.08em",
                }}
              >
                Prize Pool
              </div>
              <div
                style={{
                  fontFamily: "var(--font-syne)",
                  fontSize: "1.75rem",
                  fontWeight: 800,
                  color: "#ff2d8b",
                  letterSpacing: "-0.02em",
                }}
              >
                {t.prizePool}
              </div>
            </div>

            <div
              style={{
                display: "grid",
                gridTemplateColumns: "1fr 1fr 1fr",
                gap: "0.75rem",
                marginBottom: "1.25rem",
              }}
            >
              {[
                { label: "Entry Fee", value: t.entryFee },
                { label: "Players", value: `${t.players}/${t.maxPlayers}` },
                { label: "Ends In", value: t.endsIn },
              ].map((s) => (
                <div key={s.label} style={{ textAlign: "center" }}>
                  <div
                    style={{
                      fontFamily: "var(--font-syne)",
                      fontSize: "0.85rem",
                      fontWeight: 700,
                      color: "#f0f0fa",
                      marginBottom: "0.15rem",
                    }}
                  >
                    {s.value}
                  </div>
                  <div
                    style={{
                      fontFamily: "var(--font-outfit)",
                      fontSize: "0.7rem",
                      color: "#555577",
                    }}
                  >
                    {s.label}
                  </div>
                </div>
              ))}
            </div>

            <div style={{ marginBottom: "1.25rem" }}>
              <div
                style={{
                  height: "4px",
                  background: "#1c1c38",
                  borderRadius: "2px",
                  overflow: "hidden",
                }}
              >
                <div
                  style={{
                    height: "100%",
                    width: `${(t.players / t.maxPlayers) * 100}%`,
                    background: "linear-gradient(90deg, #ff2d8b, #ff79c6)",
                    borderRadius: "2px",
                  }}
                />
              </div>
            </div>

            <button
              className="btn-primary"
              style={{
                width: "100%",
                justifyContent: "center",
                padding: "0.65rem",
                fontSize: "0.875rem",
                animation: "none",
              }}
            >
              Join Tournament
            </button>
          </div>
        ))}
      </div>

      {/* Bottom CTA banner */}
      <div
        style={{
          marginTop: "3rem",
          background:
            "linear-gradient(135deg, rgba(255,45,139,0.1), rgba(180,77,255,0.08))",
          border: "1px solid rgba(255,45,139,0.2)",
          borderRadius: "1.5rem",
          padding: "2.5rem",
          textAlign: "center",
          position: "relative",
          overflow: "hidden",
        }}
      >
        <div
          style={{
            position: "absolute",
            top: "-50px",
            left: "50%",
            transform: "translateX(-50%)",
            width: "300px",
            height: "200px",
            background:
              "radial-gradient(ellipse, rgba(255,45,139,0.12), transparent 70%)",
            pointerEvents: "none",
          }}
        />
        <h3
          style={{
            fontFamily: "var(--font-syne)",
            fontSize: "1.6rem",
            fontWeight: 800,
            color: "#f0f0fa",
            letterSpacing: "-0.02em",
            marginBottom: "0.6rem",
            position: "relative",
          }}
        >
          Ready to compete?
        </h3>
        <p
          style={{
            fontFamily: "var(--font-outfit)",
            color: "#777799",
            marginBottom: "1.5rem",
            fontSize: "0.95rem",
            position: "relative",
          }}
        >
          Connect your wallet and join the next tournament. Real prizes, fair
          play.
        </p>
        <a
          href="#"
          className="btn-primary"
          style={{ position: "relative", fontSize: "0.95rem" }}
        >
          Connect Wallet & Play ↗
        </a>
      </div>

      <style>{`
        @media (max-width: 900px) { .tournament-grid { grid-template-columns: repeat(2, 1fr) !important; } }
        @media (max-width: 600px) { .tournament-grid { grid-template-columns: 1fr !important; } }
      `}</style>
    </section>
  );
}

/* ─── Footer ──────────────────────────────────────────── */
function Footer() {
  return (
    <footer
      style={{
        borderTop: "1px solid #1c1c38",
        padding: "4rem 1.5rem 2rem",
        position: "relative",
        overflow: "hidden",
      }}
    >
      <div
        style={{
          position: "absolute",
          bottom: 0,
          left: "50%",
          transform: "translateX(-50%)",
          width: "600px",
          height: "200px",
          background:
            "radial-gradient(ellipse, rgba(255,45,139,0.05), transparent 70%)",
          pointerEvents: "none",
        }}
      />

      <div style={{ maxWidth: "1100px", margin: "0 auto" }}>
        <div
          className="footer-grid"
          style={{
            display: "grid",
            gridTemplateColumns: "2fr 1fr 1fr 1fr",
            gap: "3rem",
            marginBottom: "3rem",
          }}
        >
          {/* Brand */}
          <div>
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "0.6rem",
                marginBottom: "1rem",
              }}
            >
              <div
                style={{
                  width: "34px",
                  height: "34px",
                  borderRadius: "10px",
                  background: "linear-gradient(135deg, #ff2d8b, #b44dff)",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  fontSize: "16px",
                  boxShadow: "0 0 16px rgba(255,45,139,0.4)",
                }}
              >
                🔮
              </div>
              <span
                style={{
                  fontFamily: "var(--font-syne)",
                  fontWeight: 800,
                  fontSize: "1.2rem",
                  color: "#f0f0fa",
                  letterSpacing: "-0.02em",
                }}
              >
                Ball<span style={{ color: "#ff2d8b" }}>Sort</span>
              </span>
            </div>
            <p
              style={{
                fontFamily: "var(--font-outfit)",
                fontSize: "0.875rem",
                color: "#555577",
                lineHeight: 1.65,
                maxWidth: "260px",
                marginBottom: "1.5rem",
              }}
            >
              The on-chain color-sorting puzzle game built on Solana with
              MagicBlock ephemeral rollups.
            </p>
            <div style={{ display: "flex", gap: "0.5rem" }}>
              {BALL_COLORS.map((color) => (
                <div
                  key={color}
                  style={{
                    width: "20px",
                    height: "20px",
                    borderRadius: "50%",
                    background: `radial-gradient(circle at 35% 35%, ${color}dd, ${color}88)`,
                    boxShadow: `0 0 8px ${color}44`,
                  }}
                />
              ))}
            </div>
          </div>

          {/* Links */}
          {Object.entries(LINKS).map(([category, items]) => (
            <div key={category}>
              <h4
                style={{
                  fontFamily: "var(--font-syne)",
                  fontSize: "0.75rem",
                  fontWeight: 700,
                  color: "#f0f0fa",
                  textTransform: "uppercase",
                  letterSpacing: "0.1em",
                  marginBottom: "1rem",
                }}
              >
                {category}
              </h4>
              <ul
                style={{
                  listStyle: "none",
                  display: "flex",
                  flexDirection: "column",
                  gap: "0.6rem",
                  padding: 0,
                }}
              >
                {items.map((item) => (
                  <li key={item.label}>
                    <Link
                      href={item.href}
                      style={{
                        fontFamily: "var(--font-outfit)",
                        fontSize: "0.875rem",
                        color: "#555577",
                        textDecoration: "none",
                        transition: "color 0.2s",
                        display: "inline-block",
                      }}
                      onMouseEnter={(e) =>
                        (e.currentTarget.style.color = "#ff79c6")
                      }
                      onMouseLeave={(e) =>
                        (e.currentTarget.style.color = "#555577")
                      }
                    >
                      {item.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Bottom bar */}
        <div
          style={{
            paddingTop: "1.5rem",
            borderTop: "1px solid #1c1c38",
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            flexWrap: "wrap",
            gap: "1rem",
          }}
        >
          <p
            style={{
              fontFamily: "var(--font-outfit)",
              fontSize: "0.8rem",
              color: "#333355",
            }}
          >
            © 2025 BallSort. All rights reserved.
          </p>
          <div style={{ display: "flex", gap: "1.5rem" }}>
            {["Privacy", "Terms", "Contact"].map((item) => (
              <a
                key={item}
                href="#"
                style={{
                  fontFamily: "var(--font-outfit)",
                  fontSize: "0.8rem",
                  color: "#333355",
                  textDecoration: "none",
                  transition: "color 0.2s",
                }}
                onMouseEnter={(e) => (e.currentTarget.style.color = "#ff79c6")}
                onMouseLeave={(e) => (e.currentTarget.style.color = "#333355")}
              >
                {item}
              </a>
            ))}
          </div>
        </div>
      </div>

      <style>{`
        @media (max-width: 900px) { .footer-grid { grid-template-columns: 1fr 1fr !important; } }
        @media (max-width: 480px) { .footer-grid { grid-template-columns: 1fr !important; } }
      `}</style>
    </footer>
  );
}

/* ─── Page ────────────────────────────────────────────── */
export default function Home() {
  return (
    <>
      <Navbar />
      <main>
        <Hero />
        <Features />
        <HowItWorks />
        <TournamentPreview />
      </main>
      <Footer />
    </>
  );
}

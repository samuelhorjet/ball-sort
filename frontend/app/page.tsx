"use client";

import Link from "next/link";
import { useState, useEffect, useRef } from "react";
import HowItWorks from "../components/HowItWorks";

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

/* ─── Diamond Logo ─────────────────────────────────────── */
function DiamondLogo({ size = 45 }: { size?: number }) {
  const ballSize = Math.round(size * 0.244);
  const centerSize = Math.round(size * 0.356);
  const positions = [
    { gridArea: "1 / 2" },
    { gridArea: "2 / 1" },
    { gridArea: "2 / 3" },
    { gridArea: "3 / 2" },
  ];
  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "repeat(3, 1fr)",
        gridTemplateRows: "repeat(3, 1fr)",
        gap: "2px",
        alignItems: "center",
        justifyItems: "center",
        width: `${size}px`,
        height: `${size}px`,
        position: "relative",
      }}
    >
      {BALL_COLORS.slice(0, 4).map((c, i) => (
        <div
          key={i}
          style={{
            ...positions[i],
            width: `${ballSize}px`,
            height: `${ballSize}px`,
            borderRadius: "50%",
            background: `radial-gradient(circle at 35% 35%, ${c}ee, ${c}88)`,
            boxShadow: `0 0 8px ${c}aa`,
          }}
        />
      ))}
      <div
        style={{
          gridArea: "2 / 2",
          width: `${centerSize}px`,
          height: `${centerSize}px`,
          borderRadius: "50%",
          background: `radial-gradient(circle at 35% 35%, #ffffff, ${
            BALL_COLORS[4] || "#fff"
          }dd)`,
          boxShadow: `0 0 12px white`,
          zIndex: 2,
        }}
      />
    </div>
  );
}

/* ─── useInView scroll hook ────────────────────────────── */
function useInView(options: IntersectionObserverInit = {}) {
  const ref = useRef<HTMLDivElement>(null);
  const [inView, setInView] = useState(false);
  const optionsRef = useRef(options);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const obs = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setInView(true);
          obs.disconnect();
        }
      },
      { threshold: 0.12, ...optionsRef.current },
    );
    obs.observe(el);
    return () => obs.disconnect();
  }, []);

  return { ref, inView };
}

/* ─── Preloader ────────────────────────────────────────── */
function Preloader({ onDone }: { onDone: () => void }) {
  const [phase, setPhase] = useState(0);
  const [isOnline, setIsOnline] = useState(true);
  const onDoneRef = useRef(onDone);
  useEffect(() => { onDoneRef.current = onDone; }, [onDone]);

  // Network tracking
  useEffect(() => {
    const check = () => setIsOnline(navigator.onLine);
    check();
    window.addEventListener("online", check);
    window.addEventListener("offline", check);

    // Also try a small fetch ping as a secondary check
    const ping = () => {
      fetch("/", { method: "HEAD", cache: "no-store" })
        .then(() => setIsOnline(true))
        .catch(() => setIsOnline(false));
    };
    const interval = setInterval(ping, 4000);

    return () => {
      window.removeEventListener("online", check);
      window.removeEventListener("offline", check);
      clearInterval(interval);
    };
  }, []);

  // Animation timeline — only completes if online
  useEffect(() => {
    if (!isOnline) return;

    const timers = [
      setTimeout(() => setPhase(1), 500),   // start wave
      setTimeout(() => setPhase(2), 1800),   // spin to diamond
      setTimeout(() => setPhase(3), 2800),   // center ball glow
      setTimeout(() => setPhase(4), 3800),   // fade out
      setTimeout(() => onDoneRef.current(), 4500),
    ];
    return () => timers.forEach(clearTimeout);
  }, [isOnline]);

  const ballColors = ["#ff2d8b", "#4d9fff", "#3dd9a0", "#ffd93d"];

  // -- Positions --
  // Logo area: 80×80, center = 40,40 (tight like DiamondLogo)
  const area = 80;
  const cx = area / 2; // 40
  const cy = area / 2; // 40
  const outerBall = 16;
  const centerBall = 22; // ~1.4x bigger like logo
  const spread = 20; // distance from center for diamond points

  // Phase 0: horizontal row, centered
  const horizPositions = [
    { x: cx - 30, y: cy },
    { x: cx - 10, y: cy },
    { x: cx + 10, y: cy },
    { x: cx + 30, y: cy },
  ];

  // Phase 2+: diamond (top, left, right, bottom)
  const diamondPositions = [
    { x: cx,          y: cy - spread },
    { x: cx - spread, y: cy },
    { x: cx + spread, y: cy },
    { x: cx,          y: cy + spread },
  ];

  const getPos = (i: number) => {
    if (phase < 2) return horizPositions[i];
    return diamondPositions[i];
  };

  return (
    <>
      <style>{`
        @keyframes preloaderFadeOut {
          from { opacity: 1; }
          to { opacity: 0; pointer-events: none; }
        }
        @keyframes preloaderBallIn {
          from { opacity: 0; transform: scale(0); }
          to { opacity: 1; transform: scale(1); }
        }
        @keyframes waveBounce0 {
          0%, 100% { transform: translateY(0); }
          50% { transform: translateY(-18px); }
        }
        @keyframes waveBounce1 {
          0%, 100% { transform: translateY(0); }
          50% { transform: translateY(18px); }
        }
        @keyframes waveBounce2 {
          0%, 100% { transform: translateY(0); }
          50% { transform: translateY(-18px); }
        }
        @keyframes waveBounce3 {
          0%, 100% { transform: translateY(0); }
          50% { transform: translateY(18px); }
        }
        @keyframes spinToDiamond {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
        @keyframes centerBallIn {
          from { opacity: 0; transform: scale(0); }
          to { opacity: 1; transform: scale(1); }
        }
        @keyframes centerGlowPulse {
          0%, 100% {
            box-shadow: 0 0 12px #ffffffaa, 0 0 24px rgba(255,45,139,0.5), 0 0 36px rgba(180,77,255,0.3);
          }
          50% {
            box-shadow: 0 0 24px #ffffffff, 0 0 48px rgba(255,45,139,0.8), 0 0 72px rgba(180,77,255,0.5);
          }
        }
        @keyframes logoScaleIn {
          from { transform: scale(0.85); opacity: 0; }
          to { transform: scale(1); opacity: 1; }
        }
        @keyframes offlinePulse {
          0%, 100% { opacity: 0.6; }
          50% { opacity: 1; }
        }
        .preloader-ball-anim {
          position: absolute;
          border-radius: 50%;
          transition: left 0.7s cubic-bezier(0.34, 1.56, 0.64, 1),
                      top 0.7s cubic-bezier(0.34, 1.56, 0.64, 1);
        }
      `}</style>

      <div
        style={{
          position: "fixed",
          inset: 0,
          zIndex: 9999,
          background: "#07070f",
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          gap: "1.5rem",
          animation:
            phase === 4 ? "preloaderFadeOut 0.7s ease 0s forwards" : "none",
        }}
      >
        {/* Ambient glow */}
        <div
          style={{
            position: "absolute",
            width: "260px",
            height: "260px",
            borderRadius: "50%",
            background:
              "radial-gradient(ellipse, rgba(255,45,139,0.06) 0%, transparent 70%)",
            pointerEvents: "none",
          }}
        />

        {/* Logo animation area */}
        <div
          style={{
            position: "relative",
            width: `${area}px`,
            height: `${area}px`,
            animation: phase === 2 ? "spinToDiamond 0.8s cubic-bezier(0.34,1.56,0.64,1) forwards" : "none",
          }}
        >
          {/* 4 colored balls */}
          {ballColors.map((c, i) => {
            const pos = getPos(i);
            return (
              <div
                key={i}
                className="preloader-ball-anim"
                style={{
                  width: `${outerBall}px`,
                  height: `${outerBall}px`,
                  background: `radial-gradient(circle at 35% 35%, ${c}ff, ${c}88)`,
                  boxShadow: `0 0 10px ${c}aa, 0 0 20px ${c}33`,
                  left: `${pos.x - outerBall / 2}px`,
                  top: `${pos.y - outerBall / 2}px`,
                  animation:
                    phase === 0
                      ? `preloaderBallIn 0.3s ease ${i * 0.1}s both`
                      : phase === 1
                      ? `waveBounce${i} 0.6s cubic-bezier(0.36, 0, 0.66, 1) ${i * 0.12}s infinite`
                      : "none",
                }}
              />
            );
          })}

          {/* White center ball — appears in phase 3 */}
          {phase >= 3 && (
            <div
              style={{
                position: "absolute",
                width: `${centerBall}px`,
                height: `${centerBall}px`,
                borderRadius: "50%",
                background:
                  "radial-gradient(circle at 35% 35%, #ffffff, #e0d0ffdd)",
                left: `${cx - centerBall / 2}px`,
                top: `${cy - centerBall / 2}px`,
                zIndex: 2,
                animation:
                  "centerBallIn 0.4s cubic-bezier(0.34,1.56,0.64,1) both, centerGlowPulse 1.8s ease-in-out 0.35s infinite",
              }}
            />
          )}
        </div>

        {/* Network status indicator */}
        {!isOnline && (
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: "0.5rem",
              animation: "offlinePulse 2s ease-in-out infinite",
            }}
          >
            <div
              style={{
                width: "8px",
                height: "8px",
                borderRadius: "50%",
                background: "#ff6b6b",
                boxShadow: "0 0 8px #ff6b6b88",
              }}
            />
            <span
              style={{
                fontFamily: "var(--font-outfit)",
                fontSize: "0.78rem",
                color: "#ff6b6b",
                fontWeight: 500,
                letterSpacing: "0.04em",
              }}
            >
              Waiting for connection…
            </span>
          </div>
        )}

        {/* Loading dots — show during wave phase */}
        {phase < 3 && isOnline && (
          <div style={{ display: "flex", gap: "6px" }}>
            {[0, 1, 2].map((i) => (
              <div
                key={i}
                style={{
                  width: "4px",
                  height: "4px",
                  borderRadius: "50%",
                  background: "#ff2d8b",
                  opacity: 0.5,
                  animation: `pulseGlow 1s ease-in-out ${i * 0.15}s infinite`,
                }}
              />
            ))}
          </div>
        )}
      </div>
    </>
  );
}

/* ─── Navbar ──────────────────────────────────────────── */
function Navbar() {
  const [menuOpen, setMenuOpen] = useState(false);
  const [walletConnected] = useState(false);
  const [scrolled, setScrolled] = useState(false);

  useEffect(() => {
    const handleScroll = () => setScrolled(window.scrollY > 20);
    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  const NAV_ITEMS = [
    { label: "Features", href: "#features" },
    { label: "How It Works", href: "#how-it-works" },
    { label: "Tournaments", href: "#tournaments" },
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
        background: scrolled ? "rgba(7,7,26,0.95)" : "rgba(7,7,26,0.85)",
        backdropFilter: "blur(20px)",
        WebkitBackdropFilter: "blur(20px)",
        borderBottom: "1px solid rgba(255,255,255,0.07)",
        transition: "background 0.3s ease",
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
        <DiamondLogo size={45} />
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
              color: "#8888aa",
              textDecoration: "none",
              fontWeight: 500,
              fontSize: "0.93rem",
              transition: "color 0.2s",
            }}
            onMouseEnter={(e) => (e.currentTarget.style.color = "#f0f0fa")}
            onMouseLeave={(e) => (e.currentTarget.style.color = "#8888aa")}
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
      <div
        style={{
          position: "absolute",
          inset: 0,
          background:
            "radial-gradient(ellipse 80% 60% at 70% 50%, rgba(255,45,139,0.08) 0%, transparent 70%), radial-gradient(ellipse 60% 50% at 85% 80%, rgba(180,77,255,0.06) 0%, transparent 60%)",
          pointerEvents: "none",
        }}
      />
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
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "flex-start",
          }}
        >
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

          <p
            style={{
              fontFamily: "var(--font-outfit)",
              fontSize: "clamp(1rem, 2vw, 1.15rem)",
              fontWeight: 400,
              color: "#aaaabb",
              lineHeight: 1.7,
              maxWidth: "460px",
              marginBottom: "2.5rem",
              animation: "fadeInUp 0.7s ease 0.2s both",
            }}
          >
            The addictive color-sorting puzzle game — fully on-chain, lightning
            fast with ephemeral rollups, and loaded with real prize pools.
          </p>

          <div
            style={{
              display: "flex",
              gap: "0.5rem",
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

          <div
            className="stats-grid"
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
                    color: "#8888aa",
                    fontWeight: 500,
                  }}
                >
                  {s.label}
                </div>
              </div>
            ))}
          </div>
        </div>

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

      <style>{`
        @media (max-width: 768px) {
          .hero-layout { grid-template-columns: 1fr !important; min-height: auto !important; padding-top: 3rem !important; padding-bottom: 4rem !important; }
          .hero-layout > div:last-child { display: none !important; }
          .stats-grid { grid-template-columns: repeat(2, 1fr) !important; }
        }
      `}</style>
    </section>
  );
}

/* ─── Feature Card (extracted for hooks) ──────────────── */
function FeatureCard({ f, idx, dir }: { f: typeof FEATURES[number]; idx: number; dir: string }) {
  const { ref, inView } = useInView();
  return (
    <div
      key={f.title}
      ref={ref}
      className={`card-glow ${inView ? `anim-${dir}` : "anim-hidden"}`}
      style={{
        background: "#0d0d1e",
        border: "1px solid #1c1c38",
        borderRadius: "1.25rem",
        padding: "1.75rem",
        position: "relative",
        overflow: "hidden",
        animationDelay: inView ? `${idx * 0.1}s` : "0s",
      }}
    >
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
  );
}

/* ─── Features ────────────────────────────────────────── */
function Features() {
  const { ref: headerRef, inView: headerInView } = useInView();

  const slideDirections = [
    "slideInLeft",
    "slideInBottom",
    "slideInBottom",
    "slideInRight",
  ];

  return (
    <section
      id="features"
      style={{ padding: "6rem 1.5rem", maxWidth: "1200px", margin: "0 auto" }}
    >
      <style>{`
        @keyframes slideInLeft {
          from { opacity: 0; transform: translateX(-48px); }
          to { opacity: 1; transform: translateX(0); }
        }
        @keyframes slideInRight {
          from { opacity: 0; transform: translateX(48px); }
          to { opacity: 1; transform: translateX(0); }
        }
        @keyframes slideInBottom {
          from { opacity: 0; transform: translateY(48px); }
          to { opacity: 1; transform: translateY(0); }
        }
        .anim-hidden { opacity: 0; }
        .anim-slideInLeft { animation: slideInLeft 0.7s cubic-bezier(0.25, 0.46, 0.45, 0.94) both; }
        .anim-slideInRight { animation: slideInRight 0.7s cubic-bezier(0.25, 0.46, 0.45, 0.94) both; }
        .anim-slideInBottom { animation: slideInBottom 0.7s cubic-bezier(0.25, 0.46, 0.45, 0.94) both; }
        .anim-fadeInUp { animation: fadeInUp 0.7s cubic-bezier(0.25, 0.46, 0.45, 0.94) both; }
      `}</style>

      {/* Section header */}
      <div
        ref={headerRef}
        className={headerInView ? "anim-fadeInUp" : "anim-hidden"}
        style={{ textAlign: "center", marginBottom: "3.5rem" }}
      >
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

      {/* 4-column features grid */}
      <div
        className="features-grid"
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(4, 1fr)",
          gap: "1.25rem",
        }}
      >
        {FEATURES.map((f, idx) => (
          <FeatureCard key={f.title} f={f} idx={idx} dir={slideDirections[idx]} />
        ))}
      </div>

      <style>{`
        @media (max-width: 1024px) { .features-grid { grid-template-columns: repeat(2, 1fr) !important; } }
        @media (max-width: 540px)  { .features-grid { grid-template-columns: 1fr !important; } }
      `}</style>
    </section>
  );
}

/* ─── Tournament Card (extracted for hooks) ───────────── */
function TournamentCard({ t, idx, dir }: { t: typeof MOCK_TOURNAMENTS[number]; idx: number; dir: string }) {
  const { ref, inView } = useInView();
  return (
    <div
      ref={ref}
      className={`card-glow ${inView ? `anim-${dir}` : "anim-hidden"}`}
      style={{
        background: "#0d0d1e",
        border: "1px solid #1c1c38",
        borderRadius: "1.25rem",
        padding: "1.5rem",
        position: "relative",
        overflow: "hidden",
        animationDelay: inView ? `${idx * 0.12}s` : "0s",
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
  );
}

/* ─── Tournament Preview ──────────────────────────────── */
function TournamentPreview() {
  const { ref: headerRef, inView: headerInView } = useInView();
  const { ref: bannerRef, inView: bannerInView } = useInView();
  const cardDirs = ["slideInLeft", "slideInBottom", "slideInRight"];

  return (
    <section
      id="tournaments"
      style={{ padding: "6rem 1.5rem", maxWidth: "1100px", margin: "0 auto" }}
    >
      {/* Header */}
      <div
        ref={headerRef}
        className={headerInView ? "anim-slideInLeft" : "anim-hidden"}
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
              fontSize: "clamp(1.35rem, 3.5vw, 2.5rem)",
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
        {MOCK_TOURNAMENTS.map((t, idx) => (
          <TournamentCard key={t.id} t={t} idx={idx} dir={cardDirs[idx]} />
        ))}
      </div>

      {/* Bottom CTA banner */}
      <div
        ref={bannerRef}
        className={bannerInView ? "anim-fadeInUp" : "anim-hidden"}
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
  const { ref, inView } = useInView();

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

      <div
        ref={ref}
        className={inView ? "anim-fadeInUp" : "anim-hidden"}
        style={{ maxWidth: "1100px", margin: "0 auto" }}
      >
        <div
          className="footer-grid"
          style={{
            display: "grid",
            gridTemplateColumns: "2fr 1fr 1fr 1fr",
            gap: "3rem",
            marginBottom: "3rem",
          }}
        >
          {/* Brand — with the real diamond logo */}
          <div>
            <div
              style={{
                display: "flex",
                alignItems: "center",
                gap: "0.75rem",
                marginBottom: "1rem",
              }}
            >
              <DiamondLogo size={40} />
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
                color: "#777799",
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
                        color: "#777799",
                        textDecoration: "none",
                        transition: "color 0.2s",
                        display: "inline-block",
                      }}
                      onMouseEnter={(e) =>
                        (e.currentTarget.style.color = "#ff79c6")
                      }
                      onMouseLeave={(e) =>
                        (e.currentTarget.style.color = "#777799")
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
              color: "#555577",
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
                  color: "#555577",
                  textDecoration: "none",
                  transition: "color 0.2s",
                }}
                onMouseEnter={(e) => (e.currentTarget.style.color = "#ff79c6")}
                onMouseLeave={(e) => (e.currentTarget.style.color = "#555577")}
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
  const [showPreloader, setShowPreloader] = useState(true);

  return (
    <>
      {showPreloader && <Preloader onDone={() => setShowPreloader(false)} />}
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

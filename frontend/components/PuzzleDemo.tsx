"use client";

import { useState, useEffect, useRef, useCallback } from "react";

// ─── Colors ──────────────────────────────────────────────────────────────────
const CLR: Record<number, string> = {
  1: "#ff2d8b", // pink
  2: "#4d9fff", // blue
  3: "#3dd9a0", // green
};
const GLW: Record<number, string> = {
  1: "rgba(255,45,139,0.55)",
  2: "rgba(77,159,255,0.55)",
  3: "rgba(61,217,160,0.55)",
};

// ─── Board ───────────────────────────────────────────────────────────────────
const INIT: number[][] = [[1, 2, 3, 1], [2, 3, 1, 2], [3, 1, 2, 3], []];
const MOVES: [number, number][] = [
  [2, 3], [1, 2], [0, 1], [3, 0], [0, 3], [0, 3],
  [2, 0], [2, 0], [1, 2], [1, 2], [1, 3], [0, 1],
  [0, 1], [0, 1], [2, 0], [2, 0], [2, 0], [2, 3],
];

// ─── Layout ──────────────────────────────────────────────────────────────────
const BALL = 38;
const CAPACITY = 4;
const TW = 52;
const TH = 200;
const TGAP = 18;
const TPAD = 24;
const TTOP = 52;
const BPAD = 8;
const BSTEP = (TH - BPAD * 2 - BALL) / (CAPACITY - 1);
const CW = TPAD * 2 + 4 * TW + 3 * TGAP;
const CH = TTOP + TH + 44;
const CARRY_Y = 20;

const tcx = (i: number) => TPAD + i * (TW + TGAP) + TW / 2;
const ballY = (bi: number) => TTOP + TH - BPAD - BALL / 2 - bi * BSTEP;

// ─── Timings (ms) ────────────────────────────────────────────────────────────
const TM = 330;
const TP = 190;
const TC = 340;
const TD = 175;
const TS = 150;

// ─── Hand Cursor SVG ─────────────────────────────────────────────────────────
function HandCursor({ x, y, dur, clicking }: {
  x: number; y: number; dur: number; clicking?: boolean;
}) {
  return (
    <div
      style={{
        position: "absolute",
        left: x - 8,
        top: y - 2,
        width: 28,
        height: 32,
        transition:
          dur > 0
            ? `left ${dur}ms cubic-bezier(.42,0,.22,1), top ${dur}ms cubic-bezier(.42,0,.22,1)`
            : "none",
        zIndex: 20,
        pointerEvents: "none",
        transform: clicking ? "scale(0.88)" : "scale(1)",
        transformOrigin: "top left",
        transitionProperty: "left, top, transform",
        filter: "drop-shadow(0 2px 6px rgba(0,0,0,0.5))",
      }}
    >
      <svg viewBox="0 0 24 28" width="28" height="32" fill="none" xmlns="http://www.w3.org/2000/svg">
        {/* pointer finger */}
        <path
          d="M10 1.5C10 0.67 10.67 0 11.5 0C12.33 0 13 0.67 13 1.5V12H10V1.5Z"
          fill="white"
          stroke="rgba(255,255,255,0.3)"
          strokeWidth="0.5"
        />
        {/* middle finger */}
        <path
          d="M14 4C14 3.17 14.67 2.5 15.5 2.5C16.33 2.5 17 3.17 17 4V13H14V4Z"
          fill="white"
          stroke="rgba(255,255,255,0.3)"
          strokeWidth="0.5"
        />
        {/* ring finger */}
        <path
          d="M18 5.5C18 4.67 18.67 4 19.5 4C20.33 4 21 4.67 21 5.5V14H18V5.5Z"
          fill="white"
          stroke="rgba(255,255,255,0.3)"
          strokeWidth="0.5"
        />
        {/* thumb + palm */}
        <path
          d="M6 12C4.5 12 3 13 3 15V17C3 22 6 26 11 27H14C19 27 22 23 22 18V14C22 13 21.5 12 20.5 12H8.5C7.5 12 6.5 12 6 12Z"
          fill="white"
          stroke="rgba(255,255,255,0.3)"
          strokeWidth="0.5"
        />
        {/* thumb */}
        <path
          d="M6 12C5 12 3.5 11 3 10C2.5 9 3 7.5 4 7C5 6.5 6.5 7 7 8L8 10.5"
          fill="white"
          stroke="rgba(255,255,255,0.2)"
          strokeWidth="0.5"
        />
        {/* inner glow highlight */}
        <ellipse cx="12" cy="6" rx="1.5" ry="3" fill="rgba(255,255,255,0.15)" />
      </svg>
    </div>
  );
}

// ─── Phases ──────────────────────────────────────────────────────────────────
type Phase = "preloader" | "start" | "solving" | "done";

export default function PuzzleDemo() {
  const [phase, setPhase] = useState<Phase>("preloader");

  // Preloader sub-phase
  const [prePhase, setPrePhase] = useState(0);

  // Solving state
  const [tubes, setTubes] = useState(() => INIT.map((a) => [...a]));
  const [srcHL, setSrcHL] = useState<number | null>(null);
  const [solved, setSolved] = useState<Set<number>>(new Set());

  const [cur, setCur] = useState({ x: CW / 2, y: CH / 2 });
  const [curDur, setCurDur] = useState(0);
  const [clicking, setClicking] = useState(false);
  const [fb, setFb] = useState<{ c: number; x: number; y: number } | null>(null);
  const [fbDur, setFbDur] = useState(0);

  // Score state
  const [moveCount, setMoveCount] = useState(0);
  const [startTime, setStartTime] = useState(0);
  const [elapsedSec, setElapsedSec] = useState(0);

  const tubesRef = useRef(INIT.map((a) => [...a]));
  const tids = useRef<ReturnType<typeof setTimeout>[]>([]);
  const moveCountRef = useRef(0);

  const later = useCallback((fn: () => void, ms: number) => {
    const id = setTimeout(fn, ms);
    tids.current.push(id);
  }, []);

  const clearAllTimers = useCallback(() => {
    tids.current.forEach(clearTimeout);
    tids.current = [];
  }, []);

  const isSolvedTube = (t: number[]) =>
    t.length === CAPACITY && t.every((b) => b === t[0]);

  // ─── Full reset ────────────────────────────────────────
  const resetAll = useCallback(() => {
    clearAllTimers();
    const fresh = INIT.map((a) => [...a]);
    tubesRef.current = fresh;
    moveCountRef.current = 0;
    setTubes(fresh);
    setSolved(new Set());
    setFb(null);
    setSrcHL(null);
    setCur({ x: CW / 2, y: CH / 2 });
    setCurDur(0);
    setClicking(false);
    setMoveCount(0);
    setElapsedSec(0);
    setPrePhase(0);
    setPhase("preloader");
  }, [clearAllTimers]);

  // ─── Phase: Preloader ──────────────────────────────────
  useEffect(() => {
    if (phase !== "preloader") return;
    const timers = [
      setTimeout(() => setPrePhase(1), 400),
      setTimeout(() => setPrePhase(2), 1200),
      setTimeout(() => setPrePhase(3), 1800),
      setTimeout(() => setPhase("start"), 2500),
    ];
    return () => timers.forEach(clearTimeout);
  }, [phase]);

  // ─── Phase: Start button → click ──────────────────────
  useEffect(() => {
    if (phase !== "start") return;
    // Cursor starts off-screen, moves to button center
    setCur({ x: CW / 2 + 60, y: CH / 2 + 40 });
    setCurDur(0);

    const t1 = setTimeout(() => {
      setCurDur(700);
      setCur({ x: CW / 2, y: CH / 2 + 6 });
    }, 300);

    const t2 = setTimeout(() => {
      setClicking(true);
    }, 1100);

    const t3 = setTimeout(() => {
      setClicking(false);
      setStartTime(Date.now());
      setPhase("solving");
    }, 1500);

    return () => {
      clearTimeout(t1);
      clearTimeout(t2);
      clearTimeout(t3);
    };
  }, [phase]);

  // ─── Phase: Solving ────────────────────────────────────
  const play = useCallback(
    (mi: number) => {
      if (mi >= MOVES.length) {
        // All moves complete → go to done
        later(() => {
          setElapsedSec(Math.round((Date.now() - startTime) / 1000));
          setPhase("done");
        }, 600);
        return;
      }

      const t = tubesRef.current;
      const [from, to] = MOVES[mi];
      const ball = t[from][t[from].length - 1];
      const srcX = tcx(from);
      const dstX = tcx(to);
      const srcBY = ballY(t[from].length - 1);
      const dstBY = ballY(t[to].length);
      let e = 0;

      // 1 — cursor glides to source
      setCurDur(TM);
      setCur({ x: srcX, y: srcBY - 28 });

      // 2 — show floating ball, highlight tube
      later(() => {
        setSrcHL(from);
        setFbDur(0);
        setFb({ c: ball, x: srcX, y: srcBY });
      }, (e += TM));

      // 3 — lift
      later(() => {
        setCurDur(TP);
        setCur({ x: srcX, y: CARRY_Y - 28 });
        setFbDur(TP);
        setFb({ c: ball, x: srcX, y: CARRY_Y });
      }, (e += 55));

      // 4 — carry across
      later(() => {
        setCurDur(TC);
        setCur({ x: dstX, y: CARRY_Y - 28 });
        setFbDur(TC);
        setFb({ c: ball, x: dstX, y: CARRY_Y });
      }, (e += TP + 60));

      // 5 — drop
      later(() => {
        setSrcHL(null);
        setCurDur(TD);
        setCur({ x: dstX, y: dstBY - 28 });
        setFbDur(TD);
        setFb({ c: ball, x: dstX, y: dstBY });
      }, (e += TC + 55));

      // 6 — commit
      later(() => {
        const nt = tubesRef.current.map((a) => [...a]);
        nt[from].pop();
        nt[to].push(ball);
        tubesRef.current = nt;

        const s = new Set<number>();
        nt.forEach((tube, i) => {
          if (isSolvedTube(tube)) s.add(i);
        });

        setTubes([...nt]);
        setSolved(s);
        setFb(null);
        moveCountRef.current += 1;
        setMoveCount(moveCountRef.current);
      }, (e += TD + 80));

      // 7 — next move
      later(() => play(mi + 1), (e += TS + 75));
    },
    [later, startTime],
  );

  useEffect(() => {
    if (phase !== "solving") return;
    const timers = tids.current;
    const id = setTimeout(() => {
      setCur({ x: tcx(2), y: ballY(3) - 28 });
      setCurDur(0);
      play(0);
    }, 400);
    return () => {
      clearTimeout(id);
      timers.forEach(clearTimeout);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [phase]);

  // ─── Phase: Done → auto-restart ────────────────────────
  useEffect(() => {
    if (phase !== "done") return;
    const id = setTimeout(() => resetAll(), 4000);
    return () => clearTimeout(id);
  }, [phase, resetAll]);

  // ─── Render ────────────────────────────────────────────
  const score = moveCount > 0 ? Math.max(100, 2000 - moveCount * 40 - elapsedSec * 5) : 0;

  // ═══════════════════════════════════════════════════════
  // PHASE: PRELOADER
  // ═══════════════════════════════════════════════════════
  if (phase === "preloader") {
    const ballColors = ["#ff2d8b", "#4d9fff", "#3dd9a0", "#ffd93d"];
    const area = 70;
    const cx = area / 2;
    const cy = area / 2;
    const bSize = 14;
    const spread = 17;

    const horizPositions = [
      { x: cx - 25, y: cy },
      { x: cx - 8, y: cy },
      { x: cx + 8, y: cy },
      { x: cx + 25, y: cy },
    ];
    const diamondPositions = [
      { x: cx, y: cy - spread },
      { x: cx - spread, y: cy },
      { x: cx + spread, y: cy },
      { x: cx, y: cy + spread },
    ];

    return (
      <div
        style={{
          width: CW,
          height: CH,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          gap: "1rem",
          position: "relative",
        }}
      >
        <style>{`
          @keyframes demoBallIn {
            from { opacity: 0; transform: scale(0); }
            to { opacity: 1; transform: scale(1); }
          }
          @keyframes demoWave0 { 0%,100% { transform: translateY(0); } 50% { transform: translateY(-14px); } }
          @keyframes demoWave1 { 0%,100% { transform: translateY(0); } 50% { transform: translateY(14px); } }
          @keyframes demoWave2 { 0%,100% { transform: translateY(0); } 50% { transform: translateY(-14px); } }
          @keyframes demoWave3 { 0%,100% { transform: translateY(0); } 50% { transform: translateY(14px); } }
          @keyframes demoSpin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
          @keyframes demoCenterIn {
            from { opacity: 0; transform: scale(0); }
            to { opacity: 1; transform: scale(1); }
          }
          @keyframes demoCenterGlow {
            0%,100% { box-shadow: 0 0 8px #ffffffaa, 0 0 16px rgba(255,45,139,0.4); }
            50% { box-shadow: 0 0 16px #ffffffff, 0 0 32px rgba(255,45,139,0.7); }
          }
          .demo-preloader-ball {
            position: absolute;
            border-radius: 50%;
            transition: left 0.6s cubic-bezier(0.34,1.56,0.64,1),
                        top 0.6s cubic-bezier(0.34,1.56,0.64,1);
          }
        `}</style>

        <div
          style={{
            position: "relative",
            width: area,
            height: area,
            animation: prePhase === 2 ? "demoSpin 0.7s cubic-bezier(0.34,1.56,0.64,1) forwards" : "none",
          }}
        >
          {ballColors.map((c, i) => {
            const pos = prePhase < 2 ? horizPositions[i] : diamondPositions[i];
            return (
              <div
                key={i}
                className="demo-preloader-ball"
                style={{
                  width: bSize,
                  height: bSize,
                  background: `radial-gradient(circle at 35% 35%, ${c}ff, ${c}88)`,
                  boxShadow: `0 0 8px ${c}aa`,
                  left: pos.x - bSize / 2,
                  top: pos.y - bSize / 2,
                  animation:
                    prePhase === 0
                      ? `demoBallIn 0.25s ease ${i * 0.08}s both`
                      : prePhase === 1
                      ? `demoWave${i} 0.5s cubic-bezier(0.36,0,0.66,1) ${i * 0.1}s infinite`
                      : "none",
                }}
              />
            );
          })}

          {prePhase >= 3 && (
            <div
              style={{
                position: "absolute",
                width: 18,
                height: 18,
                borderRadius: "50%",
                background: "radial-gradient(circle at 35% 35%, #ffffff, #e0d0ffdd)",
                left: cx - 9,
                top: cy - 9,
                zIndex: 2,
                animation: "demoCenterIn 0.35s cubic-bezier(0.34,1.56,0.64,1) both, demoCenterGlow 1.5s ease-in-out 0.3s infinite",
              }}
            />
          )}
        </div>

        <div style={{ display: "flex", gap: 5 }}>
          {[0, 1, 2].map((i) => (
            <div
              key={i}
              style={{
                width: 4,
                height: 4,
                borderRadius: "50%",
                background: "#ff2d8b",
                opacity: 0.5,
                animation: `pulseGlow 1s ease-in-out ${i * 0.15}s infinite`,
              }}
            />
          ))}
        </div>

        <span
          style={{
            fontFamily: "var(--font-syne)",
            fontSize: "0.65rem",
            fontWeight: 600,
            color: "#555577",
            letterSpacing: "0.06em",
          }}
        >
          Loading puzzle…
        </span>
      </div>
    );
  }

  // ═══════════════════════════════════════════════════════
  // PHASE: START BUTTON
  // ═══════════════════════════════════════════════════════
  if (phase === "start") {
    return (
      <div
        style={{
          width: CW,
          height: CH,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          gap: "1.25rem",
          position: "relative",
        }}
      >
        <style>{`
          @keyframes startFadeIn {
            from { opacity: 0; transform: translateY(12px); }
            to { opacity: 1; transform: translateY(0); }
          }
          @keyframes btnGlow {
            0%,100% { box-shadow: 0 0 20px rgba(255,45,139,0.3); }
            50% { box-shadow: 0 0 36px rgba(255,45,139,0.55); }
          }
        `}</style>

        {/* Mini board preview (faded) */}
        <div style={{ opacity: 0.15, pointerEvents: "none" }}>
          <div style={{ position: "relative", width: CW, height: CH }}>
            {INIT.map((tube, ti) => {
              const tx = TPAD + ti * (TW + TGAP);
              return (
                <div
                  key={ti}
                  style={{
                    position: "absolute",
                    left: tx,
                    top: TTOP,
                    width: TW,
                    height: TH,
                    borderRadius: "0 0 20px 20px",
                    border: "2px solid #1c1c38",
                    borderTop: "none",
                    background: "rgba(10,10,24,0.7)",
                  }}
                >
                  {tube.map((ball, bi) => (
                    <div
                      key={bi}
                      style={{
                        position: "absolute",
                        left: "50%",
                        bottom: BPAD + bi * BSTEP,
                        transform: "translateX(-50%)",
                        width: BALL,
                        height: BALL,
                        borderRadius: "50%",
                        background: `radial-gradient(circle at 34% 32%, ${CLR[ball]}ee, ${CLR[ball]}77)`,
                      }}
                    />
                  ))}
                </div>
              );
            })}
          </div>
        </div>

        {/* Start button overlay */}
        <div
          style={{
            position: "absolute",
            inset: 0,
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            justifyContent: "center",
            gap: "0.75rem",
            animation: "startFadeIn 0.5s ease both",
          }}
        >
          <span
            style={{
              fontFamily: "var(--font-syne)",
              fontSize: "0.68rem",
              fontWeight: 600,
              color: "#555577",
              letterSpacing: "0.08em",
              textTransform: "uppercase",
            }}
          >
            Puzzle Ready
          </span>
          <button
            style={{
              background: "linear-gradient(135deg, #ff2d8b, #c4106a)",
              color: "white",
              fontFamily: "var(--font-syne)",
              fontWeight: 700,
              fontSize: "0.85rem",
              padding: "0.7rem 1.5rem",
              borderRadius: "0.75rem",
              border: "none",
              letterSpacing: "0.03em",
              cursor: "default",
              animation: "btnGlow 2.5s ease-in-out infinite",
              transform: clicking ? "scale(0.93)" : "scale(1)",
              transition: "transform 0.15s ease",
            }}
          >
            Start Puzzle ▶
          </button>
        </div>

        {/* Hand cursor */}
        <HandCursor x={cur.x} y={cur.y} dur={curDur} clicking={clicking} />
      </div>
    );
  }

  // ═══════════════════════════════════════════════════════
  // PHASE: DONE / CONGRATULATIONS
  // ═══════════════════════════════════════════════════════
  if (phase === "done") {
    return (
      <div
        style={{
          width: CW,
          height: CH,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          position: "relative",
          overflow: "hidden",
        }}
      >
        <style>{`
          @keyframes congFadeIn {
            from { opacity: 0; transform: scale(0.9) translateY(10px); }
            to { opacity: 1; transform: scale(1) translateY(0); }
          }
          @keyframes congGlow {
            0%,100% { box-shadow: 0 0 30px rgba(61,217,160,0.2), 0 0 60px rgba(255,45,139,0.1); }
            50% { box-shadow: 0 0 50px rgba(61,217,160,0.35), 0 0 80px rgba(255,45,139,0.2); }
          }
          @keyframes starFloat {
            0%,100% { transform: translateY(0) rotate(0deg); opacity: 0.7; }
            50% { transform: translateY(-8px) rotate(180deg); opacity: 1; }
          }
          @keyframes replayDots {
            0%,20% { opacity: 0.3; }
            50% { opacity: 1; }
            80%,100% { opacity: 0.3; }
          }
        `}</style>

        {/* Background glow */}
        <div
          style={{
            position: "absolute",
            width: "200px",
            height: "200px",
            borderRadius: "50%",
            background: "radial-gradient(ellipse, rgba(61,217,160,0.08) 0%, transparent 70%)",
            pointerEvents: "none",
          }}
        />

        {/* Card */}
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            gap: "0.85rem",
            padding: "1.75rem 2rem",
            background: "rgba(13,13,30,0.9)",
            border: "1px solid rgba(61,217,160,0.25)",
            borderRadius: "1.25rem",
            animation: "congFadeIn 0.6s ease both, congGlow 3s ease-in-out 0.5s infinite",
            position: "relative",
          }}
        >
          {/* Decorative stars */}
          {["✦", "✦", "✦"].map((s, i) => (
            <span
              key={i}
              style={{
                position: "absolute",
                fontSize: "0.6rem",
                color: i === 1 ? "#ff2d8b" : "#3dd9a0",
                opacity: 0.6,
                animation: `starFloat ${2 + i * 0.5}s ease-in-out ${i * 0.3}s infinite`,
                top: i === 0 ? "8px" : i === 1 ? "14px" : "30px",
                left: i === 0 ? "16px" : i === 1 ? "unset" : "20px",
                right: i === 1 ? "16px" : "unset",
              }}
            >
              {s}
            </span>
          ))}

          <span style={{ fontSize: "1.6rem" }}>🎉</span>

          <h3
            className="gradient-text"
            style={{
              fontFamily: "var(--font-syne)",
              fontSize: "1.15rem",
              fontWeight: 800,
              letterSpacing: "-0.02em",
              margin: 0,
            }}
          >
            Puzzle Solved!
          </h3>

          {/* Score grid */}
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "1fr 1fr 1fr",
              gap: "1px",
              background: "#1c1c38",
              borderRadius: "0.6rem",
              overflow: "hidden",
              width: "100%",
            }}
          >
            {[
              { label: "Moves", value: `${moveCount}` },
              { label: "Time", value: `${elapsedSec}s` },
              { label: "Score", value: `${score.toLocaleString()}` },
            ].map((item) => (
              <div
                key={item.label}
                style={{
                  background: "#0d0d1e",
                  padding: "0.6rem 0.5rem",
                  textAlign: "center",
                }}
              >
                <div
                  style={{
                    fontFamily: "var(--font-syne)",
                    fontSize: "1rem",
                    fontWeight: 800,
                    color: "#3dd9a0",
                    marginBottom: "0.1rem",
                  }}
                >
                  {item.value}
                </div>
                <div
                  style={{
                    fontFamily: "var(--font-outfit)",
                    fontSize: "0.6rem",
                    color: "#666688",
                    fontWeight: 500,
                    textTransform: "uppercase",
                    letterSpacing: "0.08em",
                  }}
                >
                  {item.label}
                </div>
              </div>
            ))}
          </div>

          {/* Replaying indicator */}
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: "0.4rem",
              marginTop: "0.25rem",
            }}
          >
            {[0, 1, 2].map((i) => (
              <div
                key={i}
                style={{
                  width: 4,
                  height: 4,
                  borderRadius: "50%",
                  background: "#ff2d8b",
                  animation: `replayDots 1.2s ease-in-out ${i * 0.2}s infinite`,
                }}
              />
            ))}
            <span
              style={{
                fontFamily: "var(--font-syne)",
                fontSize: "0.6rem",
                fontWeight: 600,
                color: "#555577",
                letterSpacing: "0.05em",
                marginLeft: "0.2rem",
              }}
            >
              Replaying…
            </span>
          </div>
        </div>
      </div>
    );
  }

  // ═══════════════════════════════════════════════════════
  // PHASE: SOLVING (main board with hand cursor)
  // ═══════════════════════════════════════════════════════
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        gap: "0.875rem",
      }}
    >
      <div
        style={{
          position: "relative",
          width: CW,
          height: CH,
          userSelect: "none",
        }}
      >
        {/* Grid background */}
        <div
          style={{
            position: "absolute",
            inset: 0,
            backgroundImage:
              "linear-gradient(rgba(28,28,56,0.18) 1px,transparent 1px),linear-gradient(90deg,rgba(28,28,56,0.18) 1px,transparent 1px)",
            backgroundSize: "20px 20px",
            borderRadius: "0.75rem",
            pointerEvents: "none",
          }}
        />

        {/* Tubes */}
        {tubes.map((tube, ti) => {
          const tx = TPAD + ti * (TW + TGAP);
          const isSol = solved.has(ti);
          const isHL = srcHL === ti;
          return (
            <div
              key={ti}
              style={{
                position: "absolute",
                left: tx,
                top: TTOP,
                width: TW,
                height: TH,
                borderRadius: "0 0 20px 20px",
                border: `2px solid ${
                  isSol
                    ? "rgba(61,217,160,0.5)"
                    : isHL
                    ? "rgba(255,45,139,0.55)"
                    : "#1c1c38"
                }`,
                borderTop: "none",
                background: isSol
                  ? "rgba(61,217,160,0.04)"
                  : isHL
                  ? "rgba(255,45,139,0.04)"
                  : "rgba(10,10,24,0.7)",
                transition: "border-color 0.3s, background 0.3s",
                overflow: "hidden",
              }}
            >
              {isSol && (
                <div
                  style={{
                    position: "absolute",
                    bottom: 0,
                    left: 0,
                    right: 0,
                    height: "40%",
                    background:
                      "radial-gradient(ellipse at 50% 100%, rgba(61,217,160,0.12), transparent 70%)",
                    pointerEvents: "none",
                  }}
                />
              )}
              {tube.map((ball, bi) => (
                <div
                  key={bi}
                  style={{
                    position: "absolute",
                    left: "50%",
                    bottom: BPAD + bi * BSTEP,
                    transform: "translateX(-50%)",
                    width: BALL,
                    height: BALL,
                    borderRadius: "50%",
                    background: `radial-gradient(circle at 34% 32%, ${CLR[ball]}ee, ${CLR[ball]}77)`,
                    boxShadow: `0 0 10px ${GLW[ball]}, inset 0 -2px 5px rgba(0,0,0,0.28)`,
                    opacity: srcHL === ti && bi === tube.length - 1 ? 0 : 1,
                    transition: "opacity 0.06s",
                  }}
                />
              ))}
            </div>
          );
        })}

        {/* Tube open-top accents */}
        {tubes.map((_, ti) => (
          <div
            key={`c${ti}`}
            style={{
              position: "absolute",
              left: TPAD + ti * (TW + TGAP) + 2,
              top: TTOP - 1,
              width: TW - 4,
              height: 2,
              background: solved.has(ti)
                ? "linear-gradient(90deg, transparent, rgba(61,217,160,0.35), transparent)"
                : srcHL === ti
                ? "linear-gradient(90deg, transparent, rgba(255,45,139,0.4), transparent)"
                : "transparent",
              transition: "background 0.3s",
              borderRadius: 1,
            }}
          />
        ))}

        {/* Floating ball (being carried) */}
        {fb && (
          <div
            style={{
              position: "absolute",
              width: BALL + 8,
              height: BALL + 8,
              borderRadius: "50%",
              background: `radial-gradient(circle at 33% 32%, ${CLR[fb.c]}ff, ${CLR[fb.c]}99)`,
              boxShadow: `0 0 28px ${GLW[fb.c]}, 0 6px 18px rgba(0,0,0,0.45), inset 0 -3px 6px rgba(0,0,0,0.2)`,
              left: fb.x - (BALL + 8) / 2,
              top: fb.y - (BALL + 8) / 2,
              transition:
                fbDur > 0
                  ? `left ${fbDur}ms cubic-bezier(.42,0,.22,1.1), top ${fbDur}ms cubic-bezier(.42,0,.22,1.1)`
                  : "none",
              zIndex: 10,
              pointerEvents: "none",
            }}
          />
        )}

        {/* Hand Cursor */}
        <HandCursor x={cur.x} y={cur.y} dur={curDur} />

        {/* DONE labels */}
        {tubes.map((tube, ti) => (
          <div
            key={`l${ti}`}
            style={{
              position: "absolute",
              left: TPAD + ti * (TW + TGAP),
              top: TTOP + TH + 8,
              width: TW,
              textAlign: "center",
              fontFamily: "var(--font-syne)",
              fontSize: "0.58rem",
              fontWeight: 700,
              letterSpacing: "0.06em",
              color: solved.has(ti) && tube[0] ? CLR[tube[0]] : "transparent",
              transition: "color 0.4s",
            }}
          >
            {solved.has(ti) ? "DONE" : ""}
          </div>
        ))}
      </div>

      {/* Status */}
      <div
        style={{
          fontFamily: "var(--font-syne)",
          fontSize: "0.72rem",
          fontWeight: 600,
          letterSpacing: "0.05em",
          color: "#555577",
          display: "flex",
          alignItems: "center",
          gap: "0.75rem",
          minHeight: "1.2rem",
        }}
      >
        <span style={{ display: "flex", alignItems: "center", gap: "0.3rem" }}>
          <span
            style={{
              display: "inline-block",
              width: 6,
              height: 6,
              borderRadius: "50%",
              background: "#ff2d8b",
              boxShadow: "0 0 8px #ff2d8b99",
              animation: "pulseGlow 1.5s ease-in-out infinite",
            }}
          />
          Solving…
        </span>
        <span style={{ color: "#444466" }}>|</span>
        <span>Moves: {moveCount}</span>
      </div>
    </div>
  );
}

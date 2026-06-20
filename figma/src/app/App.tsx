import { useState, useEffect } from "react";
import {
  Home, Calendar, QrCode, Users, User, MapPin, Clock,
  Heart, ChevronRight, ArrowLeft, CheckCircle, Send, Award,
  Activity, X, Eye, Edit2, MessageSquare, Plus, BarChart2,
  Navigation, Star, Search,
} from "lucide-react";
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell,
} from "recharts";

// ─── Sentry Design Tokens ────────────────────────────────────────────────────

const C = {
  primary:           "#150f23",   // midnight violet — CTA on light surfaces
  inkDeep:           "#1f1633",   // dark canvas + body ink on light
  onPrimary:         "#ffffff",
  accentLime:        "#c2ef4e",   // keyword chip only — one per viewport
  accentPink:        "#fa7faa",   // sticker/mascot accents
  accentViolet:      "#6a5fc1",
  accentVioletDeep:  "#422082",
  accentVioletMid:   "#79628c",   // filter-chip / tag fills
  canvasDark:        "#1f1633",   // marketing canvas
  surfaceNight:      "#150f23",   // cards on dark, code blocks
  canvasLight:       "#ffffff",   // pricing / admin canvas
  hairlineViolet:    "#362d59",   // borders on dark
  hairlineCool:      "#cfcfdb",   // form field borders
  hairlineCloud:     "#e5e7eb",   // borders on light
  onDarkMuted:       "#bdb8c0",   // secondary text on dark
  onDarkFaint:       "#3f3849",   // ghost fill on dark
  surfacePressLight: "#f0f0f0",
};

// ─── Typography ───────────────────────────────────────────────────────────────

const DISPLAY: React.CSSProperties = {
  fontFamily: "'Space Grotesk', 'Rubik', system-ui, sans-serif",
  fontWeight: 700,
  lineHeight: 1.2,
};
const RUBIK: React.CSSProperties = {
  fontFamily: "'Rubik', -apple-system, system-ui, sans-serif",
};
const BTN_CAP: React.CSSProperties = {
  ...RUBIK, fontSize: 14, fontWeight: 700,
  lineHeight: 1.14, letterSpacing: "0.2px", textTransform: "uppercase",
};
const BODY_LG: React.CSSProperties  = { ...RUBIK, fontSize: 16, fontWeight: 400, lineHeight: 2.0 };
const BODY_MD: React.CSSProperties  = { ...RUBIK, fontSize: 16, fontWeight: 500, lineHeight: 1.5 };
const CAPTION: React.CSSProperties  = { ...RUBIK, fontSize: 14, fontWeight: 400, lineHeight: 1.43 };
const MICRO: React.CSSProperties    = { ...RUBIK, fontSize: 10, fontWeight: 600, lineHeight: 1.8, letterSpacing: "0.25px", textTransform: "uppercase" };

// ─── Types ────────────────────────────────────────────────────────────────────

type Screen = "splash" | "home" | "events" | "detail" | "checkin" | "interaction" | "profile" | "admin";
type EventStatus = "upcoming" | "live" | "ended";

interface EventData {
  id: number; name: string; sport: string; sportEmoji: string;
  date: string; time: string; venue: string;
  teams: [string, string] | null; status: EventStatus;
  score: [number, number] | null; checkins: number; capacity: number;
  imageId: string; imageAlt: string;
}

// ─── Data ─────────────────────────────────────────────────────────────────────

const EVENTS: EventData[] = [
  { id: 1, name: "Final Municipal de Futsal", sport: "Futsal", sportEmoji: "⚽",
    date: "22 Jun", time: "19h30", venue: "Ginásio Jones Minosso",
    teams: ["Lages Futsal", "Serrano FC"], status: "upcoming", score: null,
    checkins: 342, capacity: 500, imageId: "1574629810360-7efbbe195018", imageAlt: "Partida de futsal" },
  { id: 2, name: "Copa Lages de Vôlei", sport: "Vôlei", sportEmoji: "🏐",
    date: "23 Jun", time: "16h", venue: "Ginásio Ivo Silveira",
    teams: ["Atlética Serra", "União Vôlei"], status: "live", score: [2, 1],
    checkins: 218, capacity: 350, imageId: "1612872087720-bb876e2e67d1", imageAlt: "Jogo de vôlei" },
  { id: 3, name: "Corrida da Serra", sport: "Corrida", sportEmoji: "🏃",
    date: "29 Jun", time: "8h", venue: "Centro de Lages",
    teams: null, status: "upcoming", score: null,
    checkins: 87, capacity: 200, imageId: "1571008887538-b36bb32f4571", imageAlt: "Corrida de rua" },
  { id: 4, name: "Copa Municipal de Basquete", sport: "Basquete", sportEmoji: "🏀",
    date: "5 Jul", time: "18h", venue: "Arena Lages",
    teams: ["Basquete Lages", "Planalto BC"], status: "upcoming", score: null,
    checkins: 56, capacity: 400, imageId: "1546519638-68e109498ffc", imageAlt: "Partida de basquete" },
  { id: 5, name: "Torneio de Futebol Society", sport: "Futebol", sportEmoji: "⚽",
    date: "12 Jul", time: "10h", venue: "Campo Municipal",
    teams: null, status: "upcoming", score: null,
    checkins: 23, capacity: 300, imageId: "1508098682722-e99c43a406b2", imageAlt: "Campo de futebol" },
  { id: 6, name: "Copa Lages de Vôlei — Semi", sport: "Vôlei", sportEmoji: "🏐",
    date: "15 Mai", time: "15h", venue: "Ginásio Ivo Silveira",
    teams: ["Atlética Serra", "Pinheiros VB"], status: "ended", score: [3, 1],
    checkins: 290, capacity: 350, imageId: "1612872087720-bb876e2e67d1", imageAlt: "Semifinal de vôlei" },
];

const SPORTS = ["Todos", "Futsal", "Vôlei", "Corrida", "Basquete", "Futebol"];

function imgUrl(id: string, w: number, h: number) {
  return `https://images.unsplash.com/photo-${id}?w=${w}&h=${h}&fit=crop&auto=format&q=80`;
}

// ─── Shared primitives ────────────────────────────────────────────────────────

/** Starfield texture — subtle pinprick dots on dark canvas */
function Starfield({ children, className = "", style = {} }: {
  children?: React.ReactNode; className?: string; style?: React.CSSProperties;
}) {
  return (
    <div
      className={className}
      style={{
        backgroundImage: "radial-gradient(rgba(255,255,255,0.13) 1px, transparent 1px)",
        backgroundSize: "22px 22px",
        ...style,
      }}
    >
      {children}
    </div>
  );
}

/** Lime keyword highlight chip — one per viewport max */
function LimeChip({ children }: { children: React.ReactNode }) {
  return (
    <span style={{
      backgroundColor: C.accentLime,
      color: C.inkDeep,
      borderRadius: 4,
      padding: "0 12px",
      display: "inline-block",
      lineHeight: "inherit",
    }}>
      {children}
    </span>
  );
}

/** button-inverted — dominant CTA on dark canvas */
function BtnInverted({ children, onClick, className = "" }: {
  children: React.ReactNode; onClick?: () => void; className?: string;
}) {
  return (
    <button
      onClick={onClick}
      className={`inline-flex items-center justify-center gap-2 active:opacity-70 transition-opacity ${className}`}
      style={{
        ...BTN_CAP,
        backgroundColor: C.onPrimary, color: C.inkDeep,
        borderRadius: 8, padding: "12px 16px",
        boxShadow: "rgba(0,0,0,0.08) 0 2px 8px 0",
      }}
    >
      {children}
    </button>
  );
}

/** button-primary — dominant CTA on light canvas (admin) */
function BtnPrimary({ children, onClick, className = "" }: {
  children: React.ReactNode; onClick?: () => void; className?: string;
}) {
  return (
    <button
      onClick={onClick}
      className={`inline-flex items-center justify-center gap-2 active:opacity-70 transition-opacity ${className}`}
      style={{
        ...BTN_CAP,
        backgroundColor: C.primary, color: C.onPrimary,
        borderRadius: 8, padding: "12px 16px",
      }}
    >
      {children}
    </button>
  );
}

/** button-ghost-on-dark — secondary CTA on dark canvas */
function BtnGhost({ children, onClick, className = "" }: {
  children: React.ReactNode; onClick?: () => void; className?: string;
}) {
  return (
    <button
      onClick={onClick}
      className={`inline-flex items-center justify-center gap-2 active:opacity-60 transition-opacity ${className}`}
      style={{
        ...BTN_CAP,
        backgroundColor: C.onDarkFaint, color: C.onPrimary,
        borderRadius: 12, padding: "10px 16px",
        border: `1px solid ${C.hairlineViolet}`,
      }}
    >
      {children}
    </button>
  );
}

/** button-violet-token — filter chips on dark surfaces */
function VioletChip({ label, active, onClick }: {
  label: string; active: boolean; onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className="flex-shrink-0 active:opacity-70 transition-opacity"
      style={{
        ...BTN_CAP,
        backgroundColor: active ? C.onPrimary : C.accentVioletMid,
        color: active ? C.inkDeep : C.onPrimary,
        borderRadius: 12, padding: "8px 16px",
        border: `1px solid ${active ? "transparent" : "#5a4a6a"}`,
      }}
    >
      {label}
    </button>
  );
}

/** pill-neutral-dark — status badge on dark canvas */
function StatusBadge({ status }: { status: EventStatus }) {
  if (status === "live") {
    return (
      <span style={{ ...MICRO, color: C.accentLime, display: "flex", alignItems: "center", gap: 4 }}>
        <span className="w-1.5 h-1.5 rounded-full animate-pulse inline-block" style={{ backgroundColor: C.accentLime }} />
        Ao Vivo
      </span>
    );
  }
  const isEnded = status === "ended";
  return (
    <span
      style={{
        ...MICRO,
        backgroundColor: C.surfaceNight, color: isEnded ? C.onDarkMuted : C.onPrimary,
        borderRadius: 4, padding: "4px 8px", display: "inline-block",
      }}
    >
      {isEnded ? "Encerrado" : "Próximo"}
    </span>
  );
}

/** QR Code — algorithmic generation */
function QRCodeVisual() {
  const N = 21, CELL = 9;
  const getCell = (r: number, c: number): boolean => {
    const finder = (or: number, oc: number) => {
      const dr = r - or, dc = c - oc;
      if (dr < 0 || dr > 6 || dc < 0 || dc > 6) return null;
      if (dr === 0 || dr === 6 || dc === 0 || dc === 6) return true;
      if (dr >= 2 && dr <= 4 && dc >= 2 && dc <= 4) return true;
      return false;
    };
    for (const [or, oc] of [[0, 0], [0, 14], [14, 0]] as [number, number][]) {
      const v = finder(or, oc); if (v !== null) return v;
    }
    if (r === 6) return c % 2 === 0;
    if (c === 6) return r % 2 === 0;
    return (r * 13 + c * 7 + ((r ^ c) * 5)) % 3 === 0;
  };
  return (
    <div style={{ padding: 16, backgroundColor: C.onPrimary, borderRadius: 8, display: "inline-block" }}>
      <div style={{ display: "grid", gridTemplateColumns: `repeat(${N}, ${CELL}px)`, gap: 1 }}>
        {Array.from({ length: N }).map((_, r) =>
          Array.from({ length: N }).map((_, c) => (
            <div key={`${r}-${c}`} style={{ width: CELL, height: CELL, borderRadius: 2,
              backgroundColor: getCell(r, c) ? C.primary : C.onPrimary }} />
          ))
        )}
      </div>
    </div>
  );
}

// ─── Bottom Nav ───────────────────────────────────────────────────────────────

const NAV_ITEMS = [
  { screen: "home" as Screen, Icon: Home, label: "Início" },
  { screen: "events" as Screen, Icon: Calendar, label: "Eventos" },
  { screen: "checkin" as Screen, Icon: QrCode, label: "Check-in", center: true },
  { screen: "interaction" as Screen, Icon: Users, label: "Times" },
  { screen: "profile" as Screen, Icon: User, label: "Perfil" },
];

function BottomNav({ current, onNavigate }: { current: Screen; onNavigate: (s: Screen) => void }) {
  return (
    <div className="flex-shrink-0 flex items-end" style={{
      backgroundColor: C.surfaceNight,
      borderTop: `1px solid ${C.hairlineViolet}`,
      paddingBottom: 8,
    }}>
      {NAV_ITEMS.map(({ screen, Icon, label, center }) => {
        const active = current === screen;
        return (
          <button key={screen} onClick={() => onNavigate(screen)}
            className="flex-1 flex flex-col items-center py-2 gap-0.5">
            {center ? (
              <>
                <div className="w-12 h-12 rounded-full flex items-center justify-center -mt-6"
                  style={{ backgroundColor: C.onPrimary, boxShadow: "rgba(0,0,0,0.18) 0 0.5rem 1.5rem" }}>
                  <Icon className="w-5 h-5" style={{ color: C.inkDeep }} />
                </div>
                <span style={{ ...MICRO, color: active ? C.accentLime : C.onDarkMuted, marginTop: 2 }}>
                  {label}
                </span>
              </>
            ) : (
              <>
                <Icon className="w-5 h-5" style={{ color: active ? C.onPrimary : C.onDarkMuted }} />
                <span style={{ ...MICRO, color: active ? C.onPrimary : C.onDarkMuted }}>
                  {label}
                </span>
                {active && <div className="w-1 h-1 rounded-full" style={{ backgroundColor: C.accentLime }} />}
              </>
            )}
          </button>
        );
      })}
    </div>
  );
}

// ─── Splash ───────────────────────────────────────────────────────────────────

function SplashScreen({ onNext }: { onNext: () => void }) {
  useEffect(() => { const t = setTimeout(onNext, 2800); return () => clearTimeout(t); }, [onNext]);

  return (
    <Starfield className="flex-1 flex flex-col" style={{ backgroundColor: C.inkDeep }}>
      {/* Floating mascots */}
      <div style={{ position: "absolute", right: 24, top: 80, fontSize: 56, transform: "rotate(15deg)", zIndex: 5 }}>🏆</div>
      <div style={{ position: "absolute", left: 16, bottom: 120, fontSize: 40, transform: "rotate(-12deg)", zIndex: 5 }}>⚡</div>

      <div className="flex-1 flex flex-col items-center justify-center px-8 relative z-10">
        {/* Eyebrow */}
        <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 16 }}>
          Plataforma Esportiva · Lages/SC
        </p>

        {/* Display headline with lime chip */}
        <h1 style={{ ...DISPLAY, fontSize: 72, color: C.onPrimary, textAlign: "center", marginBottom: 24 }}>
          ARENA{" "}
          <LimeChip>LAGES</LimeChip>
        </h1>

        <p style={{ ...BODY_LG, color: C.onDarkMuted, textAlign: "center", marginBottom: 36, maxWidth: 280 }}>
          Viva o esporte mais de perto. Eventos, check-in e torcida em um só lugar.
        </p>

        <BtnInverted onClick={onNext}>Entrar na Arena →</BtnInverted>
      </div>

      {/* Loading bar */}
      <div style={{ height: 2, backgroundColor: C.hairlineViolet }}>
        <div className="h-full" style={{
          backgroundColor: C.accentLime,
          animation: "grow 2.7s linear forwards", width: 0,
        }} />
      </div>
      <style>{`@keyframes grow { to { width: 100%; } }`}</style>
    </Starfield>
  );
}

// ─── Home ─────────────────────────────────────────────────────────────────────

function HomeScreen({ onNavigate, onSelectEvent }: {
  onNavigate: (s: Screen) => void; onSelectEvent: (e: EventData) => void;
}) {
  const live = EVENTS.find((e) => e.status === "live");
  const upcoming = EVENTS.filter((e) => e.status === "upcoming");

  return (
    <div className="flex-1 overflow-y-auto" style={{ scrollbarWidth: "none", backgroundColor: C.canvasDark }}>
      {/* Top nav — dark variant */}
      <div className="flex items-center justify-between px-5 pt-14 pb-4">
        <h1 style={{ ...DISPLAY, fontSize: 20, color: C.onPrimary, letterSpacing: 1 }}>
          ARENA LAGES
        </h1>
        <div className="flex items-center gap-2">
          <button className="w-9 h-9 rounded-full flex items-center justify-center"
            style={{ backgroundColor: C.onDarkFaint }}>
            <Search className="w-4 h-4" style={{ color: C.onPrimary }} />
          </button>
          <button className="w-9 h-9 rounded-full flex items-center justify-center"
            style={{ backgroundColor: C.onDarkFaint }}
            onClick={() => onNavigate("admin")}>
            <BarChart2 className="w-4 h-4" style={{ color: C.onPrimary }} />
          </button>
        </div>
      </div>

      {/* Campaign hero — starfield + display headline */}
      <Starfield style={{ margin: "0 16px 24px", borderRadius: 18, backgroundColor: C.surfaceNight, padding: "32px 24px", position: "relative", overflow: "hidden" }}>
        {/* Mascot */}
        <div style={{ position: "absolute", right: 20, top: 20, fontSize: 52, transform: "rotate(8deg)" }}>🏟️</div>

        <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 12 }}>Temporada 2025</p>
        <h2 style={{ ...DISPLAY, fontSize: 52, color: C.onPrimary, lineHeight: 1.05, marginBottom: 20 }}>
          VIVA O{" "}
          <LimeChip>ESPORTE</LimeChip>
          <br />DE LAGES.
        </h2>
        <p style={{ ...BODY_LG, color: C.onDarkMuted, marginBottom: 24, maxWidth: 230 }}>
          Encontre eventos, faça check-in e interaja com suas equipes favoritas.
        </p>
        <div className="flex gap-3">
          <BtnInverted onClick={() => onNavigate("events")}>Ver eventos</BtnInverted>
          <BtnGhost onClick={() => onNavigate("checkin")}>Check-in</BtnGhost>
        </div>
      </Starfield>

      {/* Live event */}
      {live && (
        <div style={{ marginBottom: 24 }}>
          <div className="flex items-center gap-2 px-5 mb-3">
            <span className="w-2 h-2 rounded-full animate-pulse" style={{ backgroundColor: C.accentLime }} />
            <span style={{ ...MICRO, color: C.accentLime }}>Acontecendo agora</span>
          </div>
          <button onClick={() => onSelectEvent(live)} className="w-full text-left mx-4" style={{ width: "calc(100% - 32px)" }}>
            <div style={{
              backgroundColor: C.surfaceNight, borderRadius: 18,
              border: `1px solid ${C.hairlineViolet}`, overflow: "hidden",
            }}>
              <div style={{ height: 160, overflow: "hidden", position: "relative" }}>
                <img src={imgUrl(live.imageId, 700, 320)} alt={live.imageAlt}
                  className="w-full h-full object-cover" />
                <div style={{
                  position: "absolute", inset: 0,
                  background: "linear-gradient(to right, rgba(21,15,35,0.75) 0%, transparent 60%)",
                }} />
                {live.teams && live.score && (
                  <div className="absolute inset-0 flex items-center px-5">
                    <div>
                      <StatusBadge status="live" />
                      <div style={{ ...DISPLAY, fontSize: 44, color: C.onPrimary, marginTop: 4 }}>
                        {live.score[0]} <span style={{ color: C.onDarkMuted }}>×</span> {live.score[1]}
                      </div>
                      <p style={{ ...CAPTION, color: C.onDarkMuted }}>{live.teams[0]} vs {live.teams[1]}</p>
                    </div>
                  </div>
                )}
              </div>
              <div className="px-4 py-3 flex items-center justify-between">
                <div>
                  <p style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>{live.name}</p>
                  <p style={{ ...CAPTION, color: C.onDarkMuted, display: "flex", alignItems: "center", gap: 4 }}>
                    <MapPin className="w-3 h-3" /> {live.venue}
                  </p>
                </div>
                <ChevronRight className="w-5 h-5" style={{ color: C.accentVioletMid }} />
              </div>
            </div>
          </button>
        </div>
      )}

      {/* Upcoming list */}
      <div className="px-5 mb-4 flex items-center justify-between">
        <span style={{ ...RUBIK, fontSize: 16, fontWeight: 600, color: C.onPrimary }}>Próximos Eventos</span>
        <button onClick={() => onNavigate("events")}
          style={{ ...CAPTION, color: C.accentVioletMid, textDecoration: "underline" }}>
          Ver todos
        </button>
      </div>

      <div className="px-4 pb-4 flex flex-col gap-3">
        {upcoming.slice(0, 3).map((ev) => (
          <button key={ev.id} onClick={() => onSelectEvent(ev)} className="w-full text-left"
            style={{
              backgroundColor: C.surfaceNight, borderRadius: 12,
              border: `1px solid ${C.hairlineViolet}`,
              display: "flex", alignItems: "center", gap: 12, padding: 12,
            }}>
            <div style={{ width: 56, height: 56, borderRadius: 8, overflow: "hidden", flexShrink: 0, backgroundColor: C.onDarkFaint }}>
              <img src={imgUrl(ev.imageId, 112, 112)} alt={ev.imageAlt} className="w-full h-full object-cover" />
            </div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <p style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600, whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
                {ev.name}
              </p>
              <p style={{ ...CAPTION, color: C.onDarkMuted }}>{ev.sport}</p>
              <p style={{ ...MICRO, color: C.accentVioletMid, marginTop: 2 }}>
                {ev.date} · {ev.time}
              </p>
            </div>
            <ChevronRight className="w-4 h-4 flex-shrink-0" style={{ color: C.hairlineViolet }} />
          </button>
        ))}
      </div>

      {/* Stats band */}
      <div className="mx-4 mb-8 grid grid-cols-3" style={{
        backgroundColor: C.surfaceNight, borderRadius: 12,
        border: `1px solid ${C.hairlineViolet}`,
      }}>
        {[
          { label: "Eventos", value: "12" },
          { label: "Check-ins", value: "1.2k" },
          { label: "Torcedores", value: "890" },
        ].map(({ label, value }, i) => (
          <div key={label} className="py-4 text-center" style={{
            borderRight: i < 2 ? `1px solid ${C.hairlineViolet}` : "none",
          }}>
            <div style={{ ...DISPLAY, fontSize: 32, color: C.onPrimary, lineHeight: 1 }}>{value}</div>
            <div style={{ ...MICRO, color: C.onDarkMuted, marginTop: 4 }}>{label}</div>
          </div>
        ))}
      </div>
    </div>
  );
}

// ─── Events ───────────────────────────────────────────────────────────────────

function EventsScreen({ onSelectEvent }: { onSelectEvent: (e: EventData) => void }) {
  const [filter, setFilter] = useState("Todos");
  const filtered = filter === "Todos" ? EVENTS : EVENTS.filter((e) => e.sport === filter);

  return (
    <div className="flex-1 overflow-y-auto" style={{ scrollbarWidth: "none", backgroundColor: C.canvasDark }}>
      <div className="px-5 pt-14 pb-5">
        <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 6 }}>Temporada 2025</p>
        <h1 style={{ ...DISPLAY, fontSize: 52, color: C.onPrimary }}>EVENTOS</h1>
      </div>

      {/* Filters — violet token chips */}
      <div className="px-5 mb-5 flex gap-2 overflow-x-auto" style={{ scrollbarWidth: "none" }}>
        {SPORTS.map((s) => (
          <VioletChip key={s} label={s} active={filter === s} onClick={() => setFilter(s)} />
        ))}
      </div>

      {/* Mascot at section junction */}
      <div style={{ position: "relative", height: 0, marginBottom: 8 }}>
        <div style={{ position: "absolute", right: 20, top: -24, fontSize: 40, transform: "rotate(-8deg)", zIndex: 5 }}>
          {filter === "Todos" ? "🏆" : filter === "Futsal" ? "⚽" : filter === "Vôlei" ? "🏐" : filter === "Corrida" ? "🏃" : filter === "Basquete" ? "🏀" : "⚽"}
        </div>
      </div>

      <div className="px-4 pb-8 flex flex-col gap-3">
        {filtered.map((ev) => (
          <button key={ev.id} onClick={() => onSelectEvent(ev)} className="w-full text-left"
            style={{
              backgroundColor: C.surfaceNight, borderRadius: 18,
              border: `1px solid ${C.hairlineViolet}`, overflow: "hidden",
            }}>
            <div style={{ height: 160, overflow: "hidden", position: "relative" }}>
              <img src={imgUrl(ev.imageId, 700, 320)} alt={ev.imageAlt} className="w-full h-full object-cover" />
              <div style={{
                position: "absolute", inset: 0,
                background: "linear-gradient(to top, rgba(21,15,35,0.8) 0%, transparent 60%)",
              }} />
              <div className="absolute bottom-3 left-4 flex items-center gap-2">
                <StatusBadge status={ev.status} />
              </div>
              {ev.status === "live" && ev.score && (
                <div className="absolute top-3 right-4"
                  style={{ ...DISPLAY, fontSize: 28, color: C.onPrimary }}>
                  {ev.score[0]} × {ev.score[1]}
                </div>
              )}
            </div>
            <div className="px-4 py-3">
              <p style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>{ev.name}</p>
              <p style={{ ...CAPTION, color: C.onDarkMuted, marginTop: 2 }}>{ev.sport}</p>
              <div className="flex flex-wrap gap-x-4 mt-2">
                <span style={{ ...MICRO, color: C.accentVioletMid, display: "flex", alignItems: "center", gap: 3 }}>
                  <Clock className="w-3 h-3" />{ev.date} · {ev.time}
                </span>
                <span style={{ ...MICRO, color: C.accentVioletMid, display: "flex", alignItems: "center", gap: 3 }}>
                  <MapPin className="w-3 h-3" />{ev.venue}
                </span>
              </div>
              {ev.teams && (
                <p style={{ ...CAPTION, color: C.onDarkMuted, marginTop: 6 }}>
                  {ev.teams[0]} <span style={{ color: C.hairlineViolet }}>vs</span> {ev.teams[1]}
                </p>
              )}
              <div className="flex items-center justify-between mt-3 pt-3"
                style={{ borderTop: `1px solid ${C.hairlineViolet}` }}>
                <span style={{ ...MICRO, color: C.onDarkMuted }}>
                  {ev.checkins}/{ev.capacity} confirmados
                </span>
                <span style={{ ...MICRO, color: C.accentVioletMid }}>Ver detalhes →</span>
              </div>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}

// ─── Event Detail ─────────────────────────────────────────────────────────────

function EventDetailScreen({ event, onBack, onNavigate }: {
  event: EventData; onBack: () => void; onNavigate: (s: Screen) => void;
}) {
  const pct = Math.min((event.checkins / event.capacity) * 100, 100);

  return (
    <div className="flex-1 overflow-y-auto" style={{ scrollbarWidth: "none", backgroundColor: C.canvasDark }}>
      {/* Hero image */}
      <div style={{ height: 280, position: "relative", overflow: "hidden" }}>
        <img src={imgUrl(event.imageId, 800, 560)} alt={event.imageAlt} className="w-full h-full object-cover" />
        <div style={{
          position: "absolute", inset: 0,
          background: "linear-gradient(to top, rgba(21,15,35,0.9) 0%, rgba(31,22,51,0.2) 60%, transparent 100%)",
        }} />
        <button onClick={onBack}
          className="absolute top-12 left-4 w-10 h-10 rounded-full flex items-center justify-center"
          style={{ backgroundColor: "rgba(31,22,51,0.7)", backdropFilter: "blur(8px)" }}>
          <ArrowLeft className="w-5 h-5" style={{ color: C.onPrimary }} />
        </button>
        <div className="absolute bottom-0 left-0 px-5 pb-5">
          <StatusBadge status={event.status} />
          <h1 style={{ ...DISPLAY, fontSize: 36, color: C.onPrimary, marginTop: 6 }}>
            {event.name.toUpperCase()}
          </h1>
        </div>
      </div>

      <div className="px-5 py-5 flex flex-col gap-4">
        {/* Info grid */}
        <div className="grid grid-cols-2 gap-3">
          {[
            { label: "Modalidade", val: event.sport },
            { label: "Data e Hora", val: `${event.date} · ${event.time}` },
          ].map(({ label, val }) => (
            <div key={label} style={{
              backgroundColor: C.surfaceNight, borderRadius: 12,
              border: `1px solid ${C.hairlineViolet}`, padding: "12px 14px",
            }}>
              <p style={{ ...MICRO, color: C.onDarkMuted, marginBottom: 4 }}>{label.toUpperCase()}</p>
              <p style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>{val}</p>
            </div>
          ))}
        </div>

        <div style={{
          backgroundColor: C.surfaceNight, borderRadius: 12,
          border: `1px solid ${C.hairlineViolet}`, padding: "12px 14px",
        }}>
          <p style={{ ...MICRO, color: C.onDarkMuted, marginBottom: 4 }}>LOCAL</p>
          <p style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>{event.venue}</p>
        </div>

        {/* Teams / Score */}
        {event.teams && (
          <div style={{
            backgroundColor: C.surfaceNight, borderRadius: 18,
            border: `1px solid ${C.hairlineViolet}`, padding: "20px 16px",
          }}>
            <p style={{ ...MICRO, color: C.onDarkMuted, marginBottom: 16 }}>EQUIPES</p>
            <div className="flex items-center justify-between">
              <div className="text-center flex-1">
                <div className="w-14 h-14 rounded-xl flex items-center justify-center mx-auto mb-2 text-2xl"
                  style={{ backgroundColor: C.onDarkFaint }}>{event.sportEmoji}</div>
                <p style={{ ...CAPTION, color: C.onPrimary, fontWeight: 600 }}>{event.teams[0]}</p>
              </div>
              <div className="text-center px-2">
                {event.score
                  ? <span style={{ ...DISPLAY, fontSize: 44, color: event.status === "live" ? C.accentLime : C.onPrimary }}>
                      {event.score[0]} × {event.score[1]}
                    </span>
                  : <span style={{ ...BODY_MD, color: C.onDarkMuted }}>vs</span>}
              </div>
              <div className="text-center flex-1">
                <div className="w-14 h-14 rounded-xl flex items-center justify-center mx-auto mb-2 text-2xl"
                  style={{ backgroundColor: C.onDarkFaint }}>{event.sportEmoji}</div>
                <p style={{ ...CAPTION, color: C.onPrimary, fontWeight: 600 }}>{event.teams[1]}</p>
              </div>
            </div>
          </div>
        )}

        {/* Attendance */}
        <div style={{
          backgroundColor: C.surfaceNight, borderRadius: 12,
          border: `1px solid ${C.hairlineViolet}`, padding: "12px 14px",
        }}>
          <div className="flex justify-between mb-2">
            <span style={{ ...MICRO, color: C.onDarkMuted }}>PRESENÇA CONFIRMADA</span>
            <span style={{ ...MICRO, color: C.onPrimary }}>{event.checkins}/{event.capacity}</span>
          </div>
          <div style={{ height: 2, backgroundColor: C.hairlineViolet, borderRadius: 1 }}>
            <div style={{ height: "100%", width: `${pct}%`, backgroundColor: C.accentLime, borderRadius: 1 }} />
          </div>
        </div>

        {/* Sponsors */}
        <div style={{
          backgroundColor: C.surfaceNight, borderRadius: 12,
          border: `1px solid ${C.hairlineViolet}`, padding: "12px 14px",
        }}>
          <p style={{ ...MICRO, color: C.onDarkMuted, marginBottom: 10 }}>PATROCINADORES</p>
          <div className="flex flex-wrap gap-2">
            {["Prefeitura de Lages", "Serrano Esportes", "Serra Sports"].map((s) => (
              <span key={s} style={{
                ...MICRO, color: C.onDarkMuted,
                backgroundColor: C.onDarkFaint, borderRadius: 4,
                padding: "4px 8px", border: `1px solid ${C.hairlineViolet}`,
              }}>{s}</span>
            ))}
          </div>
        </div>

        {/* Actions */}
        <div className="flex flex-col gap-3 pb-4">
          {event.status !== "ended" && (
            <BtnInverted onClick={() => onNavigate("checkin")} className="w-full">
              <QrCode className="w-4 h-4" />Fazer Check-in
            </BtnInverted>
          )}
          <div className="flex gap-3">
            <BtnGhost className="flex-1"><Navigation className="w-4 h-4" />Ver Rota</BtnGhost>
            <BtnGhost onClick={() => onNavigate("interaction")} className="flex-1">
              <Users className="w-4 h-4" />Interagir
            </BtnGhost>
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── Check-in ─────────────────────────────────────────────────────────────────

function CheckInScreen({ event, onBack, onNavigate }: {
  event: EventData; onBack: () => void; onNavigate: (s: Screen) => void;
}) {
  const [done, setDone] = useState(false);

  return (
    <div className="flex-1 overflow-y-auto" style={{ scrollbarWidth: "none", backgroundColor: C.canvasDark }}>
      <div className="flex items-center gap-3 px-5 pt-14 pb-5"
        style={{ borderBottom: `1px solid ${C.hairlineViolet}` }}>
        <button onClick={onBack} className="w-10 h-10 rounded-full flex items-center justify-center"
          style={{ backgroundColor: C.onDarkFaint }}>
          <ArrowLeft className="w-5 h-5" style={{ color: C.onPrimary }} />
        </button>
        <div>
          <h1 style={{ ...DISPLAY, fontSize: 28, color: C.onPrimary }}>CHECK-IN</h1>
          <p style={{ ...CAPTION, color: C.onDarkMuted }}>{event.name}</p>
        </div>
      </div>

      {!done ? (
        <div className="px-5 pt-5">
          {/* Event pill */}
          <div className="flex items-center gap-3 p-3 mb-5" style={{
            backgroundColor: C.surfaceNight, borderRadius: 12,
            border: `1px solid ${C.hairlineViolet}`,
          }}>
            <span className="text-2xl">{event.sportEmoji}</span>
            <div>
              <p style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>{event.name}</p>
              <p style={{ ...MICRO, color: C.onDarkMuted }}>{event.date} · {event.time} · {event.venue}</p>
            </div>
          </div>

          {/* QR Block — card-feature-dark */}
          <div className="flex flex-col items-center p-6 mb-5" style={{
            backgroundColor: C.surfaceNight, borderRadius: 18,
            border: `1px solid ${C.hairlineViolet}`,
          }}>
            <p style={{ ...CAPTION, color: C.onDarkMuted, marginBottom: 16 }}>
              Apresente na entrada do evento
            </p>
            <QRCodeVisual />
            <p style={{ ...MICRO, color: C.accentVioletMid, marginTop: 12, fontFamily: "Monaco, monospace" }}>
              TOR-2025-{String(event.id).padStart(3, "0")}-A4B7
            </p>
          </div>

          <div className="flex items-center gap-3 mb-5">
            <div style={{ flex: 1, height: 1, backgroundColor: C.hairlineViolet }} />
            <span style={{ ...MICRO, color: C.onDarkMuted }}>ou confirme manualmente</span>
            <div style={{ flex: 1, height: 1, backgroundColor: C.hairlineViolet }} />
          </div>

          <BtnGhost onClick={() => setDone(true)} className="w-full mb-8">
            Confirmar Presença
          </BtnGhost>
        </div>
      ) : (
        <div className="flex flex-col items-center px-5 py-10">
          {/* Mascot moment */}
          <div style={{ fontSize: 64, marginBottom: 8, transform: "rotate(5deg)" }}>🎉</div>
          <h2 style={{ ...DISPLAY, fontSize: 44, color: C.onPrimary, textAlign: "center", lineHeight: 1, marginBottom: 12 }}>
            CHECK-IN<br />REALIZADO!
          </h2>
          <p style={{ ...BODY_LG, color: C.onDarkMuted, textAlign: "center", marginBottom: 28 }}>
            Sua presença foi confirmada.<br />Aproveite o evento!
          </p>

          {[
            { label: "Pontos conquistados", value: "+50 PTS" },
            { label: "Total acumulado", value: "400 PTS" },
          ].map(({ label, value }) => (
            <div key={label} className="w-full flex items-center justify-between p-4 mb-2" style={{
              backgroundColor: C.surfaceNight, borderRadius: 12, border: `1px solid ${C.hairlineViolet}`,
            }}>
              <span style={{ ...BODY_MD, color: C.onDarkMuted }}>{label}</span>
              <span style={{ ...DISPLAY, fontSize: 28, color: C.accentLime }}>{value}</span>
            </div>
          ))}

          <div style={{ height: 16 }} />
          <BtnInverted onClick={() => onNavigate("interaction")} className="w-full">
            Interagir com a Torcida
          </BtnInverted>
          <button onClick={onBack} style={{ ...CAPTION, color: C.onDarkMuted, marginTop: 12, textDecoration: "underline" }}>
            Voltar ao evento
          </button>
        </div>
      )}
    </div>
  );
}

// ─── Interaction ──────────────────────────────────────────────────────────────

function InteractionScreen({ event, onBack }: { event: EventData; onBack: () => void }) {
  const [vote, setVote] = useState<string | null>(null);
  const [voted, setVoted] = useState(false);
  const [poll, setPoll] = useState<number | null>(null);
  const [msg, setMsg] = useState("");
  const [msgs, setMsgs] = useState(["Vai Lages Futsal! 🔥", "Que jogo incrível! ⚽", "Gol na segunda etapa! 💪"]);
  const [fav, setFav] = useState(false);

  const players = ["João Silva", "Carlos Matos", "Pedro Lima", "Rafael Costa"];
  const pollOpts = ["Mais de 3 gols", "Empate técnico", "Menos de 2 gols"];
  const pollPct = [45, 30, 25];

  const card: React.CSSProperties = {
    backgroundColor: C.surfaceNight, borderRadius: 18,
    border: `1px solid ${C.hairlineViolet}`, padding: "16px",
    marginBottom: 12,
  };

  return (
    <div className="flex-1 overflow-y-auto" style={{ scrollbarWidth: "none", backgroundColor: C.canvasDark }}>
      <div className="flex items-center gap-3 px-5 pt-14 pb-5"
        style={{ borderBottom: `1px solid ${C.hairlineViolet}` }}>
        <button onClick={onBack} className="w-10 h-10 rounded-full flex items-center justify-center"
          style={{ backgroundColor: C.onDarkFaint }}>
          <ArrowLeft className="w-5 h-5" style={{ color: C.onPrimary }} />
        </button>
        <div>
          <h1 style={{ ...DISPLAY, fontSize: 28, color: C.onPrimary }}>TORCIDA</h1>
          <p style={{ ...CAPTION, color: C.onDarkMuted }}>{event.name}</p>
        </div>
      </div>

      <div className="px-5 pt-5">
        {/* Favorite */}
        <div style={{ ...card, display: "flex", alignItems: "center", justifyContent: "space-between" }}>
          <div>
            <p style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>Favoritar Equipe</p>
            <p style={{ ...CAPTION, color: C.onDarkMuted }}>Acompanhe todos os jogos</p>
          </div>
          <button onClick={() => setFav((f) => !f)}
            className="w-10 h-10 rounded-full flex items-center justify-center"
            style={{ backgroundColor: fav ? C.accentPink : C.onDarkFaint, border: `1px solid ${C.hairlineViolet}` }}>
            <Heart className="w-5 h-5"
              style={{ color: fav ? C.inkDeep : C.onPrimary, fill: fav ? C.inkDeep : "none" }} />
          </button>
        </div>

        {/* Vote */}
        <div style={card}>
          <div className="flex items-center gap-2 mb-4">
            <Award className="w-4 h-4" style={{ color: C.accentLime }} />
            <span style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>Jogador da Partida</span>
          </div>
          <div className="flex flex-col gap-2">
            {players.map((p) => (
              <button key={p} onClick={() => !voted && setVote(p)}
                className="flex items-center justify-between px-3 py-2.5"
                style={{
                  borderRadius: 8, border: `1px solid ${vote === p ? C.accentLime : C.hairlineViolet}`,
                  backgroundColor: vote === p ? "rgba(194,239,78,0.1)" : "transparent",
                }}>
                <span style={{ ...BODY_MD, color: vote === p ? C.accentLime : C.onPrimary }}>{p}</span>
                {vote === p && <CheckCircle className="w-4 h-4" style={{ color: C.accentLime }} />}
              </button>
            ))}
          </div>
          {vote && !voted && (
            <BtnInverted onClick={() => setVoted(true)} className="w-full mt-3">
              Votar em {vote}
            </BtnInverted>
          )}
          {voted && (
            <p style={{ ...MICRO, color: C.accentLime, textAlign: "center", marginTop: 8 }}>
              ✓ Voto registrado para {vote}
            </p>
          )}
        </div>

        {/* Poll */}
        <div style={card}>
          <div className="flex items-center gap-2 mb-4">
            <Activity className="w-4 h-4" style={{ color: C.accentVioletMid }} />
            <span style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>Enquete Rápida</span>
          </div>
          <p style={{ ...CAPTION, color: C.onDarkMuted, marginBottom: 10 }}>Qual será o resultado final?</p>
          <div className="flex flex-col gap-2">
            {pollOpts.map((opt, i) => (
              <button key={opt} onClick={() => setPoll(i)}
                className="relative flex items-center justify-between px-3 py-2.5 overflow-hidden"
                style={{
                  borderRadius: 8, border: `1px solid ${poll === i ? C.accentVioletMid : C.hairlineViolet}`,
                }}>
                {poll !== null && (
                  <div style={{
                    position: "absolute", left: 0, top: 0, height: "100%", width: `${pollPct[i]}%`,
                    backgroundColor: "rgba(121,98,140,0.25)",
                  }} />
                )}
                <span className="relative" style={{ ...BODY_MD, color: poll === i ? C.onPrimary : C.onDarkMuted, fontWeight: poll === i ? 600 : 400 }}>
                  {opt}
                </span>
                {poll !== null && (
                  <span className="relative" style={{ ...MICRO, color: C.accentVioletMid }}>{pollPct[i]}%</span>
                )}
              </button>
            ))}
          </div>
        </div>

        {/* Messages */}
        <div style={card}>
          <div className="flex items-center gap-2 mb-4">
            <MessageSquare className="w-4 h-4" style={{ color: C.accentVioletMid }} />
            <span style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600 }}>Mensagem de Torcida</span>
          </div>
          <div className="flex gap-2 mb-3">
            <input value={msg} onChange={(e) => setMsg(e.target.value)}
              onKeyDown={(e) => { if (e.key === "Enter" && msg.trim()) { setMsgs((p) => [msg, ...p]); setMsg(""); } }}
              placeholder="Mande seu grito de torcida..."
              style={{
                flex: 1, ...BODY_MD, color: C.onPrimary,
                backgroundColor: C.onDarkFaint, borderRadius: 8,
                padding: "8px 12px", outline: "none", border: `1px solid ${C.hairlineViolet}`,
              }}
            />
            <button onClick={() => { if (msg.trim()) { setMsgs((p) => [msg, ...p]); setMsg(""); } }}
              className="w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0"
              style={{ backgroundColor: C.accentVioletMid }}>
              <Send className="w-4 h-4" style={{ color: C.onPrimary }} />
            </button>
          </div>
          <div className="flex flex-col gap-1 overflow-y-auto" style={{ maxHeight: 140, scrollbarWidth: "none" }}>
            {msgs.map((m, i) => (
              <div key={i} className="flex items-start gap-2 py-2"
                style={{ borderBottom: `1px solid ${C.hairlineViolet}` }}>
                <div className="w-6 h-6 rounded-full flex items-center justify-center flex-shrink-0"
                  style={{ backgroundColor: C.onDarkFaint, fontSize: 12 }}>👤</div>
                <p style={{ ...CAPTION, color: C.onDarkMuted }}>{m}</p>
              </div>
            ))}
          </div>
        </div>

        <div style={{ height: 24 }} />
      </div>
    </div>
  );
}

// ─── Profile ──────────────────────────────────────────────────────────────────

function ProfileScreen({ onNavigate }: { onNavigate: (s: Screen) => void }) {
  const history = EVENTS.filter((e) => e.status === "ended" || e.id === 2);

  return (
    <div className="flex-1 overflow-y-auto" style={{ scrollbarWidth: "none", backgroundColor: C.canvasDark }}>
      <div className="px-5 pt-14 pb-5">
        <h1 style={{ ...DISPLAY, fontSize: 44, color: C.onPrimary }}>PERFIL</h1>
      </div>

      {/* Profile card — card-spotlight-violet */}
      <div className="mx-5 mb-5 p-5" style={{
        backgroundColor: C.accentVioletDeep, borderRadius: 18,
        position: "relative", overflow: "hidden",
      }}>
        <div style={{ position: "absolute", right: 16, top: 16, fontSize: 48, transform: "rotate(12deg)", opacity: 0.4 }}>⚡</div>
        <div className="flex items-center gap-4 mb-4">
          <div className="w-16 h-16 rounded-full flex items-center justify-center"
            style={{ backgroundColor: "rgba(255,255,255,0.15)", fontSize: 28 }}>👤</div>
          <div>
            <p style={{ ...RUBIK, fontSize: 20, fontWeight: 600, color: C.onPrimary }}>João Torcedor</p>
            <p style={{ ...CAPTION, color: "rgba(255,255,255,0.6)" }}>@joaotorcedor</p>
            <div className="flex items-center gap-1 mt-1">
              <Star className="w-3.5 h-3.5 fill-current" style={{ color: C.accentLime }} />
              <span style={{ ...MICRO, color: C.accentLime }}>400 PTS</span>
            </div>
          </div>
        </div>
        <div className="grid grid-cols-3 pt-4" style={{ borderTop: "1px solid rgba(255,255,255,0.15)" }}>
          {[{ label: "Eventos", value: "7" }, { label: "Check-ins", value: "5" }, { label: "Votos", value: "12" }]
            .map(({ label, value }, i) => (
              <div key={label} className="text-center py-2"
                style={{ borderRight: i < 2 ? "1px solid rgba(255,255,255,0.15)" : "none" }}>
                <div style={{ ...DISPLAY, fontSize: 32, color: C.onPrimary, lineHeight: 1 }}>{value}</div>
                <div style={{ ...MICRO, color: "rgba(255,255,255,0.5)", marginTop: 4 }}>{label}</div>
              </div>
            ))}
        </div>
      </div>

      {/* Favorite teams */}
      <div className="px-5 mb-5">
        <p style={{ ...MICRO, color: C.onDarkMuted, marginBottom: 10 }}>EQUIPES FAVORITAS</p>
        <div className="flex flex-wrap gap-2">
          {["Lages Futsal ⚽", "Atlética Serra 🏐"].map((t) => (
            <span key={t} className="flex items-center gap-1.5 px-3"
              style={{
                ...MICRO, color: C.onPrimary,
                backgroundColor: C.onDarkFaint, borderRadius: 12,
                height: 32, border: `1px solid ${C.hairlineViolet}`,
                lineHeight: "32px",
              }}>
              <Heart className="w-3 h-3 fill-current" style={{ color: C.accentPink }} />
              {t}
            </span>
          ))}
        </div>
      </div>

      {/* History */}
      <div className="px-5 mb-5">
        <p style={{ ...MICRO, color: C.onDarkMuted, marginBottom: 10 }}>HISTÓRICO DE CHECK-INS</p>
        <div className="flex flex-col gap-2">
          {history.map((ev) => (
            <div key={ev.id} className="flex items-center gap-3 p-3" style={{
              backgroundColor: C.surfaceNight, borderRadius: 12, border: `1px solid ${C.hairlineViolet}`,
            }}>
              <div style={{ width: 48, height: 48, borderRadius: 8, overflow: "hidden", flexShrink: 0 }}>
                <img src={imgUrl(ev.imageId, 96, 96)} alt="" className="w-full h-full object-cover" />
              </div>
              <div style={{ flex: 1, minWidth: 0 }}>
                <p style={{ ...BODY_MD, color: C.onPrimary, fontWeight: 600, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                  {ev.name}
                </p>
                <p style={{ ...MICRO, color: C.onDarkMuted }}>{ev.date} · {ev.venue}</p>
              </div>
              <CheckCircle className="w-5 h-5 flex-shrink-0" style={{ color: C.accentLime }} />
            </div>
          ))}
        </div>
      </div>

      <div className="px-5 pb-8">
        <button onClick={() => onNavigate("admin")}
          className="flex items-center gap-2"
          style={{ ...BTN_CAP, color: C.onDarkMuted, textDecoration: "underline", background: "none", border: "none" }}>
          <BarChart2 className="w-4 h-4" />
          Painel Administrativo
        </button>
      </div>
    </div>
  );
}

// ─── Admin Dashboard — light canvas ──────────────────────────────────────────

const CHART_DATA = [
  { name: "Futsal", value: 342 },
  { name: "Vôlei", value: 508 },
  { name: "Corrida", value: 87 },
  { name: "Basquete", value: 56 },
  { name: "Futebol", value: 23 },
];
const CHART_COLORS = [C.accentVioletDeep, C.accentViolet, C.accentVioletMid, C.accentPink, "#cfcfdb"];

function NewEventModal({ onClose }: { onClose: () => void }) {
  const [form, setForm] = useState({ name: "", sport: "Futsal", date: "", time: "", venue: "", capacity: "", team1: "", team2: "" });
  const up = (k: keyof typeof form, v: string) => setForm((p) => ({ ...p, [k]: v }));

  const inputStyle: React.CSSProperties = {
    width: "100%", ...BODY_MD, color: C.inkDeep,
    backgroundColor: C.canvasLight, borderRadius: 6,
    padding: "8px 12px", outline: "none",
    border: `1px solid ${C.hairlineCool}`,
  };

  return (
    <div className="fixed inset-0 flex items-center justify-center z-50 p-4"
      style={{ backgroundColor: "rgba(21,15,35,0.7)", backdropFilter: "blur(4px)" }}>
      <div className="w-full max-w-lg max-h-[90vh] overflow-y-auto"
        style={{ backgroundColor: C.canvasLight, borderRadius: 18, scrollbarWidth: "none",
          boxShadow: "rgba(0,0,0,0.18) 0 0.5rem 1.5rem" }}>
        <div className="flex items-center justify-between px-6 py-5 sticky top-0"
          style={{ backgroundColor: C.canvasLight, borderBottom: `1px solid ${C.hairlineCloud}`, borderRadius: "18px 18px 0 0" }}>
          <h2 style={{ ...DISPLAY, fontSize: 28, color: C.inkDeep }}>CADASTRAR EVENTO</h2>
          <button onClick={onClose} className="w-9 h-9 rounded-full flex items-center justify-center"
            style={{ backgroundColor: C.surfacePressLight }}>
            <X className="w-4 h-4" style={{ color: C.inkDeep }} />
          </button>
        </div>
        <div className="px-6 py-5 flex flex-col gap-4">
          {[
            { label: "Nome do Evento", key: "name" as const, ph: "Ex: Final Municipal de Futsal" },
            { label: "Local", key: "venue" as const, ph: "Ex: Ginásio Jones Minosso" },
          ].map(({ label, key, ph }) => (
            <div key={key}>
              <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 6 }}>{label.toUpperCase()}</p>
              <input value={form[key]} onChange={(e) => up(key, e.target.value)} placeholder={ph} style={inputStyle} />
            </div>
          ))}
          <div>
            <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 6 }}>MODALIDADE</p>
            <select value={form.sport} onChange={(e) => up("sport", e.target.value)}
              style={{ ...inputStyle, appearance: "none" }}>
              {["Futsal", "Vôlei", "Corrida", "Basquete", "Futebol"].map((s) => <option key={s}>{s}</option>)}
            </select>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 6 }}>DATA</p>
              <input type="date" value={form.date} onChange={(e) => up("date", e.target.value)} style={inputStyle} />
            </div>
            <div>
              <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 6 }}>HORÁRIO</p>
              <input type="time" value={form.time} onChange={(e) => up("time", e.target.value)} style={inputStyle} />
            </div>
          </div>
          <div>
            <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 6 }}>CAPACIDADE</p>
            <input type="number" value={form.capacity} onChange={(e) => up("capacity", e.target.value)} placeholder="Ex: 500" style={inputStyle} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            {(["team1", "team2"] as const).map((k, i) => (
              <div key={k}>
                <p style={{ ...MICRO, color: C.accentVioletMid, marginBottom: 6 }}>EQUIPE {i + 1}</p>
                <input value={form[k]} onChange={(e) => up(k, e.target.value)} placeholder="Opcional" style={inputStyle} />
              </div>
            ))}
          </div>
          <div className="flex gap-3 pt-2">
            <button onClick={onClose} className="flex-1 py-3 rounded-lg"
              style={{ ...BTN_CAP, backgroundColor: C.surfacePressLight, color: C.inkDeep, border: `1px solid ${C.hairlineCloud}` }}>
              Cancelar
            </button>
            <BtnPrimary onClick={onClose} className="flex-1">Cadastrar</BtnPrimary>
          </div>
        </div>
      </div>
    </div>
  );
}

function AdminDashboard({ onNavigate }: { onNavigate: (s: Screen) => void }) {
  const [showModal, setShowModal] = useState(false);

  const metrics = [
    { label: "Eventos", value: "12" },
    { label: "Check-ins", value: "1.016" },
    { label: "Torcedores", value: "834" },
    { label: "Taxa Interação", value: "68%" },
    { label: "+ Engajamento", value: "Vôlei" },
    { label: "Evento Top", value: "Copa Vôlei" },
  ];

  return (
    <div className="min-h-screen" style={{ backgroundColor: C.canvasLight, fontFamily: "'Rubik', sans-serif" }}>
      {/* Nav — nav-bar-light */}
      <div className="flex items-center justify-between px-6 sticky top-0 z-10" style={{
        height: 56, backgroundColor: C.canvasLight,
        borderBottom: `1px solid ${C.hairlineCloud}`,
        boxShadow: `inset 0 -1px 0 ${C.hairlineCloud}`,
      }}>
        <div className="flex items-center gap-3">
          <button onClick={() => onNavigate("home")}
            className="w-9 h-9 rounded-full flex items-center justify-center"
            style={{ backgroundColor: C.surfacePressLight }}>
            <ArrowLeft className="w-4 h-4" style={{ color: C.inkDeep }} />
          </button>
          <span style={{ ...DISPLAY, fontSize: 18, color: C.inkDeep, letterSpacing: 0.5 }}>
            ARENA LAGES — ADMIN
          </span>
        </div>
        <BtnPrimary onClick={() => setShowModal(true)}>
          <Plus className="w-4 h-4" />Cadastrar Evento
        </BtnPrimary>
      </div>

      <div className="px-6 py-8">
        {/* Metric cards — card-pricing style */}
        <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-6 gap-4 mb-8">
          {metrics.map(({ label, value }) => (
            <div key={label} style={{
              backgroundColor: C.canvasLight, borderRadius: 12,
              border: `1px solid ${C.hairlineCloud}`, padding: 20,
              boxShadow: "rgba(0,0,0,0.06) 0 2px 8px 0",
            }}>
              <div style={{ ...DISPLAY, fontSize: 36, color: C.inkDeep, lineHeight: 1, marginBottom: 4 }}>{value}</div>
              <div style={{ ...MICRO, color: C.accentVioletMid }}>{label}</div>
            </div>
          ))}
        </div>

        {/* Charts */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
          <div className="lg:col-span-2 p-5" style={{
            backgroundColor: C.canvasLight, borderRadius: 12,
            border: `1px solid ${C.hairlineCloud}`,
          }}>
            <p style={{ ...RUBIK, fontSize: 16, fontWeight: 600, color: C.inkDeep, marginBottom: 4 }}>
              Check-ins por Modalidade
            </p>
            <p style={{ ...CAPTION, color: C.accentVioletMid, marginBottom: 20 }}>Temporada 2025</p>
            <ResponsiveContainer width="100%" height={200}>
              <BarChart data={CHART_DATA} barSize={28}>
                <XAxis dataKey="name" tick={{ fill: C.accentVioletMid, fontSize: 12, fontFamily: "Rubik" }}
                  axisLine={false} tickLine={false} />
                <YAxis tick={{ fill: C.hairlineCool, fontSize: 11, fontFamily: "Rubik" }}
                  axisLine={false} tickLine={false} />
                <Tooltip contentStyle={{
                  backgroundColor: C.surfaceNight, border: `1px solid ${C.hairlineViolet}`,
                  borderRadius: 8, color: C.onPrimary, fontFamily: "Rubik", fontSize: 12,
                }} labelStyle={{ color: C.onPrimary, fontWeight: 600 }}
                  itemStyle={{ color: C.accentLime }}
                  cursor={{ fill: "rgba(194,239,78,0.06)" }} />
                <Bar dataKey="value" radius={[4, 4, 0, 0]}>
                  {CHART_DATA.map((_, i) => <Cell key={i} fill={CHART_COLORS[i]} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Breakdown */}
          <div className="p-5" style={{
            backgroundColor: C.canvasLight, borderRadius: 12, border: `1px solid ${C.hairlineCloud}`,
          }}>
            <p style={{ ...RUBIK, fontSize: 16, fontWeight: 600, color: C.inkDeep, marginBottom: 4 }}>
              Distribuição
            </p>
            <p style={{ ...CAPTION, color: C.accentVioletMid, marginBottom: 20 }}>por modalidade</p>
            <div className="flex flex-col gap-3">
              {CHART_DATA.map(({ name, value }, i) => {
                const total = CHART_DATA.reduce((a, d) => a + d.value, 0);
                const pct = Math.round((value / total) * 100);
                return (
                  <div key={name}>
                    <div className="flex justify-between mb-1">
                      <span style={{ ...CAPTION, color: C.inkDeep }}>{name}</span>
                      <span style={{ ...MICRO, color: C.inkDeep }}>{pct}%</span>
                    </div>
                    <div style={{ height: 4, backgroundColor: C.hairlineCloud, borderRadius: 2 }}>
                      <div style={{ width: `${pct}%`, height: "100%", backgroundColor: CHART_COLORS[i], borderRadius: 2 }} />
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Table */}
        <div style={{ border: `1px solid ${C.hairlineCloud}`, borderRadius: 12, overflow: "hidden" }}>
          <div className="flex items-center justify-between px-5 py-4"
            style={{ borderBottom: `1px solid ${C.hairlineCloud}`, backgroundColor: C.canvasLight }}>
            <span style={{ ...RUBIK, fontSize: 16, fontWeight: 600, color: C.inkDeep }}>
              Tabela de Eventos
            </span>
            <span style={{ ...MICRO, color: C.accentVioletMid }}>{EVENTS.length} eventos</span>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full" style={{ backgroundColor: C.canvasLight }}>
              <thead>
                <tr style={{ borderBottom: `1px solid ${C.hairlineCloud}` }}>
                  {["Evento", "Modalidade", "Data · Hora", "Local", "Público", "Status", "Ações"].map((h) => (
                    <th key={h} className="text-left px-5 py-3"
                      style={{ ...MICRO, color: C.accentVioletMid }}>
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {EVENTS.map((ev) => (
                  <tr key={ev.id} className="hover:bg-[#f0f0f0] transition-colors"
                    style={{ borderBottom: `1px solid ${C.hairlineCloud}` }}>
                    <td className="px-5 py-3.5">
                      <div className="flex items-center gap-2">
                        <div style={{ width: 36, height: 36, borderRadius: 8, overflow: "hidden", flexShrink: 0, backgroundColor: C.surfacePressLight }}>
                          <img src={imgUrl(ev.imageId, 72, 72)} alt="" className="w-full h-full object-cover" />
                        </div>
                        <span style={{ ...BODY_MD, color: C.inkDeep, fontWeight: 600 }}>{ev.name}</span>
                      </div>
                    </td>
                    <td className="px-5 py-3.5">
                      <span style={{ ...CAPTION, color: C.accentVioletMid }}>{ev.sport}</span>
                    </td>
                    <td className="px-5 py-3.5">
                      <span style={{ ...CAPTION, color: C.inkDeep }}>{ev.date} · {ev.time}</span>
                    </td>
                    <td className="px-5 py-3.5">
                      <span style={{ ...CAPTION, color: C.inkDeep, maxWidth: 130, display: "block", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                        {ev.venue}
                      </span>
                    </td>
                    <td className="px-5 py-3.5">
                      <span style={{ ...DISPLAY, fontSize: 22, color: C.inkDeep }}>{ev.checkins}</span>
                      <span style={{ ...CAPTION, color: C.hairlineCool }}>/{ev.capacity}</span>
                    </td>
                    <td className="px-5 py-3.5">
                      {/* Light canvas status — use accent colors */}
                      <span style={{
                        ...MICRO,
                        backgroundColor: ev.status === "live" ? "rgba(194,239,78,0.15)" : ev.status === "ended" ? C.surfacePressLight : "rgba(106,95,193,0.1)",
                        color: ev.status === "live" ? "#4a6800" : ev.status === "ended" ? C.accentVioletMid : C.accentViolet,
                        borderRadius: 4, padding: "4px 8px",
                      }}>
                        {ev.status === "live" ? "● Ao Vivo" : ev.status === "upcoming" ? "Próximo" : "Encerrado"}
                      </span>
                    </td>
                    <td className="px-5 py-3.5">
                      <div className="flex gap-1">
                        {[Eye, Edit2].map((Icon, i) => (
                          <button key={i}
                            className="w-8 h-8 rounded-lg flex items-center justify-center"
                            style={{ backgroundColor: C.surfacePressLight }}>
                            <Icon className="w-3.5 h-3.5" style={{ color: C.inkDeep }} />
                          </button>
                        ))}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {showModal && <NewEventModal onClose={() => setShowModal(false)} />}
    </div>
  );
}

// ─── App ──────────────────────────────────────────────────────────────────────

const MAIN_SCREENS: Screen[] = ["home", "events", "checkin", "interaction", "profile"];

export default function App() {
  const [screen, setScreen] = useState<Screen>("splash");
  const [prevScreen, setPrevScreen] = useState<Screen>("home");
  const [selectedEvent, setSelectedEvent] = useState<EventData>(EVENTS[1]);

  const navigate = (s: Screen) => { setPrevScreen(screen); setScreen(s); };
  const selectEvent = (ev: EventData) => { setSelectedEvent(ev); setPrevScreen(screen); setScreen("detail"); };

  if (screen === "admin") return <AdminDashboard onNavigate={navigate} />;

  return (
    <div className="min-h-screen flex items-start md:items-center justify-center"
      style={{ backgroundColor: C.primary, fontFamily: "'Rubik', -apple-system, system-ui, sans-serif" }}>
      {/* Phone frame */}
      <div className="w-full max-w-[390px] min-h-screen md:min-h-0 md:h-[844px] md:my-6 overflow-hidden flex flex-col"
        style={{
          backgroundColor: C.canvasDark,
          border: `1px solid ${C.hairlineViolet}`,
          boxShadow: "rgb(21,15,35) 0 0 8px 6px",
        }}>
        {screen === "splash" && <SplashScreen onNext={() => navigate("home")} />}
        {screen === "home" && <HomeScreen onNavigate={navigate} onSelectEvent={selectEvent} />}
        {screen === "events" && <EventsScreen onSelectEvent={selectEvent} />}
        {screen === "detail" && <EventDetailScreen event={selectedEvent} onBack={() => navigate(prevScreen)} onNavigate={navigate} />}
        {screen === "checkin" && <CheckInScreen event={selectedEvent} onBack={() => navigate("detail")} onNavigate={navigate} />}
        {screen === "interaction" && <InteractionScreen event={selectedEvent} onBack={() => navigate("detail")} />}
        {screen === "profile" && <ProfileScreen onNavigate={navigate} />}
        {MAIN_SCREENS.includes(screen) && <BottomNav current={screen} onNavigate={navigate} />}
      </div>
    </div>
  );
}

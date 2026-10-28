import { useMemo, useState } from 'react'

function mulberry32(a) {
    return function() {
        let t = a += 0x6D2B79F5
        t = Math.imul(t ^ t >>> 15, t | 1)
        t ^= t + Math.imul(t ^ t >>> 7, t | 61)
        return ((t ^ t >>> 14) >>> 0) / 4294967296
    }
}

export default function AppLogo({ size = 'md' }) {
    const px = { sm: 36, md: 56, lg: 120, xl: 220 }[size] || 56
    const seed = useMemo(() => Math.floor(Math.random() * 100000), [])
    const [isHovered, setIsHovered] = useState(false)

    const particles = useMemo(() => {
        const rand = mulberry32(seed)
        const dots = []
        for (let x = 20; x < 85; x += 3) {
            for (let y = 15; y < 85; y += 3) {
                dots.push({
                    x: x + (rand() * 1.5 - 0.75),
                    y: y + (rand() * 1.5 - 0.75),
                    r: 0.6 + rand() * 1.2,
                    delay: rand() * 3,
                    duration: 3 + rand() * 4
                })
            }
        }
        return dots
    }, [seed])

    return (
        <div
            className="relative inline-flex items-center justify-center select-none"
            style={{ width: px, height: px }}
            onMouseEnter={() => setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
        >
            <style>{`
        @keyframes particle-breathe {
          0%, 100% { opacity: 0.9; transform: scale(1); }
          50% { opacity: 0.3; transform: scale(0.85); }
        }
        .particle {
          transform-box: fill-box;
          transform-origin: center;
          animation: particle-breathe var(--dur) infinite cubic-bezier(0.4, 0, 0.2, 1);
          animation-delay: var(--delay);
        }
        
        @keyframes heartbeat {
          0%, 100% { transform: scale(1); filter: drop-shadow(0 0 8px rgba(0, 255, 157, 0.3)); }
          50% { transform: scale(1.04); filter: drop-shadow(0 0 20px rgba(0, 255, 157, 0.7)); }
        }
        .heartbeat-active {
          animation: heartbeat 1.5s infinite ease-in-out;
        }

        @keyframes vignette-pulse {
          0%, 100% { opacity: 0.4; }
          50% { opacity: 0.8; }
        }
        .vignette-active {
          animation: vignette-pulse 1.5s infinite ease-in-out;
        }
      `}</style>

            <svg
                viewBox="0 0 100 100"
                className={`absolute inset-0 w-full h-full transition-all duration-700 ${isHovered ? 'heartbeat-active' : ''}`}
            >
                <defs>
                    <linearGradient id={`grad-base-${seed}`} x1="0%" y1="0%" x2="100%" y2="100%">
                        <stop offset="0%" stopColor="#ff7b00" />
                        <stop offset="48%" stopColor="#ff7b00" />
                        <stop offset="52%" stopColor="#d400ff" />
                        <stop offset="100%" stopColor="#d400ff" />
                    </linearGradient>

                    <radialGradient id={`vignette-${seed}`} cx="50%" cy="50%" r="50%">
                        <stop offset="30%" stopColor="black" stopOpacity="0" />
                        <stop offset="100%" stopColor="black" stopOpacity="0.85" />
                    </radialGradient>

                    <clipPath id={`r-clip-${seed}`}>
                        <text x="50" y="68" textAnchor="middle" fontSize="75" fontWeight="900" fontFamily="Inter, system-ui, sans-serif">R</text>
                    </clipPath>
                </defs>

                <g clipPath={`url(#r-clip-${seed})`}>

                    {/* СЛОЙ 1: Базовые частицы (Оранж-Фиолет) */}
                    <g style={{ opacity: isHovered ? 0 : 1, transition: 'opacity 0.7s ease' }}>
                        {particles.map((p, i) => (
                            <circle
                                key={`base-${i}`}
                                cx={p.x}
                                cy={p.y}
                                r={p.r}
                                fill={`url(#grad-base-${seed})`}
                                className="particle"
                                style={{ '--dur': `${p.duration}s`, '--delay': `-${p.delay}s` }}
                            />
                        ))}
                    </g>

                    {/* СЛОЙ 2: Зеленые частицы (проявляются при наведении) */}
                    <g style={{ opacity: isHovered ? 1 : 0, transition: 'opacity 0.7s ease' }}>
                        {particles.map((p, i) => (
                            <circle
                                key={`green-${i}`}
                                cx={p.x}
                                cy={p.y}
                                r={p.r}
                                fill="#00ff9d"
                                className="particle"
                                style={{ '--dur': `${p.duration}s`, '--delay': `-${p.delay}s` }}
                            />
                        ))}
                    </g>

                    {/* СЛОЙ 3: Виньетка (пульсирует при наведении) */}
                    {isHovered && (
                        <rect
                            width="100"
                            height="100"
                            fill={`url(#vignette-${seed})`}
                            className="vignette-active"
                            pointerEvents="none"
                        />
                    )}

                </g>
            </svg>
        </div>
    )
}
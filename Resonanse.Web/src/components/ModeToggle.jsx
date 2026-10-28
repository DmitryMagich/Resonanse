import { useModeStore } from '../stores/modeStore'

const MODES = [
    { id: 'offline', label: 'Offline', color: 'bg-purple-600' },
    { id: 'online', label: 'Online', color: 'bg-orange-500' },
    { id: 'resonance', label: 'Resonance', color: 'bg-emerald-500' },
]

export default function ModeToggle() {
    const mode = useModeStore((s) => s.mode)
    const setMode = useModeStore((s) => s.setMode)

    return (
        <div className="inline-flex items-center gap-1 p-1 rounded-lg bg-neutral-900/80 border border-neutral-800 backdrop-blur-sm">
            {MODES.map((m) => {
                const active = mode === m.id
                return (
                    <button
                        key={m.id}
                        onClick={() => setMode(m.id)}
                        className={`px-3 py-1.5 rounded-md text-xs font-medium transition-all duration-300 ${
                            active
                                ? `${m.color} text-white shadow-lg shadow-${m.color}/30 scale-105`
                                : 'text-neutral-500 hover:text-neutral-300 hover:bg-neutral-800'
                        }`}
                    >
                        {m.label}
                    </button>
                )
            })}
        </div>
    )
}
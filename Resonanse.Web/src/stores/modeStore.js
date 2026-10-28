import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export const useModeStore = create(
    persist(
        (set) => ({
            mode: 'offline',
            setMode: (mode) => {
                document.documentElement.setAttribute('data-mode', mode)
                set({ mode })
            },
        }),
        {
            name: 'resonance-mode',
            onRehydrateStorage: () => (state) => {
                if (state?.mode) {
                    document.documentElement.setAttribute('data-mode', state.mode)
                }
            },
        }
    )
)
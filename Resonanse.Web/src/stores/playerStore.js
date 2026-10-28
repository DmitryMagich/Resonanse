import { create } from 'zustand'

export const usePlayerStore = create((set, get) => ({
    currentTrack: null,
    queue: [],
    isPlaying: false,
    volume: 0.8,
    progress: 0,
    duration: 0,

    playTrack: (track, queue = []) => {
        set({
            currentTrack: track,
            queue: queue.length ? queue : [track],
            isPlaying: true,
            progress: 0,
        })
    },

    togglePlay: () => set((state) => ({ isPlaying: !state.isPlaying })),

    nextTrack: () => {
        const { queue, currentTrack } = get()
        if (!currentTrack || queue.length === 0) return
        const index = queue.findIndex((t) => t.id === currentTrack.id)
        if (index === -1) return
        const nextIndex = (index + 1) % queue.length
        set({ currentTrack: queue[nextIndex], isPlaying: true, progress: 0 })
    },

    prevTrack: () => {
        const { queue, currentTrack } = get()
        if (!currentTrack || queue.length === 0) return
        const index = queue.findIndex((t) => t.id === currentTrack.id)
        if (index === -1) return
        const prevIndex = (index - 1 + queue.length) % queue.length
        set({ currentTrack: queue[prevIndex], isPlaying: true, progress: 0 })
    },

    setProgress: (progress) => set({ progress }),
    setDuration: (duration) => set({ duration }),
    setVolume: (volume) => set({ volume }),
}))
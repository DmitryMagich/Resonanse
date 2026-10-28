import { useState, useEffect } from 'react'
import { useAuthStore } from '../stores/authStore'
import { usePlayerStore } from '../stores/playerStore'
import { api } from '../api/client'
import { Play, Clock, Disc } from 'lucide-react'
import AppLogo from '../components/AppLogo'

export default function HomePage() {
    const user = useAuthStore((s) => s.user)
    const playTrack = usePlayerStore((s) => s.playTrack)
    const [recentTracks, setRecentTracks] = useState([])
    const [loading, setLoading] = useState(true)

    const hour = new Date().getHours()
    const greeting = hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening'

    useEffect(() => {
        const fetchRecent = async () => {
            try {
                const { data } = await api.get('/tracks?pageSize=6&sortBy=CreatedAt&sortDir=Desc')
                setRecentTracks(data.items || [])
            } catch (err) {
                console.error('Failed to load tracks', err)
            } finally {
                setLoading(false)
            }
        }
        fetchRecent()
    }, [])

    return (
        <div className="p-6 md:p-8 space-y-10 max-w-7xl mx-auto">
            {/* Hero Section */}
            <header className="flex items-center gap-6 pb-6 border-b border-neutral-800/50">
                <AppLogo size="lg" />
                <div>
                    <h1 className="text-4xl font-bold text-white mb-2">
                        {greeting}, <span className="text-accent">{user?.username || 'Music Lover'}</span>
                    </h1>
                    <p className="text-neutral-400 text-lg">
                        Ready to listen to something new today?
                    </p>
                </div>
            </header>

            {/* Recently Added */}
            <section>
                <div className="flex items-center justify-between mb-6">
                    <h2 className="text-2xl font-semibold text-white tracking-tight flex items-center gap-2">
                        <Clock size={20} className="text-accent" />
                        Recently Added
                    </h2>
                </div>

                {loading ? (
                    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
                        {[...Array(6)].map((_, i) => (
                            <div key={i} className="aspect-square bg-neutral-800/50 rounded-xl animate-pulse" />
                        ))}
                    </div>
                ) : recentTracks.length > 0 ? (
                    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
                        {recentTracks.map((track) => (
                            <div
                                key={track.id}
                                onClick={() => playTrack(track, recentTracks)}
                                className="group relative bg-neutral-900/40 p-4 rounded-xl hover:bg-neutral-800/80 transition-all duration-300 cursor-pointer border border-transparent hover:border-neutral-700"
                            >
                                {/* Обложка-заглушка */}
                                <div className="relative aspect-square bg-neutral-800 rounded-lg mb-4 flex items-center justify-center overflow-hidden">
                                    <Disc size={40} className="text-neutral-700 group-hover:scale-110 transition-transform duration-500" />

                                    {/* Кнопка Play */}
                                    <button
                                        onClick={(e) => { e.stopPropagation(); playTrack(track, recentTracks); }}
                                        className="absolute bottom-2 right-2 w-10 h-10 bg-accent rounded-full flex items-center justify-center translate-y-2 opacity-0 group-hover:translate-y-0 group-hover:opacity-100 shadow-xl transition-all duration-300 hover:scale-105"
                                    >
                                        <Play size={18} className="text-white ml-0.5" fill="currentColor" />
                                    </button>
                                </div>

                                <h3 className="text-sm font-semibold text-white truncate" title={track.title}>
                                    {track.title}
                                </h3>
                                <p className="text-xs text-neutral-400 truncate mt-1">
                                    {track.artistName}
                                </p>
                            </div>
                        ))}
                    </div>
                ) : (
                    <div className="flex flex-col items-center justify-center p-12 bg-neutral-900/30 rounded-2xl border border-dashed border-neutral-800">
                        <Disc size={48} className="text-neutral-600 mb-4" />
                        <p className="text-neutral-400">Your library is empty.</p>
                        <p className="text-sm text-neutral-500">Scan a folder to add music.</p>
                    </div>
                )}
            </section>

            {/* Made for You */}
            <section>
                <h2 className="text-2xl font-semibold text-white tracking-tight mb-6 flex items-center gap-2">
                    <Play size={20} className="text-accent" />
                    Made for You
                </h2>
                <div className="flex gap-4 overflow-x-auto pb-4 snap-x">
                    {[1, 2, 3, 4].map((i) => (
                        <div
                            key={i}
                            className="min-w-[200px] h-[200px] rounded-xl bg-gradient-to-br from-neutral-800 to-neutral-900 flex items-center justify-center border border-neutral-800 snap-start"
                        >
                            <span className="text-neutral-500 font-medium">Coming soon...</span>
                        </div>
                    ))}
                </div>
            </section>
        </div>
    )
}
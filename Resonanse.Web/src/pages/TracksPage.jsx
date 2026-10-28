import { useState, useEffect } from 'react'
import { api } from '../api/client'
import { Play, Music } from 'lucide-react'
import AppInput from '../components/AppInput'
import { usePlayerStore } from '../stores/playerStore'

export default function TracksPage() {
    const [tracks, setTracks] = useState([])
    const [loading, setLoading] = useState(true)
    const [search, setSearch] = useState('')
    const [debouncedSearch, setDebouncedSearch] = useState('')

    const playTrack = usePlayerStore((s) => s.playTrack)
    const currentTrack = usePlayerStore((s) => s.currentTrack)
    const isPlaying = usePlayerStore((s) => s.isPlaying)

    // Дебаунс поиска
    useEffect(() => {
        const timer = setTimeout(() => setDebouncedSearch(search), 300)
        return () => clearTimeout(timer)
    }, [search])

    // Загрузка треков
    useEffect(() => {
        const fetchTracks = async () => {
            setLoading(true)
            try {
                const url = debouncedSearch
                    ? `/tracks?search=${encodeURIComponent(debouncedSearch)}&pageSize=100`
                    : '/tracks?pageSize=100'

                const { data } = await api.get(url)
                setTracks(data.items || [])
            } catch (err) {
                console.error('Failed to fetch tracks', err)
            } finally {
                setLoading(false)
            }
        }
        fetchTracks()
    }, [debouncedSearch])

    // Форматирование времени
    const formatDuration = (durationStr) => {
        if (!durationStr) return '0:00'
        const [hours, minutes, seconds] = durationStr.split(':')
        if (hours !== '00') return `${parseInt(hours)}:${minutes}:${seconds.split('.')[0]}`
        return `${parseInt(minutes)}:${seconds.split('.')[0]}`
    }

    const handlePlay = (e, track) => {
        e.stopPropagation()
        // Если уже играет этот трек — просто toggle (опционально можно добавить pause)
        playTrack(track, tracks)
    }

    return (
        <div className="p-6 md:p-8 max-w-7xl mx-auto">
            {/* Заголовок и Поиск */}
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 mb-8">
                <div>
                    <h1 className="text-4xl font-bold text-white tracking-tight mb-2">Your Library</h1>
                    <p className="text-neutral-400">All your tracks in one place.</p>
                </div>

                <div className="w-full md:w-72">
                    <AppInput
                        placeholder="Filter tracks..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
            </div>

            {/* Таблица треков */}
            <div className="bg-[#181818] rounded-xl overflow-hidden border border-neutral-800/50">
                {/* Шапка таблицы */}
                <div className="grid grid-cols-[48px_1fr_1fr_80px] gap-4 px-4 py-3 border-b border-neutral-800/50 text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                    <div className="text-center">#</div>
                    <div>Title</div>
                    <div className="hidden md:block">Album</div>
                    <div className="text-right pr-4">Time</div>
                </div>

                {/* Список */}
                <div className="flex flex-col">
                    {loading ? (
                        [...Array(5)].map((_, i) => (
                            <div key={i} className="grid grid-cols-[48px_1fr_1fr_80px] gap-4 px-4 py-3 animate-pulse">
                                <div className="h-4 bg-neutral-800 rounded w-4 mx-auto" />
                                <div className="h-4 bg-neutral-800 rounded w-1/2" />
                                <div className="h-4 bg-neutral-800 rounded w-1/3 hidden md:block" />
                                <div className="h-4 bg-neutral-800 rounded w-8 ml-auto" />
                            </div>
                        ))
                    ) : tracks.length > 0 ? (
                        tracks.map((track, index) => {
                            const isActive = currentTrack?.id === track.id

                            return (
                                <div
                                    key={track.id}
                                    onClick={() => playTrack(track, tracks)}
                                    className={`group grid grid-cols-[48px_1fr_1fr_80px] gap-4 px-4 py-2 items-center hover:bg-white/5 transition-colors cursor-pointer rounded-md ${isActive ? 'bg-white/5' : ''}`}
                                >
                                    {/* Номер / Кнопка Play */}
                                    <div className={`relative flex items-center justify-center ${isActive ? 'text-accent' : 'text-neutral-400 group-hover:text-white'}`}>
                                        <span className="group-hover:opacity-0 transition-opacity">{index + 1}</span>
                                        <button
                                            onClick={(e) => handlePlay(e, track)}
                                            className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                                        >
                                            <Play size={16} className="text-white fill-current" />
                                        </button>
                                    </div>

                                    {/* Название и Артист */}
                                    <div className="min-w-0">
                                        <p className={`text-sm font-medium truncate ${isActive ? 'text-accent' : 'text-white'}`}>
                                            {track.title}
                                        </p>
                                        <p className="text-xs text-neutral-400 truncate hover:underline cursor-pointer">
                                            {track.artistName}
                                        </p>
                                    </div>

                                    {/* Альбом */}
                                    <div className="hidden md:block text-sm text-neutral-400 truncate hover:underline cursor-pointer">
                                        {track.albumTitle}
                                    </div>

                                    {/* Длительность + Формат */}
                                    <div className="flex items-center justify-end gap-3 pr-2">
                    <span className="text-[10px] font-mono text-neutral-500 bg-neutral-900 px-1.5 py-0.5 rounded border border-neutral-800">
                      {track.bestFormat || 'UNK'}
                    </span>
                                        <span className="text-sm text-neutral-400 font-mono tabular-nums">
                      {formatDuration(track.duration)}
                    </span>
                                    </div>
                                </div>
                            )
                        })
                    ) : (
                        <div className="p-12 flex flex-col items-center justify-center text-center">
                            <Music size={48} className="text-neutral-600 mb-4" />
                            <p className="text-neutral-400 text-lg">No tracks found</p>
                            <p className="text-sm text-neutral-500 mt-1">Try adjusting your search or scan a new folder.</p>
                        </div>
                    )}
                </div>
            </div>
        </div>
    )
}
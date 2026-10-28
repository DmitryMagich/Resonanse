import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { api } from '../api/client'
import { Play, Clock, ChevronLeft, Disc3 } from 'lucide-react'
import { usePlayerStore } from '../stores/playerStore'

export default function AlbumDetailsPage() {
    const { id } = useParams()
    const [album, setAlbum] = useState(null)
    const [loading, setLoading] = useState(true)

    const playTrack = usePlayerStore((s) => s.playTrack)
    const currentTrack = usePlayerStore((s) => s.currentTrack)

    useEffect(() => {
        const fetchAlbum = async () => {
            setLoading(true)
            try {
                const { data } = await api.get(`/albums/${id}`)
                setAlbum(data)
            } catch (err) {
                console.error('Failed to fetch album', err)
            } finally {
                setLoading(false)
            }
        }
        fetchAlbum()
    }, [id])

    const formatDuration = (durationStr) => {
        if (!durationStr) return '0:00'
        const [hours, minutes, seconds] = durationStr.split(':')
        if (hours !== '00') return `${parseInt(hours)}:${minutes}:${seconds.split('.')[0]}`
        return `${parseInt(minutes)}:${seconds.split('.')[0]}`
    }

    const formatTotalTime = (tracks) => {
        let totalSeconds = 0
        tracks.forEach(t => {
            if (t.duration) {
                const [h, m, s] = t.duration.split(':')
                totalSeconds += (parseInt(h) * 3600) + (parseInt(m) * 60) + parseFloat(s)
            }
        })
        const m = Math.floor(totalSeconds / 60)
        return `${m} min`
    }

    if (loading) return <div className="p-8 text-center text-neutral-500 animate-pulse">Loading album...</div>
    if (!album) return <div className="p-8 text-center text-neutral-500">Album not found.</div>

    return (
        <div className="pb-10">
            {/* Хедер с обложкой */}
            <div className="bg-gradient-to-b from-[#282828] to-[#121212] p-6 md:p-10 flex flex-col md:flex-row items-end gap-6">

                {/* Обложка */}
                <div className="w-48 h-48 md:w-60 md:h-60 shrink-0 bg-neutral-800 rounded-lg shadow-2xl overflow-hidden relative">
                    {album.coverPath ? (
                        <img
                            src={`/api/albums/${album.id}/cover`}
                            alt={album.title}
                            className="w-full h-full object-cover"
                        />
                    ) : (
                        <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-orange-500 to-purple-600">
                            <span className="text-6xl font-bold text-white/30">{album.title.charAt(0)}</span>
                        </div>
                    )}
                </div>

                {/* Инфо */}
                <div className="flex flex-col gap-2">
                    <Link to="/albums" className="text-sm font-semibold text-neutral-400 hover:text-white flex items-center gap-1 w-max mb-2">
                        <ChevronLeft size={16} /> Back to Albums
                    </Link>
                    <span className="text-xs font-bold uppercase tracking-wider text-neutral-300">Album</span>
                    <h1 className="text-4xl md:text-6xl font-black text-white tracking-tight">{album.title}</h1>

                    <div className="flex items-center gap-2 mt-4 text-sm text-neutral-300 font-medium">
                        <span className="hover:underline cursor-pointer text-white">{album.artistName}</span>
                        <span>•</span>
                        <span>{album.releaseYear || 'Unknown Year'}</span>
                        <span>•</span>
                        <span>{album.tracks.length} tracks, {formatTotalTime(album.tracks)}</span>
                    </div>
                </div>
            </div>

            {/* Список треков */}
            <div className="px-6 md:px-10 mt-6">
                <button
                    onClick={() => playTrack(album.tracks[0], album.tracks)}
                    className="w-14 h-14 bg-accent rounded-full flex items-center justify-center text-white hover:scale-105 transition-transform shadow-xl mb-6"
                >
                    <Play size={24} fill="currentColor" className="ml-1" />
                </button>

                <div className="bg-[#181818]/50 rounded-xl overflow-hidden">
                    <div className="grid grid-cols-[48px_1fr_80px] gap-4 px-4 py-2 border-b border-neutral-800/50 text-xs font-semibold text-neutral-400 uppercase tracking-wider">
                        <div className="text-center">#</div>
                        <div>Title</div>
                        <div className="text-right pr-4"><Clock size={14} className="inline" /></div>
                    </div>

                    <div className="flex flex-col p-2">
                        {album.tracks.map((track, index) => {
                            const isActive = currentTrack?.id === track.id
                            return (
                                <div
                                    key={track.id}
                                    onClick={() => playTrack(track, album.tracks)}
                                    className={`group grid grid-cols-[48px_1fr_80px] gap-4 px-2 py-2 items-center hover:bg-white/10 transition-colors cursor-pointer rounded-md ${isActive ? 'bg-white/10' : ''}`}
                                >
                                    <div className={`relative flex items-center justify-center ${isActive ? 'text-accent' : 'text-neutral-400 group-hover:text-white'}`}>
                                        <span className="group-hover:opacity-0 transition-opacity">{index + 1}</span>
                                        <button
                                            onClick={(e) => { e.stopPropagation(); playTrack(track, album.tracks); }}
                                            className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                                        >
                                            <Play size={16} className="text-white fill-current" />
                                        </button>
                                    </div>

                                    <div className="min-w-0">
                                        <p className={`text-sm font-medium truncate ${isActive ? 'text-accent' : 'text-white'}`}>
                                            {track.title}
                                        </p>
                                        <p className="text-xs text-neutral-400 truncate mt-0.5">
                                            {track.artistName}
                                        </p>
                                    </div>

                                    <div className="flex items-center justify-end gap-3 pr-2">
                    <span className="text-[10px] font-mono text-neutral-500 border border-neutral-800 px-1.5 py-0.5 rounded">
                      {track.bestFormat || 'UNK'}
                    </span>
                                        <span className="text-sm text-neutral-400 font-mono tabular-nums">
                      {formatDuration(track.duration)}
                    </span>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </div>
            </div>
        </div>
    )
}
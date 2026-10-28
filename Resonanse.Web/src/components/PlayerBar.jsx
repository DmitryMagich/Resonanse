import { useEffect, useRef, useState } from 'react'
import { Play, Pause, SkipBack, SkipForward, Volume2, VolumeX, Music, ListMusic } from 'lucide-react'
import { usePlayerStore } from '../stores/playerStore'
import { useAuthStore } from '../stores/authStore'

export default function PlayerBar() {
    const audioRef = useRef(null)
    const token = useAuthStore((s) => s.accessToken)

    const {
        currentTrack, isPlaying, volume, progress, duration, queue,
        togglePlay, nextTrack, prevTrack, setProgress, setDuration, setVolume, playTrack
    } = usePlayerStore()

    const [isMuted, setIsMuted] = useState(false)
    const [prevVolume, setPrevVolume] = useState(volume)
    const [isQueueOpen, setIsQueueOpen] = useState(false)

    // Синхронизация play/pause
    useEffect(() => {
        if (!audioRef.current) return
        if (isPlaying) {
            audioRef.current.play().catch((e) => console.error('Playback error:', e))
        } else {
            audioRef.current.pause()
        }
    }, [isPlaying, currentTrack])

    // Синхронизация громкости
    useEffect(() => {
        if (audioRef.current) {
            audioRef.current.volume = isMuted ? 0 : volume
        }
    }, [volume, isMuted])

    const handleTimeUpdate = () => {
        if (audioRef.current) setProgress(audioRef.current.currentTime)
    }

    const handleLoadedMetadata = () => {
        if (audioRef.current) setDuration(audioRef.current.duration)
    }

    const handleSeek = (e) => {
        const time = Number(e.target.value)
        if (audioRef.current) {
            audioRef.current.currentTime = time
            setProgress(time)
        }
    }

    const handleVolumeChange = (e) => {
        const val = Number(e.target.value)
        setVolume(val)
        if (val > 0 && isMuted) setIsMuted(false)
    }

    const toggleMute = () => {
        if (isMuted) {
            setVolume(prevVolume || 0.5)
            setIsMuted(false)
        } else {
            setPrevVolume(volume)
            setIsMuted(true)
        }
    }

    const formatTime = (secs) => {
        if (!secs || isNaN(secs)) return '0:00'
        const m = Math.floor(secs / 60)
        const s = Math.floor(secs % 60)
        return `${m}:${s.toString().padStart(2, '0')}`
    }

    if (!currentTrack) return null

    const streamUrl = `/api/stream/track/${currentTrack.id}${token ? `?token=${token}` : ''}`
    const progressPercent = (progress / (duration || 1)) * 100
    const currentVolume = isMuted ? 0 : volume

    return (
        <div className="h-20 md:h-24 bg-[#181818] border-t border-neutral-800 flex items-center px-4 justify-between fixed bottom-16 md:bottom-0 left-0 right-0 z-40 shadow-[0_-10px_30px_rgba(0,0,0,0.5)]">

            {}
            <audio
                ref={audioRef}
                src={streamUrl}
                onTimeUpdate={handleTimeUpdate}
                onLoadedMetadata={handleLoadedMetadata}
                onEnded={nextTrack}
            />

            {}
            {isQueueOpen && (
                <div className="absolute bottom-[100%] right-4 mb-2 w-80 max-h-[60vh] bg-[#181818]/95 backdrop-blur-xl border border-neutral-800 rounded-xl shadow-2xl flex flex-col overflow-hidden animate-in fade-in slide-in-from-bottom-4 z-50">
                    <div className="p-4 border-b border-neutral-800 flex justify-between items-center bg-[#282828]">
                        <h3 className="text-sm font-bold text-white flex items-center gap-2">
                            <ListMusic size={16} className="text-accent" />
                            Next in Queue
                        </h3>
                        <span className="text-xs text-neutral-400">{queue.length} tracks</span>
                    </div>
                    <div className="overflow-y-auto p-2 flex flex-col gap-1">
                        {queue.length > 0 ? queue.map((track, i) => (
                            <div
                                key={`${track.id}-${i}`}
                                onClick={() => playTrack(track, queue)}
                                className="flex items-center gap-3 p-2 rounded-md hover:bg-white/10 cursor-pointer group transition-colors"
                            >
                                <div className="w-10 h-10 bg-neutral-800 rounded shrink-0 flex items-center justify-center">
                                    <Music size={16} className="text-neutral-500 group-hover:text-accent transition-colors" />
                                </div>
                                <div className="min-w-0 flex-1">
                                    <p className={`text-sm font-medium truncate ${currentTrack?.id === track.id ? 'text-accent' : 'text-white'}`}>
                                        {track.title}
                                    </p>
                                    <p className="text-xs text-neutral-400 truncate">{track.artistName}</p>
                                </div>
                            </div>
                        )) : (
                            <div className="p-6 text-center text-neutral-500 text-sm">
                                Queue is empty
                            </div>
                        )}
                    </div>
                </div>
            )}

            {/* Инфо о треке (Левая часть) */}
            <div className="flex items-center gap-4 w-[30%] min-w-[180px]">
                <div className="w-14 h-14 bg-neutral-800 rounded-md flex items-center justify-center text-neutral-500 shadow-md">
                    <Music size={24} />
                </div>
                <div className="min-w-0 flex flex-col justify-center">
                    <p className="text-sm font-semibold text-white truncate hover:underline cursor-pointer">
                        {currentTrack.title}
                    </p>
                    <p className="text-xs text-neutral-400 truncate hover:underline cursor-pointer mt-0.5">
                        {currentTrack.artistName}
                    </p>
                </div>
            </div>

            {}
            <div className="flex flex-col items-center flex-1 max-w-[600px] px-4">
                <div className="flex items-center gap-6 mb-2">
                    <button onClick={prevTrack} className="text-neutral-400 hover:text-white transition-colors">
                        <SkipBack size={20} fill="currentColor" />
                    </button>

                    <button
                        onClick={togglePlay}
                        className="w-10 h-10 bg-white rounded-full flex items-center justify-center text-black hover:scale-105 transition-transform shadow-lg"
                    >
                        {isPlaying ? <Pause size={20} fill="currentColor" /> : <Play size={20} fill="currentColor" className="ml-1" />}
                    </button>

                    <button onClick={nextTrack} className="text-neutral-400 hover:text-white transition-colors">
                        <SkipForward size={20} fill="currentColor" />
                    </button>
                </div>

                {}
                <div className="flex items-center gap-3 w-full text-xs text-neutral-400 font-mono">
                    <span className="w-10 text-right">{formatTime(progress)}</span>

                    <div className="relative flex-1 flex items-center group h-2">
                        {}
                        <div className="absolute left-0 right-0 h-1 bg-neutral-600 rounded-full pointer-events-none" />
                        {}
                        <div
                            className="absolute left-0 h-1 bg-accent rounded-full pointer-events-none"
                            style={{ width: `${Math.min(progressPercent, 100)}%` }}
                        />
                        <input
                            type="range"
                            min="0"
                            max={duration || 100}
                            value={progress}
                            onChange={handleSeek}
                            className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                        />
                    </div>

                    <span className="w-10">{formatTime(duration)}</span>
                </div>
            </div>

            {}
            <div className="flex items-center justify-end gap-2 w-[30%] min-w-[150px]">

                {}
                <button
                    onClick={() => setIsQueueOpen(!isQueueOpen)}
                    className={`p-2 rounded-lg transition-colors mr-2 ${isQueueOpen ? 'bg-neutral-800 text-accent' : 'text-neutral-400 hover:text-white'}`}
                    title="Queue"
                >
                    <ListMusic size={20} />
                </button>

                <button
                    onClick={toggleMute}
                    className="text-neutral-400 hover:text-white transition-colors"
                >
                    {isMuted || volume === 0 ? <VolumeX size={18} /> : <Volume2 size={18} />}
                </button>

                <div className="relative w-24 flex items-center group h-2">
                    <div className="absolute left-0 right-0 h-1 bg-neutral-600 rounded-full pointer-events-none" />
                    <div
                        className="absolute left-0 h-1 bg-accent rounded-full pointer-events-none"
                        style={{ width: `${currentVolume * 100}%` }}
                    />
                    <input
                        type="range"
                        min="0"
                        max="1"
                        step="0.01"
                        value={currentVolume}
                        onChange={handleVolumeChange}
                        className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                    />
                </div>
            </div>
        </div>
    )
}
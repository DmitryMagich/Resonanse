import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { api } from '../api/client'
import { Play, ChevronLeft, Disc3, Mic2 } from 'lucide-react'
import { usePlayerStore } from '../stores/playerStore'

export default function ArtistDetailsPage() {
    const { id } = useParams()
    const [artist, setArtist] = useState(null)
    const [loading, setLoading] = useState(true)

    const playTrack = usePlayerStore((s) => s.playTrack)

    useEffect(() => {
        const fetchArtist = async () => {
            setLoading(true)
            try {
                const { data } = await api.get(`/artists/${id}`)
                setArtist(data)
            } catch (err) {
                console.error('Failed to fetch artist', err)
            } finally {
                setLoading(false)
            }
        }
        fetchArtist()
    }, [id])

    // Хелпер для генерации цвета заглушки
    const getGradientPlaceholder = (name) => {
        const colors = [
            'from-orange-500 to-red-600',
            'from-purple-500 to-indigo-600',
            'from-emerald-500 to-teal-600',
            'from-blue-500 to-cyan-600',
            'from-pink-500 to-rose-600',
        ]
        const index = (name?.length || 0) % colors.length
        return colors[index]
    }

    // Проигрываем первый трек из первого альбома артиста
    const handlePlayArtist = async () => {
        if (!artist?.albums || artist.albums.length === 0) return
        try {
            const firstAlbumId = artist.albums[0].id
            const { data } = await api.get(`/albums/${firstAlbumId}`)
            if (data.tracks && data.tracks.length > 0) {
                playTrack(data.tracks[0], data.tracks)
            }
        } catch (err) {
            console.error('Failed to play artist', err)
        }
    }

    if (loading) return <div className="p-8 text-center text-neutral-500 animate-pulse">Loading artist...</div>
    if (!artist) return <div className="p-8 text-center text-neutral-500">Artist not found.</div>

    return (
        <div className="pb-10">
            {/* Хедер с артистом */}
            <div className="bg-gradient-to-b from-[#282828] to-[#121212] p-6 md:p-10 flex flex-col md:flex-row items-center md:items-end gap-8">

                {/* Круглая аватарка */}
                <div className="w-48 h-48 md:w-64 md:h-64 shrink-0 rounded-full shadow-[0_0_40px_rgba(0,0,0,0.5)] overflow-hidden relative">
                    {artist.imagePath ? (
                        <img
                            src={`/api/artists/${artist.id}/image`}
                            alt={artist.name}
                            className="w-full h-full object-cover"
                        />
                    ) : (
                        <div className={`w-full h-full flex items-center justify-center bg-gradient-to-br ${getGradientPlaceholder(artist.name)}`}>
                            <span className="text-7xl font-bold text-white/30 uppercase">{artist.name.charAt(0)}</span>
                        </div>
                    )}
                </div>

                {/* Инфо */}
                <div className="flex flex-col gap-2 items-center md:items-start text-center md:text-left">
                    <Link to="/artists" className="text-sm font-semibold text-neutral-400 hover:text-white flex items-center gap-1 w-max mb-2">
                        <ChevronLeft size={16} /> Back to Artists
                    </Link>
                    <span className="text-xs font-bold uppercase tracking-wider text-neutral-300 flex items-center gap-1">
            <Mic2 size={14} /> Artist
          </span>
                    <h1 className="text-5xl md:text-7xl font-black text-white tracking-tight leading-none">
                        {artist.name}
                    </h1>

                    <div className="flex items-center gap-2 mt-4 text-sm text-neutral-300 font-medium">
                        <span>{artist.albumCount} Albums</span>
                        <span>•</span>
                        <span>{artist.trackCount} Tracks</span>
                    </div>
                </div>
            </div>

            {/* Контентная часть */}
            <div className="px-6 md:px-10 mt-8 space-y-10">

                {/* Кнопка Play и Био */}
                <div className="flex items-center gap-6">
                    <button
                        onClick={handlePlayArtist}
                        disabled={artist.albumCount === 0}
                        className="w-14 h-14 bg-accent rounded-full flex items-center justify-center text-white hover:scale-105 transition-transform shadow-xl disabled:opacity-50 disabled:hover:scale-100"
                    >
                        <Play size={24} fill="currentColor" className="ml-1" />
                    </button>
                    {artist.bio && (
                        <p className="text-neutral-400 text-sm max-w-2xl line-clamp-3">
                            {artist.bio}
                        </p>
                    )}
                </div>

                {/* Сетка альбомов */}
                <section>
                    <h2 className="text-2xl font-bold text-white mb-6">Discography</h2>

                    {artist.albums && artist.albums.length > 0 ? (
                        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4 md:gap-6">
                            {artist.albums.map((album) => (
                                <Link
                                    key={album.id}
                                    to={`/albums/${album.id}`}
                                    className="group flex flex-col p-4 bg-[#181818] hover:bg-[#282828] rounded-xl transition-all duration-300 cursor-pointer"
                                >
                                    {/* Обложка */}
                                    <div className="relative aspect-square w-full rounded-lg overflow-hidden mb-4 shadow-lg">
                                        {album.coverPath ? (
                                            <img
                                                src={`/api/albums/${album.id}/cover`}
                                                alt={album.title}
                                                className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                                                onError={(e) => {
                                                    e.target.style.display = 'none';
                                                    e.target.nextSibling.style.display = 'flex';
                                                }}
                                            />
                                        ) : null}

                                        <div
                                            className={`absolute inset-0 flex items-center justify-center bg-gradient-to-br ${getGradientPlaceholder(album.title)} ${album.coverPath ? 'hidden' : 'flex'}`}
                                        >
                      <span className="text-4xl font-bold text-white/30 uppercase">
                        {album.title.charAt(0)}
                      </span>
                                        </div>
                                    </div>

                                    {/* Текст */}
                                    <h3 className="text-base font-semibold text-white truncate mb-1" title={album.title}>
                                        {album.title}
                                    </h3>
                                    <p className="text-sm text-neutral-400 truncate">
                                        {album.releaseYear ? `${album.releaseYear}` : 'Unknown Year'}
                                    </p>
                                </Link>
                            ))}
                        </div>
                    ) : (
                        <div className="flex flex-col items-center justify-center p-12 bg-[#181818] rounded-xl border border-neutral-800/50">
                            <Disc3 size={48} className="text-neutral-600 mb-4" />
                            <p className="text-neutral-400 text-lg">No albums found</p>
                            <p className="text-sm text-neutral-500 mt-1">This artist has no albums in your library.</p>
                        </div>
                    )}
                </section>
            </div>
        </div>
    )
}
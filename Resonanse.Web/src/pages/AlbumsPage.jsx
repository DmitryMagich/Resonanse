import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { Play, Disc3 } from 'lucide-react'
import AppInput from '../components/AppInput'
import { usePlayerStore } from '../stores/playerStore'

export default function AlbumsPage() {
    const [albums, setAlbums] = useState([])
    const [loading, setLoading] = useState(true)
    const [search, setSearch] = useState('')
    const [debouncedSearch, setDebouncedSearch] = useState('')

    const playTrack = usePlayerStore((s) => s.playTrack)

    // Дебаунс поиска
    useEffect(() => {
        const timer = setTimeout(() => setDebouncedSearch(search), 300)
        return () => clearTimeout(timer)
    }, [search])

    // Загрузка альбомов
    useEffect(() => {
        const fetchAlbums = async () => {
            setLoading(true)
            try {
                const url = debouncedSearch
                    ? `/albums?search=${encodeURIComponent(debouncedSearch)}&pageSize=50`
                    : '/albums?pageSize=50'

                const { data } = await api.get(url)
                setAlbums(data.items || [])
            } catch (err) {
                console.error('Failed to fetch albums', err)
            } finally {
                setLoading(false)
            }
        }
        fetchAlbums()
    }, [debouncedSearch])

    // Запуск проигрывания альбома (берёт первый трек или весь альбом)
    const handlePlayAlbum = async (e, album) => {
        e.preventDefault()
        e.stopPropagation()
        try {
            // Загружаем треки конкретного альбома
            const { data } = await api.get(`/albums/${album.id}`)
            if (data.tracks && data.tracks.length > 0) {
                playTrack(data.tracks[0], data.tracks)
            }
        } catch (err) {
            console.error('Failed to play album', err)
        }
    }

    // Генератор градиента для заглушки
    const getGradientPlaceholder = (title) => {
        const colors = [
            'from-orange-500 to-red-600',
            'from-purple-500 to-indigo-600',
            'from-emerald-500 to-teal-600',
            'from-blue-500 to-cyan-600',
            'from-pink-500 to-rose-600',
        ]
        const index = (title?.length || 0) % colors.length
        return colors[index]
    }

    return (
        <div className="p-6 md:p-8 max-w-7xl mx-auto">
            {/* Заголовок и Поиск */}
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 mb-8">
                <div>
                    <h1 className="text-4xl font-bold text-white tracking-tight mb-2">Albums</h1>
                    <p className="text-neutral-400">Explore your collection by album.</p>
                </div>

                <div className="w-full md:w-72">
                    <AppInput
                        placeholder="Search albums..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
            </div>

            {/* Сетка карточек */}
            {loading ? (
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4 md:gap-6">
                    {[...Array(12)].map((_, i) => (
                        <div key={i} className="flex flex-col gap-3">
                            <div className="aspect-square bg-[#181818] rounded-lg animate-pulse" />
                            <div className="h-4 bg-neutral-800 rounded w-3/4 animate-pulse" />
                            <div className="h-3 bg-neutral-800 rounded w-1/2 animate-pulse" />
                        </div>
                    ))}
                </div>
            ) : albums.length > 0 ? (
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4 md:gap-6">
                    {albums.map((album) => (
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

                                {/* Заглушка */}
                                <div
                                    className={`absolute inset-0 flex items-center justify-center bg-gradient-to-br ${getGradientPlaceholder(album.title)} ${album.coverPath ? 'hidden' : 'flex'}`}
                                >
                  <span className="text-4xl font-bold text-white/30 uppercase">
                    {album.title.charAt(0)}
                  </span>
                                </div>

                                {/* Кнопка Play */}
                                <button
                                    onClick={(e) => handlePlayAlbum(e, album)}
                                    className="absolute bottom-2 right-2 w-12 h-12 bg-accent rounded-full flex items-center justify-center translate-y-4 opacity-0 group-hover:translate-y-0 group-hover:opacity-100 shadow-xl transition-all duration-300 hover:scale-105 hover:bg-accent/90"
                                >
                                    <Play size={20} className="text-white ml-1" fill="currentColor" />
                                </button>
                            </div>

                            {/* Текст */}
                            <h3 className="text-base font-semibold text-white truncate mb-1" title={album.title}>
                                {album.title}
                            </h3>
                            <p className="text-sm text-neutral-400 truncate">
                                {album.releaseYear ? `${album.releaseYear} • ` : ''}{album.artistName}
                            </p>
                        </Link>
                    ))}
                </div>
            ) : (
                <div className="p-12 flex flex-col items-center justify-center text-center bg-[#181818] rounded-xl border border-neutral-800/50">
                    <Disc3 size={48} className="text-neutral-600 mb-4" />
                    <p className="text-neutral-400 text-lg">No albums found</p>
                    <p className="text-sm text-neutral-500 mt-1">Your library might be empty.</p>
                </div>
            )}
        </div>
    )
}
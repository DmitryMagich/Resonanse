import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { Play, Mic2 } from 'lucide-react'
import AppInput from '../components/AppInput'

export default function ArtistsPage() {
    const [artists, setArtists] = useState([])
    const [loading, setLoading] = useState(true)
    const [search, setSearch] = useState('')
    const [debouncedSearch, setDebouncedSearch] = useState('')

    // Дебаунс поиска
    useEffect(() => {
        const timer = setTimeout(() => setDebouncedSearch(search), 300)
        return () => clearTimeout(timer)
    }, [search])

    // Загрузка артистов
    useEffect(() => {
        const fetchArtists = async () => {
            setLoading(true)
            try {
                const url = debouncedSearch
                    ? `/artists?search=${encodeURIComponent(debouncedSearch)}&pageSize=50`
                    : '/artists?pageSize=50'

                const { data } = await api.get(url)
                setArtists(data.items || [])
            } catch (err) {
                console.error('Failed to fetch artists', err)
            } finally {
                setLoading(false)
            }
        }
        fetchArtists()
    }, [debouncedSearch])

    // Генератор градиента для заглушки
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

    return (
        <div className="p-6 md:p-8 max-w-7xl mx-auto">
            {/* Заголовок и Поиск */}
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 mb-8">
                <div>
                    <h1 className="text-4xl font-bold text-white tracking-tight mb-2">Artists</h1>
                    <p className="text-neutral-400">The voices behind your favorite tracks.</p>
                </div>

                <div className="w-full md:w-72">
                    <AppInput
                        placeholder="Search artists..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>
            </div>

            {/* Сетка карточек */}
            {loading ? (
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-6">
                    {[...Array(12)].map((_, i) => (
                        <div key={i} className="flex flex-col items-center gap-4 bg-[#181818] p-4 rounded-xl">
                            <div className="w-full aspect-square bg-neutral-800 rounded-full animate-pulse" />
                            <div className="h-4 bg-neutral-800 rounded w-3/4 animate-pulse mt-2" />
                            <div className="h-3 bg-neutral-800 rounded w-1/2 animate-pulse" />
                        </div>
                    ))}
                </div>
            ) : artists.length > 0 ? (
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-6">
                    {artists.map((artist) => (
                        <Link
                            key={artist.id}
                            to={`/artists/${artist.id}`}
                            className="group flex flex-col items-center p-4 bg-[#181818] hover:bg-[#282828] rounded-xl transition-all duration-300 text-center"
                        >
                            {/* Круглая обложка (Аватар) */}
                            <div className="relative w-full aspect-square rounded-full overflow-hidden mb-4 shadow-xl border-2 border-transparent group-hover:border-accent/30 transition-colors">
                                {artist.imagePath ? (
                                    <img
                                        src={`/api/artists/${artist.id}/image`}
                                        alt={artist.name}
                                        className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                                        onError={(e) => {
                                            e.target.style.display = 'none';
                                            e.target.nextSibling.style.display = 'flex';
                                        }}
                                    />
                                ) : null}

                                {/* Заглушка, если нет фото */}
                                <div
                                    className={`absolute inset-0 flex items-center justify-center bg-gradient-to-br ${getGradientPlaceholder(artist.name)} ${artist.imagePath ? 'hidden' : 'flex'}`}
                                >
                  <span className="text-5xl font-bold text-white/40 uppercase">
                    {artist.name.charAt(0)}
                  </span>
                                </div>

                                {/* Кнопка Play поверх автарки */}
                                <button className="absolute bottom-2 right-2 w-11 h-11 bg-accent rounded-full flex items-center justify-center translate-y-4 opacity-0 group-hover:translate-y-0 group-hover:opacity-100 shadow-xl transition-all duration-300 hover:scale-105">
                                    <Play size={18} className="text-white ml-0.5" fill="currentColor" />
                                </button>
                            </div>

                            {/* Текст */}
                            <h3 className="text-base font-semibold text-white truncate w-full" title={artist.name}>
                                {artist.name}
                            </h3>
                            <p className="text-xs text-neutral-400 mt-1">
                                {artist.albumCount} {artist.albumCount === 1 ? 'Album' : 'Albums'} • {artist.trackCount} Tracks
                            </p>
                        </Link>
                    ))}
                </div>
            ) : (
                <div className="p-12 flex flex-col items-center justify-center text-center bg-[#181818] rounded-xl border border-neutral-800/50">
                    <Mic2 size={48} className="text-neutral-600 mb-4" />
                    <p className="text-neutral-400 text-lg">No artists found</p>
                    <p className="text-sm text-neutral-500 mt-1">Try searching for someone else.</p>
                </div>
            )}
        </div>
    )
}
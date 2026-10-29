import { useState } from 'react'
import { Heart } from 'lucide-react'
import { toggleFavorite } from '../api/favorites'

export default function FavoriteButton({ trackId, initialIsFavorite = false, size = 18, className = '' }) {
    const [isFavorite, setIsFavorite] = useState(initialIsFavorite)
    const [isLoading, setIsLoading] = useState(false)

    const handleToggle = async (e) => {
        // Останавливаем всплытие, чтобы клик по сердечку не запускал трек
        e.preventDefault()
        e.stopPropagation()

        if (isLoading) return
        setIsLoading(true)

        // Оптимистичное обновление (сразу меняем цвет, не ждем ответа сервера)
        const newState = !isFavorite
        setIsFavorite(newState)

        try {
            await toggleFavorite(trackId)
        } catch (err) {
            // Если сервер вернул ошибку — откатываем назад
            setIsFavorite(!newState)
            console.error('Failed to toggle favorite', err)
        } finally {
            setIsLoading(false)
        }
    }

    return (
        <button
            onClick={handleToggle}
            className={`transition-colors duration-200 ${
                isFavorite ? 'text-accent' : 'text-neutral-500 hover:text-white'
            } ${className}`}
            title={isFavorite ? 'Remove from favorites' : 'Add to favorites'}
        >
            <Heart size={size} fill={isFavorite ? 'currentColor' : 'none'} />
        </button>
    )
}
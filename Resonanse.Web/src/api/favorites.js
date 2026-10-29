import { api } from './client'

export const toggleFavorite = async (trackId) => {
    const { data } = await api.post(`/me/favorites/${trackId}`)
    return data.isFavorite
}

export const getFavorites = async (page = 1, pageSize = 100) => {
    const { data } = await api.get(`/me/favorites?page=${page}&pageSize=${pageSize}`)
    return data
}
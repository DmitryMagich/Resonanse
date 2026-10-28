import axios from 'axios'
import { useAuthStore } from '@/stores/authStore'

export const api = axios.create({
    baseURL: '/api', // Vite проксирует это на localhost:5173/api
    headers: {
        'Content-Type': 'application/json',
    },
})

// Request interceptor -добавляет Bearer токен
api.interceptors.request.use((config) => {
    const token = useAuthStore.getState().accessToken
    if (token) {
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})

// Response interceptor: если 401 — разлогиниваем
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            useAuthStore.getState().clearAuth()
            if (window.location.pathname !== '/login') {
                window.location.href = '/login'
            }
        }
        return Promise.reject(error)
    }
)
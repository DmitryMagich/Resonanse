import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export const useAuthStore = create(
    persist(
        (set) => ({
            accessToken: null,
            refreshToken: null,
            user: null,
            isAuthenticated: false,

            setAuth: ({ accessToken, refreshToken, user }) =>
                set({
                    accessToken,
                    refreshToken,
                    user,
                    isAuthenticated: true,
                }),

            clearAuth: () =>
                set({
                    accessToken: null,
                    refreshToken: null,
                    user: null,
                    isAuthenticated: false,
                }),
        }),
        { name: 'resonance-auth' }
    )
)
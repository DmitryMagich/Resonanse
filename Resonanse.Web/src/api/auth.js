import { api } from './client'

export const login = async (username, password) => {
    const { data } = await api.post('/auth/login', { username, password })
    return data
}

export const register = async (inviteCode, username, password) => {
    const { data } = await api.post('/auth/register', { inviteCode, username, password })
    return data
}

export const requestInvite = async (body) => {
    const { data } = await api.post('/public/invite-requests', body)
    return data
}
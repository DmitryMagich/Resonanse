import { api } from './client'

export const getInvites = async () => {
    const { data } = await api.get('/admin/invites')
    return data
}

export const createInvite = async (validDays = 7) => {
    const { data } = await api.post('/admin/invites', { validDays })
    return data
}

export const deleteInvite = async (id) => {
    await api.delete(`/admin/invites/${id}`)
}

export const getInviteRequests = async (status = null) => {
    const url = status ? `/admin/invite-requests?status=${status}` : '/admin/invite-requests'
    const { data } = await api.get(url)
    return data
}

export const approveInviteRequest = async (id) => {
    const { data } = await api.post(`/admin/invite-requests/${id}/approve`)
    return data
}

export const rejectInviteRequest = async (id) => {
    const { data } = await api.post(`/admin/invite-requests/${id}/reject`)
    return data
}

export const getLogs = async (page = 1, pageSize = 50, level = null) => {
    const url = level ? `/admin/logs?page=${page}&pageSize=${pageSize}&level=${level}` : `/admin/logs?page=${page}&pageSize=${pageSize}`
    const { data } = await api.get(url)
    return data
}

export const triggerScan = async (path = null) => {
    const url = path ? `/library/scan?path=${encodeURIComponent(path)}` : '/library/scan'
    const { data } = await api.post(url)
    return data
}

export const getScanStatus = async (jobId) => {
    const { data } = await api.get(`/library/scan/status/${jobId}`)
    return data
}

export const getLatestScan = async () => {
    const { data } = await api.get('/library/scan/latest')
    return data
}
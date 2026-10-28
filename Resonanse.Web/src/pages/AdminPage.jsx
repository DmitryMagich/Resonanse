import { useState, useEffect } from 'react'
import {
    Shield, Users, Server, Copy, Check, Plus, Trash2,
    RefreshCw, AlertCircle, CheckCircle2, XCircle
} from 'lucide-react'
import {
    getInvites, createInvite, deleteInvite,
    getInviteRequests, approveInviteRequest, rejectInviteRequest,
    getLogs, triggerScan, getLatestScan
} from '../api/admin'

const TABS = [
    { id: 'invites', label: 'Invites', icon: Users },
    { id: 'requests', label: 'Requests', icon: Shield },
    { id: 'logs', label: 'Logs', icon: AlertCircle },
    { id: 'system', label: 'System', icon: Server },
]

export default function AdminPage() {
    const [activeTab, setActiveTab] = useState('invites')

    return (
        <div className="p-6 md:p-8 max-w-7xl mx-auto space-y-8">
            <header>
                <h1 className="text-4xl font-bold text-white tracking-tight mb-2 flex items-center gap-3">
                    <Shield className="text-accent" size={32} />
                    Admin Panel
                </h1>
                <p className="text-neutral-400">Manage your server, users, and library.</p>
            </header>

            {/* Табы */}
            <div className="flex gap-2 border-b border-neutral-800 pb-2 overflow-x-auto">
                {TABS.map(tab => {
                    const Icon = tab.icon
                    const isActive = activeTab === tab.id
                    return (
                        <button
                            key={tab.id}
                            onClick={() => setActiveTab(tab.id)}
                            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                                isActive ? 'bg-accent text-white' : 'text-neutral-400 hover:text-white hover:bg-neutral-800'
                            }`}
                        >
                            <Icon size={16} />
                            {tab.label}
                        </button>
                    )
                })}
            </div>

            {/* Контент вкладок */}
            <div className="animate-in fade-in duration-300">
                {activeTab === 'invites' && <InvitesTab />}
                {activeTab === 'requests' && <RequestsTab />}
                {activeTab === 'logs' && <LogsTab />}
                {activeTab === 'system' && <SystemTab />}
            </div>
        </div>
    )
}

// ==========================================
// ВКЛАДКА 1: ИНВАЙТЫ
// ==========================================
function InvitesTab() {
    const [invites, setInvites] = useState([])
    const [loading, setLoading] = useState(true)
    const [copiedId, setCopiedId] = useState(null)

    const loadInvites = async () => {
        setLoading(true)
        try {
            const data = await getInvites()
            setInvites(data)
        } catch (err) {
            console.error(err)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => { loadInvites() }, [])

    const handleCreate = async () => {
        try {
            const newInvite = await createInvite(7)
            setInvites([newInvite, ...invites])
        } catch (err) {
            alert('Failed to create invite')
        }
    }

    const handleDelete = async (id) => {
        if (!confirm('Delete this invite?')) return
        try {
            await deleteInvite(id)
            setInvites(invites.filter(i => i.id !== id))
        } catch (err) {
            alert('Failed to delete invite')
        }
    }

    const copyToClipboard = (code, id) => {
        navigator.clipboard.writeText(code)
        setCopiedId(id)
        setTimeout(() => setCopiedId(null), 2000)
    }

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h2 className="text-xl font-semibold text-white">Active Invites</h2>
                <button onClick={handleCreate} className="flex items-center gap-2 px-4 py-2 bg-accent text-white rounded-lg hover:opacity-90 transition-opacity">
                    <Plus size={16} /> Create Invite
                </button>
            </div>

            {loading ? (
                <div className="text-neutral-400">Loading...</div>
            ) : (
                <div className="bg-[#181818] border border-neutral-800 rounded-xl overflow-hidden">
                    <table className="w-full text-left text-sm">
                        <thead className="bg-[#282828] text-neutral-400 border-b border-neutral-800">
                        <tr>
                            <th className="px-4 py-3">Code</th>
                            <th className="px-4 py-3">Expires</th>
                            <th className="px-4 py-3">Status</th>
                            <th className="px-4 py-3 text-right">Actions</th>
                        </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-800/50">
                        {invites.map(inv => (
                            <tr key={inv.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-4 py-3 font-mono text-white flex items-center gap-2">
                                    {inv.code}
                                    <button onClick={() => copyToClipboard(inv.code, inv.id)} className="text-neutral-500 hover:text-accent transition-colors">
                                        {copiedId === inv.id ? <Check size={14} className="text-emerald-500" /> : <Copy size={14} />}
                                    </button>
                                </td>
                                <td className="px-4 py-3 text-neutral-400">{new Date(inv.expiresAt).toLocaleDateString()}</td>
                                <td className="px-4 py-3">
                                    {inv.isActive
                                        ? <span className="text-emerald-500 flex items-center gap-1"><CheckCircle2 size={14}/> Active</span>
                                        : <span className="text-neutral-500 flex items-center gap-1"><XCircle size={14}/> Expired</span>
                                    }
                                </td>
                                <td className="px-4 py-3 text-right">
                                    <button onClick={() => handleDelete(inv.id)} className="text-neutral-500 hover:text-red-500 transition-colors">
                                        <Trash2 size={16} />
                                    </button>
                                </td>
                            </tr>
                        ))}
                        {invites.length === 0 && (
                            <tr><td colSpan="4" className="px-4 py-8 text-center text-neutral-500">No invites yet.</td></tr>
                        )}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    )
}

// ==========================================
// ВКЛАДКА 2: ЗАЯВКИ
// ==========================================
function RequestsTab() {
    const [requests, setRequests] = useState([])
    const [loading, setLoading] = useState(true)

    const loadRequests = async () => {
        try {
            const data = await getInviteRequests('Pending')
            setRequests(data)
        } catch (err) {
            console.error(err)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => { loadRequests() }, [])

    const handleApprove = async (id) => {
        await approveInviteRequest(id)
        loadRequests()
    }

    const handleReject = async (id) => {
        await rejectInviteRequest(id)
        loadRequests()
    }

    return (
        <div className="space-y-4">
            <h2 className="text-xl font-semibold text-white">Pending Requests</h2>
            {loading ? (
                <div className="text-neutral-400">Loading...</div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {requests.map(req => (
                        <div key={req.id} className="bg-[#181818] border border-neutral-800 rounded-xl p-5 flex flex-col gap-3">
                            <div>
                                <h3 className="font-semibold text-white text-lg">{req.name}</h3>
                                {req.contact && <p className="text-sm text-neutral-400">{req.contact}</p>}
                            </div>
                            {req.comment && (
                                <p className="text-sm text-neutral-300 bg-neutral-900 p-2 rounded-md italic border border-neutral-800/50">"{req.comment}"</p>
                            )}
                            <div className="flex gap-2 mt-auto pt-2">
                                <button onClick={() => handleApprove(req.id)} className="flex-1 py-1.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded text-sm transition-colors">Approve</button>
                                <button onClick={() => handleReject(req.id)} className="flex-1 py-1.5 bg-neutral-800 hover:bg-red-900 text-neutral-300 rounded text-sm transition-colors">Reject</button>
                            </div>
                        </div>
                    ))}
                    {requests.length === 0 && (
                        <div className="col-span-full p-8 text-center text-neutral-500 bg-[#181818] rounded-xl border border-neutral-800 border-dashed">
                            No pending requests.
                        </div>
                    )}
                </div>
            )}
        </div>
    )
}

// ==========================================
// ВКЛАДКА 3: ЛОГИ
// ==========================================
function LogsTab() {
    const [logs, setLogs] = useState([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        const fetchLogs = async () => {
            try {
                const data = await getLogs(1, 50)
                setLogs(data.items)
            } catch (err) {
                console.error(err)
            } finally {
                setLoading(false)
            }
        }
        fetchLogs()
    }, [])

    return (
        <div className="space-y-4">
            <h2 className="text-xl font-semibold text-white">System Logs (Last 50)</h2>
            {loading ? (
                <div className="text-neutral-400">Loading...</div>
            ) : (
                <div className="bg-[#181818] border border-neutral-800 rounded-xl overflow-hidden">
                    <table className="w-full text-left text-sm">
                        <thead className="bg-[#282828] text-neutral-400 border-b border-neutral-800">
                        <tr>
                            <th className="px-4 py-3">Time</th>
                            <th className="px-4 py-3">Level</th>
                            <th className="px-4 py-3">Message</th>
                            <th className="px-4 py-3">Path</th>
                        </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-800/50">
                        {logs.map(log => (
                            <tr key={log.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-4 py-3 text-neutral-500 whitespace-nowrap">{new Date(log.createdAt).toLocaleString()}</td>
                                <td className="px-4 py-3">
                    <span className={`px-2 py-0.5 rounded text-xs font-bold ${
                        log.level === 'Error' || log.level === 'Critical' ? 'bg-red-900/50 text-red-400' : 'bg-orange-900/50 text-orange-400'
                    }`}>
                      {log.level}
                    </span>
                                </td>
                                <td className="px-4 py-3 text-neutral-300 max-w-md truncate" title={log.message}>{log.message}</td>
                                <td className="px-4 py-3 text-neutral-500 font-mono text-xs">{log.path}</td>
                            </tr>
                        ))}
                        {logs.length === 0 && (
                            <tr><td colSpan="4" className="px-4 py-8 text-center text-neutral-500">No logs found.</td></tr>
                        )}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    )
}

// ==========================================
// ВКЛАДКА 4: СИСТЕМА / СКАН
// ==========================================
function SystemTab() {
    const [scanJob, setScanJob] = useState(null)
    const [scanning, setScanning] = useState(false)

    const checkLatest = async () => {
        try {
            const data = await getLatestScan()
            setScanJob(data)
            if (data.status === 0 || data.status === 1) setScanning(true)
            else setScanning(false)
        } catch (err) {
            // 404 если еще не было сканов
        }
    }

    useEffect(() => {
        checkLatest()
        const interval = setInterval(checkLatest, 2000)
        return () => clearInterval(interval)
    }, [])

    const handleScan = async () => {
        try {
            setScanning(true)
            await triggerScan()
            checkLatest()
        } catch (err) {
            alert(err.response?.data?.error || 'Failed to start scan')
            setScanning(false)
        }
    }

    return (
        <div className="space-y-6">
            <div className="bg-[#181818] border border-neutral-800 rounded-xl p-6">
                <h2 className="text-xl font-semibold text-white mb-2">Library Scanner</h2>
                <p className="text-neutral-400 text-sm mb-6">Scan your music folder to update the database with new files and metadata.</p>

                <div className="flex items-center gap-4">
                    <button
                        onClick={handleScan}
                        disabled={scanning}
                        className="flex items-center gap-2 px-6 py-2.5 bg-accent text-white font-medium rounded-lg hover:opacity-90 transition-opacity disabled:opacity-50"
                    >
                        <RefreshCw size={18} className={scanning ? 'animate-spin' : ''} />
                        {scanning ? 'Scanning...' : 'Start Scan'}
                    </button>
                </div>
            </div>

            {scanJob && (
                <div className="bg-[#181818] border border-neutral-800 rounded-xl p-6">
                    <h3 className="text-lg font-semibold text-white mb-4">Last Scan Status</h3>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        <div className="bg-neutral-900 rounded-lg p-3">
                            <p className="text-xs text-neutral-500 uppercase mb-1">Status</p>
                            <p className={`font-medium ${scanJob.status === 2 ? 'text-emerald-500' : scanJob.status === 3 ? 'text-red-500' : 'text-orange-500'}`}>
                                {scanJob.statusName}
                            </p>
                        </div>
                        <div className="bg-neutral-900 rounded-lg p-3">
                            <p className="text-xs text-neutral-500 uppercase mb-1">Processed</p>
                            <p className="font-medium text-white">{scanJob.filesProcessed} files</p>
                        </div>
                        <div className="bg-neutral-900 rounded-lg p-3">
                            <p className="text-xs text-neutral-500 uppercase mb-1">Created</p>
                            <p className="font-medium text-white">{scanJob.tracksCreated} new</p>
                        </div>
                        <div className="bg-neutral-900 rounded-lg p-3">
                            <p className="text-xs text-neutral-500 uppercase mb-1">Errors</p>
                            <p className={`font-medium ${scanJob.errors > 0 ? 'text-red-400' : 'text-neutral-400'}`}>{scanJob.errors}</p>
                        </div>
                    </div>
                    {scanJob.errorMessages && scanJob.errorMessages.length > 0 && (
                        <div className="mt-4 p-3 bg-red-900/20 border border-red-900/50 rounded-lg">
                            <p className="text-xs font-bold text-red-400 mb-2">Errors:</p>
                            <ul className="text-xs text-red-300 list-disc list-inside space-y-1">
                                {scanJob.errorMessages.map((msg, i) => <li key={i}>{msg}</li>)}
                            </ul>
                        </div>
                    )}
                </div>
            )}
        </div>
    )
}
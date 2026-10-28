import { NavLink } from 'react-router-dom'
import { Disc, Home, Music, Settings, Shield, User } from 'lucide-react'

const NAV_ITEMS = [
    { to: '/', label: 'Home', icon: Home, end: true },
    { to: '/tracks', label: 'Tracks', icon: Music },
    { to: '/albums', label: 'Albums', icon: Disc },
    { to: '/artists', label: 'Artists', icon: User },
    { to: '/admin', label: 'Admin', icon: Shield },
    { to: '/settings', label: 'Settings', icon: Settings },
]

export default function Sidebar() {
    return (
        <aside className="hidden md:flex w-60 shrink-0 flex-col border-r border-neutral-800/50 bg-[#121212] p-3">
            <nav className="flex-1 p-3 space-y-1">
                {NAV_ITEMS.map((item) => {
                    const Icon = item.icon
                    return (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            end={item.end}
                            className={({ isActive }) =>
                                `flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-colors ${
                                    isActive
                                        ? 'bg-neutral-900 text-accent font-medium'
                                        : 'text-neutral-400 hover:text-neutral-100 hover:bg-neutral-900/60'
                                }`
                            }
                        >
                            <Icon size={18} />
                            <span>{item.label}</span>
                        </NavLink>
                    )
                })}
            </nav>
        </aside>
    )
}
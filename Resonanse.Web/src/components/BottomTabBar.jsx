import { NavLink } from 'react-router-dom'
import { Home, Music, Disc, User, Settings } from 'lucide-react'

const ITEMS = [
    { to: '/', icon: Home, label: 'Home', end: true },
    { to: '/tracks', icon: Music, label: 'Tracks' },
    { to: '/albums', icon: Disc, label: 'Albums' },
    { to: '/artists', icon: User, label: 'Artists' },
    { to: '/settings', icon: Settings, label: 'Settings' },
]

export default function BottomTabBar() {
    return (
        <nav className="fixed bottom-0 left-0 right-0 z-50 h-16 bg-[#121212] border-t border-neutral-800 md:hidden flex items-center justify-around pb-safe">
            {ITEMS.map((item) => {
                const Icon = item.icon
                return (
                    <NavLink
                        key={item.to}
                        to={item.to}
                        end={item.end}
                        className={({ isActive }) =>
                            `flex flex-col items-center justify-center w-full h-full gap-1 transition-colors ${
                                isActive ? 'text-accent' : 'text-neutral-500 hover:text-neutral-300'
                            }`
                        }
                    >
                        {({ isActive }) => (
                            <>
                                <Icon
                                    size={20}
                                    className={isActive ? 'drop-shadow-[0_0_8px_var(--accent-glow)]' : ''}
                                />
                                <span className="text-[10px] font-medium">{item.label}</span>
                            </>
                        )}
                    </NavLink>
                )
            })}
        </nav>
    )
}
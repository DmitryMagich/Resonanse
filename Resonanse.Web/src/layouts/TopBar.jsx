import { Link, useNavigate } from 'react-router-dom'
import { LogOut, Search, User } from 'lucide-react'
import AppLogo from '../components/AppLogo'
import ModeToggle from '../components/ModeToggle'
import { useAuthStore } from '../stores/authStore'

export default function TopBar() {
    const user = useAuthStore((s) => s.user)
    const clearAuth = useAuthStore((s) => s.clearAuth)
    const navigate = useNavigate()

    const handleLogout = () => {
        clearAuth()
        navigate('/login')
    }

    return (
        <header className="h-16 flex items-center justify-between px-6 border-b border-neutral-800/50 bg-[#121212]/90 backdrop-blur-md z-50 sticky top-0">
            {/* Левая часть: Лого и Тумблер */}
            <div className="flex items-center gap-6">
                <Link to="/" className="flex items-center shrink-0">
                    {/* Увеличили лого до md (56px) */}
                    <AppLogo size="md" />
                </Link>

                {/* Тумблер теперь рядом с лого */}
                <div className="hidden sm:block">
                    <ModeToggle />
                </div>
            </div>

            {/* Центральная часть: Поиск */}
            <div className="flex-1 max-w-lg mx-6">
                <div className="relative group">
                    <Search
                        size={16}
                        className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-500 group-focus-within:text-accent transition-colors"
                    />
                    <input
                        type="text"
                        placeholder="Search library..."
                        className="w-full pl-9 pr-3 py-2 bg-neutral-900 border border-neutral-800 rounded-lg text-sm text-neutral-100 placeholder:text-neutral-600 focus:outline-none focus:border-accent focus:ring-1 focus:ring-accent transition-all"
                    />
                </div>
            </div>

            {/* Правая часть: Юзер */}
            <div className="flex items-center gap-3 shrink-0">
                {user ? (
                    <>
                        <div className="hidden md:flex items-center gap-2 px-3 py-1.5 rounded-lg bg-neutral-900 border border-neutral-800">
                            <User size={14} className="text-accent" />
                            <span className="text-sm text-neutral-300 font-medium">{user.username}</span>
                        </div>
                        <button
                            onClick={handleLogout}
                            className="p-2 rounded-lg hover:bg-neutral-800 text-neutral-400 hover:text-red-400 transition-colors"
                            title="Logout"
                        >
                            <LogOut size={18} />
                        </button>
                    </>
                ) : (
                    <Link
                        to="/login"
                        className="px-4 py-2 rounded-lg bg-accent text-white text-sm font-medium hover:opacity-90 transition-opacity"
                    >
                        Login
                    </Link>
                )}
            </div>
        </header>
    )
}
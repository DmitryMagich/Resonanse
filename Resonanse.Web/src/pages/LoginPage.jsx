import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import AppButton from '@/components/AppButton'
import AppInput from '@/components/AppInput'
import { login } from '@/api/auth'
import { useAuthStore } from '@/stores/authStore'

export default function LoginPage() {
    const [username, setUsername] = useState('')
    const [password, setPassword] = useState('')
    const [error, setError] = useState('')
    const [loading, setLoading] = useState(false)

    const navigate = useNavigate()
    const setAuth = useAuthStore((s) => s.setAuth)

    const handleSubmit = async (e) => {
        e.preventDefault()
        setError('')
        setLoading(true)
        try {
            const data = await login(username, password)
            setAuth(data)
            navigate('/')
        } catch (err) {
            setError(err.response?.data?.error || 'Login failed')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div>
                <h2 className="text-2xl font-semibold text-white">Sign In</h2>
                <p className="text-neutral-400 mt-1">Enter your credentials to access your library.</p>
            </div>

            <form className="space-y-5" onSubmit={handleSubmit}>
                {error && (
                    <div className="p-3 bg-red-900/30 border border-red-900/50 text-red-300 rounded-lg text-sm">
                        {error}
                    </div>
                )}

                <AppInput
                    label="Username"
                    placeholder="admin"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    required
                />
                <AppInput
                    label="Password"
                    type="password"
                    placeholder="••••••••"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />

                <AppButton type="submit" variant="gradient" size="lg" className="w-full mt-4" disabled={loading}>
                    {loading ? 'Signing in...' : 'Sign in'}
                </AppButton>
            </form>

            <div className="text-center text-sm text-neutral-500 space-y-2">
                <p>
                    Have an invite?{' '}
                    <Link to="/register" className="text-accent hover:text-white transition-colors">Register</Link>
                </p>
                <p>
                    No invite?{' '}
                    <Link to="/invite-request" className="text-accent hover:text-white transition-colors">Request one</Link>
                </p>
            </div>
        </div>
    )
}
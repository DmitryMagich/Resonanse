import { Link } from 'react-router-dom'
import AppButton from '@/components/AppButton'
import AppInput from '@/components/AppInput'

export default function RegisterPage() {
    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div>
                <h2 className="text-2xl font-semibold text-white">Create Account</h2>
                <p className="text-neutral-400 mt-1">You need a valid invite code to join.</p>
            </div>

            <form className="space-y-5" onSubmit={(e) => e.preventDefault()}>
                <AppInput label="Invite code" placeholder="xxxx-xxxx-xxxx-xxxx" />
                <AppInput label="Username" placeholder="yourname" />
                <AppInput label="Password" type="password" placeholder="••••••••" />
                <AppButton type="submit" variant="gradient" size="lg" className="w-full mt-4">
                    Create account
                </AppButton>
            </form>

            <div className="text-center text-sm text-neutral-500">
                Already have an account?{' '}
                <Link to="/login" className="text-accent hover:text-white transition-colors">Sign in</Link>
            </div>
        </div>
    )
}
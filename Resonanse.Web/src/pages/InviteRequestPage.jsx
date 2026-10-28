import { Link } from 'react-router-dom'
import AppButton from '@/components/AppButton'
import AppInput from '@/components/AppInput'

export default function InviteRequestPage() {
    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div>
                <h2 className="text-2xl font-semibold text-white">Request Access</h2>
                <p className="text-neutral-400 mt-1">Tell us a bit about yourself to get an invite.</p>
            </div>

            <form className="space-y-5" onSubmit={(e) => e.preventDefault()}>
                <AppInput label="Your name" placeholder="Dmitry" />
                <AppInput label="Contact (optional)" placeholder="@telegram or email" />
                <label className="block group">
          <span className="block mb-1.5 text-sm text-neutral-400 transition-colors group-focus-within:text-accent">
            Comment (optional)
          </span>
                    <textarea
                        rows={3}
                        className="w-full px-3 py-2 bg-neutral-900/50 border border-neutral-800 rounded-lg text-neutral-100 placeholder:text-neutral-600 focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/30 transition-all duration-300 hover:border-neutral-700"
                        placeholder="Why do you want to join Resonance?"
                    />
                </label>
                <AppButton type="submit" variant="gradient" size="lg" className="w-full mt-4">
                    Send request
                </AppButton>
            </form>

            <div className="text-center text-sm text-neutral-500">
                <Link to="/login" className="text-accent hover:text-white transition-colors">
                    Back to login
                </Link>
            </div>
        </div>
    )
}
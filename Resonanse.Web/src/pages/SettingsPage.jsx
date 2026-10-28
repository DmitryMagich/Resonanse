import ModeToggle from '@/components/ModeToggle'

export default function SettingsPage() {
    return (
        <div className="p-6 max-w-2xl">
            <h1 className="text-2xl font-semibold mb-6">Settings</h1>

            <section className="mb-8">
                <h2 className="text-sm text-neutral-400 mb-2">Mode</h2>
                <ModeToggle />
            </section>

            <section className="mb-8">
                <h2 className="text-sm text-neutral-400 mb-2">Accent preview</h2>
                <div className="flex gap-2">
                    <div className="px-3 py-1.5 rounded-lg bg-accent text-white text-sm">
                        Accent
                    </div>
                    <div className="px-3 py-1.5 rounded-lg bg-gradient-accent text-white text-sm">
                        Gradient
                    </div>
                </div>
            </section>

            <p className="text-neutral-500 font-mono text-sm">Coming soon</p>
        </div>
    )
}
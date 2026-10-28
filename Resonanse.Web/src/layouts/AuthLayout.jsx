import { Outlet } from 'react-router-dom'
import AppLogo from '@/components/AppLogo'

export default function AuthLayout() {
    return (
        <div className="min-h-screen w-full flex items-center justify-center p-0 md:p-6 bg-neutral-950 relative overflow-hidden">

            {/* Анимированные цветные пятна на фоне (для эффекта стекла) */}
            <div className="absolute top-[-10%] left-[-10%] w-[40vw] h-[40vw] bg-orange-600/20 rounded-full blur-[120px] mix-blend-screen animate-pulse duration-[10000ms]" />
            <div className="absolute bottom-[-10%] right-[-10%] w-[40vw] h-[40vw] bg-purple-600/20 rounded-full blur-[120px] mix-blend-screen animate-pulse duration-[8000ms]" />
            <div className="absolute top-[40%] left-[30%] w-[30vw] h-[30vw] bg-emerald-600/10 rounded-full blur-[100px] mix-blend-screen animate-pulse duration-[12000ms]" />

            {/* Главный стеклянный контейнер (на весь экран на мобилке, и по центру на ПК) */}
            <div className="w-full max-w-6xl h-full md:h-[85vh] md:max-h-[800px] flex flex-col md:flex-row bg-neutral-900/40 backdrop-blur-3xl border border-white/10 md:rounded-3xl shadow-2xl overflow-hidden relative z-10">

                {/* Левая часть: Логотип и приветствие (скрыта на мобилках) */}
                <div className="hidden md:flex flex-col items-center justify-center flex-1 p-12 border-r border-white/5 relative bg-gradient-to-br from-white/5 to-transparent">
                    <div className="scale-150 mb-8">
                        <AppLogo size="xl" />
                    </div>
                    <h1 className="text-4xl font-bold text-white mb-4 text-center tracking-tight">
                        Welcome to <span className="text-transparent bg-clip-text bg-gradient-to-r from-orange-500 to-purple-500">Resonance</span>
                    </h1>
                    <p className="text-neutral-400 text-center max-w-sm">
                        Your personal Hi-Fi music library. Private, self-hosted, and limitless.
                    </p>
                </div>

                {/* Правая часть: Форма (здесь будет рендериться Login/Register/Invite) */}
                <div className="flex-1 flex items-center justify-center p-6 md:p-12 relative">
                    <div className="w-full max-w-md">
                        {/* Логотип для мобилок */}
                        <div className="md:hidden flex justify-center mb-8">
                            <AppLogo size="lg" />
                        </div>
                        <Outlet />
                    </div>
                </div>
            </div>
        </div>
    )
}
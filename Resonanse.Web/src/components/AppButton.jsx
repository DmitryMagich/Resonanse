export default function AppButton({
                                      children,
                                      variant = 'primary',
                                      size = 'md',
                                      className = '',
                                      ...props
                                  }) {
    const base = 'relative overflow-hidden inline-flex items-center justify-center gap-2 font-medium rounded-lg transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed group'

    const sizes = {
        sm: 'px-3 py-1.5 text-xs',
        md: 'px-4 py-2 text-sm',
        lg: 'px-6 py-3 text-base',
    }

    const variants = {
        primary: 'bg-accent text-white hover:bg-opacity-90 hover:scale-[1.02] active:scale-[0.98] hover:shadow-[0_0_15px_var(--accent-glow)]',
        secondary: 'bg-neutral-800 text-neutral-100 hover:bg-neutral-700 hover:scale-[1.02] active:scale-[0.98]',
        ghost: 'bg-transparent text-neutral-300 hover:bg-neutral-800 hover:text-white hover:scale-[1.02] active:scale-[0.98]',
        danger: 'bg-red-600 text-white hover:bg-red-500 hover:scale-[1.02] active:scale-[0.98] hover:shadow-[0_0_15px_rgba(220,38,38,0.4)]',
        gradient: 'bg-gradient-accent text-white hover:scale-[1.02] active:scale-[0.98] hover:shadow-[0_0_20px_rgba(168,85,247,0.5)]',
    }

    return (
        <button
            className={`${base} ${sizes[size]} ${variants[variant]} ${className}`}
            {...props}
        >
            {}  
            {variant === 'gradient' && (
                <span className="absolute inset-0 w-full h-full bg-gradient-to-r from-transparent via-white/20 to-transparent -translate-x-full group-hover:animate-[shimmer_1.5s_infinite]" />
            )}
            <span className="relative z-10 flex items-center gap-2">{children}</span>
        </button>
    )
}
export default function AppInput({ label, className = '', ...props }) {
    return (
        <label className="block group">
            {label && (
                <span className="block mb-1.5 text-sm text-neutral-400 transition-colors duration-300 group-focus-within:text-accent">
          {label}
        </span>
            )}
            <input
                className={`w-full px-3 py-2 bg-neutral-900/50 border border-neutral-800 rounded-lg text-neutral-100 placeholder:text-neutral-600 focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/30 transition-all duration-300 hover:border-neutral-700 ${className}`}
                {...props}
            />
        </label>
    )
}
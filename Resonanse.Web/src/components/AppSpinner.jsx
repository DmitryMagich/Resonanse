import { Loader2 } from 'lucide-react'

export default function AppSpinner({ size = 20, className = '' }) {
    return (
        <Loader2
            size={size}
            className={`animate-spin text-neutral-400 ${className}`}
        />
    )
}
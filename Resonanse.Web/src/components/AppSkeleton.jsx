export default function AppSkeleton({ className = '', count = 1 }) {
    return (
        <>
            {Array.from({ length: count }).map((_, i) => (
                <div
                    key={i}
                    className={`animate-pulse rounded-lg bg-neutral-800/50 ${className}`}
                />
            ))}
        </>
    )
}
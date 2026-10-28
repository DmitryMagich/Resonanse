import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.jsx'
import './index.css'
import { useModeStore } from './stores/modeStore'

// Apply persisted mode on initial load
const initialMode = useModeStore.getState().mode
document.documentElement.setAttribute('data-mode', initialMode)

createRoot(document.getElementById('root')).render(
    <StrictMode>
        <App />
    </StrictMode>,
)
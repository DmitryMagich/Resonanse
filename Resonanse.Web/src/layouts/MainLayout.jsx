import { Outlet } from 'react-router-dom'
import Sidebar from './Sidebar'
import TopBar from './TopBar'
import PlayerBar from '../components/PlayerBar'
import BottomTabBar from '../components/BottomTabBar'

export default function MainLayout() {
    return (
        <div className="h-screen flex flex-col bg-[#121212]">
            <TopBar />
            <div className="flex-1 flex min-h-0">
                <Sidebar />
                <main className="flex-1 overflow-y-auto pb-36 md:pb-24">
                    <Outlet />
                </main>
            </div>

            <PlayerBar />
            <BottomTabBar />
        </div>
    )
}
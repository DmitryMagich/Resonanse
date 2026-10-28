import { createBrowserRouter, Navigate } from 'react-router-dom'
import MainLayout from './layouts/MainLayout'
import AuthLayout from './layouts/AuthLayout'
import ProtectedRoute from './components/ProtectedRoute'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import InviteRequestPage from './pages/InviteRequestPage'
import HomePage from './pages/HomePage'
import TracksPage from './pages/TracksPage'
import AlbumsPage from './pages/AlbumsPage'
import ArtistsPage from './pages/ArtistsPage'
import AdminPage from './pages/AdminPage'
import SettingsPage from './pages/SettingsPage'
import AlbumDetailsPage from './pages/AlbumDetailsPage'
import ArtistDetailsPage from './pages/ArtistDetailsPage'

export const router = createBrowserRouter([
    {
        element: <AuthLayout />,
        children: [
            { path: '/login', element: <LoginPage /> },
            { path: '/register', element: <RegisterPage /> },
            { path: '/invite-request', element: <InviteRequestPage /> },
        ],
    },
    {
        path: '/',
        element: <ProtectedRoute />,
        children: [
            {
                element: <MainLayout />,
                children: [
                    { index: true, element: <HomePage /> },
                    { path: 'tracks', element: <TracksPage /> },
                    { path: 'albums', element: <AlbumsPage /> },
                    { path: 'artists', element: <ArtistsPage /> },
                    { path: 'admin', element: <AdminPage /> },
                    { path: 'settings', element: <SettingsPage /> },
                    { path: 'albums/:id', element: <AlbumDetailsPage /> },
                    { path: 'artists/:id', element: <ArtistDetailsPage /> },
                ],
            },
        ],
    },
    { path: '*', element: <Navigate to="/" replace /> },
])
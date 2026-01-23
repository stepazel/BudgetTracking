import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router'
import {Login} from '@/pages/Login.tsx'
import './App.css'
import Homepage from "@/pages/Homepage.tsx";

const router = createBrowserRouter([
  { path: '/', element: <Homepage /> },
  { path: '/login', element: <Login /> },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)

import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

interface Props {
  roles?: string[]
}

export function RequireAuth({ roles }: Props) {
  const { token, hasRole } = useAuth()
  if (!token) return <Navigate to="/login" replace />
  if (roles && !hasRole(...roles)) return <Navigate to="/unauthorized" replace />
  return <Outlet />
}

import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export function NavBar() {
  const { email, roles, logout, hasRole } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <nav className="bg-blue-700 text-white px-6 py-3 flex items-center justify-between shadow">
      <div className="flex items-center gap-6 font-medium">
        <Link to="/" className="text-lg font-bold tracking-tight">
          Service Tracker
        </Link>

        {hasRole('Admin') && (
          <>
            <Link to="/admin/customers" className="hover:text-blue-200">Customers</Link>
            <Link to="/admin/technicians" className="hover:text-blue-200">Technicians</Link>
            <Link to="/admin/users" className="hover:text-blue-200">Users</Link>
          </>
        )}

        {hasRole('Admin', 'Dispatcher') && (
          <Link to="/dispatcher/tickets" className="hover:text-blue-200">Tickets</Link>
        )}

        {hasRole('Technician') && (
          <Link to="/technician/tickets" className="hover:text-blue-200">My Tickets</Link>
        )}
      </div>

      <div className="flex items-center gap-4 text-sm">
        <span className="opacity-75">{email}</span>
        <span className="bg-blue-800 px-2 py-0.5 rounded text-xs">{roles.join(', ')}</span>
        <button
          onClick={handleLogout}
          className="bg-white text-blue-700 px-3 py-1 rounded font-semibold hover:bg-blue-100"
        >
          Log out
        </button>
      </div>
    </nav>
  )
}

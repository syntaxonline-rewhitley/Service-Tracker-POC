import { useAuth } from '../context/AuthContext'
import { NavBar } from '../components/NavBar'
import { PageShell } from '../components/PageShell'

export function HomePage() {
  const { email, roles } = useAuth()

  return (
    <>
      <NavBar />
      <PageShell>
        <div className="bg-white rounded-xl shadow p-8 text-center">
          <h2 className="text-xl font-semibold text-gray-800 mb-2">Welcome back, {email}</h2>
          <p className="text-gray-500">
            You are signed in as <span className="font-medium text-blue-600">{roles.join(', ')}</span>.
            Use the navigation bar to get started.
          </p>
        </div>
      </PageShell>
    </>
  )
}

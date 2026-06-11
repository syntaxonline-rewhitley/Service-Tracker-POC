import { NavBar } from '../components/NavBar'
import { PageShell } from '../components/PageShell'

export function UnauthorizedPage() {
  return (
    <>
      <NavBar />
      <PageShell>
        <div className="bg-red-50 border border-red-200 rounded-xl p-8 text-center">
          <h2 className="text-xl font-semibold text-red-700 mb-2">Access Denied</h2>
          <p className="text-red-600">You do not have permission to view this page.</p>
        </div>
      </PageShell>
    </>
  )
}

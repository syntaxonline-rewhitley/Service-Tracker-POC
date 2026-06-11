import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { Spinner } from '../../components/Spinner'
import { ErrorMessage } from '../../components/ErrorMessage'
import { getUsers } from '../../lib/api'
import type { UserListItem } from '../../types'

const roleBadgeClass: Record<string, string> = {
  Admin: 'bg-purple-100 text-purple-700',
  Dispatcher: 'bg-blue-100 text-blue-700',
  Technician: 'bg-green-100 text-green-700',
}

export function AdminUsersPage() {
  const [users, setUsers] = useState<UserListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    getUsers()
      .then(setUsers)
      .catch(() => setError('Failed to load users.'))
      .finally(() => setLoading(false))
  }, [])

  return (
    <>
      <NavBar />
      <PageShell title="Users">
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        <div className="flex justify-end mb-4">
          <Link
            to="/admin/users/new"
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700"
          >
            + Add User
          </Link>
        </div>
        {loading ? <Spinner /> : (
          <div className="bg-white rounded-xl shadow overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 uppercase text-xs">
                <tr>
                  <th className="px-4 py-3 text-left">Email</th>
                  <th className="px-4 py-3 text-left">Role</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {users.map((u) => (
                  <tr key={u.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium">{u.email}</td>
                    <td className="px-4 py-3">
                      <div className="flex gap-1 flex-wrap">
                        {u.roles.map((r) => (
                          <span
                            key={r}
                            className={`inline-block px-2 py-0.5 rounded text-xs font-medium ${roleBadgeClass[r] ?? 'bg-gray-100 text-gray-600'}`}
                          >
                            {r}
                          </span>
                        ))}
                        {u.roles.length === 0 && <span className="text-gray-400">—</span>}
                      </div>
                    </td>
                  </tr>
                ))}
                {users.length === 0 && (
                  <tr><td colSpan={2} className="px-4 py-8 text-center text-gray-400">No users found.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </PageShell>
    </>
  )
}

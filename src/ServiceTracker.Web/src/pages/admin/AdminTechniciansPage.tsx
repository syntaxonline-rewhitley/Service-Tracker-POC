import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { Spinner } from '../../components/Spinner'
import { ErrorMessage } from '../../components/ErrorMessage'
import { getTechnicians, deleteTechnician } from '../../lib/api'
import type { Technician } from '../../types'

export function AdminTechniciansPage() {
  const [technicians, setTechnicians] = useState<Technician[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    getTechnicians()
      .then(setTechnicians)
      .catch(() => setError('Failed to load technicians.'))
      .finally(() => setLoading(false))
  }, [])

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete "${name}"?`)) return
    try {
      await deleteTechnician(id)
      setTechnicians((prev) => prev.filter((t) => t.id !== id))
    } catch {
      alert('Failed to delete technician.')
    }
  }

  return (
    <>
      <NavBar />
      <PageShell title="Technicians">
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        <div className="flex justify-end mb-4">
          <Link to="/admin/technicians/new" className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700">
            + New Technician
          </Link>
        </div>
        {loading ? <Spinner /> : (
          <div className="bg-white rounded-xl shadow overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 uppercase text-xs">
                <tr>
                  <th className="px-4 py-3 text-left">Name</th>
                  <th className="px-4 py-3 text-left">Email</th>
                  <th className="px-4 py-3 text-left">Specialization</th>
                  <th className="px-4 py-3 text-left">Status</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {technicians.map((t) => (
                  <tr key={t.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium">{t.firstName} {t.lastName}</td>
                    <td className="px-4 py-3 text-gray-600">{t.email}</td>
                    <td className="px-4 py-3 text-gray-600">{t.specialization ?? '—'}</td>
                    <td className="px-4 py-3">
                      <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${t.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                        {t.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right space-x-2">
                      <Link to={`/admin/technicians/${t.id}`} className="text-blue-600 hover:underline">Edit</Link>
                      <button onClick={() => handleDelete(t.id, `${t.firstName} ${t.lastName}`)} className="text-red-500 hover:underline">Delete</button>
                    </td>
                  </tr>
                ))}
                {technicians.length === 0 && (
                  <tr><td colSpan={5} className="px-4 py-8 text-center text-gray-400">No technicians yet.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </PageShell>
    </>
  )
}

import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { Spinner } from '../../components/Spinner'
import { ErrorMessage } from '../../components/ErrorMessage'
import { getCompanies, deleteCompany } from '../../lib/api'
import type { Company } from '../../types'

export function AdminCompaniesPage() {
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = () => {
    setLoading(true)
    getCompanies()
      .then(setCompanies)
      .catch(() => setError('Failed to load companies.'))
      .finally(() => setLoading(false))
  }

  useEffect(load, [])

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete "${name}"?`)) return
    try {
      await deleteCompany(id)
      setCompanies((prev) => prev.filter((c) => c.id !== id))
    } catch {
      alert('Failed to delete company.')
    }
  }

  return (
    <>
      <NavBar />
      <PageShell title="Companies">
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        <div className="flex justify-end mb-4">
          <Link
            to="/admin/companies/new"
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700"
          >
            + New Company
          </Link>
        </div>
        {loading ? <Spinner /> : (
          <div className="bg-white rounded-xl shadow overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 uppercase text-xs">
                <tr>
                  <th className="px-4 py-3 text-left">Name</th>
                  <th className="px-4 py-3 text-left">Email</th>
                  <th className="px-4 py-3 text-left">Phone</th>
                  <th className="px-4 py-3 text-left">Website</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {companies.map((c) => (
                  <tr key={c.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium">{c.name}</td>
                    <td className="px-4 py-3 text-gray-600">{c.email ?? '—'}</td>
                    <td className="px-4 py-3 text-gray-600">{c.phone ?? '—'}</td>
                    <td className="px-4 py-3 text-gray-600">{c.website ?? '—'}</td>
                    <td className="px-4 py-3 text-right space-x-2">
                      <Link to={`/admin/companies/${c.id}`} className="text-blue-600 hover:underline">Edit</Link>
                      <button
                        onClick={() => handleDelete(c.id, c.name)}
                        className="text-red-500 hover:underline"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
                {companies.length === 0 && (
                  <tr><td colSpan={5} className="px-4 py-8 text-center text-gray-400">No companies yet.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </PageShell>
    </>
  )
}

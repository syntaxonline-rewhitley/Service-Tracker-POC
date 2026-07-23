import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { Spinner } from '../../components/Spinner'
import { ErrorMessage } from '../../components/ErrorMessage'
import { getContacts, deleteContact } from '../../lib/api'
import type { Contact } from '../../types'

export function AdminContactsPage() {
  const [contacts, setContacts] = useState<Contact[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    getContacts()
      .then(setContacts)
      .catch(() => setError('Failed to load contacts.'))
      .finally(() => setLoading(false))
  }, [])

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Delete "${name}"?`)) return
    try {
      await deleteContact(id)
      setContacts((prev) => prev.filter((c) => c.id !== id))
    } catch {
      alert('Failed to delete contact.')
    }
  }

  return (
    <>
      <NavBar />
      <PageShell title="Contacts">
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        <div className="flex justify-end mb-4">
          <Link to="/admin/contacts/new" className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700">
            + New Contact
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
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {contacts.map((c) => (
                  <tr key={c.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium">{c.firstName} {c.lastName}</td>
                    <td className="px-4 py-3 text-gray-600">{c.email}</td>
                    <td className="px-4 py-3 text-gray-600">{c.phone ?? '—'}</td>
                    <td className="px-4 py-3 text-right space-x-2">
                      <Link to={`/admin/contacts/${c.id}`} className="text-blue-600 hover:underline">Edit</Link>
                      <button onClick={() => handleDelete(c.id, `${c.firstName} ${c.lastName}`)} className="text-red-500 hover:underline">Delete</button>
                    </td>
                  </tr>
                ))}
                {contacts.length === 0 && (
                  <tr><td colSpan={4} className="px-4 py-8 text-center text-gray-400">No contacts yet.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </PageShell>
    </>
  )
}

import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { Spinner } from '../../components/Spinner'
import { ErrorMessage } from '../../components/ErrorMessage'
import {
  getCompanies, deleteCompany,
  getContacts, deleteContact,
} from '../../lib/api'
import type { Company, Contact } from '../../types'

type Tab = 'companies' | 'contacts'

export function AdminCustomersPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const activeTab: Tab = (searchParams.get('tab') as Tab) ?? 'companies'

  const setTab = (tab: Tab) => setSearchParams({ tab })

  return (
    <>
      <NavBar />
      <PageShell title="Customer Relationships">
        {/* Tab bar */}
        <div className="flex gap-1 mb-6 border-b border-gray-200">
          <TabButton label="Companies" active={activeTab === 'companies'} onClick={() => setTab('companies')} />
          <TabButton label="Contacts"  active={activeTab === 'contacts'}  onClick={() => setTab('contacts')}  />
        </div>

        {activeTab === 'companies' && <CompaniesTab />}
        {activeTab === 'contacts'  && <ContactsTab />}
      </PageShell>
    </>
  )
}

function TabButton({ label, active, onClick }: { label: string; active: boolean; onClick: () => void }) {
  return (
    <button
      onClick={onClick}
      className={`px-5 py-2.5 text-sm font-medium border-b-2 transition-colors ${
        active
          ? 'border-blue-600 text-blue-600'
          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
      }`}
    >
      {label}
    </button>
  )
}

function CompaniesTab() {
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
    </>
  )
}

function ContactsTab() {
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
      {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
      <div className="flex justify-end mb-4">
        <Link
          to="/admin/contacts/new"
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700"
        >
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
                    <button
                      onClick={() => handleDelete(c.id, `${c.firstName} ${c.lastName}`)}
                      className="text-red-500 hover:underline"
                    >
                      Delete
                    </button>
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
    </>
  )
}

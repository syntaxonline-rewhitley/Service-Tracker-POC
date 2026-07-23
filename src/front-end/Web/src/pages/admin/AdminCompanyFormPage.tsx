import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { ErrorMessage } from '../../components/ErrorMessage'
import { Spinner } from '../../components/Spinner'
import {
  createCompany, getCompany, updateCompany,
  getCompanyContacts, getContacts, linkContact, unlinkContact,
} from '../../lib/api'
import type { Contact, CreateCompanyRequest } from '../../types'

export function AdminCompanyFormPage() {
  const { id } = useParams()
  const isEdit = Boolean(id)
  const navigate = useNavigate()

  const [form, setForm] = useState<CreateCompanyRequest>({
    name: '', email: '', phone: '', address: '', website: '',
  })
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  // Contacts state (edit mode only)
  const [linkedContacts, setLinkedContacts] = useState<Contact[]>([])
  const [allContacts, setAllContacts] = useState<Contact[]>([])
  const [selectedContactId, setSelectedContactId] = useState('')
  const [contactsLoading, setContactsLoading] = useState(false)
  const [contactError, setContactError] = useState('')

  useEffect(() => {
    if (!id) return
    getCompany(id)
      .then((c) => setForm({ name: c.name, email: c.email ?? '', phone: c.phone ?? '', address: c.address ?? '', website: c.website ?? '' }))
      .catch(() => setError('Failed to load company.'))
      .finally(() => setLoading(false))
  }, [id])

  useEffect(() => {
    if (!id) return
    setContactsLoading(true)
    Promise.all([getCompanyContacts(id), getContacts()])
      .then(([linked, all]) => {
        setLinkedContacts(linked)
        setAllContacts(all)
      })
      .catch(() => setContactError('Failed to load contacts.'))
      .finally(() => setContactsLoading(false))
  }, [id])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      if (isEdit) await updateCompany(id!, form)
      else await createCompany(form)
      navigate('/admin/customers?tab=companies')
    } catch {
      setError('Failed to save company.')
    } finally {
      setSaving(false)
    }
  }

  const handleLink = async () => {
    if (!id || !selectedContactId) return
    try {
      await linkContact(id, selectedContactId)
      const contact = allContacts.find((c) => c.id === selectedContactId)!
      setLinkedContacts((prev) => [...prev, contact])
      setSelectedContactId('')
    } catch {
      setContactError('Failed to link contact.')
    }
  }

  const handleUnlink = async (contactId: string) => {
    if (!id) return
    try {
      await unlinkContact(id, contactId)
      setLinkedContacts((prev) => prev.filter((c) => c.id !== contactId))
    } catch {
      setContactError('Failed to unlink contact.')
    }
  }

  const unlinkedContacts = allContacts.filter(
    (c) => !linkedContacts.some((lc) => lc.id === c.id)
  )

  if (loading) return <><NavBar /><Spinner /></>

  return (
    <>
      <NavBar />
      <PageShell title={isEdit ? 'Edit Company' : 'New Company'}>
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}

        {/* Company details form */}
        <div className="bg-white rounded-xl shadow p-6 max-w-lg mb-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            {(['name', 'email', 'phone', 'address', 'website'] as const).map((field) => (
              <div key={field}>
                <label className="block text-sm font-medium text-gray-700 mb-1 capitalize">{field}</label>
                <input
                  type={field === 'email' ? 'email' : field === 'website' ? 'url' : 'text'}
                  required={field === 'name'}
                  value={form[field] ?? ''}
                  onChange={(e) => setForm((f) => ({ ...f, [field]: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            ))}
            <div className="flex gap-3 pt-2">
              <button
                type="submit"
                disabled={saving}
                className="bg-blue-600 text-white px-5 py-2 rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? 'Saving…' : 'Save'}
              </button>
              <button
                type="button"
                onClick={() => navigate('/admin/customers?tab=companies')}
                className="px-5 py-2 rounded-lg border text-gray-600 hover:bg-gray-50"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>

        {/* Linked contacts (edit mode only) */}
        {isEdit && (
          <div className="bg-white rounded-xl shadow p-6 max-w-lg">
            <h2 className="text-base font-semibold text-gray-800 mb-4">Linked Contacts</h2>
            {contactError && <div className="mb-3"><ErrorMessage message={contactError} /></div>}

            {contactsLoading ? <Spinner /> : (
              <>
                {/* Existing linked contacts */}
                {linkedContacts.length === 0 ? (
                  <p className="text-sm text-gray-400 mb-4">No contacts linked yet.</p>
                ) : (
                  <ul className="divide-y divide-gray-100 mb-4">
                    {linkedContacts.map((c) => (
                      <li key={c.id} className="flex items-center justify-between py-2.5">
                        <div>
                          <span className="text-sm font-medium">{c.firstName} {c.lastName}</span>
                          <span className="text-xs text-gray-400 ml-2">{c.email}</span>
                        </div>
                        <button
                          onClick={() => handleUnlink(c.id)}
                          className="text-xs text-red-500 hover:underline"
                        >
                          Unlink
                        </button>
                      </li>
                    ))}
                  </ul>
                )}

                {/* Link a new contact */}
                {unlinkedContacts.length > 0 && (
                  <div className="flex gap-2">
                    <select
                      value={selectedContactId}
                      onChange={(e) => setSelectedContactId(e.target.value)}
                      className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">— Select a contact to link —</option>
                      {unlinkedContacts.map((c) => (
                        <option key={c.id} value={c.id}>
                          {c.firstName} {c.lastName} ({c.email})
                        </option>
                      ))}
                    </select>
                    <button
                      onClick={handleLink}
                      disabled={!selectedContactId}
                      className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-40"
                    >
                      Link
                    </button>
                  </div>
                )}

                {unlinkedContacts.length === 0 && linkedContacts.length > 0 && (
                  <p className="text-xs text-gray-400">All contacts are already linked to this company.</p>
                )}
              </>
            )}
          </div>
        )}
      </PageShell>
    </>
  )
}

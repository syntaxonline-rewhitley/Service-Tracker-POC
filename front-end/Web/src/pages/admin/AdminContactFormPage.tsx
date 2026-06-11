import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { ErrorMessage } from '../../components/ErrorMessage'
import { Spinner } from '../../components/Spinner'
import { createContact, getContact, updateContact } from '../../lib/api'
import type { CreateContactRequest } from '../../types'

export function AdminContactFormPage() {
  const { id } = useParams()
  const isEdit = Boolean(id)
  const navigate = useNavigate()

  const [form, setForm] = useState<CreateContactRequest>({
    firstName: '', lastName: '', email: '', phone: '', address: '',
  })
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return
    getContact(id)
      .then((c) => setForm({ firstName: c.firstName, lastName: c.lastName, email: c.email, phone: c.phone ?? '', address: c.address ?? '' }))
      .catch(() => setError('Failed to load contact.'))
      .finally(() => setLoading(false))
  }, [id])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      if (isEdit) await updateContact(id!, form)
      else await createContact(form)
      navigate('/admin/contacts')
    } catch {
      setError('Failed to save contact.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <><NavBar /><Spinner /></>

  return (
    <>
      <NavBar />
      <PageShell title={isEdit ? 'Edit Contact' : 'New Contact'}>
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        <div className="bg-white rounded-xl shadow p-6 max-w-lg">
          <form onSubmit={handleSubmit} className="space-y-4">
            {[
              { field: 'firstName', label: 'First Name', required: true },
              { field: 'lastName', label: 'Last Name', required: true },
              { field: 'email', label: 'Email', required: true, type: 'email' },
              { field: 'phone', label: 'Phone', required: false },
              { field: 'address', label: 'Address', required: false },
            ].map(({ field, label, required, type }) => (
              <div key={field}>
                <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
                <input
                  type={type ?? 'text'}
                  required={required}
                  value={(form as unknown as Record<string, string>)[field] ?? ''}
                  onChange={(e) => setForm((f) => ({ ...f, [field]: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            ))}
            <div className="flex gap-3 pt-2">
              <button type="submit" disabled={saving} className="bg-blue-600 text-white px-5 py-2 rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50">
                {saving ? 'Saving…' : 'Save'}
              </button>
              <button type="button" onClick={() => navigate('/admin/contacts')} className="px-5 py-2 rounded-lg border text-gray-600 hover:bg-gray-50">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </PageShell>
    </>
  )
}

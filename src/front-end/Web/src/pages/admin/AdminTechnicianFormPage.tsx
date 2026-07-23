import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { ErrorMessage } from '../../components/ErrorMessage'
import { Spinner } from '../../components/Spinner'
import { createTechnician, getTechnician, updateTechnician } from '../../lib/api'
import type { UpdateTechnicianRequest } from '../../types'

export function AdminTechnicianFormPage() {
  const { id } = useParams()
  const isEdit = Boolean(id)
  const navigate = useNavigate()

  const [form, setForm] = useState<UpdateTechnicianRequest>({
    firstName: '', lastName: '', email: '', phone: '', specialization: '', isActive: true,
  })
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return
    getTechnician(id)
      .then((t) => setForm({ firstName: t.firstName, lastName: t.lastName, email: t.email, phone: t.phone ?? '', specialization: t.specialization ?? '', isActive: t.isActive }))
      .catch(() => setError('Failed to load technician.'))
      .finally(() => setLoading(false))
  }, [id])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      if (isEdit) await updateTechnician(id!, form)
      else await createTechnician(form)
      navigate('/admin/technicians')
    } catch {
      setError('Failed to save technician.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <><NavBar /><Spinner /></>

  return (
    <>
      <NavBar />
      <PageShell title={isEdit ? 'Edit Technician' : 'New Technician'}>
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        <div className="bg-white rounded-xl shadow p-6 max-w-lg">
          <form onSubmit={handleSubmit} className="space-y-4">
            {[
              { field: 'firstName', label: 'First Name', required: true },
              { field: 'lastName', label: 'Last Name', required: true },
              { field: 'email', label: 'Email', required: true, type: 'email' },
              { field: 'phone', label: 'Phone' },
              { field: 'specialization', label: 'Specialization' },
            ].map(({ field, label, required, type }) => (
              <div key={field}>
                <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
                <input
                  type={type ?? 'text'}
                  required={required}
                  value={(form as unknown as Record<string, string | boolean>)[field] as string ?? ''}
                  onChange={(e) => setForm((f) => ({ ...f, [field]: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            ))}
            {isEdit && (
              <div className="flex items-center gap-2">
                <input
                  id="isActive"
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
                  className="h-4 w-4 text-blue-600"
                />
                <label htmlFor="isActive" className="text-sm text-gray-700">Active</label>
              </div>
            )}
            <div className="flex gap-3 pt-2">
              <button type="submit" disabled={saving} className="bg-blue-600 text-white px-5 py-2 rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50">
                {saving ? 'Saving…' : 'Save'}
              </button>
              <button type="button" onClick={() => navigate('/admin/technicians')} className="px-5 py-2 rounded-lg border text-gray-600 hover:bg-gray-50">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </PageShell>
    </>
  )
}

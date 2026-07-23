import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { ErrorMessage } from '../../components/ErrorMessage'
import { Spinner } from '../../components/Spinner'
import {
  createTicket,
  getCompanies,
  getCompanyContacts,
  getTechnicians,
  getTicket,
  updateTicket,
} from '../../lib/api'
import type { Company, Contact, Technician, UpdateServiceTicketRequest } from '../../types'

const STATUSES = ['Open', 'InProgress', 'OnHold', 'Resolved', 'Closed']
const PRIORITIES = ['Low', 'Medium', 'High', 'Critical']

export function DispatcherTicketFormPage() {
  const { id } = useParams()
  const isEdit = Boolean(id)
  const navigate = useNavigate()

  const [form, setForm] = useState<UpdateServiceTicketRequest>({
    title: '', description: '', companyId: '', contactId: '', technicianId: '',
    status: 'Open', priority: 'Medium', scheduledDate: '', resolvedAt: '', resolutionNotes: '',
  })
  const [companies, setCompanies] = useState<Company[]>([])
  const [contacts, setContacts] = useState<Contact[]>([])
  const [contactsLoading, setContactsLoading] = useState(false)
  const [technicians, setTechnicians] = useState<Technician[]>([])  
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    Promise.all([getCompanies(), getTechnicians()])
      .then(([c, t]) => {
        setCompanies(c)
        setTechnicians(t.filter((t) => t.isActive))
      })
      .catch(() => setError('Failed to load form data.'))

    if (id) {
      getTicket(id)
        .then((t) => {
          setForm({
            title: t.title,
            description: t.description ?? '',
            companyId: t.companyId,
            contactId: t.contactId ?? '',
            technicianId: t.technicianId ?? '',
            status: t.status,
            priority: t.priority,
            scheduledDate: t.scheduledDate ? t.scheduledDate.slice(0, 10) : '',
            resolvedAt: t.resolvedAt ? t.resolvedAt.slice(0, 10) : '',
            resolutionNotes: t.resolutionNotes ?? '',
          })
          // Load contacts for the ticket's company
          return getCompanyContacts(t.companyId).then(setContacts)
        })
        .catch(() => setError('Failed to load ticket.'))
        .finally(() => setLoading(false))
    } else {
      setLoading(false)
    }
  }, [id])

  // When company selection changes, reload contacts and clear any selected contact
  const handleCompanyChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const companyId = e.target.value
    setForm((f) => ({ ...f, companyId, contactId: '' }))
    if (!companyId) {
      setContacts([])
      return
    }
    setContactsLoading(true)
    getCompanyContacts(companyId)
      .then(setContacts)
      .catch(() => setError('Failed to load contacts for selected company.'))
      .finally(() => setContactsLoading(false))
  }

  const set = (field: keyof UpdateServiceTicketRequest) =>
    (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setForm((f) => ({ ...f, [field]: e.target.value }))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      const payload = {
        ...form,
        contactId: form.contactId || undefined,
        technicianId: form.technicianId || undefined,
        scheduledDate: form.scheduledDate || undefined,
        resolvedAt: form.resolvedAt || undefined,
        resolutionNotes: form.resolutionNotes || undefined,
      }
      if (isEdit) await updateTicket(id!, payload)
      else await createTicket(payload)
      navigate('/dispatcher/tickets')
    } catch {
      setError('Failed to save ticket.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <><NavBar /><Spinner /></>

  return (
    <>
      <NavBar />
      <PageShell title={isEdit ? 'Edit Ticket' : 'New Ticket'}>
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        <div className="bg-white rounded-xl shadow p-6 max-w-2xl">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Title *</label>
              <input type="text" required value={form.title} onChange={set('title')}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <textarea value={form.description} onChange={set('description')} rows={3}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Company *</label>
                <select required value={form.companyId} onChange={handleCompanyChange}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="">Select…</option>
                  {companies.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Contact</label>
                <select value={form.contactId} onChange={set('contactId')} disabled={!form.companyId || contactsLoading}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-50 disabled:text-gray-400">
                  <option value="">{!form.companyId ? 'Select a company first' : contactsLoading ? 'Loading…' : 'None'}</option>
                  {contacts.map((c) => <option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Technician</label>
                <select value={form.technicianId} onChange={set('technicianId')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="">Unassigned</option>
                  {technicians.map((t) => <option key={t.id} value={t.id}>{t.firstName} {t.lastName}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Priority</label>
                <select value={form.priority} onChange={set('priority')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  {PRIORITIES.map((p) => <option key={p}>{p}</option>)}
                </select>
              </div>
              {isEdit && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                  <select value={form.status} onChange={set('status')}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                    {STATUSES.map((s) => <option key={s}>{s}</option>)}
                  </select>
                </div>
              )}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Scheduled Date</label>
                <input type="date" value={form.scheduledDate} onChange={set('scheduledDate')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>
            {isEdit && (
              <>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Resolved At</label>
                  <input type="date" value={form.resolvedAt} onChange={set('resolvedAt')}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Resolution Notes</label>
                  <textarea value={form.resolutionNotes} onChange={set('resolutionNotes')} rows={3}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
              </>
            )}
            <div className="flex gap-3 pt-2">
              <button type="submit" disabled={saving} className="bg-blue-600 text-white px-5 py-2 rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50">
                {saving ? 'Saving…' : 'Save'}
              </button>
              <button type="button" onClick={() => navigate('/dispatcher/tickets')} className="px-5 py-2 rounded-lg border text-gray-600 hover:bg-gray-50">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </PageShell>
    </>
  )
}

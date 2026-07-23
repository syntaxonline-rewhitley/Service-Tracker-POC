import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { ErrorMessage } from '../../components/ErrorMessage'
import { Spinner } from '../../components/Spinner'
import { getTicket, updateTicket } from '../../lib/api'
import type { ServiceTicket } from '../../types'

const STATUSES = ['Open', 'InProgress', 'OnHold', 'Resolved', 'Closed']

export function TechnicianTicketUpdatePage() {
  const { id } = useParams()
  const navigate = useNavigate()

  const [ticket, setTicket] = useState<ServiceTicket | null>(null)
  const [status, setStatus] = useState('')
  const [resolutionNotes, setResolutionNotes] = useState('')
  const [resolvedAt, setResolvedAt] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return
    getTicket(id)
      .then((t) => {
        setTicket(t)
        setStatus(t.status)
        setResolutionNotes(t.resolutionNotes ?? '')
        setResolvedAt(t.resolvedAt ? t.resolvedAt.slice(0, 10) : '')
      })
      .catch(() => setError('Failed to load ticket.'))
      .finally(() => setLoading(false))
  }, [id])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!ticket) return
    setSaving(true)
    setError('')
    try {
      await updateTicket(id!, {
        title: ticket.title,
        description: ticket.description,
        companyId: ticket.companyId,
        contactId: ticket.contactId,
        technicianId: ticket.technicianId,
        priority: ticket.priority,
        status,
        scheduledDate: ticket.scheduledDate,
        resolvedAt: resolvedAt || undefined,
        resolutionNotes: resolutionNotes || undefined,
      })
      navigate('/technician/tickets')
    } catch {
      setError('Failed to update ticket.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <><NavBar /><Spinner /></>
  if (!ticket) return <><NavBar /><PageShell><ErrorMessage message="Ticket not found." /></PageShell></>

  return (
    <>
      <NavBar />
      <PageShell title={`Ticket: ${ticket.ticketNumber}`}>
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Read-only details */}
          <div className="bg-white rounded-xl shadow p-6 space-y-3 text-sm">
            <h2 className="font-semibold text-gray-800 text-base mb-2">{ticket.title}</h2>
            {ticket.description && <p className="text-gray-600">{ticket.description}</p>}
            <div className="grid grid-cols-2 gap-2 text-gray-500 pt-2">
              <span className="font-medium text-gray-700">Company</span><span>{ticket.companyName}</span>
              <span className="font-medium text-gray-700">Contact</span><span>{ticket.contactName ?? '—'}</span>
              <span className="font-medium text-gray-700">Priority</span><span>{ticket.priority}</span>
              <span className="font-medium text-gray-700">Scheduled</span>
              <span>{ticket.scheduledDate ? new Date(ticket.scheduledDate).toLocaleDateString() : '—'}</span>
            </div>
          </div>

          {/* Update form */}
          <div className="bg-white rounded-xl shadow p-6">
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                <select value={status} onChange={(e) => setStatus(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  {STATUSES.map((s) => <option key={s}>{s}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Resolved At</label>
                <input type="date" value={resolvedAt} onChange={(e) => setResolvedAt(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Resolution Notes</label>
                <textarea value={resolutionNotes} onChange={(e) => setResolutionNotes(e.target.value)} rows={4}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div className="flex gap-3 pt-2">
                <button type="submit" disabled={saving} className="bg-blue-600 text-white px-5 py-2 rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50">
                  {saving ? 'Saving…' : 'Save'}
                </button>
                <button type="button" onClick={() => navigate('/technician/tickets')} className="px-5 py-2 rounded-lg border text-gray-600 hover:bg-gray-50">
                  Back
                </button>
              </div>
            </form>
          </div>
        </div>
      </PageShell>
    </>
  )
}

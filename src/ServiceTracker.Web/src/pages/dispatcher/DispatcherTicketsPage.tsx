import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { Spinner } from '../../components/Spinner'
import { ErrorMessage } from '../../components/ErrorMessage'
import { getTickets, deleteTicket } from '../../lib/api'
import type { ServiceTicket } from '../../types'

const STATUS_COLORS: Record<string, string> = {
  Open: 'bg-blue-100 text-blue-700',
  InProgress: 'bg-yellow-100 text-yellow-700',
  OnHold: 'bg-orange-100 text-orange-700',
  Resolved: 'bg-green-100 text-green-700',
  Closed: 'bg-gray-100 text-gray-500',
}

const PRIORITY_COLORS: Record<string, string> = {
  Low: 'bg-gray-100 text-gray-500',
  Medium: 'bg-blue-100 text-blue-600',
  High: 'bg-orange-100 text-orange-600',
  Critical: 'bg-red-100 text-red-600',
}

export function DispatcherTicketsPage() {
  const [tickets, setTickets] = useState<ServiceTicket[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState('')

  useEffect(() => {
    getTickets()
      .then(setTickets)
      .catch(() => setError('Failed to load tickets.'))
      .finally(() => setLoading(false))
  }, [])

  const handleDelete = async (id: string, title: string) => {
    if (!confirm(`Delete ticket "${title}"?`)) return
    try {
      await deleteTicket(id)
      setTickets((prev) => prev.filter((t) => t.id !== id))
    } catch {
      alert('Failed to delete ticket.')
    }
  }

  const filtered = filter
    ? tickets.filter((t) => t.status === filter)
    : tickets

  return (
    <>
      <NavBar />
      <PageShell title="Service Tickets">
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        <div className="flex items-center justify-between mb-4 gap-4">
          <select
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            className="border border-gray-300 rounded-lg px-3 py-2 text-sm"
          >
            <option value="">All statuses</option>
            {['Open', 'InProgress', 'OnHold', 'Resolved', 'Closed'].map((s) => (
              <option key={s}>{s}</option>
            ))}
          </select>
          <Link to="/dispatcher/tickets/new" className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700">
            + New Ticket
          </Link>
        </div>
        {loading ? <Spinner /> : (
          <div className="bg-white rounded-xl shadow overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 uppercase text-xs">
                <tr>
                  <th className="px-4 py-3 text-left">#</th>
                  <th className="px-4 py-3 text-left">Title</th>
                  <th className="px-4 py-3 text-left">Company</th>
                  <th className="px-4 py-3 text-left">Technician</th>
                  <th className="px-4 py-3 text-left">Status</th>
                  <th className="px-4 py-3 text-left">Priority</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {filtered.map((t) => (
                  <tr key={t.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-gray-400 font-mono text-xs">{t.ticketNumber}</td>
                    <td className="px-4 py-3 font-medium">{t.title}</td>
                    <td className="px-4 py-3 text-gray-600">{t.companyName}</td>
                    <td className="px-4 py-3 text-gray-600">{t.technicianName ?? '—'}</td>
                    <td className="px-4 py-3">
                      <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${STATUS_COLORS[t.status] ?? ''}`}>{t.status}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${PRIORITY_COLORS[t.priority] ?? ''}`}>{t.priority}</span>
                    </td>
                    <td className="px-4 py-3 text-right space-x-2">
                      <Link to={`/dispatcher/tickets/${t.id}`} className="text-blue-600 hover:underline">Edit</Link>
                      <button onClick={() => handleDelete(t.id, t.title)} className="text-red-500 hover:underline">Delete</button>
                    </td>
                  </tr>
                ))}
                {filtered.length === 0 && (
                  <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-400">No tickets found.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </PageShell>
    </>
  )
}

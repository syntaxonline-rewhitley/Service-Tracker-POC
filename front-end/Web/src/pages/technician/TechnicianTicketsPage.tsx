import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { NavBar } from '../../components/NavBar'
import { PageShell } from '../../components/PageShell'
import { Spinner } from '../../components/Spinner'
import { ErrorMessage } from '../../components/ErrorMessage'
import { getMyTickets } from '../../lib/api'
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

export function TechnicianTicketsPage() {
  const [tickets, setTickets] = useState<ServiceTicket[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    getMyTickets()
      .then(setTickets)
      .catch(() => setError('Failed to load your tickets.'))
      .finally(() => setLoading(false))
  }, [])

  return (
    <>
      <NavBar />
      <PageShell title="My Tickets">
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        {loading ? <Spinner /> : (
          <div className="bg-white rounded-xl shadow overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 uppercase text-xs">
                <tr>
                  <th className="px-4 py-3 text-left">#</th>
                  <th className="px-4 py-3 text-left">Title</th>
                  <th className="px-4 py-3 text-left">Company</th>
                  <th className="px-4 py-3 text-left">Status</th>
                  <th className="px-4 py-3 text-left">Priority</th>
                  <th className="px-4 py-3 text-left">Scheduled</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {tickets.map((t) => (
                  <tr key={t.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-gray-400 font-mono text-xs">{t.ticketNumber}</td>
                    <td className="px-4 py-3 font-medium">{t.title}</td>
                    <td className="px-4 py-3 text-gray-600">{t.companyName}</td>
                    <td className="px-4 py-3">
                      <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${STATUS_COLORS[t.status] ?? ''}`}>{t.status}</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${PRIORITY_COLORS[t.priority] ?? ''}`}>{t.priority}</span>
                    </td>
                    <td className="px-4 py-3 text-gray-600">
                      {t.scheduledDate ? new Date(t.scheduledDate).toLocaleDateString() : '—'}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <Link to={`/technician/tickets/${t.id}`} className="text-blue-600 hover:underline">Update</Link>
                    </td>
                  </tr>
                ))}
                {tickets.length === 0 && (
                  <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-400">No tickets assigned to you.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </PageShell>
    </>
  )
}

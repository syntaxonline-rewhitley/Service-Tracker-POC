import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import {
  AreaChart, Area, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
} from 'recharts'
import { NavBar } from '../components/NavBar'
import { PageShell } from '../components/PageShell'
import { Spinner } from '../components/Spinner'
import { ErrorMessage } from '../components/ErrorMessage'
import { getDashboard, getTechnicianDashboard } from '../lib/api'
import { useAuth } from '../context/AuthContext'
import type { DashboardStats, RecentTicketItem, TechnicianDashboardStats, TicketsByTechnicianItem } from '../types'

// ── Colour palettes ───────────────────────────────────────────────────────────
const STATUS_FILL: Record<string, string> = {
  Open: '#3b82f6', InProgress: '#eab308', OnHold: '#f97316', Resolved: '#22c55e', Closed: '#9ca3af',
}
const PRIORITY_FILL: Record<string, string> = {
  Low: '#9ca3af', Medium: '#3b82f6', High: '#f97316', Critical: '#ef4444',
}
const statusBadge: Record<string, string> = {
  Open: 'bg-blue-100 text-blue-700', InProgress: 'bg-yellow-100 text-yellow-700',
  OnHold: 'bg-orange-100 text-orange-700', Resolved: 'bg-green-100 text-green-700', Closed: 'bg-gray-100 text-gray-600',
}
const priorityBadge: Record<string, string> = {
  Low: 'bg-gray-100 text-gray-600', Medium: 'bg-blue-100 text-blue-700',
  High: 'bg-orange-100 text-orange-700', Critical: 'bg-red-100 text-red-700',
}

// ── Shared primitives ─────────────────────────────────────────────────────────
function StatCard({ label, value, accent, sub }: { label: string; value: number; accent: string; sub?: string }) {
  return (
    <div className={`bg-white rounded-xl shadow p-5 border-l-4 ${accent}`}>
      <p className="text-xs text-gray-500 uppercase font-medium">{label}</p>
      <p className="text-3xl font-bold text-gray-800 mt-1">{value.toLocaleString()}</p>
      {sub && <p className="text-xs text-gray-400 mt-1">{sub}</p>}
    </div>
  )
}

function Badge({ text, colorClass }: { text: string; colorClass: string }) {
  return <span className={`inline-block px-2 py-0.5 rounded text-xs font-medium ${colorClass}`}>{text}</span>
}

function ChartCard({ title, children, className }: { title: string; children: ReactNode; className?: string }) {
  return (
    <div className={`bg-white rounded-xl shadow p-5 ${className ?? ''}`}>
      <h2 className="text-xs font-semibold text-gray-500 uppercase mb-4">{title}</h2>
      {children}
    </div>
  )
}

// ── Shared chart components ───────────────────────────────────────────────────
function StatusDonut({ data }: { data: { status: string; count: number }[] }) {
  if (data.length === 0) return <p className="text-sm text-gray-400 text-center py-10">No data.</p>
  return (
    <ResponsiveContainer width="100%" height={230}>
      <PieChart>
        <Pie data={data} cx="50%" cy="50%" innerRadius={62} outerRadius={92}
          paddingAngle={2} dataKey="count" nameKey="status">
          {data.map((e) => <Cell key={e.status} fill={STATUS_FILL[e.status] ?? '#9ca3af'} />)}
        </Pie>
        <Tooltip formatter={(v, n) => [v, n]} />
        <Legend iconType="circle" iconSize={10} />
      </PieChart>
    </ResponsiveContainer>
  )
}

function PriorityBar({ data }: { data: { priority: string; count: number }[] }) {
  if (data.length === 0) return <p className="text-sm text-gray-400 text-center py-10">No data.</p>
  return (
    <ResponsiveContainer width="100%" height={230}>
      <BarChart layout="vertical" data={data} margin={{ left: 8, right: 20 }}>
        <CartesianGrid strokeDasharray="3 3" horizontal={false} />
        <XAxis type="number" tick={{ fontSize: 12 }} allowDecimals={false} />
        <YAxis dataKey="priority" type="category" tick={{ fontSize: 12 }} width={72} />
        <Tooltip />
        <Bar dataKey="count" name="Tickets" radius={[0, 4, 4, 0]}>
          {data.map((e) => <Cell key={e.priority} fill={PRIORITY_FILL[e.priority] ?? '#9ca3af'} />)}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  )
}

function TrendArea({ data }: { data: { date: string; count: number }[] }) {
  if (data.length === 0) return <p className="text-sm text-gray-400 text-center py-10">No tickets in the last 30 days.</p>
  return (
    <ResponsiveContainer width="100%" height={230}>
      <AreaChart data={data} margin={{ left: 0, right: 8 }}>
        <defs>
          <linearGradient id="trendGrad" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%"  stopColor="#6366f1" stopOpacity={0.25} />
            <stop offset="95%" stopColor="#6366f1" stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="date" tick={{ fontSize: 11 }} interval="preserveStartEnd" />
        <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
        <Tooltip />
        <Area type="monotone" dataKey="count" name="Tickets" stroke="#6366f1"
          fill="url(#trendGrad)" strokeWidth={2} dot={false} />
      </AreaChart>
    </ResponsiveContainer>
  )
}

function RecentTicketsTable({ rows }: { rows: RecentTicketItem[] }) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead className="bg-gray-50 text-gray-500 text-xs uppercase">
          <tr>
            <th className="px-5 py-2 text-left">#</th>
            <th className="px-3 py-2 text-left">Title</th>
            <th className="px-3 py-2 text-left">Company</th>
            <th className="px-3 py-2 text-left">Status</th>
            <th className="px-5 py-2 text-left">Priority</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {rows.map((t) => (
            <tr key={t.id} className="hover:bg-gray-50">
              <td className="px-5 py-2 font-mono text-gray-500 text-xs">{t.ticketNumber}</td>
              <td className="px-3 py-2 font-medium max-w-xs truncate">{t.title}</td>
              <td className="px-3 py-2 text-gray-600">{t.companyName}</td>
              <td className="px-3 py-2"><Badge text={t.status} colorClass={statusBadge[t.status] ?? 'bg-gray-100 text-gray-600'} /></td>
              <td className="px-5 py-2"><Badge text={t.priority} colorClass={priorityBadge[t.priority] ?? 'bg-gray-100 text-gray-600'} /></td>
            </tr>
          ))}
          {rows.length === 0 && (
            <tr><td colSpan={5} className="px-5 py-4 text-center text-gray-400 text-sm">No tickets yet.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  )
}

// ── Tab bar ──────────────────────────────────────────────────────────────────
const TABS = ['Overview', 'Charts', 'Recent'] as const
type Tab = typeof TABS[number]

function TabBar({ active, tabs, onChange }: { active: Tab; tabs: readonly Tab[]; onChange: (t: Tab) => void }) {
  return (
    <div className="flex gap-1 bg-gray-100 rounded-lg p-1 w-fit mb-6">
      {tabs.map((t) => (
        <button
          key={t}
          onClick={() => onChange(t)}
          className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors ${
            active === t
              ? 'bg-white shadow text-gray-800'
              : 'text-gray-500 hover:text-gray-700'
          }`}
        >
          {t}
        </button>
      ))}
    </div>
  )
}

// ── Admin / Dispatcher Dashboard ──────────────────────────────────────────────
function DashboardView({ stats }: { stats: DashboardStats }) {
  const [tab, setTab] = useState<Tab>('Overview')

  return (
    <div>
      <TabBar active={tab} tabs={TABS} onChange={setTab} />

      {tab === 'Overview' && (
        <div className="space-y-6">
          <section>
            <h2 className="text-xs font-semibold text-gray-500 uppercase mb-3">Ticket Volume</h2>
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
              <StatCard label="All Time"     value={stats.totalTickets}      accent="border-indigo-500" sub="total tickets" />
              <StatCard label="This Month"   value={stats.monthlyTickets}    accent="border-purple-500" sub="calendar month" />
              <StatCard label="Last 30 Days" value={stats.last30DaysTickets} accent="border-blue-500"   sub="rolling 30 days" />
              <StatCard label="Today"        value={stats.dailyTickets}      accent="border-teal-500"   sub="created today" />
            </div>
          </section>
          <section>
            <h2 className="text-xs font-semibold text-gray-500 uppercase mb-3">Current Status</h2>
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
              <StatCard label="Open"        value={stats.openTickets}       accent="border-blue-500" />
              <StatCard label="In Progress" value={stats.inProgressTickets} accent="border-yellow-500" />
              <StatCard label="On Hold"     value={stats.onHoldTickets}     accent="border-orange-400" />
              <StatCard label="Resolved"    value={stats.resolvedTickets}   accent="border-green-500" />
              <StatCard label="Closed"      value={stats.closedTickets}     accent="border-gray-400" />
            </div>
          </section>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <StatCard label="Active Technicians" value={stats.activeTechnicians} accent="border-teal-400" />
            <StatCard label="Companies"          value={stats.totalCompanies}    accent="border-purple-400" />
            <StatCard label="Contacts"           value={stats.totalContacts}     accent="border-pink-400" />
          </div>
        </div>
      )}

      {tab === 'Charts' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <ChartCard title="Status Distribution">
              <StatusDonut data={stats.ticketsByStatus} />
            </ChartCard>
            <ChartCard title="Priority Breakdown">
              <PriorityBar data={stats.ticketsByPriority} />
            </ChartCard>
          </div>
          <ChartCard title="30-Day Ticket Creation Trend">
            <TrendArea data={stats.dailyTrend} />
          </ChartCard>
          <ChartCard title="Top Technicians — Open &amp; In Progress">
            {stats.ticketsByTechnician.length === 0 ? (
              <p className="text-sm text-gray-400 text-center py-10">No data.</p>
            ) : (
              <ResponsiveContainer width="100%" height={230}>
                <BarChart layout="vertical"
                  data={stats.ticketsByTechnician as TicketsByTechnicianItem[]}
                  margin={{ left: 8, right: 20 }}>
                  <CartesianGrid strokeDasharray="3 3" horizontal={false} />
                  <XAxis type="number" tick={{ fontSize: 12 }} allowDecimals={false} />
                  <YAxis dataKey="technicianName" type="category" tick={{ fontSize: 12 }} width={120} />
                  <Tooltip />
                  <Legend iconSize={10} />
                  <Bar dataKey="open"       name="Open"        fill="#3b82f6" stackId="a" />
                  <Bar dataKey="inProgress" name="In Progress" fill="#eab308" stackId="a" radius={[0, 4, 4, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </div>
      )}

      {tab === 'Recent' && (
        <ChartCard title="Recent Tickets">
          <RecentTicketsTable rows={stats.recentTickets as RecentTicketItem[]} />
        </ChartCard>
      )}
    </div>
  )
}

// ── Technician Dashboard ──────────────────────────────────────────────────────
function TechnicianDashboardView({ stats }: { stats: TechnicianDashboardStats }) {
  const [tab, setTab] = useState<Tab>('Overview')

  return (
    <div>
      <p className="text-gray-500 text-sm mb-4">
        Performance overview for <span className="font-medium text-gray-700">{stats.technicianName}</span>.
      </p>
      <TabBar active={tab} tabs={TABS} onChange={setTab} />

      {tab === 'Overview' && (
        <div className="space-y-6">
          <section>
            <h2 className="text-xs font-semibold text-gray-500 uppercase mb-3">My Ticket Volume</h2>
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
              <StatCard label="All Time"     value={stats.totalTickets}      accent="border-indigo-500" sub="total assigned" />
              <StatCard label="This Month"   value={stats.monthlyTickets}    accent="border-purple-500" sub="calendar month" />
              <StatCard label="Last 30 Days" value={stats.last30DaysTickets} accent="border-blue-500"   sub="rolling 30 days" />
              <StatCard label="Today"        value={stats.dailyTickets}      accent="border-teal-500"   sub="created today" />
            </div>
          </section>
          <section>
            <h2 className="text-xs font-semibold text-gray-500 uppercase mb-3">My Tickets by Status</h2>
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
              <StatCard label="Open"        value={stats.openTickets}       accent="border-blue-500" />
              <StatCard label="In Progress" value={stats.inProgressTickets} accent="border-yellow-500" />
              <StatCard label="On Hold"     value={stats.onHoldTickets}     accent="border-orange-400" />
              <StatCard label="Resolved"    value={stats.resolvedTickets}   accent="border-green-500" />
              <StatCard label="Closed"      value={stats.closedTickets}     accent="border-gray-400" />
            </div>
          </section>
        </div>
      )}

      {tab === 'Charts' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <ChartCard title="My Tickets by Status">
              <StatusDonut data={stats.ticketsByStatus} />
            </ChartCard>
            <ChartCard title="My Tickets by Priority">
              <PriorityBar data={stats.ticketsByPriority} />
            </ChartCard>
          </div>
          <ChartCard title="My 30-Day Ticket Trend">
            <TrendArea data={stats.dailyTrend} />
          </ChartCard>
        </div>
      )}

      {tab === 'Recent' && (
        <ChartCard title="My Recent Tickets">
          <RecentTicketsTable rows={stats.recentTickets as RecentTicketItem[]} />
        </ChartCard>
      )}
    </div>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────
export function HomePage() {
  const { email, roles } = useAuth()
  const canSeeDashboard = roles.includes('Admin') || roles.includes('Dispatcher')
  const isTechnician = roles.includes('Technician')

  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [techStats, setTechStats] = useState<TechnicianDashboardStats | null>(null)
  const [loading, setLoading] = useState(canSeeDashboard || isTechnician)
  const [error, setError] = useState('')

  useEffect(() => {
    if (canSeeDashboard) {
      getDashboard()
        .then(setStats)
        .catch(() => setError('Failed to load dashboard data.'))
        .finally(() => setLoading(false))
    } else if (isTechnician) {
      getTechnicianDashboard()
        .then(setTechStats)
        .catch(() => setError('Failed to load your dashboard.'))
        .finally(() => setLoading(false))
    }
  }, [canSeeDashboard, isTechnician])

  return (
    <>
      <NavBar />
      <PageShell title="Dashboard">
        {error && <div className="mb-4"><ErrorMessage message={error} /></div>}
        {loading ? (
          <Spinner />
        ) : canSeeDashboard && stats ? (
          <DashboardView stats={stats} />
        ) : isTechnician && techStats ? (
          <TechnicianDashboardView stats={techStats} />
        ) : (
          <div className="bg-white rounded-xl shadow p-8 text-center">
            <h2 className="text-xl font-semibold text-gray-800 mb-2">Welcome back, {email}</h2>
            <p className="text-gray-500">
              You are signed in as <span className="font-medium text-blue-600">{roles.join(', ')}</span>.
              Use the navigation bar to get started.
            </p>
          </div>
        )}
      </PageShell>
    </>
  )
}



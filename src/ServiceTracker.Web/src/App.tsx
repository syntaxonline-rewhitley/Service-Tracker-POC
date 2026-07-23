import { Navigate, Route, BrowserRouter as Router, Routes } from 'react-router-dom'
import { RequireAuth } from './components/RequireAuth'
import { AuthProvider } from './context/AuthContext'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { UnauthorizedPage } from './pages/UnauthorizedPage'
import { AdminCompaniesPage } from './pages/admin/AdminCompaniesPage'
import { AdminCompanyFormPage } from './pages/admin/AdminCompanyFormPage'
import { AdminContactFormPage } from './pages/admin/AdminContactFormPage'
import { AdminContactsPage } from './pages/admin/AdminContactsPage'
import { AdminCustomersPage } from './pages/admin/AdminCustomersPage'
import { AdminNewUserPage } from './pages/admin/AdminNewUserPage'
import { AdminTechnicianFormPage } from './pages/admin/AdminTechnicianFormPage'
import { AdminTechniciansPage } from './pages/admin/AdminTechniciansPage'
import { AdminUsersPage } from './pages/admin/AdminUsersPage'
import { DispatcherTicketFormPage } from './pages/dispatcher/DispatcherTicketFormPage'
import { DispatcherTicketsPage } from './pages/dispatcher/DispatcherTicketsPage'
import { TechnicianTicketUpdatePage } from './pages/technician/TechnicianTicketUpdatePage'
import { TechnicianTicketsPage } from './pages/technician/TechnicianTicketsPage'

export default function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/unauthorized" element={<UnauthorizedPage />} />

          {/* Any authenticated user */}
          <Route element={<RequireAuth />}>
            <Route path="/" element={<HomePage />} />
          </Route>

          {/* Admin only */}
          <Route element={<RequireAuth roles={['Admin']} />}>
            <Route path="/admin/customers" element={<AdminCustomersPage />} />
            <Route path="/admin/companies" element={<Navigate to="/admin/customers?tab=companies" replace />} />
            <Route path="/admin/companies/new" element={<AdminCompanyFormPage />} />
            <Route path="/admin/companies/:id" element={<AdminCompanyFormPage />} />
            <Route path="/admin/contacts" element={<Navigate to="/admin/customers?tab=contacts" replace />} />
            <Route path="/admin/contacts/new" element={<AdminContactFormPage />} />
            <Route path="/admin/contacts/:id" element={<AdminContactFormPage />} />
            <Route path="/admin/technicians" element={<AdminTechniciansPage />} />
            <Route path="/admin/technicians/new" element={<AdminTechnicianFormPage />} />
            <Route path="/admin/technicians/:id" element={<AdminTechnicianFormPage />} />
            <Route path="/admin/users" element={<AdminUsersPage />} />
            <Route path="/admin/users/new" element={<AdminNewUserPage />} />
          </Route>

          {/* Dispatcher only */}
          <Route element={<RequireAuth roles={['Dispatcher', 'Admin']} />}>
            <Route path="/dispatcher/tickets" element={<DispatcherTicketsPage />} />
            <Route path="/dispatcher/tickets/new" element={<DispatcherTicketFormPage />} />
            <Route path="/dispatcher/tickets/:id" element={<DispatcherTicketFormPage />} />
          </Route>

          {/* Technician only */}
          <Route element={<RequireAuth roles={['Technician']} />}>
            <Route path="/technician/tickets" element={<TechnicianTicketsPage />} />
            <Route path="/technician/tickets/:id" element={<TechnicianTicketUpdatePage />} />
          </Route>

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Router>
    </AuthProvider>
  )
}

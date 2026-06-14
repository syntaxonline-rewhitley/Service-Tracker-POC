import axios from 'axios'
import type {
  Company,
  Contact,
  CreateCompanyRequest,
  CreateContactRequest,
  CreateServiceTicketRequest,
  CreateTechnicianRequest,
  DashboardStats,
  TechnicianDashboardStats,
  RegisterUserRequest,
  ServiceTicket,
  Technician,
  TokenResponse,
  UpdateServiceTicketRequest,
  UpdateTechnicianRequest,
  UserListItem,
} from '../types'

// Container deployment: VITE_API_URL is NOT set at build time, so baseURL
// falls back to '/api' — nginx proxies /api/ to the backend via the API_URL
// runtime env var (see nginx.conf + docker-compose.frontend.yml).
// Vercel deployment: set VITE_API_URL in Vercel project settings so it is
// baked in at build time (e.g. https://api.example.com/api).
const api = axios.create({ baseURL: import.meta.env.VITE_API_URL ?? '/api' })

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// ── Auth ─────────────────────────────────────────────────────────────────────
export const login = (email: string, password: string) =>
  api.post<TokenResponse>('/auth/login', { email, password }).then((r) => r.data)

export const registerUser = (data: RegisterUserRequest) =>
  api.post<TokenResponse>('/auth/register', data).then((r) => r.data)

export const getUsers = () =>
  api.get<UserListItem[]>('/auth/users').then((r) => r.data)

export const deleteUser = (id: string) =>
  api.delete(`/auth/users/${id}`)

// ── Dashboard ────────────────────────────────────────────────────────────────
export const getDashboard = () =>
  api.get<DashboardStats>('/dashboard').then((r) => r.data)

export const getTechnicianDashboard = () =>
  api.get<TechnicianDashboardStats>('/dashboard/me').then((r) => r.data)

// ── Companies ────────────────────────────────────────────────────────────────
export const getCompanies = () =>
    api.get<Company[]>('/companies').then((r) => r.data);

export const getCompany = (id: string) =>
  api.get<Company>(`/companies/${id}`).then((r) => r.data)

export const createCompany = (data: CreateCompanyRequest) =>
  api.post<Company>('/companies', data).then((r) => r.data)

export const updateCompany = (id: string, data: CreateCompanyRequest) =>
  api.put<Company>(`/companies/${id}`, data).then((r) => r.data)

export const deleteCompany = (id: string) =>
  api.delete(`/companies/${id}`)

// ── Contacts ─────────────────────────────────────────────────────────────────
export const getContacts = () =>
  api.get<Contact[]>('/contacts').then((r) => r.data)

export const getContact = (id: string) =>
  api.get<Contact>(`/contacts/${id}`).then((r) => r.data)

export const createContact = (data: CreateContactRequest) =>
  api.post<Contact>('/contacts', data).then((r) => r.data)

export const updateContact = (id: string, data: CreateContactRequest) =>
  api.put<Contact>(`/contacts/${id}`, data).then((r) => r.data)

export const deleteContact = (id: string) =>
  api.delete(`/contacts/${id}`)

export const getCompanyContacts = (companyId: string) =>
  api.get<Contact[]>(`/companies/${companyId}/contacts`).then((r) => r.data)

export const linkContact = (companyId: string, contactId: string) =>
  api.post(`/companies/${companyId}/contacts/${contactId}`)

export const unlinkContact = (companyId: string, contactId: string) =>
  api.delete(`/companies/${companyId}/contacts/${contactId}`)

// ── Technicians ───────────────────────────────────────────────────────────────
export const getTechnicians = () =>
  api.get<Technician[]>('/technicians').then((r) => r.data)

export const getTechnician = (id: string) =>
  api.get<Technician>(`/technicians/${id}`).then((r) => r.data)

export const createTechnician = (data: CreateTechnicianRequest) =>
  api.post<Technician>('/technicians', data).then((r) => r.data)

export const updateTechnician = (id: string, data: UpdateTechnicianRequest) =>
  api.put<Technician>(`/technicians/${id}`, data).then((r) => r.data)

export const deleteTechnician = (id: string) =>
  api.delete(`/technicians/${id}`)

// ── Service Tickets ──────────────────────────────────────────────────────────
export const getTickets = () =>
  api.get<ServiceTicket[]>('/servicetickets').then((r) => r.data)

export const getMyTickets = () =>
  api.get<ServiceTicket[]>('/servicetickets/my').then((r) => r.data)

export const getTicket = (id: string) =>
  api.get<ServiceTicket>(`/servicetickets/${id}`).then((r) => r.data)

export const createTicket = (data: CreateServiceTicketRequest) =>
  api.post<ServiceTicket>('/servicetickets', data).then((r) => r.data)

export const updateTicket = (id: string, data: UpdateServiceTicketRequest) =>
  api.put<ServiceTicket>(`/servicetickets/${id}`, data).then((r) => r.data)

export const deleteTicket = (id: string) =>
  api.delete(`/servicetickets/${id}`)

export default api

export interface TokenResponse {
  accessToken: string
  expiresAt: string
}

export interface UserListItem {
  id: string
  email: string
  roles: string[]
}

export interface DecodedToken {
  sub: string
  email: string
  role?: string | string[]
  exp: number
}

export interface Company {
  id: string
  name: string
  email?: string
  phone?: string
  address?: string
  website?: string
  createdAt: string
  updatedAt: string
}

export interface Contact {
  id: string
  firstName: string
  lastName: string
  email: string
  phone?: string
  address?: string
  createdAt: string
  updatedAt: string
}

export interface Technician {
  id: string
  firstName: string
  lastName: string
  email: string
  phone?: string
  specialization?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface ServiceTicket {
  id: string
  ticketNumber: string
  title: string
  description?: string
  status: string
  priority: string
  companyId: string
  companyName: string
  contactId?: string
  contactName?: string
  technicianId?: string
  technicianName?: string
  scheduledDate?: string
  resolvedAt?: string
  resolutionNotes?: string
  createdAt: string
  updatedAt: string
}

export interface CreateCompanyRequest {
  name: string
  email?: string
  phone?: string
  address?: string
  website?: string
}

export interface CreateContactRequest {
  firstName: string
  lastName: string
  email: string
  phone?: string
  address?: string
}

export interface CreateTechnicianRequest {
  firstName: string
  lastName: string
  email: string
  phone?: string
  specialization?: string
}

export interface UpdateTechnicianRequest extends CreateTechnicianRequest {
  isActive: boolean
}

export interface CreateServiceTicketRequest {
  title: string
  description?: string
  companyId: string
  contactId?: string
  technicianId?: string
  priority: string
  scheduledDate?: string
}

export interface UpdateServiceTicketRequest extends CreateServiceTicketRequest {
  status: string
  resolvedAt?: string
  resolutionNotes?: string
}

export interface RegisterUserRequest {
  email: string
  password: string
  confirmPassword: string
  role: string
}

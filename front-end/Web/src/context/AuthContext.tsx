import { jwtDecode } from 'jwt-decode'
import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { login as apiLogin } from '../lib/api'
import type { DecodedToken } from '../types'

interface AuthState {
  token: string | null
  email: string | null
  roles: string[]
}

interface AuthContextValue extends AuthState {
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  hasRole: (...roles: string[]) => boolean
}

const AuthContext = createContext<AuthContextValue | null>(null)

function decodeToken(token: string): DecodedToken | null {
  try {
    return jwtDecode<DecodedToken>(token)
  } catch {
    return null
  }
}

function extractRoles(decoded: DecodedToken): string[] {
  if (!decoded.role) return []
  return Array.isArray(decoded.role) ? decoded.role : [decoded.role]
}

function stateFromToken(token: string | null): AuthState {
  if (!token) return { token: null, email: null, roles: [] }
  const decoded = decodeToken(token)
  if (!decoded || decoded.exp * 1000 < Date.now()) {
    localStorage.removeItem('token')
    return { token: null, email: null, roles: [] }
  }
  return { token, email: decoded.email, roles: extractRoles(decoded) }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>(() =>
    stateFromToken(localStorage.getItem('token')),
  )

  useEffect(() => {
    if (!state.token) return
    const decoded = decodeToken(state.token)
    if (!decoded) return
    const ms = decoded.exp * 1000 - Date.now()
    if (ms <= 0) { setState({ token: null, email: null, roles: [] }); return }
    const timer = setTimeout(() => {
      localStorage.removeItem('token')
      setState({ token: null, email: null, roles: [] })
    }, ms)
    return () => clearTimeout(timer)
  }, [state.token])

  const login = useCallback(async (email: string, password: string) => {
    const { accessToken } = await apiLogin(email, password)
    localStorage.setItem('token', accessToken)
    setState(stateFromToken(accessToken))
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('token')
    setState({ token: null, email: null, roles: [] })
  }, [])

  const hasRole = useCallback(
    (...roles: string[]) => roles.some((r) => state.roles.includes(r)),
    [state.roles],
  )

  const value = useMemo(
    () => ({ ...state, login, logout, hasRole }),
    [state, login, logout, hasRole],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}

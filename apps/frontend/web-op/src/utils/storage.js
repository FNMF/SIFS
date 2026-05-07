const ACCESS_TOKEN_KEY = 'sifs_access_token'
const REFRESH_TOKEN_KEY = 'sifs_refresh_token'
const USER_INFO_KEY = 'sifs_user_info'
const PERMISSIONS_KEY = 'sifs_admin_permissions'
const ROLES_KEY = 'sifs_admin_roles'
const LATEST_RESULT_KEY = 'sifs_latest_result'

function readJson(key, fallback = null) {
  const raw = localStorage.getItem(key)
  if (!raw || raw === 'undefined' || raw === 'null') return fallback

  try {
    return JSON.parse(raw)
  } catch {
    localStorage.removeItem(key)
    return fallback
  }
}

function writeJson(key, value) {
  if (value === undefined || value === null) {
    localStorage.removeItem(key)
    return
  }
  localStorage.setItem(key, JSON.stringify(value))
}

export const tokenStorage = {
  getAccessToken: () => localStorage.getItem(ACCESS_TOKEN_KEY) || '',
  setAccessToken: (token) => localStorage.setItem(ACCESS_TOKEN_KEY, token || ''),
  removeAccessToken: () => localStorage.removeItem(ACCESS_TOKEN_KEY),

  getRefreshToken: () => localStorage.getItem(REFRESH_TOKEN_KEY) || '',
  setRefreshToken: (token) => localStorage.setItem(REFRESH_TOKEN_KEY, token || ''),
  removeRefreshToken: () => localStorage.removeItem(REFRESH_TOKEN_KEY),

  getUserInfo: () => readJson(USER_INFO_KEY, null),
  setUserInfo: (user) => writeJson(USER_INFO_KEY, user),
  removeUserInfo: () => localStorage.removeItem(USER_INFO_KEY),

  getPermissions: () => readJson(PERMISSIONS_KEY, []),
  setPermissions: (permissions) => writeJson(PERMISSIONS_KEY, permissions || []),
  removePermissions: () => localStorage.removeItem(PERMISSIONS_KEY),

  getRoles: () => readJson(ROLES_KEY, []),
  setRoles: (roles) => writeJson(ROLES_KEY, roles || []),
  removeRoles: () => localStorage.removeItem(ROLES_KEY),

  clearAuth() {
    localStorage.removeItem(ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(USER_INFO_KEY)
    localStorage.removeItem(PERMISSIONS_KEY)
    localStorage.removeItem(ROLES_KEY)
  }
}

export const resultStorage = {
  getLatestResult: () => readJson(LATEST_RESULT_KEY, null),
  setLatestResult: (data) => writeJson(LATEST_RESULT_KEY, data),
  clearLatestResult: () => localStorage.removeItem(LATEST_RESULT_KEY)
}

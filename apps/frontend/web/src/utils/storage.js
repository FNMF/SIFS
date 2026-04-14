const TOKEN_KEY = 'sifs_access_token'
const REFRESH_TOKEN_KEY = 'sifs_refresh_token'
const USER_KEY = 'sifs_user_info'
const RESULT_KEY = 'sifs_latest_result'

export const tokenStorage = {
  getAccessToken() {
    return localStorage.getItem(TOKEN_KEY) || ''
  },
  setAccessToken(token) {
    localStorage.setItem(TOKEN_KEY, token)
  },
  getRefreshToken() {
    return localStorage.getItem(REFRESH_TOKEN_KEY) || ''
  },
  setRefreshToken(token) {
    localStorage.setItem(REFRESH_TOKEN_KEY, token)
  },
  getUser() {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? JSON.parse(raw) : null
  },
  setUser(user) {
    localStorage.setItem(USER_KEY, JSON.stringify(user))
  },
  clearAuth() {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }
}

export const resultStorage = {
  getLatestResult() {
    const raw = localStorage.getItem(RESULT_KEY)
    return raw ? JSON.parse(raw) : null
  },
  setLatestResult(result) {
    localStorage.setItem(RESULT_KEY, JSON.stringify(result))
  },
  clearLatestResult() {
    localStorage.removeItem(RESULT_KEY)
  }
}

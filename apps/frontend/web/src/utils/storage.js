const ACCESS_TOKEN_KEY = 'sifs_access_token'
const REFRESH_TOKEN_KEY = 'sifs_refresh_token'
const USER_INFO_KEY = 'sifs_user_info'
const LATEST_RESULT_KEY = 'sifs_latest_result'

export const tokenStorage = {
  getAccessToken() {
    return localStorage.getItem(ACCESS_TOKEN_KEY) || ''
  },
  setAccessToken(token) {
    localStorage.setItem(ACCESS_TOKEN_KEY, token)
  },
  removeAccessToken() {
    localStorage.removeItem(ACCESS_TOKEN_KEY)
  },

  getRefreshToken() {
    return localStorage.getItem(REFRESH_TOKEN_KEY) || ''
  },
  setRefreshToken(token) {
    localStorage.setItem(REFRESH_TOKEN_KEY, token)
  },
  removeRefreshToken() {
    localStorage.removeItem(REFRESH_TOKEN_KEY)
  },

  getUserInfo() {
    const raw = localStorage.getItem(USER_INFO_KEY)
    return raw ? JSON.parse(raw) : null
  },
  setUserInfo(user) {
    localStorage.setItem(USER_INFO_KEY, JSON.stringify(user))
  },
  removeUserInfo() {
    localStorage.removeItem(USER_INFO_KEY)
  },

  clearAuth() {
    localStorage.removeItem(ACCESS_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(USER_INFO_KEY)
  }
}

export const resultStorage = {
  getLatestResult() {
    const raw = localStorage.getItem(LATEST_RESULT_KEY)
    return raw ? JSON.parse(raw) : null
  },
  setLatestResult(data) {
    localStorage.setItem(LATEST_RESULT_KEY, JSON.stringify(data))
  },
  clearLatestResult() {
    localStorage.removeItem(LATEST_RESULT_KEY)
  }
}
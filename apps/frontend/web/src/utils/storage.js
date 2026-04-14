const ACCESS_TOKEN_KEY = 'sifs_access_token'
const REFRESH_TOKEN_KEY = 'sifs_refresh_token'
const USER_INFO_KEY = 'sifs_user_info'
const LATEST_RESULT_KEY = 'sifs_latest_result'

export const tokenStorage = {
  getAccessToken() {
    return localStorage.getItem(ACCESS_TOKEN_KEY) || ''
  },

  setAccessToken(token) {
    localStorage.setItem(ACCESS_TOKEN_KEY, token || '')
  },

  removeAccessToken() {
    localStorage.removeItem(ACCESS_TOKEN_KEY)
  },

  getRefreshToken() {
    return localStorage.getItem(REFRESH_TOKEN_KEY) || ''
  },

  setRefreshToken(token) {
    localStorage.setItem(REFRESH_TOKEN_KEY, token || '')
  },

  removeRefreshToken() {
    localStorage.removeItem(REFRESH_TOKEN_KEY)
  },

  getUserInfo() {
    const raw = localStorage.getItem(USER_INFO_KEY)

    if (!raw || raw === 'undefined' || raw === 'null') {
      return null
    }

    try {
      return JSON.parse(raw)
    } catch (error) {
      console.warn('解析用户信息失败，已清除损坏的本地数据:', raw)
      localStorage.removeItem(USER_INFO_KEY)
      return null
    }
  },

  setUserInfo(user) {
    if (!user) {
      localStorage.removeItem(USER_INFO_KEY)
      return
    }

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

    if (!raw || raw === 'undefined' || raw === 'null') {
      return null
    }

    try {
      return JSON.parse(raw)
    } catch (error) {
      console.warn('解析识别结果失败，已清除损坏的本地数据:', raw)
      localStorage.removeItem(LATEST_RESULT_KEY)
      return null
    }
  },

  setLatestResult(data) {
    if (!data) {
      localStorage.removeItem(LATEST_RESULT_KEY)
      return
    }

    localStorage.setItem(LATEST_RESULT_KEY, JSON.stringify(data))
  },

  clearLatestResult() {
    localStorage.removeItem(LATEST_RESULT_KEY)
  }
}
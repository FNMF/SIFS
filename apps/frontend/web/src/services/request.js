import { ElMessage } from 'element-plus'
import { tokenStorage } from '../utils/storage'
import { useAuthStore } from '../stores/auth'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5220'

let isRefreshing = false
let pendingQueue = []

function processQueue(newToken) {
  pendingQueue.forEach((cb) => cb(newToken))
  pendingQueue = []
}

async function doRefreshToken() {
  const accessToken = tokenStorage.getAccessToken()
  const refreshToken = tokenStorage.getRefreshToken()
  const authStore = useAuthStore()

  if (!accessToken || !refreshToken) {
    throw new Error('没有可用的登录信息')
  }

  const response = await fetch(`${API_BASE_URL}/api/identity/refresh`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${accessToken}`
    },
    body: JSON.stringify(refreshToken)
  })

  if (!response.ok) {
    authStore.clearAuth()
    throw new Error('登录已过期，请重新登录')
  }

  const data = await response.json()

  const loginData = data.Data || data.data || data
  const newAccessToken = loginData.AccessToken || loginData.accessToken
  const newRefreshToken = loginData.RefreshToken || loginData.refreshToken
  const userInfo = loginData.UserReadDto || loginData.userReadDto || authStore.state.userInfo

  if (!newAccessToken || !newRefreshToken) {
    authStore.clearAuth()
    throw new Error('刷新 token 返回数据无效')
  }

  authStore.setAuth({
    AccessToken: newAccessToken,
    RefreshToken: newRefreshToken,
    UserReadDto: userInfo
  })

  return newAccessToken
}

export async function request(url, options = {}, extra = {}) {
  const { skipAutoRefresh = false } = extra
  const headers = new Headers(options.headers || {})
  const token = tokenStorage.getAccessToken()

  if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }

  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  let response

  try {
    response = await fetch(`${API_BASE_URL}${url}`, {
      ...options,
      headers
    })
  } catch (error) {
    ElMessage.error('无法连接到后端服务')
    throw error
  }

  if (response.status === 401 && !skipAutoRefresh) {
    if (!isRefreshing) {
      isRefreshing = true
      try {
        const newToken = await doRefreshToken()
        processQueue(newToken)
      } catch (error) {
        processQueue('')
        isRefreshing = false
        throw error
      }
      isRefreshing = false
    }

    return new Promise((resolve, reject) => {
      pendingQueue.push(async (newToken) => {
        if (!newToken) {
          reject(new Error('登录已失效'))
          return
        }

        try {
          const retryHeaders = new Headers(options.headers || {})
          if (!retryHeaders.has('Content-Type') && !(options.body instanceof FormData)) {
            retryHeaders.set('Content-Type', 'application/json')
          }
          retryHeaders.set('Authorization', `Bearer ${newToken}`)

          const retryResponse = await fetch(`${API_BASE_URL}${url}`, {
            ...options,
            headers: retryHeaders
          })

          const contentType = retryResponse.headers.get('content-type') || ''
          const retryData = contentType.includes('application/json')
            ? await retryResponse.json()
            : await retryResponse.text()

          if (!retryResponse.ok) {
            reject(new Error(typeof retryData === 'string' ? retryData : retryData?.message || '请求失败'))
            return
          }

          resolve(retryData)
        } catch (err) {
          reject(err)
        }
      })
    })
  }

  const contentType = response.headers.get('content-type') || ''
  const data = contentType.includes('application/json')
    ? await response.json()
    : await response.text()

  if (!response.ok) {
    const message =
      typeof data === 'string'
        ? data
        : data?.message || data?.Message || '请求失败'
    ElMessage.error(message)
    throw new Error(message)
  }

  return data
}
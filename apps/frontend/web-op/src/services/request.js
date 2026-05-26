import { ElMessage } from 'element-plus'
import { tokenStorage } from '../utils/storage'
import { useAuthStore } from '../stores/auth'

const API_BASE_URL = ''

let isRefreshing = false
let pendingQueue = []
let hasRedirectedToLogin = false

function processQueue(token) {
  pendingQueue.forEach(({ resolve, reject }) => {
    if (token) {
      resolve(token)
    } else {
      reject(new Error('登录状态已失效'))
    }
  })
  pendingQueue = []
}

function unwrapLoginData(data) {
  return data?.Data || data?.data || data
}

function redirectToLogin(authStore) {
  authStore.clearAuth()

  if (typeof window === 'undefined' || hasRedirectedToLogin) {
    return
  }

  if (window.location.pathname === '/admin/login') {
    return
  }

  const currentPath = `${window.location.pathname}${window.location.search}${window.location.hash}` || '/'
  hasRedirectedToLogin = true
  window.location.href = `/admin/login?redirect=${encodeURIComponent(currentPath)}`
}

async function doRefreshToken() {
  const accessToken = tokenStorage.getAccessToken()
  const refreshToken = tokenStorage.getRefreshToken()
  const authStore = useAuthStore()

  if (!accessToken || !refreshToken) {
    authStore.clearAuth()
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

  const data = unwrapLoginData(await response.json())
  const newAccessToken = data?.AccessToken || data?.accessToken || data

  if (!newAccessToken || typeof newAccessToken !== 'string') {
    authStore.clearAuth()
    throw new Error('刷新 token 返回数据无效')
  }

  authStore.updateAccessToken(newAccessToken)
  return newAccessToken
}

async function parseResponse(response) {
  const contentType = response.headers.get('content-type') || ''
  return contentType.includes('application/json') ? await response.json() : await response.text()
}

function getErrorMessage(data, fallback = '请求失败') {
  if (typeof data === 'string') return data || fallback
  return data?.message || data?.Message || data?.title || fallback
}

export async function request(url, options = {}, extra = {}) {
  const { skipAutoRefresh = false, silent = false } = extra
  const authStore = useAuthStore()
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
    if (!silent) ElMessage.error('无法连接到后端服务')
    throw error
  }

  if (response.status === 401 && !skipAutoRefresh) {
    try {
      const newToken = await new Promise((resolve, reject) => {
        pendingQueue.push({ resolve, reject })

        if (!isRefreshing) {
          isRefreshing = true
          doRefreshToken()
            .then(processQueue)
            .catch((error) => {
              authStore.clearAuth()
              processQueue(null)
              if (!silent) ElMessage.error(error.message || '登录已过期，请重新登录')
              redirectToLogin(authStore)
            })
            .finally(() => {
              isRefreshing = false
            })
        }
      })

      const retryHeaders = new Headers(options.headers || {})
      if (!retryHeaders.has('Content-Type') && !(options.body instanceof FormData)) {
        retryHeaders.set('Content-Type', 'application/json')
      }
      retryHeaders.set('Authorization', `Bearer ${newToken}`)

      response = await fetch(`${API_BASE_URL}${url}`, {
        ...options,
        headers: retryHeaders
      })
    } catch (error) {
      if (error.status === 401) {
        redirectToLogin(authStore)
      }
      throw error
    }
  }

  const data = await parseResponse(response)

  if (!response.ok) {
    if (response.status === 401) {
      redirectToLogin(authStore)
    }

    const message = response.status === 403 ? '没有权限访问该资源' : getErrorMessage(data)
    if (!silent) ElMessage.error(message)
    const error = new Error(message)
    error.status = response.status
    throw error
  }

  return data
}

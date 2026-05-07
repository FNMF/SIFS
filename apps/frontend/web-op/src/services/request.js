import { ElMessage } from 'element-plus'
import { tokenStorage } from '../utils/storage'
import { useAuthStore } from '../stores/auth'

const API_BASE_URL = ''

let isRefreshing = false
let pendingQueue = []

function processQueue(token) {
  pendingQueue.forEach(({ resolve, reject }) => {
    if (token) {
      resolve(token)
    } else {
      reject(new Error('登录已失效'))
    }
  })
  pendingQueue = []
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

  const data = await response.json()

  const loginData = data?.Data || data?.data || data
  const newAccessToken = loginData?.AccessToken || loginData?.accessToken
  const newRefreshToken = loginData?.RefreshToken || loginData?.refreshToken
  const userInfo =
    loginData?.UserReadDto ||
    loginData?.userReadDto ||
    authStore.state.userInfo

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

async function parseResponse(response) {
  const contentType = response.headers.get('content-type') || ''
  return contentType.includes('application/json')
    ? await response.json()
    : await response.text()
}

export async function request(url, options = {}, extra = {}) {
  const { skipAutoRefresh = false } = extra
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
    ElMessage.error('无法连接到后端服务')
    throw error
  }

  if (response.status === 401 && !skipAutoRefresh) {
    try {
      const newToken = await new Promise((resolve, reject) => {
        pendingQueue.push({ resolve, reject })

        if (!isRefreshing) {
          isRefreshing = true

          doRefreshToken()
            .then((token) => {
              processQueue(token)
            })
            .catch((error) => {
              authStore.clearAuth()
              processQueue(null)
              ElMessage.error(error.message || '登录已过期，请重新登录')
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

      const retryResponse = await fetch(`${API_BASE_URL}${url}`, {
        ...options,
        headers: retryHeaders
      })

      const retryData = await parseResponse(retryResponse)

      if (!retryResponse.ok) {
        const retryMessage =
          typeof retryData === 'string'
            ? retryData
            : retryData?.message || retryData?.Message || '请求失败'
        ElMessage.error(retryMessage)
        throw new Error(retryMessage)
      }

      return retryData
    } catch (error) {
      throw error
    }
  }

  const data = await parseResponse(response)

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
import { ElMessage } from 'element-plus'
import { tokenStorage } from '../utils/storage'

const API_BASE_URL = 'http://localhost:5220'

export async function request(url, options = {}) {
  const headers = new Headers(options.headers || {})
  const token = tokenStorage.getAccessToken()

  if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }

  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers
  })

  const contentType = response.headers.get('content-type') || ''
  const data = contentType.includes('application/json')
    ? await response.json()
    : await response.text()

  if (!response.ok) {
    const message = typeof data === 'string'
      ? data
      : data?.message || data?.Message || '请求失败'
    ElMessage.error(message)
    throw new Error(message)
  }

  return data
}

export { API_BASE_URL }

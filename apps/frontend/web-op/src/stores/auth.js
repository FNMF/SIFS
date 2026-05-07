import { computed, reactive } from 'vue'
import { tokenStorage } from '../utils/storage'

const state = reactive({
  accessToken: tokenStorage.getAccessToken(),
  refreshToken: tokenStorage.getRefreshToken(),
  userInfo: tokenStorage.getUserInfo(),
  permissions: tokenStorage.getPermissions()
})

function unwrapLoginData(loginData) {
  return loginData?.Data || loginData?.data || loginData
}

export function useAuthStore() {
  const isLoggedIn = computed(() => !!state.accessToken)
  const isAdmin = computed(() => state.permissions.includes('admin:access'))

  function setAuth(loginData) {
    const data = unwrapLoginData(loginData)
    const accessToken = data?.AccessToken || data?.accessToken || ''
    const refreshToken = data?.RefreshToken || data?.refreshToken || ''
    const userInfo = data?.UserReadDto || data?.userReadDto || data?.user || null

    state.accessToken = accessToken
    state.refreshToken = refreshToken
    state.userInfo = userInfo

    accessToken ? tokenStorage.setAccessToken(accessToken) : tokenStorage.removeAccessToken()
    refreshToken ? tokenStorage.setRefreshToken(refreshToken) : tokenStorage.removeRefreshToken()
    tokenStorage.setUserInfo(userInfo)
  }

  function setPermissions(permissions) {
    state.permissions = Array.isArray(permissions) ? permissions : []
    tokenStorage.setPermissions(state.permissions)
  }

  function updateAccessToken(accessToken) {
    state.accessToken = accessToken || ''
    accessToken ? tokenStorage.setAccessToken(accessToken) : tokenStorage.removeAccessToken()
  }

  function clearAuth() {
    state.accessToken = ''
    state.refreshToken = ''
    state.userInfo = null
    state.permissions = []
    tokenStorage.clearAuth()
  }

  function restoreAuth() {
    state.accessToken = tokenStorage.getAccessToken()
    state.refreshToken = tokenStorage.getRefreshToken()
    state.userInfo = tokenStorage.getUserInfo()
    state.permissions = tokenStorage.getPermissions()
  }

  return {
    state,
    isLoggedIn,
    isAdmin,
    setAuth,
    setPermissions,
    updateAccessToken,
    clearAuth,
    restoreAuth
  }
}

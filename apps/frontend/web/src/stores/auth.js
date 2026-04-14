import { reactive, computed } from 'vue'
import { tokenStorage } from '../utils/storage'

const state = reactive({
  accessToken: tokenStorage.getAccessToken(),
  refreshToken: tokenStorage.getRefreshToken(),
  userInfo: tokenStorage.getUserInfo()
})

export function useAuthStore() {
  const isLoggedIn = computed(() => !!state.accessToken && !!state.userInfo)

  function setAuth(loginData) {
  const accessToken = loginData?.AccessToken || loginData?.accessToken || ''
  const refreshToken = loginData?.RefreshToken || loginData?.refreshToken || ''
  const userInfo = loginData?.UserReadDto || loginData?.userReadDto || null

  state.accessToken = accessToken
  state.refreshToken = refreshToken
  state.userInfo = userInfo

  if (accessToken) {
    tokenStorage.setAccessToken(accessToken)
  } else {
    tokenStorage.removeAccessToken()
  }

  if (refreshToken) {
    tokenStorage.setRefreshToken(refreshToken)
  } else {
    tokenStorage.removeRefreshToken()
  }

  tokenStorage.setUserInfo(userInfo)
}

  function updateAccessToken(accessToken) {
    state.accessToken = accessToken || ''
    if (accessToken) {
      tokenStorage.setAccessToken(accessToken)
    } else {
      tokenStorage.removeAccessToken()
    }
  }

  function clearAuth() {
    state.accessToken = ''
    state.refreshToken = ''
    state.userInfo = null
    tokenStorage.clearAuth()
  }

  function restoreAuth() {
    state.accessToken = tokenStorage.getAccessToken()
    state.refreshToken = tokenStorage.getRefreshToken()
    state.userInfo = tokenStorage.getUserInfo()
  }

  return {
    state,
    isLoggedIn,
    setAuth,
    updateAccessToken,
    clearAuth,
    restoreAuth
  }
}
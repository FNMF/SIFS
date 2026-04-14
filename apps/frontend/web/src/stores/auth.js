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
    state.accessToken = loginData.AccessToken
    state.refreshToken = loginData.RefreshToken
    state.userInfo = loginData.UserReadDto

    tokenStorage.setAccessToken(loginData.AccessToken)
    tokenStorage.setRefreshToken(loginData.RefreshToken)
    tokenStorage.setUserInfo(loginData.UserReadDto)
  }

  function updateAccessToken(accessToken) {
    state.accessToken = accessToken
    tokenStorage.setAccessToken(accessToken)
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
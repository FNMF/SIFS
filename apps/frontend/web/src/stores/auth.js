import { reactive, computed } from 'vue'
import { tokenStorage } from '../utils/storage'

const state = reactive({
  accessToken: tokenStorage.getAccessToken(),
  refreshToken: tokenStorage.getRefreshToken(),
  user: tokenStorage.getUser()
})

export function useAuthStore() {
  const isAuthenticated = computed(() => Boolean(state.accessToken))

  const setAuth = (payload) => {
    const accessToken = payload?.AccessToken || payload?.accessToken || ''
    const refreshToken = payload?.RefreshToken || payload?.refreshToken || ''
    const user = payload?.UserReadDto || payload?.userReadDto || payload?.user || null

    state.accessToken = accessToken
    state.refreshToken = refreshToken
    state.user = user

    tokenStorage.setAccessToken(accessToken)
    tokenStorage.setRefreshToken(refreshToken)
    tokenStorage.setUser(user)
  }

  const logout = () => {
    state.accessToken = ''
    state.refreshToken = ''
    state.user = null
    tokenStorage.clearAuth()
  }

  return {
    state,
    isAuthenticated,
    setAuth,
    logout
  }
}

import { reactive, computed } from 'vue'
import { tokenStorage } from '../utils/storage'

const state = reactive({
  accessToken: tokenStorage.getAccessToken(),
  refreshToken: tokenStorage.getRefreshToken(),
  userInfo: tokenStorage.getUserInfo(),
  roles: tokenStorage.getRoles(),
  permissions: tokenStorage.getPermissions()
})

export function useAuthStore() {
  const isLoggedIn = computed(() => !!state.accessToken && !!state.userInfo)

  function setAuth(loginData) {
  const accessToken = loginData?.AccessToken || loginData?.accessToken || ''
  const refreshToken = loginData?.RefreshToken || loginData?.refreshToken || ''
  const userInfo = loginData?.UserReadDto || loginData?.userReadDto || null
  const roles = userInfo?.Roles || userInfo?.roles || loginData?.roles || []
  const permissions = userInfo?.Permissions || userInfo?.permissions || loginData?.permissions || []

  state.accessToken = accessToken
  state.refreshToken = refreshToken
  state.userInfo = userInfo
  state.roles = [...new Set(Array.isArray(roles) ? roles : [])]
  state.permissions = [...new Set(Array.isArray(permissions) ? permissions : [])]

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
  tokenStorage.setRoles(state.roles)
  tokenStorage.setPermissions(state.permissions)
}

  function hasPermission(permissionCode) {
    return !!permissionCode && state.permissions.includes(permissionCode)
  }

  function hasAnyPermission(permissionCodes) {
    return (permissionCodes || []).some((code) => hasPermission(code))
  }

  function hasRole(roleName) {
    return !!roleName && state.roles.includes(roleName)
  }

  function isAdmin() {
    return hasPermission('admin:access') || hasRole('admin')
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
    state.roles = []
    state.permissions = []
    tokenStorage.clearAuth()
  }

  function restoreAuth() {
    state.accessToken = tokenStorage.getAccessToken()
    state.refreshToken = tokenStorage.getRefreshToken()
    state.userInfo = tokenStorage.getUserInfo()
    state.roles = tokenStorage.getRoles()
    state.permissions = tokenStorage.getPermissions()
  }

  return {
    state,
    isLoggedIn,
    hasPermission,
    hasAnyPermission,
    hasRole,
    isAdmin,
    setAuth,
    updateAccessToken,
    clearAuth,
    restoreAuth
  }
}

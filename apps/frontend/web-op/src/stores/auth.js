import { computed, reactive } from 'vue'
import { tokenStorage } from '../utils/storage'

const state = reactive({
  accessToken: tokenStorage.getAccessToken(),
  refreshToken: tokenStorage.getRefreshToken(),
  userInfo: tokenStorage.getUserInfo(),
  roles: tokenStorage.getRoles(),
  permissions: tokenStorage.getPermissions()
})

function unwrapLoginData(loginData) {
  return loginData?.Data || loginData?.data || loginData
}

export function useAuthStore() {
  const isLoggedIn = computed(() => !!state.accessToken)
  const isAdmin = computed(() => hasPermission('admin:access') || hasRole('admin'))

  function setAuth(loginData) {
    const data = unwrapLoginData(loginData)
    const accessToken = data?.AccessToken || data?.accessToken || ''
    const refreshToken = data?.RefreshToken || data?.refreshToken || ''
    const userInfo = data?.UserReadDto || data?.userReadDto || data?.user || null
    const roles = userInfo?.Roles || userInfo?.roles || data?.roles || []
    const permissions = userInfo?.Permissions || userInfo?.permissions || data?.permissions || []

    state.accessToken = accessToken
    state.refreshToken = refreshToken
    state.userInfo = userInfo

    setRoles(roles)
    setPermissions(permissions)

    accessToken ? tokenStorage.setAccessToken(accessToken) : tokenStorage.removeAccessToken()
    refreshToken ? tokenStorage.setRefreshToken(refreshToken) : tokenStorage.removeRefreshToken()
    tokenStorage.setUserInfo(userInfo)
  }

  function setPermissions(permissions) {
    state.permissions = [...new Set(Array.isArray(permissions) ? permissions : [])]
    tokenStorage.setPermissions(state.permissions)
  }

  function setRoles(roles) {
    state.roles = [...new Set(Array.isArray(roles) ? roles : [])]
    tokenStorage.setRoles(state.roles)
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

  function updateAccessToken(accessToken) {
    state.accessToken = accessToken || ''
    accessToken ? tokenStorage.setAccessToken(accessToken) : tokenStorage.removeAccessToken()
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
    isAdmin,
    setAuth,
    setRoles,
    setPermissions,
    hasPermission,
    hasAnyPermission,
    hasRole,
    updateAccessToken,
    clearAuth,
    restoreAuth
  }
}

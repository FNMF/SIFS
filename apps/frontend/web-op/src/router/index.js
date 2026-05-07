import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '../views/Dashboard.vue'
import AlgoManagement from '../views/AlgoManagement.vue'
import TaskManagement from '../views/TaskManagement.vue'
import OperationLogs from '../views/OperationLogs.vue'
import UserRoles from '../views/UserRoles.vue'
import Login from '../views/Login.vue'
import Forbidden from '../views/Forbidden.vue'
import { rbacApi } from '../services/admin'
import { useAuthStore } from '../stores/auth'
import { tokenStorage } from '../utils/storage'

const routes = [
  { path: '/', redirect: '/admin/dashboard' },
  { path: '/login', name: 'login', component: Login },
  { path: '/403', name: 'forbidden', component: Forbidden },
  { path: '/dashboard', redirect: '/admin/dashboard' },
  { path: '/admin/dashboard', name: 'dashboard', component: Dashboard, meta: { requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/algos', name: 'algos', component: AlgoManagement, meta: { requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/tasks', name: 'tasks', component: TaskManagement, meta: { requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/operation-logs', name: 'operation-logs', component: OperationLogs, meta: { requiresAuth: true, requiresAdmin: true } },
  { path: '/admin/users/roles', name: 'user-roles', component: UserRoles, meta: { requiresAuth: true, requiresAdmin: true } }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  const accessToken = tokenStorage.getAccessToken()

  if (to.meta.requiresAuth && !accessToken) {
    next({ path: '/login', query: { redirect: to.fullPath } })
    return
  }

  if (to.meta.requiresAdmin) {
    try {
      if (!authStore.state.permissions.includes('admin:access')) {
        const permissions = await rbacApi.myPermissions()
        authStore.setPermissions(permissions)
      }

      if (!authStore.state.permissions.includes('admin:access')) {
        next('/403')
        return
      }
    } catch (error) {
      if (error.status === 401) {
        authStore.clearAuth()
        next({ path: '/login', query: { redirect: to.fullPath } })
        return
      }
      next('/403')
      return
    }
  }

  next()
})

export default router

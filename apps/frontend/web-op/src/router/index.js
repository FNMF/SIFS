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
  { path: '/login', redirect: '/admin/login' },
  { path: '/403', redirect: '/admin/403' },
  { path: '/admin/login', name: 'login', component: Login },
  { path: '/admin/403', name: 'forbidden', component: Forbidden },
  { path: '/dashboard', redirect: '/admin/dashboard' },
  { path: '/admin/dashboard', name: 'dashboard', component: Dashboard, meta: { requiresAuth: true, requiredPermission: 'admin:access' } },
  { path: '/admin/algos', name: 'algos', component: AlgoManagement, meta: { requiresAuth: true, requiredPermission: 'algo:view' } },
  { path: '/admin/tasks', name: 'tasks', component: TaskManagement, meta: { requiresAuth: true, requiredPermission: 'task:view:all' } },
  { path: '/admin/operation-logs', name: 'operation-logs', component: OperationLogs, meta: { requiresAuth: true, requiredPermission: 'log:view' } },
  { path: '/admin/users/roles', name: 'user-roles', component: UserRoles, meta: { requiresAuth: true, requiredPermission: 'admin:access' } }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  const accessToken = tokenStorage.getAccessToken()

  if (to.meta.requiresAuth && !accessToken) {
    next({ path: '/admin/login', query: { redirect: to.fullPath } })
    return
  }

  if (to.meta.requiredPermission) {
    try {
      if (!authStore.state.permissions.length) {
        const data = await rbacApi.me()
        authStore.setRoles(data?.user?.roles || [])
        authStore.setPermissions(data?.user?.permissions || [])
      }

      if (!authStore.hasPermission(to.meta.requiredPermission)) {
        next('/admin/403')
        return
      }
    } catch (error) {
      if (error.status === 401) {
        authStore.clearAuth()
        next({ path: '/admin/login', query: { redirect: to.fullPath } })
        return
      }
      next('/admin/403')
      return
    }
  }

  next()
})

export default router

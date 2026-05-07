import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '../views/Dashboard.vue'
import Login from '../views/Login.vue'
import { tokenStorage } from '../utils/storage'

const routes = [
  { path: '/', redirect: '/dashboard' },
  { path: '/login', name: 'login', component: Login },
  { path: '/dashboard', name: 'dashboard', component: Dashboard, meta: { requiresAuth: true } }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const accessToken = tokenStorage.getAccessToken()
  if (to.meta.requiresAuth && !accessToken) {
    next('/login')
    return
  }
  next()
})

export default router

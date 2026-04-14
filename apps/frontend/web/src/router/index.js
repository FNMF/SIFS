import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Login from '../views/Login.vue'
import Register from '../views/Register.vue'
import Upload from '../views/Upload.vue'
import { tokenStorage } from '../utils/storage'

const routes = [
  { path: '/', name: 'home', component: Home },
  { path: '/login', name: 'login', component: Login },
  { path: '/register', name: 'register', component: Register },
  { path: '/upload', name: 'upload', component: Upload, meta: { requiresAuth: true } }
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
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import './styles/global.css'
import { useAuthStore } from './stores/auth'

const app = createApp(App)

const authStore = useAuthStore()
authStore.restoreAuth()

app.use(router)
app.use(ElementPlus)
app.mount('#app')
<template>
  <div class="op-shell">
    <aside class="op-sidebar">
      <div class="op-brand">
        <div class="op-brand__mark">SIFS</div>
        <div>
          <strong>管理控制台</strong>
          <span>Satellite Image Forensics</span>
        </div>
      </div>

      <nav class="op-nav">
        <router-link v-for="item in navItems" :key="item.path" :to="item.path" class="op-nav__item">
          <component :is="item.icon" />
          <span>{{ item.label }}</span>
        </router-link>
      </nav>
    </aside>

    <main class="op-main">
      <header class="op-topbar">
        <div>
          <h1>{{ title }}</h1>
          <p>{{ subtitle }}</p>
        </div>
        <div class="op-topbar__actions">
          <slot name="actions" />
          <span v-if="realtimeLabel" :class="['signal-badge', realtimeOnline ? 'is-online' : '']">
            {{ realtimeLabel }}
          </span>
          <el-button :icon="SwitchButton" @click="logout">退出</el-button>
        </div>
      </header>

      <slot />
    </main>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { Cpu, Document, Monitor, SwitchButton, Tickets, UserFilled } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '../stores/auth'

defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  realtimeLabel: { type: String, default: '' },
  realtimeOnline: { type: Boolean, default: false }
})

const router = useRouter()
const authStore = useAuthStore()

const navItems = [
  { path: '/admin/dashboard', label: '看板总览', icon: Monitor },
  { path: '/admin/algos', label: '算法管理', icon: Cpu },
  { path: '/admin/tasks', label: '任务管理', icon: Tickets },
  { path: '/admin/operation-logs', label: '操作日志', icon: Document },
  { path: '/admin/users/roles', label: '用户角色', icon: UserFilled }
]

function logout() {
  authStore.clearAuth()
  ElMessage.success('已退出')
  router.push('/login')
}
</script>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '../stores/auth'

const props = defineProps({
  variant: {
    type: String,
    default: 'landing' // landing | app
  }
})

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const isScrolled = ref(false)

const isLoggedIn = computed(() => authStore.isLoggedIn.value)
const username = computed(() => {
  const user = authStore.state.userInfo
  return user?.Account || user?.account || '用户'
})

const isLanding = computed(() => props.variant === 'landing')
const isApp = computed(() => props.variant === 'app')

function handleScroll() {
  isScrolled.value = window.scrollY > 10
}

function goHome() {
  router.push('/')
}

function goLogin() {
  router.push('/login')
}

function goRegister() {
  router.push('/register')
}

function goUpload() {
  router.push('/upload')
}

function goHistory() {
  router.push('/history')
}

function goTry() {
  if (isLoggedIn.value) {
    router.push('/upload')
  } else {
    router.push('/login')
  }
}

function handleLogout() {
  authStore.clearAuth()
  ElMessage.success('已退出登录')
  router.push('/')
}

onMounted(() => {
  handleScroll()
  window.addEventListener('scroll', handleScroll)
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

<template>
  <header
    class="header"
    :class="{
      'header--shadow': isScrolled,
      'header--app': isApp
    }"
  >
    <div class="container header__inner">
      <div class="header__brand" @click="goHome" style="cursor: pointer">
        <div class="brand__logo">SI</div>
        <div>
          <div class="brand__title">卫星图片识别系统</div>
          <div class="brand__subtitle">Satellite Image Recognition System</div>
        </div>
      </div>

      <nav v-if="isLanding" class="header__nav">
        <a href="#hero">首页</a>
        <a href="#features">系统能力</a>
        <a href="#workflow">处理流程</a>
        <a href="#about">项目介绍</a>
      </nav>

      <nav v-else class="header__nav header__nav--app">
        <button
          class="header__nav-btn"
          :class="{ 'is-active': route.path.startsWith('/upload') }"
          @click="goUpload"
        >
          上传
        </button>
        <button
          class="header__nav-btn"
          :class="{ 'is-active': route.path.startsWith('/history') || route.path.startsWith('/tasks') }"
          @click="goHistory"
        >
          历史
        </button>
      </nav>

      <div class="header__actions" v-if="!isLoggedIn">
        <el-button plain round @click="goLogin">登录</el-button>
        <el-button type="primary" round @click="goRegister">注册</el-button>
      </div>

      <div class="header__actions" v-else>
        <span class="user-welcome">你好，{{ username }}</span>
        <el-button v-if="isLanding" type="primary" round @click="goTry">立即尝试</el-button>
        <el-button v-if="isApp" type="primary" round @click="goUpload">新建任务</el-button>
        <el-button round @click="handleLogout">退出登录</el-button>
      </div>
    </div>
  </header>
</template>
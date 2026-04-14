<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isLoggedIn = computed(() => authStore.isLoggedIn.value)
const username = computed(() => authStore.state.userInfo?.Account || '用户')

const isScrolled = ref(false)

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

function goTry() {
  router.push('/upload')
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
  <header class="header" :class="{ 'header--scrolled': isScrolled }">
    <div class="container header-inner">
      <div class="brand" @click="goHome">
        <span class="brand-logo">S</span>
        <div>
          <div class="brand-title">卫星图片识别系统</div>
          <div class="brand-subtitle">Satellite Image Identification System</div>
        </div>
      </div>

      <nav class="nav">
        <a href="#hero">首页</a>
        <a href="#features">系统特点</a>
        <a href="#timeline">处理流程</a>
        <a href="#about">项目介绍</a>
      </nav>

      <div class="header-actions" v-if="!isLoggedIn">
        <el-button text @click="goLogin">登录</el-button>
        <el-button type="primary" round @click="goRegister">注册</el-button>
      </div>

      <div class="header-actions" v-else>
        <span class="user-welcome">你好，{{ username }}</span>
        <el-button type="primary" plain round @click="goTry">立即尝试</el-button>
        <el-button round @click="handleLogout">退出登录</el-button>
      </div>
    </div>
  </header>
</template>
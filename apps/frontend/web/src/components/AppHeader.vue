<template>
  <header class="header" :class="{ 'header--shadow': isScrolled }">
    <div class="container header__inner">
      <div class="header__brand">
        <div class="brand__logo">SI</div>
        <div>
          <div class="brand__title">卫星图片识别系统</div>
          <div class="brand__subtitle">Satellite Image Recognition System</div>
        </div>
      </div>

      <nav class="header__nav">
        <a href="#hero">首页</a>
        <a href="#features">系统能力</a>
        <a href="#workflow">处理流程</a>
        <a href="#about">项目介绍</a>
      </nav>

      <div class="header__actions">
        <el-button plain round>登录</el-button>
        <el-button type="primary" round>注册</el-button>
      </div>
    </div>
  </header>
</template>

<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue'

const isScrolled = ref(false)

const handleScroll = () => {
  isScrolled.value = window.scrollY > 10
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll)
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

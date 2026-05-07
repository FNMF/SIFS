<template>
  <div class="auth-layout">
    <div class="auth-layout__aside">
      <router-link class="auth-brand" to="/">
        <div class="auth-brand__logo">SI</div>
        <div>
          <div class="auth-brand__title">卫星图片识别系统</div>
          <div class="auth-brand__subtitle">Satellite Image Recognition System</div>
        </div>
      </router-link>

      <div class="auth-layout__content">
        <div class="section-tag">身份认证</div>
        <h1>{{ title }}</h1>
        <p>{{ description }}</p>

        <div class="auth-feature-list">
          <div v-for="item in featureList" :key="item.title" class="auth-feature-item">
            <strong>{{ item.title }}</strong>
            <span>{{ item.desc }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="auth-layout__main">
      <slot />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: String,
  description: String,
  featureList: {
    type: Array,
    default: () => []
  }
})
</script>

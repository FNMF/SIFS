<script setup>
import { onMounted, watch } from 'vue'
import { useAuthStore } from './stores/auth'
import { startTaskRealtime, stopTaskRealtime } from './services/taskRealtime'

const authStore = useAuthStore()

onMounted(() => {
  if (authStore.isLoggedIn.value) {
    startTaskRealtime()
  }
})

watch(
  () => authStore.state.accessToken,
  (token) => {
    if (token && authStore.isLoggedIn.value) {
      startTaskRealtime()
    } else {
      stopTaskRealtime()
    }
  }
)
</script>

<template>
  <router-view />
</template>

<style scoped></style>

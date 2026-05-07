<template>
  <button class="floating-btn" @click="goTry">
    立即尝试
  </button>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isLoggedIn = computed(() => authStore.isLoggedIn.value)
const props = defineProps({
  target: {
    type: String,
    default: '#workflow'
  }
})

const scrollToTarget = () => {
  const el = document.querySelector(props.target)
  if (el) {
    el.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }
}

function goTry() {
  if (isLoggedIn) {
    router.push('/upload')
  } else {
    router.push('/login')
  }
}
</script>

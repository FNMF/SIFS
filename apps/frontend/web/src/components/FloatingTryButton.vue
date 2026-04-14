<template>
  <button class="floating-btn" @click="goTry">
    立即尝试
  </button>
</template>

<script setup>
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
</script>

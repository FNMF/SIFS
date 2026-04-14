<template>
  <button class="floating-btn" @click="scrollToTarget">
    <span>立即尝试</span>
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

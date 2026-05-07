<template>
  <span :class="['status-cell', `status-cell--${status || 'unknown'}`]">
    <i :class="['status-dot', `is-${status || 'unknown'}`]" />
    {{ label }}
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: { type: String, default: '' }
})

const label = computed(() => {
  const map = {
    online: '在线',
    offline: '离线',
    timeout: '超时',
    disabled: '停用',
    running: '运行中',
    queued: '等待中',
    pending: '等待中',
    done: '成功',
    success: '成功',
    failed: '失败',
    canceled: '已取消',
    deleted: '已删除'
  }
  return map[props.status] || props.status || '未知'
})
</script>

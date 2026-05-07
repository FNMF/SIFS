<template>
  <div class="op-panel">
    <div class="panel-header"><h2>{{ title }}</h2></div>
    <div v-if="!items.length" class="empty-inline">暂无数据</div>
    <ul v-else class="recent-list">
      <li v-for="item in items" :key="item.id || item.task_id">
        <div>
          <strong>{{ line(item) }}</strong>
          <span>{{ sub(item) }}</span>
        </div>
        <time>{{ formatTime(item.created_at || item.finished_at) }}</time>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { formatTime, statusText } from '../utils/format'

defineProps({
  title: { type: String, required: true },
  items: { type: Array, default: () => [] },
  type: { type: String, default: 'task' }
})

function line(item) {
  if (item.operation_type) return `${item.operation_type || '-'} · ${item.actor_username || 'system'}`
  return `${item.algorithm_name || '-'} · ${statusText(item.status)}`
}

function sub(item) {
  if (item.operation_type) return item.request_path || item.failure_reason || '-'
  return item.failure_reason || item.task_id || '-'
}
</script>

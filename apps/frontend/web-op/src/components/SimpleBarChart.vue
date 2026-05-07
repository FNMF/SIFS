<template>
  <div class="simple-chart">
    <div v-if="!items.length" class="empty-inline">暂无统计数据</div>
    <div v-for="item in items" :key="item[labelKey]" class="chart-row">
      <span class="chart-row__label">{{ formatLabel(item[labelKey]) }}</span>
      <div class="chart-row__track">
        <i :style="{ width: `${barWidth(item)}%` }" />
      </div>
      <strong>{{ item[valueKey] || 0 }}</strong>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  items: { type: Array, default: () => [] },
  labelKey: { type: String, required: true },
  valueKey: { type: String, required: true },
  labelMap: { type: Object, default: () => ({}) }
})

const maxValue = computed(() => Math.max(1, ...props.items.map((item) => Number(item[props.valueKey]) || 0)))

function barWidth(item) {
  return Math.max(4, ((Number(item[props.valueKey]) || 0) / maxValue.value) * 100)
}

function formatLabel(value) {
  return props.labelMap[value] || value || '-'
}
</script>

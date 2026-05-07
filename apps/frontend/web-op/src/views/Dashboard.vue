<template>
  <AdminLayout
    title="运营看板"
    subtitle="集中查看任务运行、算法健康和最近操作"
    :realtime-label="realtimeConnected ? '实时已连接' : '轮询刷新'"
    :realtime-online="realtimeConnected"
  >
    <template #actions>
      <el-button :icon="Refresh" :loading="loading" @click="loadDashboard">刷新</el-button>
    </template>

    <el-alert v-if="errorMessage" class="op-alert" type="warning" show-icon :closable="false" :title="errorMessage" />

    <section class="summary-grid" v-loading="loading">
      <article v-for="card in summaryCards" :key="card.key" class="summary-card">
        <div :class="['summary-card__icon', card.tone]">
          <component :is="card.icon" />
        </div>
        <div>
          <span>{{ card.label }}</span>
          <strong>{{ card.value }}</strong>
        </div>
      </article>
    </section>

    <section class="panel-grid">
      <div class="op-panel">
        <div class="panel-header"><h2>任务状态分布</h2></div>
        <SimpleBarChart :items="taskStatusItems" label-key="status" value-key="count" :label-map="statusMap" />
      </div>
      <div class="op-panel">
        <div class="panel-header"><h2>成功 / 失败比例</h2></div>
        <SimpleBarChart :items="successRatioItems" label-key="status" value-key="count" :label-map="statusMap" />
      </div>
      <div class="op-panel">
        <div class="panel-header"><h2>算法健康分布</h2></div>
        <SimpleBarChart :items="algoHealthChartItems" label-key="status" value-key="count" :label-map="statusMap" />
      </div>
    </section>

    <section class="op-panel">
      <div class="panel-header">
        <h2>算法健康状态</h2>
        <el-select v-model="healthFilter.status" clearable placeholder="状态筛选" class="status-filter" @change="loadAlgoHealth">
          <el-option label="在线" value="online" />
          <el-option label="离线" value="offline" />
          <el-option label="超时" value="timeout" />
          <el-option label="停用" value="disabled" />
        </el-select>
      </div>
      <el-table :data="algoHealth" border empty-text="暂无算法健康数据">
        <el-table-column label="状态" width="110">
          <template #default="{ row }"><StatusTag :status="row.health_status" /></template>
        </el-table-column>
        <el-table-column prop="name" label="算法名称" min-width="140" />
        <el-table-column prop="api_url" label="API 地址" min-width="260" show-overflow-tooltip />
        <el-table-column prop="response_time_ms" label="响应 ms" width="110" />
        <el-table-column label="检查时间" min-width="170">
          <template #default="{ row }">{{ formatTime(row.checked_at) }}</template>
        </el-table-column>
        <el-table-column prop="failure_reason" label="失败原因" min-width="220" show-overflow-tooltip />
      </el-table>
    </section>

    <section class="panel-grid panel-grid--lists">
      <RecentList title="最近任务" :items="recentTasks" type="task" />
      <RecentList title="最近失败任务" :items="recentFailedTasks" type="task" />
      <RecentList title="最近操作日志" :items="recentLogs" type="log" />
    </section>
  </AdminLayout>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { CircleCheck, Cpu, Files, Finished, Refresh, Timer, TrendCharts, Warning } from '@element-plus/icons-vue'
import AdminLayout from '../layouts/AdminLayout.vue'
import SimpleBarChart from '../components/SimpleBarChart.vue'
import StatusTag from '../components/StatusTag.vue'
import RecentList from '../components/RecentList.vue'
import { dashboardApi } from '../services/admin'
import { createDashboardConnection } from '../services/realtime'
import { formatTime } from '../utils/format'

const loading = ref(false)
const errorMessage = ref('')
const realtimeConnected = ref(false)
const summary = ref({})
const taskStatus = ref({ items: [] })
const algoStatus = ref({})
const algoHealth = ref([])
const recentTasks = ref([])
const recentFailedTasks = ref([])
const recentLogs = ref([])
const healthFilter = reactive({ status: '' })
let refreshTimer = null
let realtimeConnection = null

const statusMap = {
  online: '在线',
  offline: '离线',
  timeout: '超时',
  disabled: '停用',
  running: '运行中',
  queued: '等待中',
  pending: '等待中',
  done: '成功',
  failed: '失败',
  success: '成功'
}

const summaryCards = computed(() => [
  { key: 'today', label: '今日任务数', value: summary.value.today_task_count ?? 0, icon: Timer, tone: 'tone-blue' },
  { key: 'total', label: '总任务数', value: summary.value.total_task_count ?? 0, icon: Files, tone: 'tone-slate' },
  { key: 'running', label: '运行中任务数', value: summary.value.running_task_count ?? 0, icon: TrendCharts, tone: 'tone-cyan' },
  { key: 'waiting', label: '等待中任务数', value: summary.value.waiting_task_count ?? 0, icon: Finished, tone: 'tone-amber' },
  { key: 'success', label: '成功任务数', value: summary.value.success_task_count ?? 0, icon: CircleCheck, tone: 'tone-green' },
  { key: 'failed', label: '失败任务数', value: summary.value.failed_task_count ?? 0, icon: Warning, tone: 'tone-red' },
  { key: 'algoTotal', label: '算法总数', value: summary.value.algo_total_count ?? 0, icon: Cpu, tone: 'tone-slate' },
  { key: 'algoEnabled', label: '启用算法数', value: summary.value.algo_enabled_count ?? 0, icon: Cpu, tone: 'tone-blue' },
  { key: 'algoOnline', label: '在线算法数', value: summary.value.algo_online_count ?? algoStatus.value.online ?? 0, icon: CircleCheck, tone: 'tone-green' },
  { key: 'algoOffline', label: '离线算法数', value: summary.value.algo_offline_count ?? algoStatus.value.offline ?? 0, icon: Warning, tone: 'tone-red' },
  { key: 'algoTimeout', label: '超时算法数', value: summary.value.algo_timeout_count ?? algoStatus.value.timeout ?? 0, icon: Timer, tone: 'tone-amber' }
])

const taskStatusItems = computed(() => taskStatus.value.items || [])
const successRatioItems = computed(() => [
  { status: 'done', count: summary.value.success_task_count ?? 0 },
  { status: 'failed', count: summary.value.failed_task_count ?? 0 }
])
const algoHealthChartItems = computed(() => [
  { status: 'online', count: algoStatus.value.online ?? 0 },
  { status: 'offline', count: algoStatus.value.offline ?? 0 },
  { status: 'timeout', count: algoStatus.value.timeout ?? 0 },
  { status: 'disabled', count: algoStatus.value.disabled ?? 0 }
])

async function loadDashboard() {
  loading.value = true
  errorMessage.value = ''
  try {
    const [summaryData, taskCounts, algoCounts, tasks, failedTasks, logs] = await Promise.all([
      dashboardApi.summary(),
      dashboardApi.taskStatusCount(),
      dashboardApi.algoStatusCount(),
      dashboardApi.recentTasks({ limit: 8 }),
      dashboardApi.recentTasks({ limit: 8, failed_only: true }),
      dashboardApi.recentLogs({ limit: 8 })
    ])
    summary.value = summaryData || {}
    taskStatus.value = taskCounts || { items: [] }
    algoStatus.value = algoCounts || {}
    recentTasks.value = Array.isArray(tasks) ? tasks : []
    recentFailedTasks.value = Array.isArray(failedTasks) ? failedTasks : []
    recentLogs.value = Array.isArray(logs) ? logs : []
    await loadAlgoHealth()
  } catch (error) {
    errorMessage.value = error.message || '看板数据加载失败'
  } finally {
    loading.value = false
  }
}

async function loadAlgoHealth() {
  const data = await dashboardApi.algoHealth({ status: healthFilter.status, page: 1, pageSize: 20, page_size: 20 })
  algoHealth.value = data?.items || []
}

async function setupRealtime() {
  try {
    realtimeConnection = createDashboardConnection(() => loadDashboard())
    realtimeConnection.onreconnected(() => {
      realtimeConnected.value = true
      loadDashboard()
    })
    realtimeConnection.onclose(() => {
      realtimeConnected.value = false
    })
    await realtimeConnection.start()
    realtimeConnected.value = true
  } catch {
    realtimeConnected.value = false
  }
}

onMounted(() => {
  loadDashboard()
  setupRealtime()
  refreshTimer = window.setInterval(loadDashboard, 30000)
})

onBeforeUnmount(() => {
  if (refreshTimer) window.clearInterval(refreshTimer)
  if (realtimeConnection) realtimeConnection.stop()
})
</script>

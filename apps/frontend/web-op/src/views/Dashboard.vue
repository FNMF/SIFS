<template>
  <div class="op-shell">
    <aside class="op-sidebar">
      <div class="op-brand">
        <div class="op-brand__mark">SIFS</div>
        <div>
          <strong>管理控制台</strong>
          <span>Satellite Image Forensics</span>
        </div>
      </div>
      <nav class="op-nav">
        <button class="op-nav__item is-active" type="button">
          <Monitor />
          <span>看板总览</span>
        </button>
        <button class="op-nav__item" type="button">
          <Cpu />
          <span>算法健康</span>
        </button>
      </nav>
    </aside>

    <main class="op-main">
      <header class="op-topbar">
        <div>
          <h1>运营看板</h1>
          <p>任务、算法健康与最近操作的集中监控视图</p>
        </div>
        <div class="op-topbar__actions">
          <span :class="['signal-badge', realtimeConnected ? 'is-online' : '']">
            {{ realtimeConnected ? '实时已连接' : '轮询刷新' }}
          </span>
          <el-button :icon="Refresh" :loading="loading" @click="loadDashboard">刷新</el-button>
          <el-button :icon="SwitchButton" @click="logout">退出</el-button>
        </div>
      </header>

      <el-alert
        v-if="errorMessage"
        class="op-alert"
        type="warning"
        show-icon
        :closable="false"
        :title="errorMessage"
      />

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
          <div class="panel-header">
            <h2>任务状态分布</h2>
          </div>
          <SimpleBarChart :items="taskStatusItems" value-key="count" label-key="status" />
        </div>

        <div class="op-panel">
          <div class="panel-header">
            <h2>算法健康分布</h2>
          </div>
          <SimpleBarChart :items="algoHealthChartItems" value-key="count" label-key="status" />
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
            <template #default="{ row }">
              <span class="status-cell">
                <i :class="['status-dot', `is-${row.health_status}`]" />
                {{ statusText(row.health_status) }}
              </span>
            </template>
          </el-table-column>
          <el-table-column prop="name" label="算法名称" min-width="140" />
          <el-table-column label="启用" width="90">
            <template #default="{ row }">
              <el-tag :type="row.enabled ? 'success' : 'info'" effect="plain">
                {{ row.enabled ? '启用' : '停用' }}
              </el-tag>
            </template>
          </el-table-column>
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
    </main>
  </div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  Cpu,
  Finished,
  Monitor,
  Refresh,
  SwitchButton,
  Timer,
  Warning,
  CircleCheck,
  Files,
  TrendCharts
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import {
  getAlgoHealth,
  getAlgoStatusCount,
  getDashboardSummary,
  getRecentLogs,
  getRecentTasks,
  getTaskStatusCount
} from '../services/dashboard'
import { createDashboardConnection } from '../services/realtime'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()
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

const summaryCards = computed(() => [
  { key: 'today', label: '今日任务数', value: summary.value.today_task_count ?? 0, icon: Timer, tone: 'tone-blue' },
  { key: 'total', label: '总任务数', value: summary.value.total_task_count ?? 0, icon: Files, tone: 'tone-slate' },
  { key: 'running', label: '运行中任务数', value: summary.value.running_task_count ?? 0, icon: TrendCharts, tone: 'tone-cyan' },
  { key: 'waiting', label: '等待中任务数', value: summary.value.waiting_task_count ?? 0, icon: Finished, tone: 'tone-amber' },
  { key: 'failed', label: '失败任务数', value: summary.value.failed_task_count ?? 0, icon: Warning, tone: 'tone-red' },
  { key: 'success', label: '成功任务数', value: summary.value.success_task_count ?? 0, icon: CircleCheck, tone: 'tone-green' },
  { key: 'algoTotal', label: '算法总数', value: summary.value.algo_total_count ?? 0, icon: Cpu, tone: 'tone-slate' },
  { key: 'algoEnabled', label: '启用算法数', value: summary.value.algo_enabled_count ?? 0, icon: Cpu, tone: 'tone-blue' },
  { key: 'algoOnline', label: '在线算法数', value: summary.value.algo_online_count ?? algoStatus.value.online ?? 0, icon: CircleCheck, tone: 'tone-green' },
  { key: 'algoOffline', label: '离线算法数', value: summary.value.algo_offline_count ?? algoStatus.value.offline ?? 0, icon: Warning, tone: 'tone-red' },
  { key: 'algoTimeout', label: '超时算法数', value: summary.value.algo_timeout_count ?? algoStatus.value.timeout ?? 0, icon: Timer, tone: 'tone-amber' }
])

const taskStatusItems = computed(() => taskStatus.value.items || [])
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
      getDashboardSummary(),
      getTaskStatusCount(),
      getAlgoStatusCount(),
      getRecentTasks({ limit: 8 }),
      getRecentTasks({ limit: 8, failed_only: true }),
      getRecentLogs({ limit: 8 })
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
  const data = await getAlgoHealth({
    status: healthFilter.status,
    page: 1,
    page_size: 20
  })
  algoHealth.value = data?.items || []
}

function statusText(status) {
  return {
    online: '在线',
    offline: '离线',
    timeout: '超时',
    disabled: '停用'
  }[status] || '未知'
}

function formatTime(value) {
  return value ? new Date(value).toLocaleString() : '-'
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

function logout() {
  authStore.clearAuth()
  ElMessage.success('已退出')
  router.push('/login')
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

<script>
export default {
  components: {
    SimpleBarChart: {
      props: {
        items: { type: Array, default: () => [] },
        labelKey: { type: String, required: true },
        valueKey: { type: String, required: true }
      },
      computed: {
        maxValue() {
          return Math.max(1, ...this.items.map((item) => Number(item[this.valueKey]) || 0))
        }
      },
      template: `
        <div class="simple-chart">
          <div v-if="!items.length" class="empty-inline">暂无统计数据</div>
          <div v-for="item in items" :key="item[labelKey]" class="chart-row">
            <span class="chart-row__label">{{ item[labelKey] }}</span>
            <div class="chart-row__track">
              <i :style="{ width: ((Number(item[valueKey]) || 0) / maxValue * 100) + '%' }"></i>
            </div>
            <strong>{{ item[valueKey] || 0 }}</strong>
          </div>
        </div>
      `
    },
    RecentList: {
      props: {
        title: { type: String, required: true },
        items: { type: Array, default: () => [] },
        type: { type: String, default: 'task' }
      },
      methods: {
        line(item) {
          if (this.type === 'log') {
            return `${item.operation_type || '-'} · ${item.actor_username || 'system'}`
          }
          return `${item.algorithm_name || '-'} · ${item.status || '-'}`
        },
        sub(item) {
          if (this.type === 'log') {
            return item.request_path || item.failure_reason || '-'
          }
          return item.failure_reason || item.task_id || '-'
        },
        time(item) {
          const value = item.created_at || item.finished_at
          return value ? new Date(value).toLocaleString() : '-'
        }
      },
      template: `
        <div class="op-panel">
          <div class="panel-header"><h2>{{ title }}</h2></div>
          <div v-if="!items.length" class="empty-inline">暂无数据</div>
          <ul v-else class="recent-list">
            <li v-for="item in items" :key="item.id || item.task_id">
              <div>
                <strong>{{ line(item) }}</strong>
                <span>{{ sub(item) }}</span>
              </div>
              <time>{{ time(item) }}</time>
            </li>
          </ul>
        </div>
      `
    }
  }
}
</script>

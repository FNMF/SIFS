<template>
  <AdminLayout title="任务管理" subtitle="查看所有任务、状态时间线并执行取消、重试、删除操作">
    <template #actions>
      <el-button :icon="Refresh" :loading="loading" @click="loadTasks">刷新</el-button>
    </template>

    <section class="op-panel">
      <div class="filter-row filter-row--wide">
        <el-input v-model="query.keyword" placeholder="关键词" clearable @keyup.enter="loadTasks" />
        <el-input v-model="query.user_id" placeholder="用户 ID" clearable />
        <el-input v-model="query.algorithm_name" placeholder="算法名称" clearable />
        <el-select v-model="query.status" clearable placeholder="状态">
          <el-option label="运行中" value="1" />
          <el-option label="成功" value="2" />
          <el-option label="失败" value="3" />
          <el-option label="已取消" value="4" />
        </el-select>
        <el-select v-model="query.failed" clearable placeholder="失败筛选">
          <el-option label="仅失败" :value="true" />
          <el-option label="非失败" :value="false" />
        </el-select>
        <el-date-picker v-model="timeRange" type="datetimerange" start-placeholder="开始时间" end-placeholder="结束时间" />
        <el-button type="primary" @click="loadTasks">查询</el-button>
      </div>

      <el-table v-loading="loading" :data="items" border empty-text="暂无任务数据">
        <el-table-column prop="taskId" label="任务 ID" min-width="230" show-overflow-tooltip />
        <el-table-column prop="createdByUsername" label="用户" min-width="130" />
        <el-table-column prop="algorithmName" label="算法" min-width="150" />
        <el-table-column label="状态" width="120">
          <template #default="{ row }"><StatusTag :status="row.currentStatus" /></template>
        </el-table-column>
        <el-table-column label="子任务" width="110">
          <template #default="{ row }">{{ row.completedSubTaskCount }}/{{ row.subTaskCount }}</template>
        </el-table-column>
        <el-table-column label="创建时间" min-width="170">
          <template #default="{ row }">{{ formatTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column prop="failureReason" label="失败原因" min-width="200" show-overflow-tooltip />
        <el-table-column label="操作" width="260" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openDetail(row.taskId)">详情</el-button>
            <el-button size="small" :loading="operatingId === row.taskId" @click="operate('cancel', row)">取消</el-button>
            <el-button size="small" type="primary" :loading="operatingId === row.taskId" @click="operate('retry', row)">重试</el-button>
            <el-button size="small" type="danger" :loading="operatingId === row.taskId" @click="operate('delete', row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        class="table-pagination"
        layout="total, sizes, prev, pager, next"
        :total="total"
        :current-page="query.page"
        :page-size="query.page_size"
        @current-change="(page) => { query.page = page; loadTasks() }"
        @size-change="(size) => { query.page_size = size; query.page = 1; loadTasks() }"
      />
    </section>

    <el-drawer v-model="detailVisible" title="任务详情" size="720px">
      <div v-loading="detailLoading" class="detail-stack">
        <el-descriptions v-if="detail" :column="1" border>
          <el-descriptions-item label="任务 ID">{{ detail.taskId }}</el-descriptions-item>
          <el-descriptions-item label="用户">{{ detail.createdByUsername || detail.createdByUserId }}</el-descriptions-item>
          <el-descriptions-item label="算法">{{ detail.algorithmName || '-' }}</el-descriptions-item>
          <el-descriptions-item label="当前状态"><StatusTag :status="detail.currentStatus" /></el-descriptions-item>
          <el-descriptions-item label="原始图片">{{ detail.originalImagePath || '-' }}</el-descriptions-item>
          <el-descriptions-item label="结果路径">{{ detail.resultPath || '-' }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatTime(detail.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="开始时间">{{ formatTime(detail.startedAt) }}</el-descriptions-item>
          <el-descriptions-item label="完成时间">{{ formatTime(detail.finishedAt) }}</el-descriptions-item>
          <el-descriptions-item label="耗时">{{ formatDuration(detail.duration) }}</el-descriptions-item>
          <el-descriptions-item label="失败原因">{{ detail.failureReason || '-' }}</el-descriptions-item>
        </el-descriptions>

        <div class="op-panel op-panel--inner">
          <div class="panel-header"><h2>状态时间线</h2></div>
          <el-timeline v-if="timeline.length">
            <el-timeline-item v-for="item in timeline" :key="`${item.createdAt}-${item.toStatus}`" :timestamp="formatTime(item.createdAt)">
              <strong>{{ item.fromStatus || '初始' }} → {{ item.toStatus }}</strong>
              <p>{{ item.reason || '-' }}</p>
            </el-timeline-item>
          </el-timeline>
          <div v-else class="empty-inline">暂无状态记录</div>
        </div>

        <div class="op-panel op-panel--inner">
          <div class="panel-header"><h2>子任务</h2></div>
          <el-table :data="detail?.subTasks || []" border empty-text="暂无子任务">
            <el-table-column prop="algorithmName" label="算法" />
            <el-table-column prop="statusText" label="状态" width="120" />
            <el-table-column prop="failureReason" label="失败原因" show-overflow-tooltip />
          </el-table>
        </div>
      </div>
    </el-drawer>
  </AdminLayout>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Refresh } from '@element-plus/icons-vue'
import AdminLayout from '../layouts/AdminLayout.vue'
import StatusTag from '../components/StatusTag.vue'
import { taskApi } from '../services/admin'
import { formatDuration, formatTime } from '../utils/format'

const loading = ref(false)
const detailLoading = ref(false)
const detailVisible = ref(false)
const operatingId = ref(null)
const items = ref([])
const total = ref(0)
const detail = ref(null)
const timeline = ref([])
const timeRange = ref([])
const query = reactive({
  keyword: '',
  user_id: '',
  algorithm_name: '',
  status: '',
  failed: '',
  page: 1,
  page_size: 20
})

function apiParams() {
  return {
    userId: query.user_id,
    algorithmName: query.algorithm_name,
    status: query.status,
    failed: query.failed,
    keyword: query.keyword,
    startTime: timeRange.value?.[0]?.toISOString?.(),
    endTime: timeRange.value?.[1]?.toISOString?.(),
    page: query.page,
    pageSize: query.page_size,
    page_size: query.page_size
  }
}

async function loadTasks() {
  loading.value = true
  try {
    const data = await taskApi.list(apiParams())
    items.value = data.items || []
    total.value = data.total || 0
  } finally {
    loading.value = false
  }
}

async function openDetail(taskId) {
  detailVisible.value = true
  detailLoading.value = true
  try {
    const [detailData, flowData] = await Promise.all([taskApi.detail(taskId), taskApi.statusFlow(taskId)])
    detail.value = detailData
    timeline.value = flowData || detailData?.statusTimeline || []
  } finally {
    detailLoading.value = false
  }
}

async function operate(type, row) {
  const messages = {
    cancel: ['确认取消该任务？', '任务已取消'],
    retry: ['确认重试该任务？', '任务已重试'],
    delete: ['确认删除该任务？', '任务已删除']
  }
  await ElMessageBox.confirm(messages[type][0], '确认操作', { type: type === 'delete' ? 'warning' : 'info' })

  operatingId.value = row.taskId
  try {
    if (type === 'cancel') await taskApi.cancel(row.taskId)
    if (type === 'retry') await taskApi.retry(row.taskId)
    if (type === 'delete') await taskApi.remove(row.taskId)
    ElMessage.success(messages[type][1])
    await loadTasks()
    if (detail.value?.taskId === row.taskId) await openDetail(row.taskId)
  } finally {
    operatingId.value = null
  }
}

onMounted(loadTasks)
</script>

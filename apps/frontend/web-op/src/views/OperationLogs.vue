<template>
  <AdminLayout title="操作日志" subtitle="查询管理端和关键业务操作记录">
    <template #actions>
      <el-button :icon="Refresh" :loading="loading" @click="loadLogs">刷新</el-button>
    </template>

    <section class="op-panel">
      <div class="filter-row filter-row--wide">
        <el-input v-model="query.actor_username" placeholder="操作人" clearable />
        <el-input v-model="query.operation_type" placeholder="操作类型" clearable />
        <el-input v-model="query.target_type" placeholder="目标类型" clearable />
        <el-select v-model="query.success" clearable placeholder="结果">
          <el-option label="成功" :value="true" />
          <el-option label="失败" :value="false" />
        </el-select>
        <el-date-picker
          v-model="timeRange"
          type="datetimerange"
          start-placeholder="开始时间"
          end-placeholder="结束时间"
        />
        <el-button
          class="operation-log-query-button"
          type="primary"
          :icon="Search"
          @click="loadLogs"
        >
          查询
        </el-button>
      </div>

      <el-table v-loading="loading" :data="items" border empty-text="暂无日志数据">
        <el-table-column label="时间" min-width="170">
          <template #default="{ row }">{{ formatTime(row.createdAt || row.created_at) }}</template>
        </el-table-column>
        <el-table-column prop="actorUsername" label="操作人" min-width="120">
          <template #default="{ row }">{{ row.actorUsername || row.actor_username || '-' }}</template>
        </el-table-column>
        <el-table-column prop="operationType" label="操作类型" min-width="170">
          <template #default="{ row }">{{ row.operationType || row.operation_type }}</template>
        </el-table-column>
        <el-table-column prop="targetType" label="目标类型" min-width="120">
          <template #default="{ row }">{{ row.targetType || row.target_type || '-' }}</template>
        </el-table-column>
        <el-table-column prop="targetId" label="目标 ID" min-width="160" show-overflow-tooltip>
          <template #default="{ row }">{{ row.targetId || row.target_id || '-' }}</template>
        </el-table-column>
        <el-table-column label="方法" width="90">
          <template #default="{ row }">{{ row.requestMethod || row.request_method || '-' }}</template>
        </el-table-column>
        <el-table-column label="路径" min-width="220" show-overflow-tooltip>
          <template #default="{ row }">{{ row.requestPath || row.request_path || '-' }}</template>
        </el-table-column>
        <el-table-column label="结果" width="90">
          <template #default="{ row }">
            <el-tag :type="(row.success ?? row.Success) ? 'success' : 'danger'" effect="plain">
              {{ (row.success ?? row.Success) ? '成功' : '失败' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="失败原因" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">{{ row.failureReason || row.failure_reason || '-' }}</template>
        </el-table-column>
      </el-table>

      <el-pagination
        class="table-pagination"
        layout="total, sizes, prev, pager, next"
        :total="total"
        :current-page="query.page"
        :page-size="query.page_size"
        @current-change="(page) => { query.page = page; loadLogs() }"
        @size-change="(size) => { query.page_size = size; query.page = 1; loadLogs() }"
      />
    </section>
  </AdminLayout>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { Refresh, Search } from '@element-plus/icons-vue'
import AdminLayout from '../layouts/AdminLayout.vue'
import { operationLogApi } from '../services/admin'
import { formatTime } from '../utils/format'

const loading = ref(false)
const items = ref([])
const total = ref(0)
const timeRange = ref([])
const query = reactive({
  actor_username: '',
  operation_type: '',
  target_type: '',
  success: '',
  page: 1,
  page_size: 20
})

function params() {
  return {
    actorUsername: query.actor_username,
    operationType: query.operation_type,
    targetType: query.target_type,
    success: query.success,
    startTime: timeRange.value?.[0]?.toISOString?.(),
    endTime: timeRange.value?.[1]?.toISOString?.(),
    page: query.page,
    pageSize: query.page_size,
    page_size: query.page_size
  }
}

async function loadLogs() {
  loading.value = true
  try {
    const data = await operationLogApi.list(params())
    items.value = data.items || []
    total.value = data.total || 0
  } finally {
    loading.value = false
  }
}

onMounted(loadLogs)
</script>

<style scoped>
.operation-log-query-button {
  min-width: 88px;
  flex-shrink: 0;
  justify-content: center;
}
</style>

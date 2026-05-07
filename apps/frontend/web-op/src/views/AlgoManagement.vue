<template>
  <AdminLayout title="算法管理" subtitle="维护算法 API 地址、启用状态和健康结果">
    <template #actions>
      <el-button :icon="Refresh" :loading="loading" @click="loadData">刷新</el-button>
      <el-button type="primary" @click="openCreate">新增算法</el-button>
    </template>

    <section class="op-panel">
      <div class="filter-row">
        <el-input v-model="query.name" placeholder="算法名称" clearable @keyup.enter="loadData" />
        <el-select v-model="query.enabled" clearable placeholder="启用状态">
          <el-option label="启用" :value="true" />
          <el-option label="停用" :value="false" />
        </el-select>
        <el-button type="primary" :loading="loading" @click="loadData">查询</el-button>
      </div>

      <el-table v-loading="loading" :data="items" border empty-text="暂无算法数据">
        <el-table-column prop="name" label="名称" min-width="140" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.enabled ? 'success' : 'info'" effect="plain">{{ row.enabled ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="健康" width="120">
          <template #default="{ row }"><StatusTag :status="healthMap[row.id]?.health_status || (row.enabled ? 'offline' : 'disabled')" /></template>
        </el-table-column>
        <el-table-column prop="apiUrl" label="API 地址" min-width="280" show-overflow-tooltip />
        <el-table-column label="响应 ms" width="100">
          <template #default="{ row }">{{ healthMap[row.id]?.response_time_ms ?? '-' }}</template>
        </el-table-column>
        <el-table-column label="检查时间" min-width="170">
          <template #default="{ row }">{{ formatTime(healthMap[row.id]?.checked_at) }}</template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="180" show-overflow-tooltip />
        <el-table-column label="操作" width="250" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openEdit(row)">编辑</el-button>
            <el-button
              size="small"
              :type="row.enabled ? 'warning' : 'success'"
              :loading="operatingId === row.id"
              @click="toggleEnabled(row)"
            >
              {{ row.enabled ? '停用' : '启用' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        class="table-pagination"
        layout="total, sizes, prev, pager, next"
        :total="total"
        :current-page="query.page"
        :page-size="query.page_size"
        @current-change="(page) => { query.page = page; loadData() }"
        @size-change="(size) => { query.page_size = size; query.page = 1; loadData() }"
      />
    </section>

    <el-dialog v-model="dialogVisible" :title="editingId ? '编辑算法' : '新增算法'" width="620px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="110px">
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="API 地址" prop="api_url">
          <el-input v-model="form.api_url" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item label="扩展 JSON">
          <el-input v-model="form.reserved_json_text" type="textarea" :rows="4" placeholder='例如 {"params":{}}' />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="saveAlgo">保存</el-button>
      </template>
    </el-dialog>
  </AdminLayout>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { Refresh } from '@element-plus/icons-vue'
import AdminLayout from '../layouts/AdminLayout.vue'
import StatusTag from '../components/StatusTag.vue'
import { algoApi, dashboardApi } from '../services/admin'
import { formatTime, tryParseJson } from '../utils/format'

const loading = ref(false)
const saving = ref(false)
const operatingId = ref(null)
const dialogVisible = ref(false)
const editingId = ref(null)
const formRef = ref(null)
const items = ref([])
const total = ref(0)
const healthItems = ref([])
const query = reactive({ name: '', enabled: '', page: 1, page_size: 20 })
const form = reactive({ name: '', api_url: '', description: '', reserved_json_text: '' })

const rules = {
  name: [{ required: true, message: '请输入算法名称', trigger: 'blur' }],
  api_url: [{ required: true, message: '请输入 API 地址', trigger: 'blur' }]
}

const healthMap = computed(() => Object.fromEntries(healthItems.value.map((item) => [item.algo_model_id, item])))

async function loadData() {
  loading.value = true
  try {
    const params = {
      name: query.name,
      enabled: query.enabled,
      page: query.page,
      pageSize: query.page_size,
      page_size: query.page_size
    }
    const [algoData, healthData] = await Promise.all([
      algoApi.list(params),
      dashboardApi.algoHealth({ page: 1, page_size: 100 })
    ])
    items.value = (algoData.items || []).map((item) => ({
      ...item,
      apiUrl: item.apiUrl || item.api_url,
      reservedJson: item.reservedJson || item.reserved_json
    }))
    total.value = algoData.total || 0
    healthItems.value = healthData.items || []
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editingId.value = null
  Object.assign(form, { name: '', api_url: '', description: '', reserved_json_text: '' })
  dialogVisible.value = true
}

function openEdit(row) {
  editingId.value = row.id
  Object.assign(form, {
    name: row.name || '',
    api_url: row.apiUrl || '',
    description: row.description || '',
    reserved_json_text: row.reservedJson || ''
  })
  dialogVisible.value = true
}

async function saveAlgo() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  const reservedJson = form.reserved_json_text.trim() ? tryParseJson(form.reserved_json_text, undefined) : null
  if (reservedJson === undefined) {
    ElMessage.error('扩展 JSON 格式不正确')
    return
  }

  saving.value = true
  try {
    const payload = {
      name: form.name.trim(),
      Name: form.name.trim(),
      api_url: form.api_url.trim(),
      ApiUrl: form.api_url.trim(),
      description: form.description,
      Description: form.description,
      reserved_json: reservedJson,
      ReservedJson: reservedJson
    }
    if (editingId.value) {
      await algoApi.update(editingId.value, payload)
      ElMessage.success('算法已更新')
    } else {
      await algoApi.create(payload)
      ElMessage.success('算法已创建')
    }
    dialogVisible.value = false
    await loadData()
  } finally {
    saving.value = false
  }
}

async function toggleEnabled(row) {
  operatingId.value = row.id
  try {
    if (row.enabled) {
      await algoApi.disable(row.id)
      ElMessage.success('算法已停用')
    } else {
      await algoApi.enable(row.id)
      ElMessage.success('算法已启用')
    }
    await loadData()
  } finally {
    operatingId.value = null
  }
}

onMounted(loadData)
</script>

<template>
  <AdminLayout title="用户角色" subtitle="搜索用户、分配角色并查看最终权限集合">
    <template #actions>
      <el-button :icon="Refresh" :loading="loading || detailLoading" @click="refreshAll">刷新</el-button>
    </template>

    <section class="op-panel">
      <div class="user-role-search">
        <el-input
          v-model="query.keyword"
          placeholder="搜索账号或用户 ID"
          clearable
          @keyup.enter="loadUsers"
        />
        <el-button type="primary" :loading="loading" @click="loadUsers">查询</el-button>
      </div>

      <el-table
        v-loading="loading"
        :data="users"
        border
        empty-text="暂无用户数据"
        highlight-current-row
        @current-change="selectUser"
      >
        <el-table-column prop="account" label="账号" min-width="160" />
        <el-table-column prop="id" label="用户 ID" min-width="260" show-overflow-tooltip />
        <el-table-column label="操作" width="100">
          <template #default="{ row }">
            <el-button size="small" type="primary" plain @click.stop="selectUser(row)">选择</el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        class="table-pagination"
        layout="total, sizes, prev, pager, next"
        :total="total"
        :current-page="query.page"
        :page-size="query.page_size"
        @current-change="(page) => { query.page = page; loadUsers() }"
        @size-change="(size) => { query.page_size = size; query.page = 1; loadUsers() }"
      />
    </section>

    <section class="op-panel">
      <div class="panel-header">
        <h2>角色分配</h2>
        <span v-if="selectedUser">{{ selectedUser.account }} / {{ selectedUser.id }}</span>
      </div>

      <div v-if="!selectedUser" class="empty-inline">请先从上方选择一个用户</div>
      <div v-else v-loading="detailLoading" class="user-role-detail">
        <el-checkbox-group v-model="selectedRoles" class="role-checkbox-grid">
          <el-checkbox
            v-for="role in roles"
            :key="role.name"
            :label="role.name"
            border
          >
            <strong>{{ role.name }}</strong>
            <span>{{ role.description || '暂无描述' }}</span>
          </el-checkbox>
        </el-checkbox-group>

        <div class="user-role-actions">
          <el-button
            type="primary"
            :loading="saving"
            :disabled="!selectedUser"
            @click="saveRoles"
          >
            保存角色
          </el-button>
        </div>
      </div>
    </section>

    <section class="op-panel">
      <div class="panel-header">
        <h2>用户权限</h2>
        <span v-if="selectedUser">由当前角色实时解析</span>
      </div>
      <div v-if="!selectedUser" class="empty-inline">选择用户后可查看权限集合</div>
      <div v-else-if="!permissions.length" class="empty-inline">暂无权限数据</div>
      <div v-else class="permission-grid">
        <el-tag v-for="permission in permissions" :key="permission" effect="plain">{{ permission }}</el-tag>
      </div>
    </section>
  </AdminLayout>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { Refresh } from '@element-plus/icons-vue'
import AdminLayout from '../layouts/AdminLayout.vue'
import { rbacApi } from '../services/admin'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const loading = ref(false)
const detailLoading = ref(false)
const saving = ref(false)
const roles = ref([])
const users = ref([])
const total = ref(0)
const selectedUser = ref(null)
const selectedRoles = ref([])
const permissions = ref([])
const query = reactive({
  keyword: '',
  page: 1,
  page_size: 20
})

function getCurrentUserId() {
  const user = authStore.state.userInfo || {}
  return user.id || user.Id || user.guid || user.Guid || ''
}

async function loadRoles() {
  roles.value = await rbacApi.roles()
}

async function loadUsers() {
  loading.value = true
  try {
    const data = await rbacApi.users({
      keyword: query.keyword,
      page: query.page,
      pageSize: query.page_size,
      page_size: query.page_size
    })
    users.value = data.items || []
    total.value = data.total || 0
  } finally {
    loading.value = false
  }
}

async function loadSelectedUserRbac() {
  if (!selectedUser.value?.id) return
  detailLoading.value = true
  try {
    const [roleData, permissionData] = await Promise.all([
      rbacApi.userRoles(selectedUser.value.id),
      rbacApi.userPermissions(selectedUser.value.id)
    ])
    selectedRoles.value = roleData || []
    permissions.value = permissionData || []
  } finally {
    detailLoading.value = false
  }
}

async function selectUser(row) {
  if (!row?.id) return
  selectedUser.value = row
  await loadSelectedUserRbac()
}

async function saveRoles() {
  if (!selectedUser.value?.id) return
  saving.value = true
  try {
    const data = await rbacApi.setUserRoles(selectedUser.value.id, selectedRoles.value)
    selectedRoles.value = data.roles || []
    permissions.value = data.permissions || []
    if (String(selectedUser.value.id).toLowerCase() === String(getCurrentUserId()).toLowerCase()) {
      authStore.setRoles(selectedRoles.value)
      authStore.setPermissions(permissions.value)
    }
    ElMessage.success('用户角色已保存')
  } finally {
    saving.value = false
  }
}

async function refreshAll() {
  await Promise.all([loadRoles(), loadUsers()])
  if (selectedUser.value) {
    await loadSelectedUserRbac()
  }
}

onMounted(refreshAll)
</script>

<style scoped>
.user-role-search {
  display: grid;
  grid-template-columns: minmax(260px, 420px) auto;
  gap: 12px;
  align-items: center;
  justify-content: start;
  margin-bottom: 16px;
}

.user-role-detail {
  display: grid;
  gap: 16px;
}

.role-checkbox-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(220px, 1fr));
  gap: 12px;
}

.role-checkbox-grid :deep(.el-checkbox) {
  height: auto;
  margin: 0;
  padding: 12px 14px;
}

.role-checkbox-grid :deep(.el-checkbox__label) {
  display: grid;
  gap: 4px;
  line-height: 1.4;
}

.role-checkbox-grid :deep(.el-checkbox__label span) {
  color: var(--el-text-color-secondary);
  font-size: 12px;
}

.user-role-actions {
  display: flex;
  justify-content: flex-end;
}

@media (max-width: 900px) {
  .user-role-search,
  .role-checkbox-grid {
    grid-template-columns: 1fr;
  }
}
</style>

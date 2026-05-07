<template>
  <AdminLayout title="用户角色" subtitle="查看当前账号权限；用户角色管理 API 完成后可在此扩展分配功能">
    <template #actions>
      <el-button :icon="Refresh" :loading="loading" @click="loadPermissions">刷新权限</el-button>
    </template>

    <section class="op-panel">
      <div class="panel-header">
        <h2>当前账号权限</h2>
      </div>
      <el-alert
        class="op-alert"
        type="info"
        show-icon
        :closable="false"
        title="后端目前仅提供当前用户权限查询和 admin 角色检查，尚未提供用户列表、角色列表、角色分配/移除接口。此页不会模拟未实现的保存操作。"
      />
      <div v-loading="loading">
        <div v-if="!permissions.length" class="empty-inline">暂无权限数据</div>
        <div v-else class="permission-grid">
          <el-tag v-for="permission in permissions" :key="permission" effect="plain">{{ permission }}</el-tag>
        </div>
      </div>
    </section>

    <section class="op-panel">
      <div class="panel-header"><h2>待后端补充的接口</h2></div>
      <el-table :data="todos" border>
        <el-table-column prop="name" label="能力" />
        <el-table-column prop="endpoint" label="建议接口" />
      </el-table>
    </section>
  </AdminLayout>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { Refresh } from '@element-plus/icons-vue'
import AdminLayout from '../layouts/AdminLayout.vue'
import { rbacApi } from '../services/admin'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const loading = ref(false)
const permissions = ref([])

const todos = [
  { name: '角色列表', endpoint: 'GET /api/admin/rbac/roles' },
  { name: '用户搜索', endpoint: 'GET /api/admin/users' },
  { name: '查看用户角色', endpoint: 'GET /api/admin/rbac/users/{id}/roles' },
  { name: '分配用户角色', endpoint: 'POST /api/admin/rbac/users/{id}/roles' },
  { name: '查看用户权限', endpoint: 'GET /api/admin/rbac/users/{id}/permissions' }
]

async function loadPermissions() {
  loading.value = true
  try {
    permissions.value = await rbacApi.myPermissions()
    authStore.setPermissions(permissions.value)
  } finally {
    loading.value = false
  }
}

onMounted(loadPermissions)
</script>

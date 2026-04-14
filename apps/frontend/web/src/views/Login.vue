<template>
  <AuthLayout
    title="欢迎回来"
    description="登录后可进入系统上传卫星图片、查看识别结果，并使用 token 保持认证状态。"
    :feature-list="features"
  >
    <div class="auth-card">
      <div class="auth-card__header">
        <h2>登录账号</h2>
        <p>请输入你注册时使用的账号和密码。</p>
      </div>

      <el-form ref="formRef" :model="form" :rules="rules" size="large" @submit.prevent>
        <el-form-item prop="account">
          <el-input v-model="form.account" placeholder="请输入账号" clearable />
        </el-form-item>

        <el-form-item prop="password">
          <el-input
            v-model="form.password"
            type="password"
            placeholder="请输入密码"
            show-password
            clearable
            @keyup.enter="handleLogin"
          />
        </el-form-item>

        <div class="auth-card__meta">
          <span>认证状态将保存到本地，用于后续接口访问。</span>
        </div>

        <el-button type="primary" class="auth-submit" :loading="submitting" @click="handleLogin">
          立即登录
        </el-button>
      </el-form>

      <div class="auth-card__footer">
        <span>还没有账号？</span>
        <router-link to="/register">去注册</router-link>
      </div>
    </div>
  </AuthLayout>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import AuthLayout from '../layouts/AuthLayout.vue'
import { loginApi } from '../services/auth'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const route = useRoute()
const { setAuth } = useAuthStore()

const formRef = ref(null)
const submitting = ref(false)

const form = reactive({
  account: '',
  password: ''
})

const rules = {
  account: [{ required: true, message: '请输入账号', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

const features = [
  { title: 'Token 持久化', desc: '登录成功后自动保存 accessToken 和 refreshToken。' },
  { title: '接口认证基础完成', desc: '后续上传、结果、历史记录请求可直接复用认证头。' },
  { title: '业务数据空间预留', desc: '已为 mask 与参数结果预留本地状态存储结构。' }
]

const handleLogin = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  submitting.value = true
  try {
    const data = await loginApi({
      Account: form.account,
      Password: form.password
    })
    console.log('登录接口返回:', data)
    setAuth(data)
    console.log('accessToken ->', localStorage.getItem('sifs_access_token'))
    console.log('refreshToken ->', localStorage.getItem('sifs_refresh_token'))
    console.log('userInfo ->', localStorage.getItem('sifs_user_info'))
    ElMessage.success('登录成功')
    router.push(route.query.redirect || '/')
  } finally {
    submitting.value = false
  }
}
</script>

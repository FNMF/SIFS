<template>
  <main class="login-page">
    <section class="login-panel">
      <div class="login-panel__intro">
        <div class="op-brand__mark">SIFS</div>
        <h1>管理控制台</h1>
        <p>登录后查看任务运行状态、算法健康检查与最近操作日志。</p>
      </div>

      <el-form ref="formRef" :model="form" :rules="rules" size="large" class="login-form" @submit.prevent>
        <h2>账号登录</h2>
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
        <el-button type="primary" class="login-form__button" :loading="submitting" @click="handleLogin">
          登录
        </el-button>
      </el-form>
    </section>
  </main>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
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

async function handleLogin() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  submitting.value = true
  try {
    const data = await loginApi({
      Account: form.account,
      Password: form.password
    })
    setAuth(data)
    ElMessage.success('登录成功')
    router.push(route.query.redirect || '/dashboard')
  } finally {
    submitting.value = false
  }
}
</script>

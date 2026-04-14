<template>
  <AuthLayout
    title="创建账号"
    description="注册后即可获得系统访问身份，后续可直接进入上传页体验卫星图片识别流程。"
    :feature-list="features"
  >
    <div class="auth-card">
      <div class="auth-card__header">
        <h2>注册账号</h2>
        <p>仅需设置账号和密码，界面保持简洁，方便毕设展示。</p>
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
          />
        </el-form-item>

        <el-form-item prop="confirmPassword">
          <el-input
            v-model="form.confirmPassword"
            type="password"
            placeholder="请再次输入密码"
            show-password
            clearable
            @keyup.enter="handleRegister"
          />
        </el-form-item>

        <div class="auth-card__meta">
          <span>注册成功后将自动跳转到登录页。</span>
        </div>

        <el-button type="primary" class="auth-submit" :loading="submitting" @click="handleRegister">
          立即注册
        </el-button>
      </el-form>

      <div class="auth-card__footer">
        <span>已经有账号？</span>
        <router-link to="/login">去登录</router-link>
      </div>
    </div>
  </AuthLayout>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import AuthLayout from '../layouts/AuthLayout.vue'
import { registerApi } from '../services/auth'

const router = useRouter()
const formRef = ref(null)
const submitting = ref(false)

const form = reactive({
  account: '',
  password: '',
  confirmPassword: ''
})

const validateConfirmPassword = (_, value, callback) => {
  if (!value) {
    callback(new Error('请再次输入密码'))
    return
  }
  if (value !== form.password) {
    callback(new Error('两次输入的密码不一致'))
    return
  }
  callback()
}

const rules = {
  account: [
    { required: true, message: '请输入账号', trigger: 'blur' },
    { min: 4, message: '账号长度不能少于 4 位', trigger: 'blur' }
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码长度不能少于 6 位', trigger: 'blur' }
  ],
  confirmPassword: [{ validator: validateConfirmPassword, trigger: 'blur' }]
}

const features = [
  { title: '轻量注册流程', desc: '与后端 UserRegisterDto 字段严格保持一致。' },
  { title: '界面风格统一', desc: '与首页保持同一套视觉语言，答辩展示更完整。' },
  { title: '可继续扩展', desc: '后续可无缝追加邮箱、手机号、验证码等字段。' }
]

const handleRegister = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  submitting.value = true
  try {
    await registerApi({
      Account: form.account,
      Password: form.password
    })
    ElMessage.success('注册成功，请登录')
    router.push('/login')
  } finally {
    submitting.value = false
  }
}
</script>

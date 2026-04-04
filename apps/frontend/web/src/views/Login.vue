<template>
  <div>
    <h2>登录</h2>
    <input v-model="username" placeholder="用户名" />
    <input v-model="password" type="password" placeholder="密码" />
    <button @click="login">登录</button>

    <p @click="$router.push('/register')">去注册</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'

const username = ref('')
const password = ref('')
const router = useRouter()

const login = async () => {
  try {
    await axios.post('/api/login', {
      username: username.value,
      password: password.value
    })
    router.push('/home')
  } catch (e) {
    alert('登录失败')
  }
}
</script>
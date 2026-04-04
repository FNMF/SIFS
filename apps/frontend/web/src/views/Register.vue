<template>
  <div>
    <h2>注册</h2>
    <input v-model="username" placeholder="用户名" />
    <input v-model="password" type="password" placeholder="密码" />
    <button @click="register">注册</button>

    <p @click="$router.push('/login')">返回登录</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

const username = ref('')
const password = ref('')

const register = async () => {
  try {
    await axios.post('/api/register', {
      username: username.value,
      password: password.value
    })
    alert('注册成功')
  } catch (e) {
    alert('注册失败')
  }
}
</script>
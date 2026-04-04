<template>
  <div>
    <h2>历史记录</h2>

    <ul>
      <li v-for="item in list" :key="item.id">
        {{ item.filename }} - {{ item.result }}
      </li>
    </ul>

    <button @click="$router.push('/home')">返回</button>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const list = ref([])

onMounted(async () => {
  const res = await axios.get('/api/history')
  list.value = res.data
})
</script>
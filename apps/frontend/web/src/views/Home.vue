<template>
  <div>
    <h2>上传卫星图片</h2>

    <input type="file" multiple @change="handleFile" />

    <button @click="upload">上传</button>

    <p @click="$router.push('/history')">查看历史</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

const files = ref([])

const handleFile = (e) => {
  files.value = e.target.files
}

const upload = async () => {
  const formData = new FormData()

  for (let i = 0; i < files.value.length; i++) {
    formData.append('files', files.value[i])
  }

  await axios.post('/api/upload', formData)
  alert('上传成功')
}
</script>
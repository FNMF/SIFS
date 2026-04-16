<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import { getDetectionTaskListApi } from '../services/detectionTask'

const router = useRouter()
const loading = ref(false)
const taskList = ref([])

function normalizeTask(item) {
  return {
    guid: item.guid ?? item.Guid,
    subTaskCount: item.subTaskCount ?? item.SubTaskCount ?? 0,
    completedSubTaskCount: item.completedSubTaskCount ?? item.CompletedSubTaskCount ?? 0,
    completion: Number(item.completion ?? item.Completion ?? 0),
    url: item.url ?? item.Url ?? '',
    level: item.level ?? item.Level,
    updatedAt: item.updatedAt ?? item.UpdatedAt ?? ''
  }
}

const empty = computed(() => !loading.value && !taskList.value.length)

async function fetchTasks() {
  loading.value = true
  try {
    const data = await getDetectionTaskListApi()
    taskList.value = (data || []).map(normalizeTask)
  } finally {
    loading.value = false
  }
}

function goDetail(guid) {
  router.push(`/tasks/${guid}`)
}

onMounted(fetchTasks)
</script>

<template>
  <div class="app-page">
    <AppHeader variant="app" />

    <main class="container page-section">
      <section class="page-hero">
        <div>
          <h1>历史任务</h1>
          <p>查看已提交的卫星图片识别任务，了解完成进度，并进入详情页查看子任务结果。</p>
        </div>
        <el-button round @click="fetchTasks">刷新列表</el-button>
      </section>

      <section v-loading="loading" class="history-grid">
        <div v-if="empty" class="empty-card">
          <h3>暂时还没有历史任务</h3>
          <p>先去上传页创建你的第一个识别任务吧。</p>
          <el-button type="primary" @click="$router.push('/upload')">去上传</el-button>
        </div>

        <article v-for="task in taskList" :key="task.guid" class="task-card">
          <div class="task-card__preview">
            <img v-if="task.url" :src="task.url" alt="原图预览" />
            <div v-else class="task-card__placeholder">No Preview</div>
          </div>

          <div class="task-card__body">
            <div class="task-card__top">
              <div>
                <h3>{{ task.guid }}</h3>
                <p>最近更新时间：{{ task.updatedAt || '暂无' }}</p>
              </div>
              <el-tag :type="task.completion >= 1 ? 'success' : 'warning'">
                {{ Math.round(task.completion * 100) }}%
              </el-tag>
            </div>

            <div class="task-card__meta">
              <span>Level：{{ task.level ?? '未设置' }}</span>
              <span>子任务：{{ task.completedSubTaskCount }}/{{ task.subTaskCount }}</span>
            </div>

            <el-progress :percentage="Math.round(task.completion * 100)" :stroke-width="10" />

            <div class="task-card__actions">
              <el-button type="primary" round @click="goDetail(task.guid)">查看详情</el-button>
            </div>
          </div>
        </article>
      </section>
    </main>
  </div>
</template>
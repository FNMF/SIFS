<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from '../components/AppHeader.vue'
import { getDetectionTaskDetailApi } from '../services/detectionTask'

const route = useRoute()
const loading = ref(false)
const detail = ref(null)

function normalizeAlgo(item) {
  return {
    guid: item.guid ?? item.Guid,
    url: item.url ?? item.Url ?? item.maskUrl ?? item.MaskUrl ?? '',
    type: item.type ?? item.Type ?? item.name ?? item.Name ?? '未知算法',
    status: item.status ?? item.Status ?? 0,
    level: item.level ?? item.Level,
    updatedAt: item.updatedAt ?? item.UpdatedAt ?? ''
  }
}

function normalizeDetail(item) {
  return {
    guid: item.guid ?? item.Guid,
    imageUrls: item.imageUrls ?? item.ImageUrls ?? [],
    previewImageUrl: item.previewImageUrl ?? item.PreviewImageUrl ?? '',
    imageCount: item.imageCount ?? item.ImageCount ?? 0,
    subTaskCount: item.subTaskCount ?? item.SubTaskCount ?? 0,
    completedSubTaskCount: item.completedSubTaskCount ?? item.CompletedSubTaskCount ?? 0,
    completion: Number(item.completion ?? item.Completion ?? 0),
    level: item.level ?? item.Level,
    updatedAt: item.updatedAt ?? item.UpdatedAt ?? '',
    algoTasks: (item.algoTasks ?? item.AlgoTasks ?? []).map(normalizeAlgo)
  }
}

const task = computed(() => detail.value)

async function fetchDetail() {
  loading.value = true
  try {
    const data = await getDetectionTaskDetailApi(route.params.guid)
    detail.value = data ? normalizeDetail(data) : null
  } finally {
    loading.value = false
  }
}

function getStatusText(status) {
  if (status === 2) return '已完成'
  if (status === 1) return '处理中'
  if (status === 3) return '失败'
  return '排队中'
}

function getStatusType(status) {
  if (status === 2) return 'success'
  if (status === 1) return 'warning'
  if (status === 3) return 'danger'
  return 'info'
}

function openCompare(algo) {
  console.log('预留跳转 compare 页:', algo)
}

onMounted(fetchDetail)
</script>

<template>
  <div class="app-page">
    <AppHeader variant="app" />

    <main class="container page-section" v-loading="loading">
      <section class="page-hero">
        <div>
          <h1>任务详情</h1>
          <p>查看当前检测任务下各算法子任务的处理状态，并在完成后进入对比展示页面。</p>
        </div>
        <el-button round @click="fetchDetail">刷新状态</el-button>
      </section>

      <section v-if="task" class="detail-layout">
        <div class="detail-main">
          <div class="panel-card">
            <div class="panel-card__header">
              <h3>任务总览</h3>
              <span>{{ task.guid }}</span>
            </div>

            <div class="detail-summary">
              <div class="detail-summary__preview">
                <img v-if="task.previewImageUrl" :src="task.previewImageUrl" alt="原图预览" />
                <div v-else class="task-card__placeholder">No Preview</div>

                <div v-if="task.imageCount > 1" class="task-card__badge">
                  +{{ task.imageCount - 1 }}张
                </div>
              </div>

              <div class="detail-summary__info">
                <div class="summary-item"><span>Level</span><strong>{{ task.level ?? '未设置' }}</strong></div>
                <div class="summary-item"><span>完成度</span><strong>{{ Math.round(task.completion * 100) }}%</strong></div>
                <div class="summary-item"><span>子任务</span><strong>{{ task.completedSubTaskCount }}/{{ task.subTaskCount }}</strong></div>
                <div class="summary-item"><span>更新时间</span><strong>{{ task.updatedAt || '暂无' }}</strong></div>
              </div>
            </div>
          </div>

          <div class="panel-card">
            <div class="panel-card__header">
              <h3>算法子任务</h3>
              <span>任务完成后可进入对比展示</span>
            </div>

            <div v-if="task.algoTasks.length" class="algo-task-list">
              <div v-for="algo in task.algoTasks" :key="algo.guid" class="algo-task-item">
                <div class="algo-task-item__left">
                  <div class="algo-task-item__thumb">
                    <img v-if="algo.url" :src="algo.url" alt="算法图预览" />
                    <div v-else class="task-card__placeholder">No Img</div>
                  </div>

                  <div>
                    <h4>{{ algo.type || '未命名算法' }}</h4>
                    <p>{{ algo.guid }}</p>
                    <span>更新时间：{{ algo.updatedAt || '暂无' }}</span>
                  </div>
                </div>

                <div class="algo-task-item__right">
                  <el-tag :type="getStatusType(algo.status)">
                    {{ getStatusText(algo.status) }}
                  </el-tag>
                  <el-button
                    type="primary"
                    plain
                    round
                    :disabled="algo.status !== 2"
                    @click="openCompare(algo)"
                  >
                    查看对比
                  </el-button>
                </div>
              </div>
            </div>

            <div v-else class="empty-card">
              <h3>暂无子任务数据</h3>
              <p>当前任务详情接口尚未返回可展示的子任务内容。</p>
            </div>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>
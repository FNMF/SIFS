<script setup>
import { computed, onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import AppHeader from '../components/AppHeader.vue'
import { createDetectionTaskApi } from '../services/detectionTask'
import { getAlgoListApi } from '../services/algo'
import { useRouter } from 'vue-router'

const router = useRouter()

const fileList = ref([])
const algoOptions = ref([])
const selectedTypes = ref([])
const level = ref(null)
const loadingAlgo = ref(false)
const submitting = ref(false)

const totalSubTasks = computed(() => fileList.value.length * selectedTypes.value.length)

function normalizeAlgoList(list) {
  return (list || []).map(item => ({
    id: item.id ?? item.Id,
    name: item.name ?? item.Name
  }))
}

async function fetchAlgoList() {
  loadingAlgo.value = true
  try {
    const data = await getAlgoListApi()
    console.log('算法列表返回:', data)
    algoOptions.value = normalizeAlgoList(data)
  } catch (error) {
    console.error('获取算法列表失败:', error)
    ElMessage.error('获取算法列表失败，请检查登录状态或接口返回')
    algoOptions.value = []
  } finally {
    loadingAlgo.value = false
  }
}

function handleFileChange(uploadFile, uploadFiles) {
  fileList.value = uploadFiles
    .map((item, index) => ({
      uid: item.uid,
      name: item.name,
      raw: item.raw,
      url: item.url || (item.raw ? URL.createObjectURL(item.raw) : ''),
      order: index
    }))
    .filter(item => !!item.raw)
}

function handleRemove(file, uploadFiles) {
  handleFileChange(file, uploadFiles)
}

async function handleSubmit() {
  if (!fileList.value.length) {
    ElMessage.warning('请先选择至少一张图片')
    return
  }

  if (!selectedTypes.value.length) {
    ElMessage.warning('请至少选择一个算法')
    return
  }

  submitting.value = true
  try {
    const payload = {
      images: fileList.value.map((item, index) => ({
        order: index,
        file: item.raw
      })),
      types: selectedTypes.value,
      level: level.value
    }

    const data = await createDetectionTaskApi(payload)
    ElMessage.success('任务已创建，正在跳转历史页面')

    const guid = data?.guid ?? data?.Guid
    if (guid) {
      router.push(`/tasks/${guid}`)
    } else {
      router.push('/history')
    }
  } finally {
    submitting.value = false
  }
}

onMounted(fetchAlgoList)
</script>

<template>
  <div class="app-page">
    <AppHeader variant="app" />

    <main class="container page-section">
      <section class="page-hero">
        <div>
          <h1>新建识别任务</h1>
          <p>上传一张或多张卫星图片，选择算法后提交任务。系统将进入后端队列处理，并在完成后更新结果状态。</p>
        </div>
      </section>

      <section class="upload-layout">
        <div class="upload-main">
          <div class="panel-card">
            <div class="panel-card__header">
              <h3>上传图片</h3>
              <span>支持拖拽或点击添加，可一次上传多张</span>
            </div>

            <el-upload
              drag
              multiple
              :auto-upload="false"
              :show-file-list="false"
              :on-change="handleFileChange"
              :on-remove="handleRemove"
            >
              <div class="upload-dropzone__content">
                <div class="upload-dropzone__icon">+</div>
                <div class="upload-dropzone__title">拖入图片到这里，或点击添加</div>
                <div class="upload-dropzone__desc">建议上传清晰卫星影像，支持多图批量任务</div>
              </div>
            </el-upload>

            <div v-if="fileList.length" class="upload-preview-grid">
              <div v-for="file in fileList" :key="file.uid" class="upload-preview-card">
                <img :src="file.url" :alt="file.name" />
                <span>{{ file.name }}</span>
              </div>
            </div>
          </div>

          <div class="panel-card">
            <div class="panel-card__header">
              <h3>算法选择</h3>
              <span>可多选，系统会为每个算法生成一个子任务</span>
            </div>

            <el-checkbox-group v-model="selectedTypes" class="algo-grid">
              <el-checkbox
                v-for="algo in algoOptions"
                :key="algo.id"
                :label="algo.id"
                class="algo-chip"
              >
                {{ algo.name }}
              </el-checkbox>
            </el-checkbox-group>

            <div class="level-box">
              <div class="level-box__label">识别等级（可选）</div>
              <el-select v-model="level" clearable placeholder="请选择识别等级">
                <el-option label="Level 1 (速度最快,质量最低)" :value="0" />
                <el-option label="Level 2" :value="1" />
                <el-option label="Level 3" :value="2" />
                <el-option label="Level 4 (速度最慢,质量最高)" :value="3" />
              </el-select>
            </div>
          </div>
        </div>

        <aside class="upload-side">
          <div class="panel-card sticky-card">
            <div class="panel-card__header">
              <h3>任务摘要</h3>
              <span>提交前确认本次配置</span>
            </div>

            <div class="summary-list">
              <div class="summary-item">
                <span>图片数量</span>
                <strong>{{ fileList.length }}</strong>
              </div>
              <div class="summary-item">
                <span>算法数量</span>
                <strong>{{ selectedTypes.length }}</strong>
              </div>
              <div class="summary-item">
                <span>预计子任务</span>
                <strong>{{ totalSubTasks }}</strong>
              </div>
              <div class="summary-item">
                <span>识别等级</span>
                <strong>{{ level + 1 ?? '未设置' }}</strong>
              </div>
            </div>

            <el-button
              type="primary"
              class="full-btn"
              :loading="submitting"
              @click="handleSubmit"
            >
              提交识别任务
            </el-button>
          </div>
        </aside>
      </section>
    </main>
  </div>
</template>
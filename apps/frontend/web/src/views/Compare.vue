<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'

const route = useRoute()
const router = useRouter()

const baseCanvasRef = ref(null)
const overlayCanvasRef = ref(null)

const loading = ref(false)
const maskVisible = ref(true)
const compareHolding = ref(false)

const opacity = ref(0.55)
const maskColor = ref('#ff3b30')

const compareData = ref({
  originImageUrl: '',
  maskUrl: '',
  type: '',
  status: '',
  level: '',
  isFake: null,
  confidence: null
})

const originalMaskImageData = ref(null)
const canvasSize = ref({ width: 0, height: 0 })

const overlayStyle = computed(() => ({
  opacity: compareHolding.value || !maskVisible.value ? 0 : opacity.value
}))

function goBack() {
  router.push('/history')
}

function withNoCache(url) {
  if (!url) return ''
  const connector = url.includes('?') ? '&' : '?'
  return `${url}${connector}_t=${Date.now()}`
}

function normalizeNullableValue(value) {
  if (value === undefined || value === null || value === '') return null
  return value
}

function normalizeBoolean(value) {
  if (value === undefined || value === null || value === '') return null
  if (typeof value === 'boolean') return value
  const str = String(value).toLowerCase()
  if (str === 'true') return true
  if (str === 'false') return false
  return value
}

function normalizeNumber(value) {
  if (value === undefined || value === null || value === '') return null
  const num = Number(value)
  return Number.isNaN(num) ? value : num
}

function initData() {
  compareData.value = {
    originImageUrl: route.query.originImageUrl || '',
    maskUrl: route.query.maskUrl || '',
    type: route.query.type || '',
    status: route.query.status || '',
    level: route.query.level || '',
    isFake: normalizeBoolean(route.query.isFake),
    confidence: normalizeNumber(route.query.confidence)
  }
}

async function loadImage(src) {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.crossOrigin = 'anonymous'
    img.onload = () => resolve(img)
    img.onerror = (event) => reject(event)
    img.src = src
  })
}

async function initCanvases() {
  if (!compareData.value.originImageUrl || !compareData.value.maskUrl) {
    ElMessage.warning('缺少原图或 mask 地址，无法进行比对')
    return
  }

  const baseCanvas = baseCanvasRef.value
  const overlayCanvas = overlayCanvasRef.value
  if (!baseCanvas || !overlayCanvas) return

  loading.value = true
  try {
    const [originImg, maskImg] = await Promise.all([
      loadImage(withNoCache(compareData.value.originImageUrl)),
      loadImage(withNoCache(compareData.value.maskUrl))
    ])

    const width = originImg.naturalWidth
    const height = originImg.naturalHeight
    canvasSize.value = { width, height }

    baseCanvas.width = width
    baseCanvas.height = height
    overlayCanvas.width = width
    overlayCanvas.height = height

    const baseCtx = baseCanvas.getContext('2d')
    const overlayCtx = overlayCanvas.getContext('2d')

    baseCtx.clearRect(0, 0, width, height)
    baseCtx.drawImage(originImg, 0, 0, width, height)

    overlayCtx.clearRect(0, 0, width, height)
    overlayCtx.drawImage(maskImg, 0, 0, width, height)

    originalMaskImageData.value = overlayCtx.getImageData(0, 0, width, height)

    renderOverlay()
  } catch (error) {
    console.error(error)
    ElMessage.error('加载图像失败，请检查原图和 mask 地址是否可访问')
  } finally {
    loading.value = false
  }
}

function renderOverlay() {
  const overlayCanvas = overlayCanvasRef.value
  const sourceMask = originalMaskImageData.value
  if (!overlayCanvas || !sourceMask) return

  const overlayCtx = overlayCanvas.getContext('2d')
  const { width, height } = canvasSize.value

  const cloned = new ImageData(
    new Uint8ClampedArray(sourceMask.data),
    width,
    height
  )

  const data = cloned.data

  const hex = maskColor.value.replace('#', '')
  const r = parseInt(hex.slice(0, 2), 16)
  const g = parseInt(hex.slice(2, 4), 16)
  const b = parseInt(hex.slice(4, 6), 16)

  for (let i = 0; i < data.length; i += 4) {
    const red = data[i]
    const green = data[i + 1]
    const blue = data[i + 2]
    const alpha = data[i + 3]

    if (alpha === 0) continue

    const isWhite = red > 240 && green > 240 && blue > 240
    const isBlack = red < 30 && green < 30 && blue < 30

    if (isWhite) {
      data[i + 3] = 0
      continue
    }

    if (isBlack) {
      data[i] = r
      data[i + 1] = g
      data[i + 2] = b
      data[i + 3] = 255
      continue
    }

    const gray = (red + green + blue) / 3
    const intensity = 1 - gray / 255

    data[i] = r
    data[i + 1] = g
    data[i + 2] = b
    data[i + 3] = Math.round(255 * intensity)
  }

  overlayCtx.clearRect(0, 0, width, height)
  overlayCtx.putImageData(cloned, 0, 0)
}

function handleHoldStart() {
  compareHolding.value = true
}

function handleHoldEnd() {
  compareHolding.value = false
}

watch(maskColor, () => {
  requestAnimationFrame(() => {
    renderOverlay()
  })
})

onMounted(() => {
  initData()
  initCanvases()
})
</script>

<template>
  <div class="compare-page">
    <header class="compare-header">
      <div class="container compare-header__inner">
        <el-button round @click="goBack">返回历史</el-button>
        <div class="compare-header__title">结果比对</div>
      </div>
    </header>

    <main class="container compare-layout">
      <section class="compare-view-card" v-loading="loading">
        <div class="compare-view-card__header">
          <div>
            <h1>原图 / Mask 对比</h1>
            <p>黑色问题区域已转换为可配置的覆盖颜色，白色区域自动透明。</p>
          </div>
        </div>

        <div class="compare-stage">
          <canvas ref="baseCanvasRef" class="compare-canvas compare-canvas--base"></canvas>
          <canvas
            ref="overlayCanvasRef"
            class="compare-canvas compare-canvas--overlay"
            :style="overlayStyle"
          ></canvas>
        </div>
      </section>

      <aside class="compare-panel">
        <div class="compare-side-card">
          <div class="compare-side-card__header">
            <h3>显示设置</h3>
          </div>

          <div class="compare-control">
            <div class="compare-control__label">
              <span>Mask 透明度</span>
              <strong>{{ Math.round(opacity * 100) }}%</strong>
            </div>
            <el-slider v-model="opacity" :min="0" :max="1" :step="0.01" />
          </div>

          <div class="compare-control">
            <div class="compare-control__label">
              <span>Mask 颜色</span>
            </div>
            <input v-model="maskColor" type="color" class="color-picker" />
          </div>

          <div class="compare-control compare-control--row">
            <span>显示 Mask</span>
            <el-switch v-model="maskVisible" />
          </div>

          <div class="compare-control">
            <el-button
              type="primary"
              class="full-width-btn"
              @mousedown="handleHoldStart"
              @mouseup="handleHoldEnd"
              @mouseleave="handleHoldEnd"
              @touchstart.prevent="handleHoldStart"
              @touchend.prevent="handleHoldEnd"
            >
              按住查看原图
            </el-button>
          </div>
        </div>

        <div class="compare-side-card">
          <div class="compare-side-card__header">
            <h3>任务信息</h3>
          </div>

          <div class="summary-row" v-if="compareData.type">
            <span>算法类型</span>
            <strong>{{ compareData.type }}</strong>
          </div>

          <div class="summary-row" v-if="compareData.level !== ''">
            <span>Level</span>
            <strong>{{ compareData.level }}</strong>
          </div>

          <div class="summary-row" v-if="compareData.status !== ''">
            <span>状态</span>
            <strong>{{ compareData.status }}</strong>
          </div>
        </div>
      </aside>
    </main>

    <section class="container compare-metrics">
      <div class="compare-side-card">
        <div class="compare-side-card__header">
          <h3>分析参数</h3>
        </div>

        <div class="metrics-grid">
          <div class="metric-item" v-if="compareData.isFake !== null">
            <span>Is Fake</span>
            <strong>{{ String(compareData.isFake) }}</strong>
          </div>

          <div class="metric-item" v-if="compareData.confidence !== null">
            <span>Confidence</span>
            <strong>{{ compareData.confidence }}</strong>
          </div>
        </div>

        <div
          v-if="compareData.isFake === null && compareData.confidence === null"
          class="empty-tip"
        >
          当前后端未返回额外分析参数。
        </div>
      </div>
    </section>
  </div>
</template>
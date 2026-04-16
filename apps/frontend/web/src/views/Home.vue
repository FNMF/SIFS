<template>
  <div class="page">
    <AppHeader variant="landing" />
    <FloatingTryButton target="#workflow" />

    <section id="hero" class="hero section container">
      <div class="hero__content">
        <div class="section-tag">遥感识别 · 智能分割 · 可视化展示</div>
        <h1 class="hero__title">卫星图像识别结果更直观</h1>
        <p class="hero__desc">
          面向卫星图片识别场景，系统支持上传单张或多张卫星图片，调用后端完成模型处理，
          并返回 mask 图像地址与关键参数，帮助用户快速查看识别结果与分析信息。
        </p>

        <div class="hero__actions">
          <el-button type="primary" size="large" round @click="goTry">
              立即尝试
          </el-button>
          <el-button size="large" round @click="jumpTo('#about')">了解更多</el-button>
        </div>

        <div class="hero__stats">
          <div class="stat-card">
            <strong>多图上传</strong>
            <span>支持批量输入卫星影像</span>
          </div>
          <div class="stat-card">
            <strong>Mask 展示</strong>
            <span>识别结果可视化呈现</span>
          </div>
          <div class="stat-card">
            <strong>参数输出</strong>
            <span>返回结果参数便于分析</span>
          </div>
        </div>
      </div>

      <div class="hero__visual">
        <div class="preview-card preview-card--main">
          <div class="preview-card__head">
            <span class="dot dot--red"></span>
            <span class="dot dot--yellow"></span>
            <span class="dot dot--green"></span>
          </div>
          <div class="preview-card__body">
            <div class="satellite-image"></div>
            <div class="mask-layer"></div>
            <div class="preview-badge">Mask Overlay</div>
          </div>
        </div>

        <div class="preview-float preview-float--top">
          <span>目标识别完成</span>
          <strong>结果已生成</strong>
        </div>

        <div class="preview-float preview-float--bottom">
          <span>参数回传</span>
          <strong>面积 / 像素占比 / 分类信息</strong>
        </div>
      </div>
    </section>

    <section id="features" class="section section--soft">
      <div class="container">
        <div class="section-header">
          <div class="section-tag">系统能力</div>
          <h2>一个成熟系统应该告诉用户：它能做什么</h2>
        </div>

        <div class="feature-grid">
          <div class="feature-card" v-for="item in features" :key="item.title">
            <div class="feature-card__icon">{{ item.icon }}</div>
            <h3>{{ item.title }}</h3>
            <p>{{ item.desc }}</p>
          </div>
        </div>
      </div>
    </section>

    <section id="workflow" class="section container">
      <div class="section-header section-header--left">
        <div class="section-tag">处理流程</div>
        <h2>从上传图片到查看识别结果，只需要 3 步</h2>
      </div>

      <div class="timeline">
        <div class="timeline-item" v-for="(step, index) in steps" :key="step.title">
          <div class="timeline-item__index">0{{ index + 1 }}</div>
          <div class="timeline-item__content">
            <h3>{{ step.title }}</h3>
            <p>{{ step.desc }}</p>
          </div>
        </div>
      </div>
    </section>

    <section id="about" class="section section--dark">
      <div class="container about">
        <div class="about__left">
          <div class="section-tag section-tag--dark">项目介绍</div>
          <h2>卫星图片识别系统，不只是“上传图片”这么简单</h2>
          <p>
            该系统面向遥感图像分析场景，前端主要承担结果可视化与交互呈现任务。
          </p>
          <p>
            拥有上传页、任务结果页、历史记录页与登录注册模块，形成完整闭环。
          </p>
        </div>
        <div class="about__right">
          <div class="about-panel">
            <div class="about-panel__row">
              <span>系统定位</span>
              <strong>毕业设计 / 智能识别平台</strong>
            </div>
            <div class="about-panel__row">
              <span>前端技术</span>
              <strong>Vue3 + Vite + Element Plus</strong>
            </div>
            <div class="about-panel__row">
              <span>结果展示</span>
              <strong>Mask 图 + 参数信息</strong>
            </div>
            <div class="about-panel__row">
              <span>联系我们</span>
              <strong>2205050217</strong>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { computed } from 'vue'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()

const isLoggedIn = computed(() => authStore.isLoggedIn.value)

const router = useRouter()

function goTry() {
  if (isLoggedIn) {
    router.push('/upload')
  } else {
    router.push('/login')
  }
}

function goLogin() {
  router.push('/login')
}

function goRegister() {
  router.push('/register')
}

import AppHeader from '../components/AppHeader.vue'
import FloatingTryButton from '../components/FloatingTryButton.vue'

const jumpTo = (selector) => {
  document.querySelector(selector)?.scrollIntoView({ behavior: 'smooth' })
}

const features = [
  {
    icon: '🛰️',
    title: '多卫星图像输入',
    desc: '支持用户一次上传一张或多张卫星图片，为后续识别和批量分析提供入口。'
  },
  {
    icon: '🎯',
    title: '智能识别结果输出',
    desc: '前端对接后端推理服务，接收并展示识别后的 mask 图像 URL 与相关参数。'
  },
  {
    icon: '📊',
    title: '关键参数可视化',
    desc: '将识别过程中的关键参数转化为结构化展示内容，提升可读性与专业感。'
  },
  {
    icon: '⚡',
    title: '面向展示的界面设计',
    desc: '首页先采用产品官网化表达，增强毕业设计答辩中的完整度与观感。'
  }
]

const steps = [
  {
    title: '上传单张或多张卫星图片',
    desc: '用户选择本地图片后提交给后端接口，系统开始执行图像识别与处理任务。'
  },
  {
    title: '后端完成模型推理并返回结果',
    desc: '后端返回 mask 图像地址以及对应的参数信息，前端负责接收并组织展示。'
  },
  {
    title: '前端展示 mask 和分析参数',
    desc: '用户可直接查看识别后的图像结果与关键指标，为后续判断和分析提供支持。'
  }
]
</script>

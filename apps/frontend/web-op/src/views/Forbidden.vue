<template>
  <main class="state-page">
    <section class="state-card">
      <h1>403</h1>
      <p>当前账号没有管理端访问权限。</p>
      <el-button type="primary" @click="$router.push('/login')">重新登录</el-button>
    </section>
  </main>
</template>

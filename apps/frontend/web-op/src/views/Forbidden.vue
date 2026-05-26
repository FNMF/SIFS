<template>
  <main class="state-page">
    <section class="state-card">
      <h1>403</h1>
      <p>Admin access is not available for this account.</p>
      <el-button type="primary" @click="$router.push('/admin/login')">Login again</el-button>
    </section>
  </main>
</template>

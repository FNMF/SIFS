import { dashboardApi } from './admin'

export function getDashboardSummary() {
  return dashboardApi.summary()
}

export function getRecentTasks(params = {}) {
  return dashboardApi.recentTasks(params)
}

export function getRecentLogs(params = {}) {
  return dashboardApi.recentLogs(params)
}

export function getTaskStatusCount() {
  return dashboardApi.taskStatusCount()
}

export function getAlgoStatusCount() {
  return dashboardApi.algoStatusCount()
}

export function getAlgoHealth(params = {}) {
  return dashboardApi.algoHealth(params)
}

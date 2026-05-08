import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { ElNotification } from 'element-plus'
import { tokenStorage } from '../utils/storage'

let connection = null
let starting = false
const recentMessages = new Set()

function normalizePayload(payload = {}) {
  return {
    event: payload.event ?? payload.Event ?? 'algo_task.finished',
    taskId: payload.task_id ?? payload.taskId ?? payload.TaskId ?? '',
    algoTaskId: payload.algo_task_id ?? payload.algoTaskId ?? payload.AlgoTaskId ?? '',
    status: payload.status ?? payload.Status ?? '',
    statusText: payload.status_text ?? payload.statusText ?? payload.StatusText ?? '',
    algorithm: payload.algorithm ?? payload.Algorithm ?? '',
    resultUrl: payload.result_url ?? payload.resultUrl ?? payload.ResultUrl ?? '',
    failureReason: payload.failure_reason ?? payload.failureReason ?? payload.FailureReason ?? '',
    parentTaskCompleted:
      payload.parent_task_completed ?? payload.parentTaskCompleted ?? payload.ParentTaskCompleted ?? false,
    finishedAt: payload.finished_at ?? payload.finishedAt ?? payload.FinishedAt ?? ''
  }
}

function handleTaskMessage(payload) {
  const data = normalizePayload(payload)
  const messageKey = `${data.algoTaskId || data.taskId}:${data.status}:${data.finishedAt}`
  if (recentMessages.has(messageKey)) {
    return
  }

  recentMessages.add(messageKey)
  window.setTimeout(() => recentMessages.delete(messageKey), 5000)

  window.dispatchEvent(new CustomEvent('sifs:algo-task-finished', { detail: data }))

  const isFailed = data.status === 'failed'
  ElNotification({
    title: isFailed ? '子任务执行失败' : '子任务执行完成',
    message: data.algorithm
      ? `${data.algorithm} ${data.statusText || data.status}`
      : data.statusText || data.status || '任务状态已更新',
    type: isFailed ? 'error' : 'success',
    duration: 4500
  })
}

export async function startTaskRealtime() {
  const token = tokenStorage.getAccessToken()
  if (!token || starting || connection?.state === 'Connected') {
    return connection
  }

  starting = true
  connection = new HubConnectionBuilder()
    .withUrl('/task-notifications/hub', {
      accessTokenFactory: () => tokenStorage.getAccessToken() || ''
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  connection.on('algo_task.finished', handleTaskMessage)
  connection.on('task.message', handleTaskMessage)

  try {
    await connection.start()
  } catch (error) {
    console.warn('Task realtime connection failed', error)
  } finally {
    starting = false
  }

  return connection
}

export async function stopTaskRealtime() {
  const current = connection
  connection = null
  starting = false

  if (current) {
    try {
      await current.stop()
    } catch (error) {
      console.warn('Task realtime disconnect failed', error)
    }
  }
}

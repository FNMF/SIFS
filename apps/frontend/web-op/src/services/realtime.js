import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { tokenStorage } from '../utils/storage'

export function createDashboardConnection(onMessage) {
  const connection = new HubConnectionBuilder()
    .withUrl('/admin/dashboard/hub', {
      accessTokenFactory: () => tokenStorage.getAccessToken() || ''
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  const refreshEvents = [
    'dashboard.summary.changed',
    'task.status.changed',
    'task.created',
    'task.failed',
    'operation.log.created',
    'algo.status.changed',
    'algo.health.changed',
    'dashboard.message'
  ]

  refreshEvents.forEach((eventName) => {
    connection.on(eventName, (payload) => onMessage?.(eventName, payload))
  })

  return connection
}

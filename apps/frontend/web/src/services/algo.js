import { request } from './request'

export function getAlgoListApi() {
  return request('/api/info/algo', {
    method: 'GET'
  })
}
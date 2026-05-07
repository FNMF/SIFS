import { request } from './request'

export function getAlgoTaskDetailApi(guid) {
  return request(`/api/algo-task/${guid}`, {
    method: 'GET'
  })
}
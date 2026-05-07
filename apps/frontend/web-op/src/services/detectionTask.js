import { request } from './request'

export function createDetectionTaskApi(payload) {
  const formData = new FormData()

  payload.images.forEach((item, index) => {
    formData.append(`images[${index}].order`, item.order)
    formData.append(`images[${index}].file`, item.file)
  })

  payload.types.forEach((type, index) => {
    formData.append(`types[${index}]`, type)
  })

  if (payload.level !== null && payload.level !== undefined && payload.level !== '') {
    formData.append('level', payload.level)
  }

  return request('/api/de-task', {
    method: 'POST',
    body: formData
  })
}

export function getDetectionTaskListApi() {
  return request('/api/de-task', {
    method: 'GET'
  })
}

export function getDetectionTaskDetailApi(guid) {
  return request(`/api/de-task/${guid}`, {
    method: 'GET'
  })
}
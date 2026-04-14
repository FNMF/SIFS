import { reactive } from 'vue'
import { resultStorage } from '../utils/storage'

const state = reactive({
  latestResult: resultStorage.getLatestResult(),
  history: []
})

export function useResultStore() {
  const setLatestResult = (result) => {
    state.latestResult = result
    resultStorage.setLatestResult(result)
  }

  const clearLatestResult = () => {
    state.latestResult = null
    resultStorage.clearLatestResult()
  }

  const setHistory = (list = []) => {
    state.history = list
  }

  return {
    state,
    setLatestResult,
    clearLatestResult,
    setHistory
  }
}

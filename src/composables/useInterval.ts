import { onBeforeUnmount } from 'vue'

export const useInterval = (callback: () => void, ms: number) => {
  //let intervalId = -1;
  const intervalId = window.setInterval(callback, ms)
  console.log(`started interval ${ms} ms with id ${intervalId}`)

  const clear = () => {
    console.log(`stopping interval with id ${intervalId}`)
    window.clearInterval(intervalId)
  }

  onBeforeUnmount(clear)

  return { clear }
}

import { readonly, ref } from 'vue'

interface IPerfomanceMeasure {
  name: string
  stack: number
  duration: number
  measuring: boolean
}

export const usePerformanceCounter = () => {
  const frame = ref(0)
  const startTimeStamp = ref(0)
  const fps = ref(0)
  const measures = ref<IPerfomanceMeasure[]>([])

  const startMeasure = (name: string) => {
    let measure = measures.value.find((measure) => measure.name == name)
    if (!measure) {
      measure = {
        name,
        stack: 0,
        duration: 0,
        measuring: false
      }
      measures.value.push(measure)
    }
    if (measure.measuring) {
      throw `startMeasure('${name}') is called twice`
    }
    performance.mark(`${name}:start`)
    measure.measuring = true
  }

  const endMeasure = (name: string) => {
    const measure = measures.value.find((measure) => measure.name == name)
    if (!measure || !measure.measuring) {
      throw `startMeasure('${name}') must be called before endMeasure('${name}')`
    }
    performance.measure(name, `${name}:start`)
    measure.stack += performance.getEntriesByName(name)[0].duration
    measure.measuring = false
  }

  const startFrame = (timestamp: number) => {
    if (measures.value.some((measure) => measure.measuring)) {
      throw `some measures are not ended`
    }
    frame.value++
    if (timestamp > startTimeStamp.value + 1000) {
      // duration には、1000msのうち、何ms消費していたかが入る
      // fpsが31以上出ていて、total.duration が100ms以下なら許容範囲
      // fpsが30を切り始めるとよくない
      for (const p of measures.value) {
        p.duration = p.stack
        p.stack = 0
      }
      startTimeStamp.value = timestamp
      fps.value = frame.value
      frame.value = 0
    }
    performance.clearMarks()
    performance.clearMeasures()
  }

  return {
    frame: readonly(frame),
    startTimeStamp: readonly(startTimeStamp),
    fps: readonly(fps),
    measures: readonly(measures),
    startMeasure,
    endMeasure,
    startFrame
  }
}

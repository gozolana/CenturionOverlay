<script setup lang="ts">
import { IconId, TImageSize, TMapImage, ZoneProvider } from 'ffxiv-lib'
import { IconProvider } from 'ffxiv-lib/plugin'
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { usePerformanceCounter } from '../composables/performanceCounter'
import { useOverlayStore } from '../store/overlay'
import { useOverlaySettingsStore } from '../store/overlaySettings'
import OverlayCanvasHeader from './MobSelectHeader.vue'

interface IPosition {
  x: number
  y: number
}

interface IEdgePositon extends IPosition {
  labelOffsetX: number
  labelOffsetY: number
  distance: number
}

interface IRect {
  left: number
  top: number
  right: number
  bottom: number
}

const store = useOverlayStore()
const settingsStore = useOverlaySettingsStore()
const pc = usePerformanceCounter()

const zone = computed(() => {
  const zone = ZoneProvider.findFieldZone(store.zoneId)
  return zone
})

const factor = computed(() =>
  zone.value ? zone.value.scale.xRange / canvasSize.value : 1
)
const canvas = ref<HTMLCanvasElement>()
const canvasSize = ref(0)

const mapImage = computed<HTMLImageElement>(() => {
  const mapImage = new Image()
  if (zone.value) {
    const url = zone.value.getMapImageUrl(
      settingsStore.map.type,
      TImageSize.Small
    )
    console.log('url', url)
    if (url) {
      //loadImage(url, mapImage)
      mapImage.src = url
    }
  }
  return mapImage
})

const handleWheel = (e: { deltaY: number }) => {
  if (e.deltaY < 0) {
    //e.deltaY == -100
    const newScale = settingsStore.scale * 1.1
    settingsStore.scale = Math.min(newScale, 8.0)
  } else if (e.deltaY > 0) {
    //e.deltaY == 100
    const newScale = settingsStore.scale * 0.9
    settingsStore.scale = Math.max(newScale, 1.0)
  }
}

const drawMap = (
  context: CanvasRenderingContext2D,
  scale: {
    xMin: any
    xMax?: number
    xRange: any
    yMin: any
    yMax?: number
    yRange: any
  }
) => {
  if (mapImage.value.complete) {
    context.save()
    if (settingsStore.map.type === TMapImage.Outline) {
      context.globalAlpha = settingsStore.map.opacity / 100.0
    }
    context.drawImage(
      mapImage.value,
      mapImage.value.x,
      mapImage.value.y,
      mapImage.value.width,
      mapImage.value.height,
      scale.xMin,
      scale.yMin,
      scale.xRange,
      scale.yRange
    )
    context.restore()
  }
}

const drawDetection = (context: CanvasRenderingContext2D) => {
  context.fillStyle = 'rgba(0, 0, 255, 0.25)'
  context.fillRect(store.player.x - 2.2, store.player.y - 2.2, 4.4, 4.4)
}

const drawSymbols = (context: CanvasRenderingContext2D) => {
  // see: ffxiv-lib/src/assets/icons/preview.htm for symbols
  zone.value?.markers
    .filter(marker =>
      [
        IconId.Aetheryte,
        '060441',
        '060446',
        '060447',
        '060456',
        '060467',
      ].includes(marker.icon.substring(0, 6))
    )
    .sort((a, b) => a.y - b.y)
    .forEach(marker => {
      const icon = IconProvider.findIcon(marker.icon)
      if (icon && icon.complete && icon.width != 0 && icon.height != 0) {
        //console.log(marker.icon, icon.complete, icon.width, icon.height)
        context.save()
        context.translate(marker.x, marker.y)

        context.scale(
          factor.value / settingsStore.scale,
          factor.value / settingsStore.scale
        )

        context.drawImage(icon, -icon.width / 2, -icon.height / 2)
        context.restore()
      }
    })
}

const drawTargetsBackground = (
  context: CanvasRenderingContext2D,
  alpha: number
) => {
  const indexAndColors: {
    index: number
    color: string
  }[] = []

  store.targets.forEach(target => {
    const indices = zone.value?.getEliteLocationIndices(target, 0x1f, 3.0)
    if (indices && indices.length > 0) {
      indexAndColors.push({
        index: indices[0],
        color: zone.value!.getMobColors(target.bNpcNameId)[1],
      })
    }
  })

  indexAndColors.forEach(({ index, color }) => {
    const { flag, x, y } = zone.value!.elite.locations[index]
    if (flag & useOverlaySettingsStore().flagRanks) {
      context.save()

      const scale = Math.max(
        factor.value / 3 / settingsStore.scale,
        Math.min(1 / 20, 0.2 / settingsStore.scale)
      )
      const radius = 10 * 1.46
      context.lineWidth = radius * 0.4

      context.translate(x, y)

      context.scale(scale, scale)
      context.globalAlpha = alpha
      context.strokeStyle = color
      context.beginPath()
      context.arc(0, 0, radius, 0, 2 * Math.PI, false)
      context.closePath()
      context.stroke()
      context.restore()
    }
  })
}

const drawEliteSymbols = (context: CanvasRenderingContext2D) => {
  zone.value?.elite.locations.forEach((loc, _) => {
    //filter if not to be displayed
    context.save()
    context.translate(loc.x, loc.y)

    const scale =
      Math.max(
        factor.value / 3 / settingsStore.scale,
        Math.min(1 / 20, 0.2 / settingsStore.scale)
      ) * 0.8

    context.scale(scale, scale)

    const displayFlag = useOverlaySettingsStore().flagRanks
    const icon = IconProvider.findElite(loc.flag & displayFlag)
    if (icon && icon.complete && icon.width != 0 && icon.height != 0) {
      context.globalAlpha = settingsStore.scale > 2.0 ? 0.5 : 1.0
      context.drawImage(icon, -icon.width / 2, -icon.height / 2)
    }

    context.restore()
  })
}

const drawSSSymbols = (context: CanvasRenderingContext2D) => {
  if (useOverlaySettingsStore().displaySS) {
    return
  }
  zone.value?.ss?.locations.forEach(loc => {
    //filter if not to be displayed
    context.save()
    context.translate(loc.x, loc.y)

    const scale =
      Math.max(
        factor.value / 3 / settingsStore.scale,
        Math.min(1 / 20, 0.2 / settingsStore.scale)
      ) * 0.8

    context.scale(scale, scale)

    const icon = IconProvider.findIcon(loc.icon)
    if (icon && icon.complete && icon.width != 0 && icon.height != 0) {
      context.globalAlpha = settingsStore.scale > 2.0 ? 0.5 : 1.0
      context.drawImage(icon, -icon.width / 2, -icon.height / 2)
    }
    context.restore()
  })
}

const drawNotifiedLocations = (
  context: CanvasRenderingContext2D,
  alpha: number
) => {
  store.notifiedLocations
    .filter(({ zoneId }) => zoneId == store.zoneId)
    .forEach(notifiedLocation => {
      const pos: IPosition = { x: notifiedLocation.x, y: notifiedLocation.y }
      const indices = zone.value?.getEliteLocationIndices(
        notifiedLocation,
        useOverlaySettingsStore().flagRanks,
        1.0
      )
      if (indices && indices.length > 0) {
        const index = indices[0]
        pos.x = zone.value!.elite.locations[index].x
        pos.y = zone.value!.elite.locations[index].y
        const onBorderPos = getEdgePosition(pos)
        if (onBorderPos) {
          drawOnBorderPos(context, onBorderPos, alpha, 'orange', undefined)
        } else {
          context.save()
          const scale = Math.max(
            factor.value / 3 / settingsStore.scale,
            Math.min(1 / 20, 0.2 / settingsStore.scale)
          )
          const radius = 10 * 1.47
          context.lineWidth = radius * 0.3

          context.translate(pos.x, pos.y)

          context.scale(scale, scale)
          context.globalAlpha = alpha
          context.strokeStyle = 'orange'
          context.beginPath()
          context.arc(0, 0, radius, 0, 2 * Math.PI, false)
          context.closePath()
          context.stroke()
          context.restore()
        }
      } else {
        // flag または 範囲外
        const onBorderPos = getEdgePosition(pos)
        if (onBorderPos) {
          drawOnBorderPos(context, onBorderPos, alpha, 'orange', undefined)
        } else {
          context.save()
          context.translate(pos.x, pos.y)

          context.scale(
            factor.value / settingsStore.scale,
            factor.value / settingsStore.scale
          )
          const icon = IconProvider.findIcon(IconId.Flag)
          if (icon && icon.complete && icon.width != 0 && icon.height != 0) {
              context.globalAlpha = alpha
            context.drawImage(icon, -icon.width / 2, -icon.height / 2)
          }
          context.restore()
        }
      }
    })
}

const drawOnBorderPos = (
  context: CanvasRenderingContext2D,
  onBorderPos: IEdgePositon,
  alpha: number,
  color: string = 'orange',
  additinalLabel: string | undefined
) => {
  const x = onBorderPos.x
  const y = onBorderPos.y

  context.save()

  const scale = Math.max(
    factor.value / 3 / settingsStore.scale,
    Math.min(1 / 20, 0.2 / settingsStore.scale)
  )
  const radius = 10 * 1.46
  context.lineWidth = radius * 0.4

  context.translate(x, y)

  context.save()
  context.scale(scale, scale)
  context.globalAlpha = alpha
  context.fillStyle = color
  context.beginPath()
  context.arc(0, 0, radius, 0, 2 * Math.PI, false)
  context.closePath()
  context.fill()
  context.restore()

  context.scale(
    factor.value / settingsStore.scale,
    factor.value / settingsStore.scale
  )

  // offset
  context.globalAlpha = 1.0
  const iconFontScale = settingsStore.iconFontScale ?? 1.0
  context.scale(iconFontScale, iconFontScale)

  // Distance + HP label
  context.globalAlpha = 1.0
  context.translate(onBorderPos.labelOffsetX, onBorderPos.labelOffsetY)
  context.strokeStyle = `#000000ff`
  context.fillStyle = color
  context.font = '12px sans-serif'
  context.textAlign = 'center'
  if (additinalLabel) {
    context.fillRect(-23, -16, 46, 32)
    context.fillStyle = 'white'
    context.fillText(`${onBorderPos.distance.toFixed(0)}m`, 0, -4)
    context.fillText(additinalLabel, 0, 12) // `${(m.hpp * 100).toFixed(1)}%`
  } else {
    context.fillRect(-23, -8, 46, 16)
    context.fillStyle = 'white'
    context.fillText(`${onBorderPos.distance.toFixed(0)}m`, 0, 4)
  }
  context.restore()
}

const drawPartyMembers = (context: CanvasRenderingContext2D) => {
  const icon = IconProvider.findIcon(IconId.PartyMember)
  if (icon && icon.complete && icon.width != 0 && icon.height != 0) {
    store.party.forEach(({ x, y }) => {
      context.save()
      context.translate(x, y)

      context.scale(
        factor.value / settingsStore.scale,
        factor.value / settingsStore.scale
      )
      context.drawImage(icon, -icon.width / 2, -icon.height / 2)
      context.restore()
    })
  }
}

const drawPlayer = (context: CanvasRenderingContext2D) => {
  context.save()

  context.translate(store.player.x, store.player.y)
  context.rotate(Math.PI - store.player.heading)

  context.beginPath()
  context.fillStyle = 'rgba(192, 255, 192, 0.3)'
  //context.arc(0, 0, 16 * this.factor, Math.PI + 0.8, -0.8);
  context.arc(0, 0, 2.2, Math.PI + 0.8, -0.8)
  context.lineTo(0, 0)
  context.fill()
  context.closePath()

  context.scale(
    factor.value / settingsStore.scale,
    factor.value / settingsStore.scale
  )
  const icon = IconProvider.findIcon(IconId.Player)
  if (icon && icon.complete && icon.width != 0 && icon.height != 0) {
    context.drawImage(icon, -icon.width / 2, -icon.height / 2)
  }
  context.restore()
}

const drawTargets = (context: CanvasRenderingContext2D, alpha: number) => {
  const drawnIndices: number[] = []
  store.targets.forEach(target => {
    const pos: IPosition = { x: target.x, y: target.y }
    const indices = zone.value?.getEliteLocationIndices(target, 0x1f, 3.0)
    if (indices && indices.length > 0) {
      const index = indices[0]
      pos.x = zone.value!.elite.locations[index].x
      pos.y = zone.value!.elite.locations[index].y
      drawnIndices.push(index)
    }

    const mobColors = zone.value!.getMobColors(target.bNpcNameId)
    context.save()

    const onBorderPos = getEdgePosition(pos)
    if (onBorderPos) {
      // スクリーン外
      const label = `${(target.hpp * 100).toFixed(1)}%`
      drawOnBorderPos(context, onBorderPos, alpha, mobColors[1], label)
    } else {
      // スクリーン内
      context.translate(target.x, target.y)

      context.scale(
        factor.value / settingsStore.scale,
        factor.value / settingsStore.scale
      )
      const icon = IconProvider.findIcon(IconId.Mob)
      if (icon && icon.complete && icon.width != 0 && icon.height != 0) {
        context.globalAlpha = alpha
        context.drawImage(icon, -icon.width / 2, -icon.height / 2)
      }

      context.globalAlpha = 1.0
      const iconFontScale = settingsStore.iconFontScale ?? 1.0
      context.scale(iconFontScale, iconFontScale)

      // HP
      context.translate(0.0, -20.0)
      context.strokeStyle = `#000000ff`
      context.fillStyle = mobColors[1]
      context.fillRect(-23, -8, 46, 16)
      context.font = '12px sans-serif'
      context.textAlign = 'center'
      context.fillStyle = 'white'
      context.fillText(`${(target.hpp * 100).toFixed(1)}%`, 0, 4)

      // distance and direction
      if (settingsStore.scale < 2.0) {
        context.fillStyle = mobColors[1]
        context.fillRect(-23, -24, 46, 16)
        const distance = Math.sqrt(
          (store.player.x - target.x) * (store.player.x - target.x) * 2500 +
            (store.player.y - target.y) * (store.player.y - target.y) * 2500
        )
        context.fillStyle = 'white'
        if (distance < 100) {
          //
          context.save()
          context.translate(-12, -16)
          const atan = Math.atan2(
            target.x - store.player.x,
            store.player.y - target.y
          )
          context.rotate(atan)
          context.beginPath()
          context.moveTo(-2, 6)
          context.lineTo(-2, 0)
          context.lineTo(-6, 0)
          context.lineTo(0, -6)
          context.lineTo(6, 0)
          context.lineTo(2, 0)
          context.lineTo(2, 6)
          context.closePath()
          context.fill()
          context.restore()
          context.fillText(`${distance.toFixed(0)}m`, 8, -12)
        } else {
          context.fillText(`${distance.toFixed(0)}m`, 0, -12)
        }
      }
    }

    context.restore()
  })
}

const drawFrame = (context: CanvasRenderingContext2D, timestamp: number) => {
  pc.startFrame(timestamp)
  if (store.player && zone.value) {
    const alphaOfTimestamp = getAlphaOfTimestamp(timestamp, 0.7)

    context.fillStyle = 'rgba(0, 0, 0, 0.01)'
    context.clearRect(0, 0, canvasSize.value, canvasSize.value)
    context.fillRect(0, 0, canvasSize.value, canvasSize.value)

    const scalec2a = canvasSize.value / zone.value.scale.xRange
    //const scalea2i = this.zone.scale.xRange / this.mapImage.width;
    const prefferedX = zone.value.scale.xRange / (2.0 * settingsStore.scale)
    const prefferedY = zone.value.scale.yRange / (2.0 * settingsStore.scale)
    const offsetLeft = -zone.value.scale.xMin + store.player.x
    const offsetRight =
      zone.value.scale.xRange / settingsStore.scale -
      (zone.value.scale.xMax - store.player.x)
    const offsetTop = -zone.value.scale.yMin + store.player.y
    const offsetBottom =
      zone.value.scale.xRange / settingsStore.scale -
      (zone.value.scale.yMax - store.player.y)

    pc.startMeasure('total')

    context.save()

    context.scale(scalec2a, scalec2a)
    context.scale(settingsStore.scale, settingsStore.scale)

    context.translate(1.0 - store.player.x, 1.0 - store.player.y)
    context.translate(
      prefferedX < offsetLeft ? Math.max(prefferedX, offsetRight) : offsetLeft,
      prefferedY < offsetTop ? Math.max(prefferedY, offsetBottom) : offsetTop
    )

    context.translate(-zone.value.scale.xMin, -zone.value.scale.yMin)

    // マップ
    pc.startMeasure('drawMap')
    drawMap(context, zone.value.scale)
    pc.endMeasure('drawMap')

    // 索敵範囲
    pc.startMeasure('drawDetection')
    drawDetection(context)
    pc.endMeasure('drawDetection')

    // エーテライト,移動系アイコン
    pc.startMeasure('drawSymbols')
    drawSymbols(context)
    pc.endMeasure('drawSymbols')

    pc.startMeasure('drawTargetsBackground')
    drawTargetsBackground(context, alphaOfTimestamp)
    pc.endMeasure('drawTargetsBackground')

    // S,A,Bの座標アイコン
    pc.startMeasure('drawEliteSymbols')
    drawEliteSymbols(context)
    pc.endMeasure('drawEliteSymbols')

    // SSの座標アイコン
    pc.startMeasure('drawSSSymbols')
    drawSSSymbols(context)
    pc.endMeasure('drawSSSymbols')

    pc.startMeasure('drawNotifiedLocations')
    drawNotifiedLocations(context, alphaOfTimestamp)
    pc.endMeasure('drawNotifiedLocations')

    pc.startMeasure('drawPartyMembers')
    drawPartyMembers(context)
    pc.endMeasure('drawPartyMembers')

    // Playerキャラクター(アイコン,視認範囲)
    pc.startMeasure('drawPlayer')
    drawPlayer(context)
    pc.endMeasure('drawPlayer')

    // ACTが認識している索敵範囲内のMob情報(アイコン,HP,距離,向き)
    pc.startMeasure('drawTargets')
    drawTargets(context, alphaOfTimestamp)
    pc.endMeasure('drawTargets')

    context.restore()
    pc.endMeasure('total')
  }
  window.requestAnimationFrame(timestamp => drawFrame(context, timestamp))
}

const drawCanvas = async () => {
  if (canvas.value) {
    const context = canvas.value.getContext('2d')
    window.requestAnimationFrame(timestamp => drawFrame(context!, timestamp))
  }
}

onMounted(() => {
  window.addEventListener('resize', onResize)
  onResize()
  if (canvas.value) {
    canvas.value.addEventListener('wheel', handleWheel)
  }
  drawCanvas()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', onResize)
  if (canvas.value) {
    canvas.value.removeEventListener('wheel', handleWheel)
  }
})

/*
const fontSize = computed(() => {
  const ratio = Math.min(canvasSize.value / 400, 1)
  return `${12 * ratio}px`
})
const copyRightsTop = computed(() => {
  const ratio = Math.min(canvasSize.value / 400, 1)
  return `${canvasSize.value - ratio * 24}px`
})
*/

const textColor = computed(() =>
  settingsStore.map.type === TMapImage.Default ? '#330000' : '#0077ff'
)

const resizeIconTop = ref('0px')

const border = computed<IRect>(() => {
  try {
    const s = zone.value!.scale
    const width = s.xRange / settingsStore.scale
    const height = s.yRange / settingsStore.scale

    let left = store.player.x - width / 2
    let top = store.player.y - height / 2
    const offsetLeft =
      left >= s.xMin
        ? Math.min(s.xMax - (left + width), 0)
        : Math.max(0, s.xMin - left)
    const offsetTop =
      top >= s.yMin
        ? Math.min(s.yMax - (top + height), 0)
        : Math.max(0, s.yMin - top)
    left += offsetLeft
    top += offsetTop
    return {
      left: left,
      top: top,
      right: left + width,
      bottom: top + height,
    }
  } catch {
    return { left: 0, top: 0, right: 0, bottom: 0 }
  }
})

const onResize = () => {
//  canvasSize.value = document.documentElement.clientWidth
  //canvasSize.value = window.innerWidth;
  const clientWidth =
    document.querySelector<HTMLElement>('div.main')!.clientWidth
  canvasSize.value = clientWidth
  resizeIconTop.value = `${clientWidth - 66}px`
}

const getAlphaOfTimestamp = (
  timestamp: number,
  frequency = 1.0,
  max = 1.0,
  min = 0.0
) => {
  const rad = 2 * Math.PI * frequency * 0.001 * (timestamp % (1000 / frequency))
  return (Math.cos(rad) / 2 + 0.5) * (max - min) + min
}

const getEdgePosition = (
  target: IPosition,
  offsetYBase = 25
): IEdgePositon | null => {
  const self = store.player
  const rect = border.value
  const points: IPosition[] = []
  const result: IEdgePositon = {
    x: 0,
    y: 0,
    labelOffsetX: 0,
    labelOffsetY: 0,
    distance: Math.sqrt(
      (target.x - self.x) * (target.x - self.x) * 2500 +
        (target.y - self.y) * (target.y - self.y) * 2500
    ),
  }
  if (target.x < rect.left) {
    points.push({
      x: rect.left,
      y:
        self.y -
        ((self.x - rect.left) * (self.y - target.y)) / (self.x - target.x),
    })
  }
  if (target.y < rect.top) {
    points.push({
      x:
        self.x -
        ((self.y - rect.top) * (self.x - target.x)) / (self.y - target.y),
      y: rect.top,
    })
  }
  if (target.x > rect.right) {
    points.push({
      x: rect.right,
      y:
        ((rect.right - self.x) * (target.y - self.y)) / (target.x - self.x) +
        self.y,
    })
  }
  if (target.y > rect.bottom) {
    points.push({
      x:
        ((rect.bottom - self.y) * (target.x - self.x)) / (target.y - self.y) +
        self.x,
      y: rect.bottom,
    })
  }
  const point =
    points.length > 0
      ? points.reduce((a: IPosition, b: IPosition) => {
          const diffAx = a.x - self.x
          const diffAy = a.y - self.y
          const diffBx = b.x - self.x
          const diffBy = b.y - self.y
          return diffAx * diffAx + diffAy * diffAy <
            diffBx * diffBx + diffBy * diffBy
            ? a
            : b
        })
      : null
  if (point != null) {
    result.x = point.x
    result.y = point.y
    const leftPixel =
      ((result.x - rect.left) * settingsStore.scale) / factor.value
    const topPixel =
      ((result.y - rect.top) * settingsStore.scale) / factor.value
    const rightPixel =
      ((rect.right - result.x) * settingsStore.scale) / factor.value
    const bottomPixel =
      ((rect.bottom - result.y) * settingsStore.scale) / factor.value
    if (leftPixel < 40) {
      result.labelOffsetX = 40 - Math.max(leftPixel, 0)
    } else if (rightPixel < 40) {
      result.labelOffsetX = Math.max(rightPixel, 0) - 40
    }
    if (topPixel < offsetYBase) {
      result.labelOffsetY = offsetYBase - Math.max(topPixel, 0)
    } else if (bottomPixel < offsetYBase) {
      result.labelOffsetY = Math.max(bottomPixel, 0) - offsetYBase
    }
  }
  return point != null ? result : null
}
</script>

<template>
  <div>
    <div id="canvasWrapper">
      <canvas
        ref="canvas"
        :width="canvasSize"
        :height="zone ? canvasSize : 0"
      />
      <div id="debug" v-if="settingsStore.debugMode">
        <!-- <performance-counter></performance-counter> -->
        {{ store.dump }}
      </div>
      <div id="fieldInstance" 
        v-if="store.instance > 0"
        :style="{ color: textColor }">
        <span class="material-icons-outlined md-48">{{store.instanceIcon}}</span>
      </div>
      <!--div id="specifiedLocation">
        <location-map></location-map>
      </div-->

      <!--div
        id="copyRights"
        v-if="settingsStore.map.type === TMapImage.Default"
        :style="{ 'font-size': fontSize, top: copyRightsTop, color: textColor }"
      >
        {{ $t('copyRights') }}
      </div-->
    </div>
    <div id="resizeCorner" :style="{ color: textColor, top: resizeIconTop }">
      <!--v-icon size="50" icon="mdi-resize-bottom-right"></v-icon-->
    </div>
  </div>
</template>

<style lang="scss">
#wrapper {
  position: relative;
  width: 100%;
  height: 100%;
}

#canvasWrapper {
  background-color: transparent;
  position: relative;
  width: 100%;
  height: 100%;
}

canvas {
  background-color: transparent;
}
.overlayError {
  background-color: white;
}
#specifiedLocation {
  position: absolute;
  top: 8px;
  left: 5px;
  width: 40%;
}
#specifiedLocation.withheader {
  top: 48px;
}

#debug {
  position: absolute;
  top: 0px;
  left: 0px;
  padding: 14px;
  font-size: 14px;
  width: 100%;
  background-color: rgba(120, 120, 120, 0.5) !important;
}
#fieldInstance {
  position: absolute;
  font-family: monospace;
  font-weight: bold;
  color: #330000ff;
  font-size: 36px;
  top: 6px;
  right: 6px;
}
#fieldInstance > span {
  font-size: 36px;
}

#fieldInstanceWarning {
  position: absolute;
  font-family: monospace;
  font-weight: normal;
  font-size: 16px;
  top: 20px;
  left: 10px;
  width: calc(100% - 20px);
  height: 50px;
}
#copyRights {
  position: absolute;
  font-family: sans-serif;
  text-align: center;
  width: 100%;
}
#resizeCorner {
  position: absolute;
  font-family: sans-serif;
  text-align: center;
  right: 0px;
}
</style>

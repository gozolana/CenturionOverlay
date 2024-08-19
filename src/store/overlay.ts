import { MessageProvider, ZoneProvider } from 'ffxiv-lib'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useInterval } from '../composables/useInterval'
import type {
  ICharacter,
  ICombatDataEvent,
  IFateEvent,
  ILocationNotifiedEvent,
  IPlayerLoggedInEvent,
  IPlayerLoggedOutEvent,
  IZoneChangedEvent,
  IZoneInstanceChangedEvent
} from '../lib/overlayEvents'
import { DEFAULT_PLAYER, toCharacter } from '../lib/overlayEvents'
import { useOverlaySettingsStore } from './overlaySettings'
import { useWorldStore } from './world'

interface INotifiedLocation {
  zoneId: number
  x: number
  y: number
  nearMobLocationIndex: number
  timestamp: number
}

interface IMapLocation {
  zoneId: number
  x: number
  y: number
}

export const useOverlayStore = defineStore('overlay', () => {
  const settingsStore = useOverlaySettingsStore()

  const playerName = ref<string>('')
  const player = ref<ICharacter>(DEFAULT_PLAYER)
  const party = ref<ICharacter[]>([])
  const targets = ref<ICharacter[]>([])
  const worldId = computed(() => useWorldStore().worldId)
  const zoneId = ref<number>(0)
  const instance = ref<number>(0)
  const refreshedAt = ref<number>(Date.now())

  const notifiedLocations = ref<INotifiedLocation[]>([])
  const lastNotifiedLocation = ref<IMapLocation>({ zoneId: 0, x: 0, y: 0 })

  const showBorder = ref<boolean>(true)

  const fateStatus = ref<Record<number, number>>([])

  const instanceIcon = computed(() =>
    instance.value > 0 ? `filter_${instance.value}` : null
  )

  const onPlayerLoggedIn = async (event: IPlayerLoggedInEvent) => {
    playerName.value = event.playerName
  }

  const onPlayerLoggedOut = async (_event: IPlayerLoggedOutEvent) => {
    playerName.value = ''
    player.value = DEFAULT_PLAYER
    party.value = []
    zoneId.value = 0
    instance.value = 0
  }

  const onZoneChanged = async (event: IZoneChangedEvent) => {
    //    fieldInstanceStore.load()
    useWorldStore().setWorldId(event.worldId)
    zoneId.value = event.zoneId
    //    instance.value =
    //      fieldInstanceStore.getInstanceCount(event.zoneId) == 1 ? 1 : 0
    instance.value = 0

    // clear old cache
    //fateStatus.value = {}
  }

  const onZoneInstanceChanged = async (event: IZoneInstanceChangedEvent) => {
    instance.value = event.instance
  }

  const onLocationNotified = async (event: ILocationNotifiedEvent) => {
    if (!settingsStore.posNotification.enabled) {
      return
    }
    const zone = ZoneProvider.getFieldZones().find((zone) =>
      zone.names.includes(event.zoneName)
    )
    if (zone) {
      lastNotifiedLocation.value = { zoneId: zone.id, x: event.x, y: event.y }
      // 今回のPointに近い過去報告Pointは削除(連続報告の対策)
      notifiedLocations.value = notifiedLocations.value.filter(
        (notifiedLocation) => {
          const distance = Math.sqrt(
            (notifiedLocation.x - event.x) * (notifiedLocation.x - event.x) +
              (notifiedLocation.y - event.y) * (notifiedLocation.y - event.y)
          )
          return distance > 1.0
        }
      )
      const indices = zone.getEliteLocationIndices(event, 0x0f, 1.0)
      notifiedLocations.value.push({
        zoneId: zone.id,
        x: event.x,
        y: event.y,
        nearMobLocationIndex: indices.length > 0 ? indices[0] : -1,
        timestamp: new Date(event.timestamp).getTime()
      })
      notifiedLocations.value.sort((a, b) => b.timestamp - a.timestamp)
    } else {
      //console.debug('ignore NotifiedLocation');
    }
  }

  const onCombatData = async (event: ICombatDataEvent) => {
    const zone = ZoneProvider.findZone(event.zoneId)
    player.value = toCharacter(event.self, zone)
    party.value = event.party.map((c) => toCharacter(c, zone))
    targets.value = event.targets.map((c) => toCharacter(c, zone))

    updateNotifiedLocations()
  }

  const onFateStateChanged = async (event: IFateEvent) => {
    if (event.state == 'Update') {
      fateStatus.value[event.fateId] = event.progress
    } else if (event.state == 'Remove') {
      delete fateStatus.value[event.fateId]
    } else if (event.state == 'Add' && !fateStatus.value[event.fateId]) {
      // fates that are not started will not trigger update event
      fateStatus.value[event.fateId] = event.progress // always 0
    }
  }

  const dump = computed(() => {
    const { name, x, y, z } = player.value
    return `world(${worldId.value}) ${MessageProvider.getZoneName(
      zoneId.value
    )} ${zoneId.value} ${instance.value} ${
      instanceIcon.value
    }  ${name}: X:${x.toFixed(1)}, Y:${y.toFixed(1)}, Z:${z.toFixed(1)}`
  })

  // private
  const updateNotifiedLocations = () => {
    // Index付きPointにEliteが実在していたらリストから削除(別途管理するため)
    // 毎描画ごとに更新
    const zone = ZoneProvider.findFieldZone(zoneId.value)
    if (zone) {
      const targetIndices = targets.value
        .filter((target: ICharacter) =>
          zone.elite.ids.includes(target.bNpcNameId)
        )
        .map((target: ICharacter) => {
          const indices = zone.getEliteLocationIndices(target, 0xf, 1.0)
          return indices ? indices[0] : undefined
        })
        .filter(Boolean) // undefined 除去
      notifiedLocations.value = notifiedLocations.value.filter(
        (notifiedLocation) =>
          notifiedLocation.zoneId != zoneId.value ||
          !targetIndices.includes(notifiedLocation.nearMobLocationIndex)
      )
    }
  }

  // 一定時間過ぎたPointをリストから削除
  const refreshNotifiedLocations = () => {
    const timeoutMilliSeconds =
      settingsStore.posNotification.markingTimeoutSeconds * 1000
    notifiedLocations.value = notifiedLocations.value.filter(
      ({ timestamp }) => timestamp + timeoutMilliSeconds > refreshedAt.value
    )
  }

  // call 3 refresh functions in sync
  useInterval(() => {
    refreshedAt.value = Date.now()
    refreshNotifiedLocations()
  }, 1000)

  return {
    playerName,
    player,
    party,
    targets,
    worldId,
    zoneId,
    instance,
    instanceIcon,
    notifiedLocations,
    lastNotifiedLocation,
    showBorder,
    fateStatus,
    onPlayerLoggedIn,
    onPlayerLoggedOut,
    onZoneChanged,
    onZoneInstanceChanged,
    onLocationNotified,
    onCombatData,
    onFateStateChanged,
    dump
  }
})

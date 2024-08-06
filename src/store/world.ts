import { defineStore} from 'pinia'
import { ref, computed } from 'vue'
import {
  DataCenterRegionId,
  WorldProvider,
  type World,
  type WorldId,
} from 'ffxiv-lib'

export const useWorldStore = defineStore(
  'world',
  () => {
    // states
    const worldId = ref(
      WorldProvider.getWorldsOfRegion(DataCenterRegionId.Japan)[0].id
    )

    // getters
    const world = computed<World>(
      () =>
        WorldProvider.findWorld(worldId.value) ??
        WorldProvider.getWorldsOfRegion(DataCenterRegionId.Japan)[0]
    )

    // methods
    const setWorldId = (newWorldId: WorldId | number) => {
      if (WorldProvider.findWorld(newWorldId) && newWorldId != worldId.value) {
        worldId.value = newWorldId as WorldId
      }
    }

    return {
      worldId,
      world,
      setWorldId,
    }
  },
  {
    persist: true,
  }
)

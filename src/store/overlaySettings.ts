import { TLang, TMapImage } from 'ffxiv-lib'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

// <pos>情報のマップ表示設定
interface PosNotificationSettings {
  enabled: boolean
  // 座標マーク維持時間
  markingTimeoutSeconds: number
  // ミニマップ表示時間
  miniMapTimeoutSeconds: number
}

interface MapSettings {
  type: TMapImage
  opacity: number
}

export const useOverlaySettingsStore = defineStore(
  'overlaySettings',
  () => {
    // 言語
    // undefinedの場合は、Autoの扱いでOverlay起動時に
    // Act for FFXIVの言語設定がJapaneseならjp、それ以外ならenを採用する
    //const preferredLanguage = ref<TLang | undefined>()
    const language = ref<TLang>('ja')

    // ヘッダー表示
    const showHeader = ref(true)

    // ズーム率
    const scale = ref(1.0)

    // マップ種別
    const mapTypeCandidates: { name: string; id: TMapImage }[] = [
      {
        name: 'MAP_IMAGE_DEFAULT',
        id: TMapImage.Default
      },
      { name: 'MAP_IMAGE_OUTLINE', id: TMapImage.Outline },
      { name: 'MAP_IMAGE_TRANSPARENT', id: TMapImage.Transparent }
    ]
    const map = ref<MapSettings>({
      type: TMapImage.Default,
      opacity: 1.0
    })

    // 表示オプション
    const displayUpDown = ref(false)

    // Sの絞り込み状況の表示
    const debugMode = ref(false)

    const tts = ref<string[]>(['SBpop', 'B', 'A', 'S', 'Brepop', 'Sstart'])

    const iconFontScale = ref(1.0)

    const posNotification = ref<PosNotificationSettings>({
      enabled: true,
      markingTimeoutSeconds: 180,
      miniMapTimeoutSeconds: 5
    })

    const flagRanks = ref(0x3f) // 0b111111 -> SS, S, A1, A2, B1, B2
    const displaySS = computed({
      get: () => (flagRanks.value & (1 << 5)) > 0,
      set: (value: boolean) =>
        value ? (flagRanks.value |= 1 << 5) : (flagRanks.value &= ~(1 << 5))
    })
    const displayS = computed({
      get: () => (flagRanks.value & (1 << 4)) > 0,
      set: (value: boolean) =>
        value ? (flagRanks.value |= 1 << 4) : (flagRanks.value &= ~(1 << 4))
    })
    const displayA = computed({
      get: () => (flagRanks.value & (1 << 3)) > 0,
      set: (value: boolean) =>
        value ? (flagRanks.value |= 1 << 3) : (flagRanks.value &= ~(1 << 3))
    })
    const displayA2 = computed({
      get: () => (flagRanks.value & (1 << 2)) > 0,
      set: (value: boolean) =>
        value ? (flagRanks.value |= 1 << 2) : (flagRanks.value &= ~(1 << 2))
    })
    const displayB = computed({
      get: () => (flagRanks.value & (1 << 1)) > 0,
      set: (value: boolean) =>
        value ? (flagRanks.value |= 1 << 1) : (flagRanks.value &= ~(1 << 1))
    })
    const displayB2 = computed({
      get: () => (flagRanks.value & (1 << 0)) > 0,
      set: (value: boolean) =>
        value ? (flagRanks.value |= 1 << 0) : (flagRanks.value &= ~(1 << 0))
    })

    return {
      //preferredLanguage,
      language,
      showHeader,
      scale,
      map,
      mapTypeCandidates,
      displayUpDown,
      debugMode,
      tts,
      iconFontScale,
      posNotification,
      flagRanks,
      displaySS,
      displayS,
      displayA,
      displayA2,
      displayB,
      displayB2
    }
  },
  {
    persist: true
  }
)

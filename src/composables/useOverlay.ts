import { MobProvider, TMobRank, ZoneProvider } from 'ffxiv-lib'
import { ref } from 'vue'
import type {
  IMobFAEvent,
  IMobLocationEvent,
  IMobStateChangedEvent,
  IMobTriggerEvent
} from '../lib/overlayEvents'
import { useOverlayStore } from '../store/overlay'
import { useOverlaySettingsStore } from '../store/overlaySettings'
import { useLocale } from './useLocale'

declare global {
  interface Window {
    addOverlayListener(
      eventType: string,
      callback: (event: any) => Promise<void>
    ): void
    callOverlayHandler(data: any): Promise<any>
    removeOverlayListener(eventType: string): void
    startOverlayEvents(): void
  }
}

export const useOverlay = () => {
  const errorCode = ref(0)
  const store = useOverlayStore()
  const settingsStore = useOverlaySettingsStore()

  const load = () => {
    const url =
      'https://overlayplugin.github.io/OverlayPlugin/assets/shared/common.min.js'
    const script = document.createElement('script')
    script.src = url
    script.onload = async () => {
      console.log(`Successfully loaded ${url}`)
      connect()
    }
    script.onerror = () => console.error(`Error loading ${url}`)
    document.head.append(script)
  }

  const disconnect = (): void => {
    window.removeOverlayListener('CenturionPlayerLoggedInEvent')
    window.removeOverlayListener('CenturionPlayerLoggedOutEvent')
    window.removeOverlayListener('CenturionZoneChangedEvent')
    window.removeOverlayListener('CenturionZoneInstanceChangedEvent')
    window.removeOverlayListener('CenturionLocationNotifiedEvent')
    window.removeOverlayListener('CenturionMobFAEvent')
    window.removeOverlayListener('CenturionMobTriggerEvent')
    window.removeOverlayListener('CenturionMobStateChangedEvent')
    window.removeOverlayListener('CenturionMobLocationEvent')
    window.removeOverlayListener('CenturionCombatData')
  }

  const connect = () => {
    disconnect()
    window.addOverlayListener(
      'CenturionPlayerLoggedInEvent',
      store.onPlayerLoggedIn
    )
    window.addOverlayListener(
      'CenturionPlayerLoggedOutEvent',
      store.onPlayerLoggedOut
    )
    window.addOverlayListener('CenturionZoneChangedEvent', store.onZoneChanged)
    window.addOverlayListener(
      'CenturionZoneInstanceChangedEvent',
      store.onZoneInstanceChanged
    )
    window.addOverlayListener(
      'CenturionLocationNotifiedEvent',
      store.onLocationNotified
    )
    window.addOverlayListener(
      'CenturionMobFAEvent',
      async (ev: IMobFAEvent) => {
        console.log(ev.toString(), ev)
      }
    )
    window.addOverlayListener(
      'CenturionMobTriggerEvent',
      async (ev: IMobTriggerEvent) => {
        console.log(ev.toString(), ev)
        if (settingsStore.tts.includes('SBpop')) {
          if (ev.triggerType === 'SSChallengeStarted') {
            tts('配下が湧きました')
          } else if (ev.triggerType === 'SSChallengeFailed') {
            tts('配下が帰還しました')
          }
        }
      }
    )
    window.addOverlayListener(
      'CenturionMobLocationEvent',
      async (ev: IMobLocationEvent) => {
        const { zoneId, mobId } = ev
        const zone = ZoneProvider.findFieldZone(zoneId)
        const mob = MobProvider.findMob(mobId)
        if (zone && mob) {
          const [x, y, z] = zone.toLocalPosXYZ(ev)
          console.log(`posting ${mobId} at Zone(${zoneId})(${x}, ${y}, ${z})`)
          // can't send CORS
          /*
          const url = `https://centurion.ffxiv.mydns.jp/api/mob-locations/${zoneId}`
          try {
            await fetch(url, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify({
                mobId,
                x,
                y,
                z
              })
            })
          } catch {
            console.error('oops')
          }
          */
        }
        console.log(ev)
      }
    )
    window.addOverlayListener(
      'CenturionMobStateChangedEvent',
      async (ev: IMobStateChangedEvent) => {
        const { mobId, state } = ev
        const mob = MobProvider.findMob(mobId)
        if (mob) {
          switch (state) {
            case 'Found':
              if (
                (mob.rank == TMobRank.B && settingsStore.tts.includes('B')) ||
                (mob.rank == TMobRank.A && settingsStore.tts.includes('A')) ||
                (mob.rank == TMobRank.S && settingsStore.tts.includes('S'))
              ) {
                tts(mob.tts)
              }
              break
            case 'Killed':
              if (
                mob.rank === TMobRank.B &&
                settingsStore.tts.includes('Brepop')
              ) {
                setTimeout(() => {
                  tts('Bモブリポップ')
                }, 13000)
              }
              break
            case 'StartCombat':
              if (
                mob.rank === TMobRank.S &&
                settingsStore.tts.includes('Sstart')
              ) {
                tts('戦闘開始')
              }
              break
          }
        }
      }
    )
    window.addOverlayListener('CenturionCombatData', async (ev) =>
      store.onCombatData(ev)
    )

    window.startOverlayEvents()
    window.callOverlayHandler({
      call: 'CenturionSetZoneMobs',
      data: { '10': [2, 4, 5, 6], '20': [1, 2, 3] }
    })
    window.callOverlayHandler({ call: 'getLanguage' }).then((language) => {
      console.log(
        `FFXIV ACT Plugin Act - Language: ${language.language}(${language.languageId})`
      )
      if (language.language === 'Japanese') {
        useLocale().setLocale('ja')
        settingsStore.language = 'ja'
      } else {
        if (language.language !== 'English') {
          console.warn('Locale fallback to en')
        }
        useLocale().setLocale('en')
        settingsStore.language = 'en'
      }
    })
    window
      .callOverlayHandler({
        call: 'CenturionGetVersion'
      })
      .then((value) => {
        if (value == null) {
          console.error('CenturionGetVersion returned null')
          errorCode.value = 1
        } else if (value.version != '1.0.0.0') {
          console.error(
            `CenturionGetVersion expected 1.0.0.0, actual ${value.version}`
          )
          errorCode.value = 2
        }
      })
    window.callOverlayHandler({
      call: 'CenturionInitClient'
    })
  }
  const tts = (text: string) => {
    window.callOverlayHandler({
      call: 'CenturionSay',
      text
    })
  }
  return { load, tts, disconnect }
}

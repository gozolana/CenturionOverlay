import dayjs from 'dayjs'
import 'dayjs/locale/ja'
import utc from 'dayjs/plugin/utc'
import { MessageProvider, TLang } from 'ffxiv-lib'
import { ref } from 'vue'
dayjs.extend(utc)

export const useLocale = () => {
  const locale = ref<TLang>('en')

  const setLocale = (lang: TLang) => {
    locale.value = lang
    MessageProvider.setLang(lang)
    dayjs.locale(lang)
  }

  if (navigator.language) {
    const lang = navigator.language.toLowerCase().split('-')[0]
    if (lang == 'ja') {
      setLocale('ja')
    }
    console.log('browser.lang', lang)
  }

  return {
    locale,
    setLocale,
  }
}

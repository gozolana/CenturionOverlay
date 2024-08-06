import '@mdi/font/css/materialdesignicons.css'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import { createApp } from 'vue'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import 'vuetify/styles'
import App from './App.vue'

const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

const vuetify = createVuetify({
  components,
  icons: {
    defaultSet: 'mdi'
  },
  directives
})

createApp(App).use(vuetify).use(pinia).mount('#app')

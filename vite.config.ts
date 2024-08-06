import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'
import { viteSingleFile } from 'vite-plugin-singlefile'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue(), viteSingleFile()],
  optimizeDeps: {
    exclude: ['ffxiv-lib'],
  },
})

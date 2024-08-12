<script setup lang="ts">
import { onBeforeUnmount, onMounted } from 'vue'
import Main from './components/Main.vue'
import TopBar from './components/TopBar.vue'
import WindowsBorders from './components/WindowBorders.vue'
import { useOverlay } from './composables/useOverlay'
const { load, disconnect } = useOverlay()
onMounted(async () => {
  load()
  /*  
const url = 'http://localhost:3001/api/mob-locations/134/types/elite'
  const res = await fetch(url)
  console.log(await res.json())
  */
})
onBeforeUnmount(() => disconnect())
</script>

<template>
  <div class="relative h-full w-full">
    <div class="info"></div>
    <WindowsBorders class="z-40 h-screen w-screen"></WindowsBorders>
    <div
      class="absolute right-0 top-6 z-50 h-[calc(100vh_-_1.5rem)] w-[calc(100vh_-_4.5rem)]"
    >
      <Main></Main>
    </div>
    <div class="absolute right-0 top-0 z-50 h-6 w-[calc(100vh_-_4.5rem)]">
      <TopBar></TopBar>
    </div>
  </div>
</template>

<style lang="scss">
html {
  overflow: hidden;
}
body {
  overflow: hidden;
  min-width: 100px;
  min-height: 100px;
  margin: 0;
  z-index: 0;
  background-color: transparent;
}
div.info {
  position: absolute;
  left: 0px;
  top: 0px;
  height: 200px;
  width: 200px;
  margin: 20px;

  z-index: 4;
  background-color: rgba($color: #000000, $alpha: 0.001);
}
</style>

<script setup lang="ts">
import { onBeforeUnmount, onMounted } from 'vue';
import Main from './components/Main.vue';
import { useOverlay } from './composables/useOverlay';
const { load, disconnect } = useOverlay()
onMounted(() => load())
onBeforeUnmount(() => disconnect())
</script>

<template>

  <div class="main"><Main></Main></div>
  <div class="info">
    Notification
  </div>
  <div class="handle"></div>
</template>

<style lang="scss">
html {
  overflow: hidden;
}
body {
  overflow: hidden;
  margin: 0;
  z-index: 0;
  background-color: white;
  position: relative;
}
div.main {
  height: calc(100vh);
  width: calc(100vh - 66px);
  z-index: 1;
  //background-color: rgb(174, 255, 0);
  position: absolute;
  right: 0px;
}
div.info {
  position: absolute;
  left: 0px;
  top: 0px;
  height: 200px;
  width: 200px;
  margin: 20px;

  z-index: 2;
  background-color: transparent;
}
div.handle {
  position: relative;
  bottom: 0px;
  z-index: -1;
  background-color: transparent;
  height: calc(100vh);
  width: calc(100vw);
  background-image: url(/handle.png);
  background-position: bottom right;
  background-repeat: no-repeat;
}
</style>
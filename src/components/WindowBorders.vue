<script setup lang="ts">
//import { toRefs } from 'vue'
//import { useOverlayStore } from '../store/overlay'
import { ref } from 'vue'

const showBorder = ref(false)

const mouseEntered = () => (showBorder.value = true)
const mouseLeft = () => (showBorder.value = false)
</script>

<template>
  <div class="relative bg-transparent">
    <div
      class="absolute top-0 h-2 w-full border-t-4 border-dashed border-t-white border-opacity-50 bg-white bg-opacity-5"
      :class="{ 'border-opacity-5': !showBorder }"
      @mouseenter="mouseEntered"
      @mouseleave="mouseLeft"
    ></div>
    <div
      class="absolute left-0 h-full w-2 border-l-4 border-dashed border-l-white border-opacity-50 bg-white bg-opacity-5"
      :class="{ 'border-opacity-5': !showBorder }"
      @mouseenter="mouseEntered"
      @mouseleave="mouseLeft"
    ></div>
    <div
      class="absolute bottom-0 h-2 w-full border-b-4 border-dashed border-b-white border-opacity-50 bg-white bg-opacity-5"
      :class="{ 'border-opacity-5': !showBorder }"
      @mouseenter="mouseEntered"
      @mouseleave="mouseLeft"
    ></div>
    <div
      class="absolute right-0 h-full w-2 border-r-4 border-dashed border-r-white border-opacity-50 bg-white bg-opacity-5"
      :class="{ 'border-opacity-5': !showBorder }"
      @mouseenter="mouseEntered"
      @mouseleave="mouseLeft"
    ></div>
    <div
      class="absolute bottom-0 right-0 h-4 w-4 bg-[url('handle.png')]"
      @mouseenter="mouseEntered"
      @mouseleave="mouseLeft"
    ></div>
  </div>
</template>

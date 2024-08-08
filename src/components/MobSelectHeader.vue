<script setup lang="ts">
import { MessageProvider, ZoneProvider } from 'ffxiv-lib';
import ToggleButton from 'primevue/togglebutton';
import { computed, toRefs } from 'vue';
import { useOverlayStore } from '../store/overlay';
import { useOverlaySettingsStore } from '../store/overlaySettings';

const { showHeader, displayA, displayA2, displayB, displayB2, displayS, displaySS } = toRefs(useOverlaySettingsStore())

const zone = computed(() =>
  ZoneProvider.findFieldZone(useOverlayStore().zoneId)
)
</script>

<template>
  <div class="grid grid-rows-2 grid-cols-3 gap-0" v-if="zone && showHeader">
    <div :class="[zone.ss ? 'row-span-1' : 'row-span-2']">
      <ToggleButton class="w-full h-full truncate text-white bg-orange-700" 
      :class="{ 'brightness-50': !displayS }" v-model="displayS"  
      >{{MessageProvider.getBNpcName(zone.elite.sId)}}</ToggleButton>
    </div>
    <div :class="[zone.elite.length > 3 ? 'row-span-1' : 'row-span-2']">
      <ToggleButton class="w-full h-full truncate text-white bg-amber-400" 
      :class="{ 'brightness-50': !displayA }" v-model="displayA"  
      >{{MessageProvider.getBNpcName(zone.elite.aIds[0])}}</ToggleButton>
    </div>
    <div :class="[zone.elite.length > 3 ? 'row-span-1' : 'row-span-2']">
      <ToggleButton class="w-full h-full truncate text-white bg-blue-400" 
      :class="{ 'brightness-50': !displayB }" v-model="displayB"  
      >{{MessageProvider.getBNpcName(zone.elite.bIds[0])}}</ToggleButton>
    </div>
          <div v-if="zone.ss">
      <ToggleButton class="w-full   h-full truncate text-white bg-gray-700" 
      :class="{ 'brightness-50': !displaySS }" v-model="displaySS" 
      >{{ MessageProvider.getBNpcName(zone.ss.sId) }}</ToggleButton>
    </div>
    <div v-if="zone.elite.length > 3">
      <ToggleButton class="w-full h-full truncate text-white bg-green-600" 
      :class="{ 'brightness-50': !displayA2 }" v-model="displayA2"  
      >{{MessageProvider.getBNpcName(zone.elite.aIds[1])}}</ToggleButton>
    </div>
    <div v-if="zone.elite.length > 3">
      <ToggleButton class="w-full h-full truncate text-white bg-fuchsia-800"
      :class="{ 'brightness-50': !displayB2 }" v-model="displayB2"  
      >{{MessageProvider.getBNpcName(zone.elite.bIds[1])}}</ToggleButton>
    </div>
  </div>
</template>

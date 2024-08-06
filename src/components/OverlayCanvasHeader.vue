<script setup lang="ts">
import { MessageProvider, ZoneProvider } from 'ffxiv-lib';
import { computed, toRefs } from 'vue';
import { useOverlayStore } from '../store/overlay';
import { useOverlaySettingsStore } from '../store/overlaySettings';

const { showHeader, displayA, displayA2, displayB, displayB2, displayS, displaySS } = toRefs(useOverlaySettingsStore())

const zone = computed(() =>
  ZoneProvider.findFieldZone(useOverlayStore().zoneId)
)
</script>

<template>
  <div class="mob-selector pa-0" v-if="zone && showHeader">
    aaa
    <v-row class="ma-0">
      <v-col cols="4" class="pa-0">
        <v-btn-toggle
          rounded="0"
          :class="zone.ss ? 'full-h20' : 'full-h40'"
          v-model="displayS"
        >
          <v-btn variant="text" size="x-small" class="ffcolor-red text-white">{{
            MessageProvider.getBNpcName(zone.elite.sId)
          }}</v-btn>
        </v-btn-toggle>
        <v-btn-toggle
          rounded="0"
          class="full-h20"
          v-model="displaySS"
          v-if="zone.ss"
        >
          <v-btn
            variant="text"
            size="x-small"
            class="ffcolor-grey text-white"
            >{{ MessageProvider.getBNpcName(zone.ss.sId) }}</v-btn
          >
        </v-btn-toggle>
      </v-col>
      <v-col cols="4" class="pa-0">
        <v-btn-toggle
          rounded="0"
          :class="zone.elite.length > 3 ? 'full-h20' : 'full-h40'"
          v-model="displayA"
        >
          <v-btn
            variant="text"
            size="x-small"
            class="ffcolor-yellow text-white"
            >{{ MessageProvider.getBNpcName(zone.elite.aIds[0]) }}</v-btn
          >
        </v-btn-toggle>
        <v-btn-toggle
          rounded="0"
          class="full-h20"
          v-model="displayA2"
          v-if="zone.elite.length > 3"
        >
          <v-btn
            variant="text"
            size="x-small"
            class="ffcolor-green text-white"
            >{{ MessageProvider.getBNpcName(zone.elite.aIds[1]) }}</v-btn
          >
        </v-btn-toggle>
      </v-col>
      <v-col cols="4" class="pa-0">
        <v-btn-toggle
          rounded="0"
          :class="zone.elite.length > 3 ? 'full-h20' : 'full-h40'"
          v-model="displayB"
        >
          <v-btn
            variant="text"
            size="x-small"
            class="ffcolor-blue text-white"
            >{{ MessageProvider.getBNpcName(zone.elite.bIds[0]) }}</v-btn
          >
        </v-btn-toggle>
        <v-btn-toggle
          rounded="0"
          class="full-h20"
          v-model="displayB2"
          v-if="zone.elite.length > 3"
        >
          <v-btn
            variant="text"
            size="x-small"
            class="ffcolor-purple text-white"
            >{{ MessageProvider.getBNpcName(zone.elite.bIds[1]) }}</v-btn
          >
        </v-btn-toggle>
      </v-col>
    </v-row>
  </div>
</template>

<style scoped lang="scss">
.mob-selector {
  line-height: 0;
  padding: 0px;

  .full-h40 {
    width: 100%;
    height: 40px !important;
  }
  .full-h20 {
    width: 100%;
    height: 20px !important;
  }

  .ffcolor-red {
    background-color: #ca330f;
    filter: brightness(50%);
  }
  .ffcolor-grey {
    background-color: #1f8cad;
    filter: brightness(50%);
  }
  .ffcolor-yellow {
    background-color: #dfa824;
    filter: brightness(50%);
  }
  .ffcolor-blue {
    background-color: #3171ba;
    filter: brightness(50%);
  }
  .ffcolor-green {
    background-color: #3c933b;
    filter: brightness(50%);
  }
  .ffcolor-purple {
    background-color: #a54674;
    filter: brightness(50%);
  }

  .v-btn--active {
    filter: brightness(100%);
  }
  .v-btn-toggle .v-btn {
    width: 100%;
    height: 100%;
    text-overflow: clip;
  }
}
</style>

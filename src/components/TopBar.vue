<script setup lang="ts">
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import SelectButton from 'primevue/selectbutton'
import Toolbar from 'primevue/toolbar'
import { useOverlayStore } from '../store/overlay'
import { DisplayMode, useOverlaySettingsStore } from '../store/overlaySettings'

import { ref } from 'vue'

const store = useOverlayStore()
const settingsStore = useOverlaySettingsStore()

const visible = ref(true)
const options = ref([
  { name: 'MobHunt', value: DisplayMode.MobHunt },
  { name: 'Fate', value: DisplayMode.Fate },
  { name: 'Gathering', value: DisplayMode.Gathering }
])
</script>

<template>
  <Toolbar class="flex h-6 bg-blue-800 text-white">
    <template #start>
      <Button label="Login" class="h-6" @click="visible = true">
        <i class="material-icons">menu</i>
      </Button>
    </template>
    <template #center>{{ store.playerName }}</template>
    <template #end> </template>
  </Toolbar>

  <Dialog v-model:visible="visible" class="border-0 bg-white" position="left">
    <template #container="{ closeCallback }">
      <div class="flex flex-col gap-6 rounded-2xl px-8 py-8">
        <div class="inline-flex flex-col gap-2">
          <SelectButton
            class="p-2"
            v-model="settingsStore.displayMode"
            :options="options"
            optionLabel="name"
            optionValue="value"
          />
          {{ settingsStore.displayMode }}
          <label for="username" class="text-primary-50 font-semibold"
            >Username</label
          >
          <InputText
            id="username"
            class="!text-primary-50 w-80 !border-0 !bg-white/20 !p-4"
          ></InputText>
        </div>
        <div class="inline-flex flex-col gap-2">
          <label for="password" class="text-primary-50 font-semibold"
            >Password</label
          >
          <InputText
            id="password"
            class="!text-primary-50 w-80 !border-0 !bg-white/20 !p-4"
            type="password"
          ></InputText>
        </div>
        <div class="flex items-center gap-4">
          <Button
            label="Cancel"
            @click="closeCallback"
            text
            class="!text-primary-50 w-full !border !border-white/30 !p-4 hover:!bg-white/10"
          ></Button>
          <Button
            label="Sign-In"
            @click="closeCallback"
            text
            class="!text-primary-50 w-full !border !border-white/30 !p-4 hover:!bg-white/10"
          ></Button>
        </div>
      </div>
    </template>
  </Dialog>
</template>

<style lang="scss" scoped></style>

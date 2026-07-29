<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { BFF } from '../api/bff'
import type { Tile } from '../types/dashboard'

const tiles = ref<Tile[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const userName = ref<string>('User')
const isSigningOut = ref(false)

const handleLogout = () => {
  error.value = null
  isSigningOut.value = true

  BFF.logout()
}

const loadDashboard = async () => {
  const data = await BFF.dashboard()
  tiles.value = data.tiles
}

onMounted(async () => {
  try {
    const session = await BFF.session()
    if (!session) {
      BFF.login()
      return
    }
    if (session?.userName) userName.value = session.userName
    
    await loadDashboard()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Something went wrong'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="min-h-screen flex flex-col items-center pt-16 bg-[#0f2244] space-y-6">

    <!-- User profile box -->
    <div class="border border-white/50 w-72 py-8 text-center flex flex-col items-center gap-3">
      <span class="text-white text-2xl font-bold">{{ userName }}</span>
      <button
        @click="handleLogout"
        :disabled="isSigningOut"
        class="border border-white/50 text-white text-sm px-3 py-1.5 hover:bg-white/10 transition-colors disabled:opacity-60"
      >
        {{ isSigningOut ? 'Signing out...' : 'Sign out' }}
      </button>
    </div>

    <!-- Service tiles -->
    <div v-if="!loading && !error" class="w-72 flex flex-col gap-2">
      <div
        v-for="tile in tiles"
        :key="tile.provider"
        class="border border-white/50 px-4 py-3"
      >
        <div class="flex items-center justify-between">
          <span class="text-white font-medium capitalize">
            {{ tile.provider }}
          </span>

          <button
            v-if="!tile.connected"
            @click="BFF.connect(tile.provider)"
            class="border border-white/50 text-white text-xs px-3 py-1.5 hover:bg-white/10 transition-colors"
          >
            Connect
          </button>

          <span
            v-else
            class="text-emerald-400 text-xs font-medium"
          >
            Connected
          </span>
        </div>

        <div
          v-if="tile.connected"
          class="mt-2 text-xs space-y-1"
        >
          <div
            v-if="tile.connectedAccount"
            class="text-white/60"
          >
            Signed in as
            <span class="text-white">{{ tile.connectedAccount }}</span>
          </div>

          <div class="flex items-center justify-center">
            <button
              @click="async () => {
                await BFF.unlink(tile.provider)
                await loadDashboard()
              }"
              class="text-white/50 hover:text-white transition-colors"
            >
            Remove link
            </button>
          </div>
        </div>

        <div v-if="!tile.connected && tile.provider === 'github'"
          class="mt-2 text-xs text-white/50"
        >
          To connect a different GitHub account, it's recommended to 
          <a
            href="https://github.com/logout"
            target="_blank"
            rel="noopener noreferrer"
            class="text-blue-400 hover:underline"
          >
            sign out of Github
          </a>
          before connecting.
        </div>
      </div>
    </div>

    <p v-if="loading" class="text-white/40 text-sm">Loading...</p>
    <p v-if="error"   class="text-red-400 text-sm">{{ error }}</p>

  </div>
</template>
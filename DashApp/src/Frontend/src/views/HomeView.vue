<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { BFF } from '../api/bff'

const router = useRouter()

onMounted(async () => {
  try {
    const session = await BFF.session()
    if (session) router.push('/dashboard')
  } catch {
    // session check failed — stay on home page, let user sign in manually
  }
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-[#0f2244]">
    <div class="border border-white/50 p-10 w-72 text-center space-y-8">

      <div class="space-y-3">
        <h1 class="text-4xl font-bold text-white tracking-wide">DashApp</h1>
        <p class="text-white/60 text-sm leading-relaxed">
          Connect your services<br>in one place
        </p>
      </div>

      <button
        @click="BFF.login()"
        class="w-full py-2.5 bg-black text-white text-sm font-medium hover:bg-gray-900 transition-colors"
      >
        Sign in using Microsoft
      </button>

    </div>
  </div>
</template>
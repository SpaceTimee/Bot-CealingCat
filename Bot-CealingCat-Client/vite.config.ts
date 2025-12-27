import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import plugin from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [plugin()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})

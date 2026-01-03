import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import plugin from '@vitejs/plugin-vue'
import { VitePWA } from 'vite-plugin-pwa'
import Sitemap from 'vite-plugin-sitemap'

export default defineConfig({
  plugins: [
    plugin(),
    VitePWA({
      registerType: 'autoUpdate',
      manifest: {
        name: 'Bot CealingCat',
        short_name: 'Bot CealingCat',
        description: 'Cealing 猫猫，把你超超',
        theme_color: '#ea909a',
        icons: [
          {
            src: 'pwa-192x192.png',
            sizes: '192x192',
            type: 'image/png'
          },
          {
            src: 'pwa-512x512.png',
            sizes: '512x512',
            type: 'image/png'
          }
        ]
      }
    }),
    Sitemap({
      hostname: 'https://bot.spacetimee.xyz'
    })
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})

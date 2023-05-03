<script setup lang="ts">
import { useGameServerStats } from '@/composables/use-game-server-stats';

definePage({
  meta: {
    layout: 'empty',
  },
});

const { gameServerStats, loadGameServerStats } = useGameServerStats();

await loadGameServerStats();
</script>

<template>
  <div class="relative h-screen p-4 md:p-8">
    <Bg bg="background-1.webp" />

    <div class="relative flex h-full items-center border border-border-300 text-content-200">
      <SwitchLanguageDropdown class="absolute right-6 top-6">
        <template #default="{ shown, locale }">
          <OButton :variant="shown ? 'transparent-active' : 'transparent'" size="sm">
            <span class="text-xs font-normal">{{ locale.toUpperCase() }}</span>
            <div class="flex items-center gap-2.5">
              <SvgSpriteImg :name="`locale-${locale}`" viewBox="0 0 18 18" class="w-4" />
              <div class="h-4 w-px select-none bg-border-300"></div>
              <OIcon
                icon="chevron-down"
                size="lg"
                :rotation="shown ? 180 : 0"
                class="text-content-400"
              />
            </div>
          </OButton>
        </template>
      </SwitchLanguageDropdown>

      <div class="mx-auto space-y-8 md:w-1/2 2xl:w-1/3 2xl:space-y-10">
        <div class="item-center mb-6 flex select-none justify-center gap-6 md:gap-12">
          <SvgSpriteImg
            name="logo-decor"
            viewBox="0 0 108 10"
            class="w-16 rotate-180 transform xl:w-20 2xl:w-24"
          />
          <SvgSpriteImg name="logo" viewBox="0 0 162 124" class="w-24 xl:w-36 2xl:w-40" />
          <SvgSpriteImg name="logo-decor" viewBox="0 0 108 10" class="w-16 xl:w-20 2xl:w-24" />
        </div>

        <Divider />

        <div class="prose prose-invert mx-auto text-center">
          <i18n-t keypath="homePage.intro" tag="p" scope="global">
            <template #link>
              <a
                class="text-content-link hover:text-content-link-hover"
                href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord"
                target="_blank"
              >
                Mount & Blade II: Bannerlord.
              </a>
            </template>
          </i18n-t>

          <p>
            {{ $t('homePage.description') }}
          </p>
        </div>

        <div class="flex justify-center">
          <OnlinePlayers :gameServerStats="gameServerStats" showLabel />
        </div>

        <div class="flex justify-center gap-4">
          <Login />
          <InstallationGuide />
        </div>

        <Divider />

        <Socials class="justify-center" />
      </div>
    </div>
  </div>
</template>

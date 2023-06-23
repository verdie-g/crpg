<script setup lang="ts">
import { useGameServerStats } from '@/composables/use-game-server-stats';

import { getLeaderBoard } from '@/services/leaderboard-service';

const { state: leaderBoard } = useAsyncState(() => getLeaderBoard(), [], {
  // immediate: false,
});

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
      <div class="absolute right-6 top-6 flex items-center gap-6">
        <OnlinePlayers :gameServerStats="gameServerStats" showLabel />

        <SwitchLanguageDropdown>
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
      </div>

      <div class="mx-auto flex flex-col items-center justify-center gap-14 md:w-1/2 2xl:w-1/3">
        <div class="space-y-6">
          <div class="item-center flex select-none justify-center gap-6 md:gap-12">
            <SvgSpriteImg name="logo-decor" viewBox="0 0 108 10" class="w-24 rotate-180" />
            <SvgSpriteImg name="logo" viewBox="0 0 162 124" class="w-24 xl:w-28 2xl:w-32" />
            <SvgSpriteImg name="logo-decor" viewBox="0 0 108 10" class="w-24" />
          </div>
        </div>

        <div class="prose prose-invert text-center">
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

          <iframe
            class="mx-auto block aspect-video w-full w-full rounded-lg lg:w-3/4 xl:w-2/3"
            src="https://www.youtube-nocookie.com/embed/MZx56LD0o4c"
            title="YouTube video player"
            frameborder="0"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
            allowfullscreen
          />
        </div>

        <div class="flex justify-center gap-4">
          <Login />
          <InstallationGuide />
        </div>

        <Socials class="absolute bottom-6 left-6" />
      </div>
    </div>
  </div>
</template>

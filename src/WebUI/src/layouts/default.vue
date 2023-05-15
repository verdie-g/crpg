<script setup lang="ts">
import { useTransition } from '@vueuse/core';
import { useElementSize } from '@vueuse/core';
import VueCountdown from '@chenfengyuan/vue-countdown';
import { Region } from '@/models/region';
import { useUserStore } from '@/stores/user';
import { logout } from '@/services/auth-service';
import { getUserActiveJoinRestriction } from '@/services/users-service';
import { useHappyHours } from '@/composables/use-hh';
import { useGameServerStats } from '@/composables/use-game-server-stats';
import { usePollInterval } from '@/composables/use-poll-interval';
import { mainHeaderHeightKey } from '@/symbols/common';
import { scrollToTop } from '@/utils/scroll';

const userStore = useUserStore();
const route = useRoute();

const { state: joinRestrictionRemainingDuration, execute: loadJoinRestriction } = useAsyncState(
  () => getUserActiveJoinRestriction(userStore.user!.id),
  null,
  {
    immediate: false,
  }
);

const {
  HHEvent,
  HHEventRemaining,
  isHHCountdownEnded,
  onStartHHCountdown,
  onEndHHCountdown,
  transformSlotProps,
} = useHappyHours();

const { gameServerStats, loadGameServerStats } = useGameServerStats();

const animatedUserGold = useTransition(toRef(() => userStore.user!.gold));

const promises: Array<Promise<any>> = [loadGameServerStats(), loadJoinRestriction()];

if (userStore.clan === null) {
  promises.push(userStore.getUserClanAndRole());
}

const mainHeader = ref(null);
const { height: mainHeaderHeight } = useElementSize(
  mainHeader,
  { width: 0, height: 0 },
  { box: 'border-box' }
);
provide(mainHeaderHeightKey, mainHeaderHeight);

const { subscribe, unsubscribe } = usePollInterval();
const id = Symbol('fetchUser');

onMounted(() => {
  subscribe(id, userStore.fetchUser);
});

onBeforeUnmount(() => {
  unsubscribe(id);
});

await Promise.all(promises);
</script>

<template>
  <div class="relative flex min-h-screen flex-col">
    <Bg v-if="route.meta?.bg" :bg="route.meta.bg" />

    <header
      ref="mainHeader"
      class="z-20 border-b border-solid border-border-200 bg-bg-main"
      :class="{ 'sticky top-0 bg-opacity-10 backdrop-blur-sm': !route.meta?.noStickyHeader }"
    >
      <UserRestrictionNotification
        v-if="joinRestrictionRemainingDuration !== null"
        :restriction="joinRestrictionRemainingDuration"
      />

      <div class="flex flex-wrap items-center justify-between px-6 py-3">
        <div class="flex items-center gap-6">
          <RouterLink :to="{ name: 'Root' }">
            <SvgSpriteImg name="logo" viewBox="0 0 162 124" class="w-16" />
          </RouterLink>

          <div class="flex items-center gap-2">
            <OnlinePlayers :gameServerStats="gameServerStats" />
          </div>

          <!-- TODO: to divider -->
          <div class="h-8 w-px select-none bg-border-200" />

          <MainNavigation />

          <template v-if="isHHCountdownEnded && HHEventRemaining !== 0">
            <div class="h-8 w-px select-none bg-border-200" />

            <HHTooltip :region="userStore.user!.region!">
              <div
                class="flex cursor-pointer items-center gap-2 text-sm text-primary hover:text-primary-hover"
              >
                <OIcon icon="gift" size="sm" />
                <VueCountdown
                  class="w-24"
                  :time="HHEventRemaining"
                  :transform="transformSlotProps"
                  v-slot="{ hours, minutes, seconds }"
                  @start="onStartHHCountdown"
                  @end="onEndHHCountdown"
                >
                  {{ $t('dateTimeFormat.countdown', { hours, minutes, seconds }) }}
                </VueCountdown>
              </div>
            </HHTooltip>
          </template>
        </div>

        <div v-if="userStore.user" class="gap flex items-center gap-5">
          <Coin
            :value="Number(animatedUserGold.toFixed(0))"
            v-tooltip.bottom="$t('user.field.gold')"
          />

          <!-- TODO: to divider -->
          <div class="h-8 w-px select-none bg-border-200" />

          <div
            class="flex items-center gap-2 font-bold text-primary"
            v-tooltip.bottom="$t('user.field.heirloom')"
          >
            <OIcon icon="blacksmith" size="lg" />
            {{ userStore.user.heirloomPoints }}
          </div>

          <!-- TODO: to divider -->
          <div class="h-8 w-px select-none bg-border-200" />

          <UserMedia
            :user="{ ...userStore.user, avatar: userStore.user.avatar }"
            :clan="userStore.clan"
            :clanRole="userStore.clanMemberRole"
            hiddenPlatform
            size="lg"
          />

          <!-- TODO: to divider -->
          <div class="h-8 w-px select-none bg-border-200" />

          <VDropdown :triggers="['click']" placement="bottom-end" class="-ml-2.5">
            <template #default="{ shown }">
              <OButton
                :variant="shown ? 'transparent-active' : 'transparent'"
                size="xl"
                rounded
                iconLeft="dots"
              />
            </template>

            <template #popper="{ hide }">
              <SwitchLanguageDropdown #default="{ shown, locale }" placement="left-start">
                <DropdownItem :active="shown">
                  <SvgSpriteImg :name="`locale-${locale}`" viewBox="0 0 18 18" class="w-5" />
                  {{ $t('setting.language') }} | {{ locale.toUpperCase() }}
                </DropdownItem>
              </SwitchLanguageDropdown>

              <!-- TODO: DropdownItem tag RouterLink -->
              <DropdownItem tag="RouterLink" :to="{ name: 'Settings' }" @click="hide">
                <OIcon icon="settings" size="lg" />
                {{ $t('setting.settings') }}
              </DropdownItem>

              <DropdownItem
                @click="
                  () => {
                    hide();
                    logout();
                  }
                "
              >
                <OIcon icon="logout" size="lg" />
                {{ $t('setting.logout') }}
              </DropdownItem>
            </template>
          </VDropdown>
        </div>
      </div>
    </header>

    <main class="relative flex-1">
      <RouterView />
    </main>

    <footer
      v-if="!route.meta.noFooter"
      class="relative mt-auto flex flex-wrap items-center justify-between border-t border-solid border-border-200 px-6 py-5 text-2xs text-content-300"
    >
      <Socials patreonExpanded size="sm" />

      <div class="flex items-center gap-5">
        <HHTooltip #default="{ shown }" :region="userStore.user!.region!">
          <div
            class="group flex cursor-pointer select-none items-center gap-2 hover:text-content-100"
            :class="{ 'text-content-100': shown }"
          >
            <OIcon icon="gift" size="lg" class="text-content-100" />
            {{
              $t('hh.tooltip-trigger', {
                region: $t(`region.${userStore.user?.region ?? Region.Eu}`, 1),
              })
            }}
            <span
              class="group-hover:text-content-100"
              :class="[shown ? 'text-content-100' : 'text-content-200']"
            >
              {{ $d(HHEvent.start, 'time') }} - {{ $d(HHEvent.end, 'time') }}
            </span>
          </div>
        </HHTooltip>

        <div class="h-8 w-px select-none bg-border-200" />

        <OButton
          v-tooltip="$t('scrollToTop')"
          variant="transparent"
          size="xl"
          iconRight="arrow-up"
          rounded
          @click="scrollToTop"
        />
      </div>
    </footer>
  </div>
</template>

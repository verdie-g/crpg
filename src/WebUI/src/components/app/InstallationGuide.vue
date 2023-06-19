<script setup lang="ts">
import { Platform } from '@/models/platform';
import { platformToIcon } from '@/services/platform-service';
import { usePlatform } from '@/composables/use-platform';

const { platform } = usePlatform();

enum PossibleValues {
  Steam = 'Steam',
  Other = 'Other',
}

const tabsModel = ref<PossibleValues>(PossibleValues.Steam);

watch(
  () => platform.value,
  () => {
    tabsModel.value =
      platform.value === Platform.Steam ? PossibleValues.Steam : PossibleValues.Other;
  },
  {
    immediate: true,
  }
);
</script>

<template>
  <Modal closable>
    <OButton
      variant="secondary"
      size="xl"
      iconLeft="download"
      target="_blank"
      :label="$t('installation.title')"
    />

    <template #popper>
      <div class="space-y-10 py-10">
        <div class="prose prose-invert space-y-10 px-12">
          <h3 class="text-center">{{ $t('installation.title') }}</h3>

          <OTabs v-model="tabsModel" size="xl" :animated="false">
            <OTabItem
              :label="$t(`platform.${Platform.Steam}`)"
              :icon="platformToIcon[Platform.Steam]"
              :value="PossibleValues.Steam"
            >
              <div class="space-y-6">
                <ol>
                  <i18n-t scope="global" keypath="installation.platform.steam.subscribe" tag="li">
                    <template #steamWorkshopsLink>
                      <a
                        target="_blank"
                        href="https://steamcommunity.com/sharedfiles/filedetails/?id=2878356589"
                      >
                        Steam Workshops
                      </a>
                    </template>
                  </i18n-t>
                  <li>{{ $t('installation.common.bannerlordLauncher') }}</li>
                  <li>{{ $t('installation.common.multiplayerModsTab') }}</li>
                  <li>{{ $t('installation.common.activateMod') }}</li>
                  <li>{{ $t('installation.common.launchMultiplayerGame') }}</li>
                </ol>
                <p class="text-content-400">{{ $t('installation.platform.steam.update') }}</p>
              </div>
            </OTabItem>

            <OTabItem :value="PossibleValues.Other">
              <template #header>
                <OIcon :icon="platformToIcon[Platform.Microsoft]" size="xl" />
                <OIcon :icon="platformToIcon[Platform.EpicGames]" size="xl" />
                {{ $t('installation.platform.other.title') }}
              </template>
              <div>
                <ol>
                  <i18n-t
                    scope="global"
                    keypath="installation.platform.other.downloadLauncher"
                    tag="li"
                  >
                    <template #launcherLink>
                      <a target="_blank" href="https://www.moddb.com/mods/crpg/downloads/crpg-launcher1">launcher</a>
                    </template>
                  </i18n-t>
                  <li>{{ $t('installation.platform.other.startLauncher') }}</li>
                  <li>{{ $t('installation.common.bannerlordLauncher') }}</li>
                  <li>{{ $t('installation.common.multiplayerModsTab') }}</li>
                  <li>{{ $t('installation.common.activateMod') }}</li>
                  <li>{{ $t('installation.common.launchMultiplayerGame') }}</li>
                </ol>
                <p class="text-content-400">{{ $t('installation.platform.other.update') }}</p>
              </div>
            </OTabItem>
          </OTabs>
        </div>

        <Divider />

        <div class="prose prose-invert px-12">
          <i18n-t scope="global" keypath="installation.help" tag="p" class="text-content-400">
            <template #discordLink>
              <a
                target="_blank"
                href="https://discord.com/channels/279063743839862805/761283333840699392"
              >
                Discord
              </a>
            </template>
          </i18n-t>
        </div>
      </div>
    </template>
  </Modal>
</template>

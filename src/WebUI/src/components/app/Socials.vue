<script setup lang="ts">
const props = withDefaults(defineProps<{ patreonExpanded?: boolean; size?: string }>(), {
  patreonExpanded: false,
  size: 'xl',
});

interface SocialLink {
  id: string;
  title: string;
  href: string;
  icon: string;
}

const socialsLinks: SocialLink[] = [
  {
    id: 'patreon',
    title: 'Patreon',
    icon: 'patreon',
    href: 'https://www.patreon.com/crpg',
  },
  {
    id: 'discord',
    title: 'Discord',
    icon: 'discord',
    href: 'https://discord.gg/c-rpg',
  },
  {
    id: 'reddit',
    title: 'Reddit',
    icon: 'reddit',
    href: 'https://www.reddit.com/r/CRPG_Bannerlord',
  },
  {
    id: 'moddb',
    title: 'Moddb',
    icon: 'moddb',
    href: 'https://www.moddb.com/mods/crpg',
  },
  {
    id: 'steam',
    title: 'Steam',
    icon: 'steam',
    href: 'https://steamcommunity.com/sharedfiles/filedetails/?id=2878356589',
  },
  {
    id: 'github',
    title: 'Github',
    icon: 'github',
    href: 'https://github.com/namidaka/crpg',
  },
];

const links = computed(() =>
  props.patreonExpanded ? socialsLinks.filter(l => l.id !== 'patreon') : socialsLinks
);

const patreonLink = computed(() => socialsLinks.find(l => l.id === 'patreon')!);
</script>

<template>
  <div class="flex flex-wrap items-center gap-6">
    <template v-if="patreonExpanded">
      <div v-html="$t('patreon')" />

      <OButton
        variant="secondary"
        :size="size"
        outlined
        tag="a"
        :icon-left="patreonLink.icon"
        :href="patreonLink.href"
        target="_blank"
        label="Patreon"
      />

      <div class="h-8 w-px select-none bg-border-200" />
    </template>

    <div class="flex flex-wrap items-center gap-4">
      <OButton
        v-for="social in links"
        v-tooltip.bottom="social.title"
        variant="secondary"
        :size="size"
        outlined
        rounded
        tag="a"
        :icon-left="social.icon"
        :href="social.href"
        target="_blank"
      />
    </div>
  </div>
</template>

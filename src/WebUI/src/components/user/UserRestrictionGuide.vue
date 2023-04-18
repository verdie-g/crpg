<script setup lang="ts"></script>

<template>
  <div class="space-y-10">
    <Divider />

    <div class="prose prose-invert px-12">
      <p class="">
        {{ $t('user.restriction.guide.intro') }}
      </p>

      <ol>
        <i18n-t scope="global" keypath="user.restriction.guide.step.join" tag="li">
          <template #discordLink>
            <a
              class="text-content-link hover:text-content-link-hover"
              target="_blank"
              href="https://discord.gg/c-rpg"
            >
              Discord
            </a>
          </template>
        </i18n-t>
        <i18n-t scope="global" keypath="user.restriction.guide.step.navigate" tag="li">
          <template #modMailLink>
            <!-- prettier-ignore -->
            <a
              class="text-content-link hover:text-content-link-hover"
              target="_blank"
              href="https://discord.com/channels/279063743839862805/1034895358435799070"
            >ModMail</a>
          </template>
        </i18n-t>
        <li>{{ $t('user.restriction.guide.step.follow') }}</li>
      </ol>
    </div>

    <Divider />

    <div class="px-12">
      <p class="text-content-400">
        {{ $t('user.restriction.guide.outro') }}
      </p>
    </div>
  </div>
</template>

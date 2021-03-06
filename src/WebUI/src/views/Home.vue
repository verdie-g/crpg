<template>
  <div class="container">
    <section class="section">
      <div class="content is-large">
        <h1>cRPG</h1>
        <p>
          cRPG is a mod for
          <a
            href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord"
            target="_blank"
          >
            Mount & Blade II: Bannerlord
          </a>
          . It adds persistence to the multiplayer. You start as a peasant and you'll develop your
          unique character with different stats and items.
        </p>

        <b-button
          size="is-large"
          icon-left="steam-symbol"
          icon-pack="fab"
          @click="onClick"
          v-if="!isSignedIn"
        >
          Sign in through Steam
        </b-button>
      </div>

      <div class="content is-medium">
        <h2>F.A.Q</h2>
        <dl id="faq">
          <template v-for="qa in faq">
            <dt>{{ qa.question }}</dt>
            <dd v-html="qa.answer"></dd>
          </template>
        </dl>

        <script v-html="faqJsonLd" type="application/ld+json"></script>
      </div>
    </section>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { signIn } from '@/services/auth-service';
import userModule from '@/store/user-module';

@Component
export default class Home extends Vue {
  faq = [
    {
      question: 'Is the mod available?',
      answer:
        'The mod was announced recently in this' +
        ' <a href="https://forums.taleworlds.com/index.php?threads/crpg.432051" target="_blank">Forum thread</a>.' +
        ' As TaleWorlds has not provided the tools to make multiplayer mods yet, only a singleplayer mod is available on' +
        ' <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/2208?tab=files" target="_blank">Nexus Mods</a>.' +
        ' Follow the development on the <a href="https://discord.gg/83RJDN9" target="_blank">Discord</a> or' +
        ' <a href="https://forum.c-rpg.eu" target="_blank">forum</a>.',
    },
    {
      question: 'How to create a new character?',
      answer: 'Simply launch a DefendTheVirgin game and a level character 1 should be created.',
    },
    {
      question: 'How to gain experience and gold?',
      answer: 'Experience and gold are gained by playing the singleplayer cRPG mod.',
    },
    {
      question: 'Will my progress (experience, gold) be saved until the official release of cRPG?',
      answer:
        'Once the multiplayer mods are available and that cRPG is released, the database will be wiped.',
    },
    {
      question:
        "I've renamed my steam account and a new character level 1 was created, how can I play with my original character?",
      answer:
        "When you connect to a cRPG server, it searches for a character with the same name as your steam name. If it doesn't find" +
        ' one, a new character is created. To use your original character, rename it to your new steam name it in the Web UI.',
    },
    {
      question: 'What is retiring?',
      answer:
        'When a cRPG character reaches level 31, it can be retired. Retiring a character resets it to level 1 but grants an' +
        ' experience bonus multiplier and an heirloom point which can used on an item to increase its statistics.',
    },
    {
      question: 'What is respecialization?',
      answer:
        'If you want to play a different class of soldier, you can respecialize (or respec) to reset your character stats for the' +
        ' cost of half of your experience.',
    },
    // {
    // question: 'Can I host my own cRPG game server?',
    // answer: 'For security and maintainability reasons, players won't be able to host their own server.','
    // },
    {
      question: 'Is the project open-source?',
      answer: 'Not decided yet.',
    },
    {
      question: 'How can I help?',
      answer:
        'We\'re looking for:<ul style="margin-top: 0">' +
        '<li>Game developer (.NET)</li>' +
        '<li>UX designer for this website</li>' +
        '<li>Front-end developer (Vue.js) for this website</li>' +
        "</ul>If you don't have any of the above skills you can also donate on the Patreon.",
    },
    {
      question: 'How to donate?',
      answer:
        'You can donate on the <a href="https://patreon.com/crpg" target="_blank">Patreon</a>. Note that donations will only be used' +
        ' to cover server costs.',
    },
  ];

  // https://developers.google.com/search/docs/data-types/faqpage
  faqJsonLd = {
    '@context': 'https://schema.org',
    '@type': 'FAQPage',
    mainEntity: this.faq.map(qa => ({
      '@type': 'Question',
      name: qa.question,
      acceptedAnswer: {
        '@type': 'Answer',
        text: qa.answer,
      },
    })),
  };

  get isSignedIn(): boolean {
    return userModule.isSignedIn;
  }

  onClick(): void {
    signIn();
  }
}
</script>

<style scoped lang="scss">
#faq {
  dt {
    font-weight: bold;
  }
}
</style>

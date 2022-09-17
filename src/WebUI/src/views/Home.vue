<template>
  <div class="container">
    <section class="section">
      <div class="content is-large">
        <h1>{{ $t('homeTitle') }}</h1>
        <p v-html="$t('homeDescriptionPart')"></p>

        <b-button
          size="is-large"
          icon-left="steam-symbol"
          icon-pack="fab"
          @click="onClick"
          :loading="isSigningIn"
          v-if="!isSignedIn"
        >
          {{ $t('homeSignIn') }}
        </b-button>
      </div>

      <div class="content is-medium">
        <h2>{{ $t('homeFAQTitle') }}</h2>
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
      question: this.$t('homeQuestion1'),
      answer: this.$t('homeAnswer1'),
    },
    {
      question: this.$t('homeQuestion2'),
      answer: this.$t('homeAnswer2'),
    },
    {
      question: this.$t('homeQuestion3'),
      answer: this.$t('homeAnswer3'),
    },
    {
      question: this.$t('homeQuestion4'),
      answer: this.$t('homeAnswer4'),
    },
    {
      question: this.$t('homeQuestion5'),
      answer: this.$t('homeAnswer5'),
    },
    {
      question: this.$t('homeQuestion6'),
      answer: this.$t('homeAnswer6'),
    },
    {
      question: this.$t('homeQuestion7'),
      answer: this.$t('homeAnswer7'),
    },
    {
      question: this.$t('homeQuestion8'),
      answer: this.$t('homeAnswer8'),
    },
    {
      question: this.$t('homeQuestion9'),
      answer: this.$t('homeAnswer9'),
    },
    {
      question: this.$t('homeQuestion10'),
      answer: this.$t('homeAnswer10'),
    },
    {
      question: this.$t('homeQuestion11'),
      answer: this.$t('homeAnswer11'),
    },
    {
      question: this.$t('homeQuestion12'),
      answer: this.$t('homeAnswer12'),
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

  signingIn = false;

  get isSignedIn(): boolean {
    return userModule.isSignedIn;
  }

  get isSigningIn(): boolean {
    return userModule.userLoading || this.signingIn;
  }

  onClick(): void {
    this.signingIn = true;
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

<script setup lang="ts">
definePage({
  meta: {
    layout: 'empty',
    skipAuth: true,
  },
});
</script>

<template>
  <div class="flex w-full items-center justify-center py-12">
    <div class="mx-auto max-w-3xl rounded-xl border border-border-200 px-6 py-10">
      <div class="prose prose-invert">
        <h1 class="text-center">Terms of Service</h1>

        <Divider />

        <p>
          Welcome to our website for the cRPG multiplayer mod of Mount and Blade II: Bannerlord. Our
          website provides a user interface to connect players and facilitate communication, and
          also features Steam, EpicGames, and Xbox logins to link the player in-game.
        </p>

        <Divider />

        <ol>
          <li>
            Use of our website: By using our website, you agree to the terms and conditions set
            forth in this document. If you do not agree to these terms, please do not use our
            website.
          </li>

          <li>
            User accounts: To access certain features of our website, such as managing your in-game
            characters, you may be required to create a user account. When creating a user account,
            you must provide accurate and complete information. You are responsible for maintaining
            the confidentiality of your account information.
          </li>

          <li>
            User login: Our website allows users to login using their Steam, EpicGames, or Xbox
            account. The only information we collect through these logins is the user ID provided by
            the login provider. We do not collect or store any other sensitive information.
          </li>

          <li>
            Code of conduct: Our website is intended to be a friendly and respectful community. We
            do not tolerate any form of harassment or abusive behavior towards other users. Users
            who violate this code of conduct may have their account suspended or terminated.
          </li>

          <li>
            Limitation of liability: We are not responsible for any damages or losses that may
            result from your use of our website, including but not limited to loss of data or
            income. We make no warranties or representations, express or implied, about the accuracy
            or completeness of the information on our website.
          </li>

          <li>
            Changes to the Terms of Service: We reserve the right to change these Terms of Service
            at any time without prior notice. Your continued use of our website after any changes to
            these Terms of Service constitutes your acceptance of the new Terms of Service.
          </li>

          <li>
            Governing law: These Terms of Service shall be governed by and construed in accordance
            with the laws of France.
          </li>
        </ol>

        <Divider />

        <p>
          Thank you for using our website, and enjoy playing cRPG in Mount and Blade II: Bannerlord!
        </p>
      </div>
    </div>
  </div>
</template>

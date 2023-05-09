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
        <h1 class="text-center">Privacy Policy</h1>

        <Divider />

        <p>
          We respect your privacy and are committed to protecting your personal information. This
          Privacy Policy explains how we collect, use, and disclose the information you provide to
          us when using our website for the cRPG multiplayer mod of Mount and Blade II: Bannerlord.
        </p>

        <ol>
          <li>
            Information we collect: When you use our website, we collect your user ID from the
            Steam, EpicGames, or Xbox login provider you use to access the website. We do not
            collect or store any other personal information, such as your name, address, or email
            address.
          </li>

          <li>
            Use of information: The user ID we collect is used solely for the purpose of linking
            your in-game characters with your user account on our website. We do not share or sell
            your personal information to any third parties.
          </li>

          <li>
            Cookies: We use cookies on our website to persist user authentication. Cookies are small
            data files that are placed on your device when you access our website. You can disable
            cookies in your browser settings if you do not wish to have them stored on your device.
          </li>

          <li>
            Third-party links: Our website may contain links to third-party websites, such as the
            Steam, EpicGames, or Xbox websites. We are not responsible for the privacy practices or
            content of these third-party websites.
          </li>

          <li>
            Security: We take reasonable precautions to protect the information we collect from
            unauthorized access, use, and disclosure. However, no data transmission over the
            internet can be guaranteed to be 100% secure, and we cannot guarantee the security of
            your personal information.
          </li>

          <li>
            Children's privacy: Our website is not intended for use by children under the age of 13.
            We do not knowingly collect personal information from children under the age of 13. If
            we become aware that we have collected personal information from a child under the age
            of 13, we will take steps to delete the information as soon as possible.
          </li>

          <li>
            Changes to this Privacy Policy: We reserve the right to change this privacy policy at
            any time without prior notice. Your continued use of our website after any changes to
            this Privacy Policy constitutes your acceptance of the new Privacy Policy.
          </li>
          
          <li>
              Contact us: If you have any questions or concerns about this privacy policy, please
              contact us at <a href="mailto:hello@c-rpg.eu">hello@c-rpg.eu</a>.
          </li>
        </ol>
      </div>
    </div>
  </div>
</template>

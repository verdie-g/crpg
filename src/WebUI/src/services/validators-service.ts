import * as validators from '@vuelidate/validators';
import { clanTagRegex, clanBannerKeyRegex } from '@root/data/constants.json';
import { t } from '@/services/translate-service';

const withI18nMessage = validators.createI18nMessage({ t });

export const required = withI18nMessage(validators.required);

export const url = withI18nMessage(validators.url);

export const sameAs = withI18nMessage(validators.sameAs, { withArguments: true });

export const minLength = withI18nMessage(validators.minLength, { withArguments: true });

export const maxLength = withI18nMessage(validators.maxLength, { withArguments: true });

export const clanTagPattern = withI18nMessage(validators.helpers.regex(new RegExp(clanTagRegex)));

export const clanBannerKeyPattern = withI18nMessage(
  validators.helpers.regex(new RegExp(clanBannerKeyRegex))
);

export const discordLinkPattern = withI18nMessage(
  validators.helpers.regex(
    new RegExp('(https?://)?(www.)?(discord.(gg|io|me|li)|discordapp.com/invite)/.+[a-z]') // https://www.regextester.com/99527
  )
);

import Vue from 'vue';
import Buefy from 'buefy';
import App from './App.vue';
import router from './router';
import store from './store';
import { Icon } from 'leaflet';
import 'buefy/dist/buefy.css';
import '@fortawesome/fontawesome-free/css/all.css';
import '@fortawesome/fontawesome-free/css/fontawesome.css';
import 'leaflet/dist/leaflet.css';
import '@/assets/scss/index.scss';
import VueI18n from 'vue-i18n';
import en from '@/resources/i18n/i18n_en';

Vue.use(VueI18n);
Vue.use(Buefy, {
  defaultIconPack: 'fas',
});

//Default icons
delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl: require('leaflet/dist/images/marker-icon-2x.png'),
  iconUrl: require('leaflet/dist/images/marker-icon.png'),
  shadowUrl: require('leaflet/dist/images/marker-shadow.png'),
});

Vue.config.productionTip = false;

const i18n = new VueI18n({
  locale: 'en', // set locale
  fallbackLocale: 'en',
  messages: {
    en,
  },
});

new Vue({
  i18n,
  router,
  store,
  render: h => h(App),
}).$mount('#app');

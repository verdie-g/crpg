import Vue from 'vue';
import Buefy from 'buefy';
import App from './App.vue';
import router from './router';
import store from './store';
import { Icon } from 'leaflet';
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
import shareMutations from 'vuex-shared-mutations';

import 'buefy/dist/buefy.css';
import '@fortawesome/fontawesome-free/css/all.css';
import '@fortawesome/fontawesome-free/css/fontawesome.css';
import 'leaflet/dist/leaflet.css';
import '@/assets/scss/index.scss';
import PollManager from '@/utils/poll-manager';

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

shareMutations({
  predicate: ['setUser', 'setCharacters'],
})(store);

PollManager.getInstance(1000 * 60 * 2).start();

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount('#app');

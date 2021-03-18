import Vue from 'vue';
import Buefy from 'buefy';
import App from './App.vue';
import router from './router';
import store from './store';
import 'buefy/dist/buefy.css';
import '@fortawesome/fontawesome-free/css/all.css';
import '@fortawesome/fontawesome-free/css/fontawesome.css';
import 'leaflet/dist/leaflet.css';
import "@/scss/_variables.scss"

Vue.use(Buefy, {
  defaultIconPack: 'fas',
});

Vue.config.productionTip = false;

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount('#app');

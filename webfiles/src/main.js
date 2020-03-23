import Vue from 'vue';
import VueCookie from 'vue-cookie';
import GlobalMethods from './global-methods.js';

import './assets/css/tailwind.css';
import './global-components.js';

import { store } from './store.js';

Vue.use(VueCookie);
Vue.use(GlobalMethods);

const app = new Vue({
    el: '#app',
    data: {
        currentRoute: window.location.pathname
    },
    computed: {
        ViewComponent() {
            store.currentRoute = this.currentRoute;
            store.jsonData = null;
            store.loading = true;

            new Promise((resolve) => {
                try {
                    var jsonData;

                    if (this.currentRoute == '/home' || this.currentRoute == '/') {
                        jsonData = require('./jsonList.json');
                    }
                    else {
                        jsonData = require('.' + this.currentRoute + '.json');
                    }

                    resolve(jsonData);
                } catch (e) {
                    resolve(require('./404.json'));
                }
            }).then((data) => {
                store.jsonData = data;
                store.loading = false;
            });

            return require('./app.vue');
        }
    },
    render(h) {
        return h(this.ViewComponent);
    }
});

window.addEventListener('popstate', () => {
    app.currentRoute = window.location.pathname;
});
import Vue from 'vue';
import VueCookie from 'vue-cookie';
import GlobalMethods from './global-methods.js';

import './assets/css/tailwind.css';
import './global-components.js';

import { store } from './store.js';

var App = require('./app.vue');

Vue.use(VueCookie);
Vue.use(GlobalMethods);

const app = new Vue({
    el: '#app',
    data: {
        currentRoute: window.location.pathname
    },
    computed: {
        UpdateComponent() {
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
        }
    },
    render(h) {
        this.UpdateComponent;

        return h(App);
    }
});

window.addEventListener('popstate', () => {
    app.currentRoute = window.location.pathname;
});
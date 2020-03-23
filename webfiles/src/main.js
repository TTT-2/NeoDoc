import Vue from 'vue';
import VueCookie from 'vue-cookie';
import GlobalMethods from './global-methods.js';

import { library } from '@fortawesome/fontawesome-svg-core'
import { faExclamationTriangle, faTimes, faCheck, faInfo } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

import './assets/css/tailwind.css';
import './global-components.js';

import { store } from './store.js';

var App = require('./app.vue');

Vue.use(VueCookie);
Vue.use(GlobalMethods);

// font-awesome;
library.add(faExclamationTriangle, faTimes, faCheck, faInfo)

// font-awesome-icon;
Vue.component('font-awesome-icon', FontAwesomeIcon)

const app = new Vue({
    el: '#app',
    data: {
        currentRoute: window.location.pathname
    },
    computed: {
        UpdateComponent() {
            this.currentRoute = window.location.pathname; // update relative paths

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

                    console.log(jsonData)

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
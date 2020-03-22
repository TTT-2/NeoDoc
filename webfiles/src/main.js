import Vue from 'vue'
import VueCookie from 'vue-cookie'

import './assets/css/tailwind.css';
import './globalComponents.js'

import { store } from './store.js'

Vue.use(VueCookie);

const app = new Vue({
    el: '#app',
    data: {
        currentRoute: window.location.pathname
    },
    computed: {
        ViewComponent() {
            var ret

            try {
                ret = require('.' + this.currentRoute + '.vue')
            } catch (e) {
                try {
                    require('.' + this.currentRoute + '.json')

                    ret = require('./json.vue')
                } catch (e) {
                    ret = require('./404.vue')
                }
            }

            store.currentRoute = this.currentRoute

            return ret
        }
    },
    render(h) {
        return h(this.ViewComponent)
    }
})

window.addEventListener('popstate', () => {
    app.currentRoute = window.location.pathname
})
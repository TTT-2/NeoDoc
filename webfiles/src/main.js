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

            store.jsonData = null

            try {
                ret = require('.' + this.currentRoute + '.vue')
            } catch (e) {
                try {
                    var jsonData = require('.' + this.currentRoute + '.json')

                    store.jsonData = jsonData

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
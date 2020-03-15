import Vue from 'vue'

import './assets/css/tailwind.css';
import './globalComponents.js'

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
                ret = require('./docu/404.vue')
            }

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
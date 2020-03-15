import Vue from 'vue'
import routes from './routes'

const app = new Vue({
    el: '#app',
    data: {
        currentRoute: window.location.pathname
    },
    computed: {
        ViewComponent() {
            const matchingView = routes[this.currentRoute]

            if (matchingView)
                return require('./' + matchingView + '.vue')
            else {
                var ret

                try {
                    ret = require('.' + this.currentRoute + '.vue')
                } catch (e) {
                    ret = require('./docu/404.vue')
                }

                return ret
            }
        }
    },
    render(h) {
        return h(this.ViewComponent)
    }
})

window.addEventListener('popstate', () => {
    app.currentRoute = window.location.pathname
})
import Vue from 'vue'

import MainLayout from './layouts/Main.vue'
import VLink from './components/VLink.vue'
import PrettyCode from './components/PrettyCode.vue'
import MainContainer from './components/MainContainer.vue'

import Error from './components/Error.vue'
import Info from './components/Info.vue'
import Success from './components/Success.vue'
import Warning from './components/Warning.vue'

Vue.component('main-layout', MainLayout)
Vue.component('v-link', VLink)
Vue.component('pretty-code', PrettyCode)
Vue.component('main-container', MainContainer)

Vue.component('error', Error)
Vue.component('info', Info)
Vue.component('success', Success)
Vue.component('warning', Warning)

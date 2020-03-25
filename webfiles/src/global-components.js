import Vue from 'vue';


// components
import VLink from './components/VLink.vue';
import TitleText from './components/TitleText.vue';
import PrettyCode from './components/PrettyCode.vue';
import Accordion from './components/Accordion.vue';
import MainContainer from './components/container/MainContainer.vue';

Vue.component('v-link', VLink)
Vue.component('title-text', TitleText)
Vue.component('pretty-code', PrettyCode)
Vue.component('accordion', Accordion)
Vue.component('main-container', MainContainer)


// hints
import Error from './components/Error.vue';
import Info from './components/Info.vue';
import Success from './components/Success.vue';
import Warning from './components/Warning.vue';

Vue.component('error', Error);
Vue.component('info', Info);
Vue.component('success', Success);
Vue.component('warning', Warning);

import Vue from 'vue';


// components
import VLink from './components/VLink.vue';
import PrettyCode from './components/PrettyCode.vue';
import MainContainer from './components/container/MainContainer.vue';

Vue.component('v-link', VLink)
Vue.component('pretty-code', PrettyCode)
Vue.component('main-container', MainContainer)


// params
import DocuParam from './components/params/DocuParam.vue';
import RealmParam from './components/params/RealmParam.vue';

Vue.component('docu-param', DocuParam);
Vue.component('realm-param', RealmParam);


// hints
import Error from './components/Error.vue';
import Info from './components/Info.vue';
import Success from './components/Success.vue';
import Warning from './components/Warning.vue';

Vue.component('error', Error);
Vue.component('info', Info);
Vue.component('success', Success);
Vue.component('warning', Warning);

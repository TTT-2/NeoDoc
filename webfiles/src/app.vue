<template>
    <div class="page-container min-h-screen flex flex-col">
        <header>
            <nav-bar>
                <div slot="title" class="flex items-center flex-shrink-0 text-on-brand mr-6">
                    <span class="font-semibold text-xl tracking-tight">NeoDoc</span>
                </div>

                <div slot="content" class="w-full block flex-grow lg:flex lg:items-center lg:w-auto">
                    <div class="text-sm lg:flex-grow">
                        <a href="https://docs.ttt2.neoxult.de/" class="block mt-4 lg:inline-block lg:mt-0 text-on-brand hover:text-on-brand-hover mr-4">Manual</a>
                    </div>
                    <div class="text-sm lg:flex-grow">
                        <a href="https://github.com/TTT-2/TTT2" class="inline-block text-sm px-4 py-2 leading-none border rounded text-on-brand-hover border-on-brand-hover hover:bg-on-brand hover:text-brand mt-4 lg:mt-0">GitHub</a>
                    </div>
                </div>
            </nav-bar>

            <breadcrumb :paths="getPathSplits" />
        </header>

        <div class="flex flex-grow sm:flex-col md:flex-row">
            <sidebar>
                <input class="autocomplete-input bg-background focus:outline-none focus:shadow-outline border border-on-background rounded-lg py-2 px-4 block w-full appearance-none leading-normal text-on-background"
                        placeholder="Search..."
                        aria-label="Search..."
                        v-model="searchQuery">

                <ul class="sidebar-panel-nav">
                    <li v-for="(wrapperData, wrapperName) in resultQuery.data">
                        <accordion :title="wrapperName" :forceActive="searchQuery != null">
                            <v-link slot="title" :href="'/docu/' + transformURI(wrapperName)">{{ wrapperName }}</v-link>

                            <ul slot="content" v-if="wrapperData.sections" class="ml-4">
                                <li v-for="(sectionEntry, sectionName) in wrapperData.sections">
                                    <div v-for="(dsList, ds) in sectionEntry">
                                        <v-link v-for="dsEntry in dsList" :href="'/docu/' + transformURI(wrapperName) + '/' + transformURI(dsEntry)" class="block mt-4 lg:mt-0 mr-4">{{ dsEntry }}</v-link>
                                    </div>
                                </li>
                            </ul>
                            <accordion slot="content" v-else-if="wrapperName == '_globals'" v-for="(dsList, dsEntry) in wrapperData" :forceActive="searchQuery != null">
                                <v-link slot="title" :href="'/docu/' + transformURI(wrapperName) + '/' + transformURI(dsEntry)">{{ dsEntry }}</v-link>

                                <ul slot="content" class="ml-4">
                                    <li v-for="singleEntry in dsList">
                                        <v-link :href="'/docu/_globals/' + transformURI(singleEntry)">{{ singleEntry }}</v-link>
                                    </li>
                                </ul>
                            </accordion>
                            <div slot="content" v-else>
                                Unknown: {{ wrapperData }}
                            </div>
                        </accordion>
                    </li>
                </ul>
            </sidebar>

            <main-container class="flex flex-grow flex-col justify-center" v-if="isLoading">
                <loading-spinner isLoading="true" />
            </main-container>
            <main-container class="flex flex-grow flex-col" v-else>
                <slot v-if="!getJsonData"></slot>
                <overview :jsonData="getJsonData" v-else-if="getJsonDataType && getJsonDataType == 'overview'" />
                <div v-else-if="getJsonDataType && getJsonDataType == 'error'" class="flex flex-grow flex-col">
                    <error>
                        <p><b>Error 404 - File not found.</b></p>
                        <p>Please try another URL...</p>
                    </error>
                </div>
                <entry-function :jsonData="getJsonData" v-else-if="getJsonDataSubType && getJsonDataSubType == 'function'" />
                <div v-else class="flex flex-grow flex-col">
                    <warning>
                        <p><b>Error 400 - Bad request.</b></p>
                        <p>We are sorry, but your request failed...</p>
                    </warning>
                </div>
            </main-container>
        </div>

        <footer class="flex justify-around bg-brand p-2 text-on-brand">
            <v-link href="/impressum" class="block pb-4 mt-4 lg:inline-block hover:text-on-brand-hover">Impressum</v-link>

            <div class="w-20 flex justify-around">
                <button @click="theme()" class="hover:text-on-brand-hover"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="adjust" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path fill="currentColor" d="M8 256c0 136.966 111.033 248 248 248s248-111.034 248-248S392.966 8 256 8 8 119.033 8 256zm248 184V72c101.705 0 184 82.311 184 184 0 101.705-82.311 184-184 184z"></path></svg></button>
                <button @click="theme('light')" class="hover:text-on-brand-hover"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="sun" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path fill="currentColor" d="M256 160c-52.9 0-96 43.1-96 96s43.1 96 96 96 96-43.1 96-96-43.1-96-96-96zm246.4 80.5l-94.7-47.3 33.5-100.4c4.5-13.6-8.4-26.5-21.9-21.9l-100.4 33.5-47.4-94.8c-6.4-12.8-24.6-12.8-31 0l-47.3 94.7L92.7 70.8c-13.6-4.5-26.5 8.4-21.9 21.9l33.5 100.4-94.7 47.4c-12.8 6.4-12.8 24.6 0 31l94.7 47.3-33.5 100.5c-4.5 13.6 8.4 26.5 21.9 21.9l100.4-33.5 47.3 94.7c6.4 12.8 24.6 12.8 31 0l47.3-94.7 100.4 33.5c13.6 4.5 26.5-8.4 21.9-21.9l-33.5-100.4 94.7-47.3c13-6.5 13-24.7.2-31.1zm-155.9 106c-49.9 49.9-131.1 49.9-181 0-49.9-49.9-49.9-131.1 0-181 49.9-49.9 131.1-49.9 181 0 49.9 49.9 49.9 131.1 0 181z"></path></svg></button>
                <button @click="theme('dark')" class="hover:text-on-brand-hover"><svg aria-hidden="true" focusable="false" data-prefix="fas" data-icon="moon" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><path fill="currentColor" d="M283.211 512c78.962 0 151.079-35.925 198.857-94.792 7.068-8.708-.639-21.43-11.562-19.35-124.203 23.654-238.262-71.576-238.262-196.954 0-72.222 38.662-138.635 101.498-174.394 9.686-5.512 7.25-20.197-3.756-22.23A258.156 258.156 0 0 0 283.211 0c-141.309 0-256 114.511-256 256 0 141.309 114.511 256 256 256z"></path></svg></button>
            </div>
        </footer>

        <cookie-consent cookie-expiration-time="1Y" />
    </div>
</template>

<script>
    import NavBar from './components/menu/NavBar.vue';
    import Breadcrumb from './components/menu/Breadcrumb.vue';
    import Sidebar from './components/menu/Sidebar.vue';
    import CookieConsent from './components/CookieConsent.vue';
    import Overview from './components/Overview.vue';
    import EntryFunction from './components/EntryFunction.vue';
    import LoadingSpinner from './components/LoadingSpinner.vue';

    import { store } from './store.js';

    export default {
        data() {
            return {
                themes: ["light", "dark"],
                searchQuery: null
            }
        },
        methods: {
            theme(name) {
                document.body.removeAttribute('data-theme')
                this.$cookie.delete('theme')

                if (name) {
                    document.body.setAttribute('data-theme', name)
                    this.$cookie.set('theme', name, { expires: '1Y' }) // expires after 1 year
                }
            },
            transformURI(uri) {
                return this.$globalMethods.transformURI(uri)
            },
            getSize(obj) {
                var size = 0, key;

                for (key in obj) {
                    if (obj.hasOwnProperty(key)) {
                        size++;
                    }
                }

                return size;
            }
        },
        mounted() {
            this.theme(this.$cookie.get('theme'))
        },
        components: {
            NavBar,
            Breadcrumb,
            Sidebar,
            CookieConsent,
            Overview,
            EntryFunction,
            LoadingSpinner
        },
        computed: {
            getPathSplits() {
                return store.currentRoute.substring(1).split('/')
            },
            getJsonData() {
                return store.jsonData
            },
            getJsonDataType() {
                return store.jsonData.type
            },
            getJsonDataSubType() {
                return store.jsonData.subtype
            },
            isMobile() {
                return this.$globalMethods.IsMobile();
            },
            isLoading() {
                return store.loading;
            },
            resultQuery() {
                if (this.searchQuery) {
                    var retObj = {
                        data: {}
                    };

                    for (var wrapper in store.jsonList.data) {
                        var wrapperData = store.jsonList.data[wrapper];

                        // initialize wrapper data
                        retObj.data[wrapper] = {};

                        if (wrapperData.sections) {
                            retObj.data[wrapper].sections = {};

                            for (var sectionName in wrapperData.sections) {
                                retObj.data[wrapper].sections[sectionName] = {};

                                var nextList = wrapperData.sections[sectionName];

                                for (var dataStructure in nextList) {
                                    retObj.data[wrapper].sections[sectionName][dataStructure] = [];

                                    var tmpResult = nextList[dataStructure].filter((item) => {
                                        return this.searchQuery.toLowerCase().split(' ').every(v => item.toLowerCase().includes(v))
                                    });

                                    // set or merge
                                    if (retObj.data[wrapper].sections[sectionName][dataStructure]) {
                                        retObj.data[wrapper].sections[sectionName][dataStructure] = retObj.data[wrapper].sections[sectionName][dataStructure].concat(tmpResult);
                                    }
                                    else {
                                        retObj.data[wrapper].sections[sectionName][dataStructure] = tmpResult;
                                    }

                                    // clear empty lists
                                    if (retObj.data[wrapper].sections[sectionName][dataStructure].length == 0) {
                                        delete retObj.data[wrapper].sections[sectionName][dataStructure];
                                    }
                                }

                                // clear empty lists
                                if (this.getSize(retObj.data[wrapper].sections[sectionName]) == 0) {
                                    delete retObj.data[wrapper].sections[sectionName];
                                }
                            }

                            // clear empty lists
                            if (this.getSize(retObj.data[wrapper].sections) == 0) {
                                delete retObj.data[wrapper].sections;
                            }
                        }
                        else {
                            for (var dataStructure in wrapperData) {
                                retObj.data[wrapper][dataStructure] = [];

                                retObj.data[wrapper][dataStructure] = wrapperData[dataStructure].filter((item) => {
                                    return this.searchQuery.toLowerCase().split(' ').every(v => item.toLowerCase().includes(v))
                                });

                                // clear empty lists
                                if (retObj.data[wrapper][dataStructure].length == 0) {
                                    delete retObj.data[wrapper][dataStructure];
                                }
                            }
                        }

                        // clear empty lists
                        if (this.getSize(retObj.data[wrapper]) == 0) {
                            delete retObj.data[wrapper];
                        }
                    }

                    console.log(retObj);

                    return retObj;
                } else {
                    this.searchQuery = null;

                    return store.jsonList;
                }
            }
        }
    }
</script>

<style scoped>
    header > nav.navbar .active {
        color: rgb(var(--color-highlight-on-brand));
    }
</style>
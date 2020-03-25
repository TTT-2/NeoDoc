import Vue from "vue";

export const store = Vue.observable({
    isSideNavOpen: false,
    isNavBarOpen: false,
    jsonData: "", /* the dynamic data based on the current path */
    jsonList: "", /* static list of e.g. all functions, e.g. for the sidebar */
    currentRoute: "",
    loading: false,
});

export const mutations = {
    toggleSideNav() {
        store.isSideNavOpen = !store.isSideNavOpen
    },
    toggleNavBar() {
        store.isNavBarOpen = !store.isNavBarOpen
    }
};
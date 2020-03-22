import Vue from "vue";

export const store = Vue.observable({
    isSideNavOpen: false,
    isNavBarOpen: false,
    currentRoute: ""
});

export const mutations = {
    toggleSideNav() {
        store.isSideNavOpen = !store.isSideNavOpen
    },
    toggleNavBar() {
        store.isNavBarOpen = !store.isNavBarOpen
    }
};
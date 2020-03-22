import Vue from "vue";

export const store = Vue.observable({
    isSideNavOpen: false,
    isNavBarOpen: false,
    jsonData: "",
    currentRoute: "",
});

export const mutations = {
    toggleSideNav() {
        store.isSideNavOpen = !store.isSideNavOpen
    },
    toggleNavBar() {
        store.isNavBarOpen = !store.isNavBarOpen
    }
};
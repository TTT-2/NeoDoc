import Vue from "vue";

// save our state (isPanel open or not)
export const store = Vue.observable({
    isSideNavOpen: false,
    isNavBarOpen: false
});

// We call toggleNav anywhere we need it in our app 
export const mutations = {
    toggleSideNav() {
        store.isSideNavOpen = !store.isSideNavOpen
    },
    toggleNavBar() {
        store.isNavBarOpen = !store.isNavBarOpen
    }
};
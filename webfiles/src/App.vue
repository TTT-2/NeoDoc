<template>
    <div id="app">
        <!-- Nav -->
        <nav class="bg-green-600 pt-2 md:pt-1 pb-1 px-1 mt-0 h-auto fixed w-full z-20 top-0">
            <div class="flex flex-wrap items-center">

                <!-- Left top side -->
                <div class="flex flex-shrink md:w-1/3 justify-center md:justify-start text-white">
                    <a href="https://docs.ttt2.neoxult.de/" class="block mt-4 lg:inline-block lg:mt-0 text-green-200 hover:text-white mr-4">
                        Docs
                    </a>

                    <a href="https://wiki.ttt2.neoxult.de/" class="block mt-4 lg:inline-block lg:mt-0 text-green-200 hover:text-white mr-4">
                        Wiki
                    </a>

                    <a href="https://steamcommunity.com/sharedfiles/filedetails/?id=1357204556" class="block mt-4 lg:inline-block lg:mt-0 text-green-200 hover:text-white">
                        Steam
                    </a>
                </div>

                <!-- Search -->
                <div class="flex flex-1 md:w-1/3 justify-center md:justify-start text-green-600 px-2">
                    <div class="relative w-full">
                        <input type="search" placeholder="Search" class="w-full bg-gray-100 text-sm text-green-600 transition border border-transparent focus:outline-none focus:border-green-400 rounded py-1 px-2 pl-10 appearance-none leading-normal">

                        <div class="absolute search-icon" style="top: .5rem; left: .8rem;">
                            <svg class="fill-current pointer-events-none text-green-600 w-4 h-4" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20">
                                <path d="M12.9 14.32a8 8 0 1 1 1.41-1.41l5.35 5.33-1.42 1.42-5.33-5.34zM8 14A6 6 0 1 0 8 2a6 6 0 0 0 0 12z"></path>
                            </svg>
                        </div>
                    </div>
                </div>

                <!-- Right top side -->
                <div class="flex w-full pt-2 content-center justify-between md:w-1/3 md:justify-end">
                    <ul class="list-reset flex justify-between flex-1 md:flex-none items-center">
                        <li class="flex-1 md:flex-none md:mr-3">
                            <a href="https://github.com/TTT-2/TTT2" class="inline-block text-sm px-4 py-2 leading-none border rounded text-white border-white hover:border-transparent hover:text-green-600 hover:bg-white mt-4 lg:mt-0">GitHub</a>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>

        <!-- Sidebar -->
        <div class="flex flex-col md:flex-row">
            <div class="bg-green-600 shadow-lg h-16 fixed bottom-0 mt-12 md:relative md:h-screen z-10 w-full md:w-48">
                <div class="md:mt-12 md:w-48 md:fixed md:left-0 md:top-0 content-center md:content-start text-left justify-between">
                    <ul class="list-reset flex flex-row md:flex-col py-0 md:py-3 px-1 md:px-2 text-center md:text-left">
                        <li class="mr-3 flex-1">
                            <a href="#" class="block py-1 md:py-3 pl-1 align-middle text-white no-underline hover:text-white border-b-2 border-green-500 hover:border-pink-500">
                                <i class="fas fa-tasks pr-0 md:pr-3"></i><span class="pb-1 md:pb-0 text-xs md:text-base text-green-300 md:text-green-100 block md:inline-block">Functions</span>
                            </a>
                        </li>

                        <li class="mr-3 flex-1">
                            <a href="#" class="block py-1 md:py-3 pl-1 align-middle text-white no-underline hover:text-white border-b-2 border-green-500 hover:border-purple-500">
                                <i class="fa fa-envelope pr-0 md:pr-3"></i><span class="pb-1 md:pb-0 text-xs md:text-base text-green-300 md:text-green-100 block md:inline-block">Hooks</span>
                            </a>
                        </li>

                        <li class="mr-3 flex-1">
                            <a href="#" class="block py-1 md:py-3 pl-1 align-middle text-white no-underline hover:text-white border-b-2 border-blue-600">
                                <i class="fas fa-chart-area pr-0 md:pr-3 text-blue-600"></i><span class="pb-1 md:pb-0 text-xs md:text-base text-white md:text-white block md:inline-block">ConVars</span>
                            </a>
                        </li>
                    </ul>
                </div>
            </div>

            <!-- Main content  -->
            <div class="main-content flex-1 bg-gray-100 mt-12 md:mt-2 pb-24 md:pb-5" id="main-container">

                <!-- Subtitle -->
                <div class="bg-green-800 p-2 shadow text-xl text-white">
                    <h3 class="font-bold pl-2">Documentation</h3>
                </div>

                <!-- Any content -->
                <div class="flex flex-wrap">
                    <router-view />
                </div>
            </div>
        </div>
    </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
#main-container {
    background-color: #ffffff;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='28' height='49' viewBox='0 0 28 49'%3E%3Cg fill-rule='evenodd'%3E%3Cg id='hexagons' fill='%23eeeeee' fill-opacity='0.42' fill-rule='nonzero'%3E%3Cpath d='M13.99 9.25l13 7.5v15l-13 7.5L1 31.75v-15l12.99-7.5zM3 17.9v12.7l10.99 6.34 11-6.35V17.9l-11-6.34L3 17.9zM0 15l12.98-7.5V0h-2v6.35L0 12.69v2.3zm0 18.5L12.98 41v8h-2v-6.85L0 35.81v-2.3zM15 0v7.5L27.99 15H28v-2.31h-.01L17 6.35V0h-2zm0 49v-8l12.99-7.5H28v2.31h-.01L17 42.15V49h-2z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
}
</style>

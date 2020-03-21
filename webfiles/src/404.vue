<template>
    <main-layout>
        <div class="flex flex-col">
            <warning>
                <p class="leading-tight">
                    Lorem ipsum dolor sit amet, consectetur adipisicing elit.
                </p>
            </warning>

            <success>
                <p class="leading-tight">
                    Lorem ipsum dolor sit amet, consectetur adipisicing elit. Ab beatae consequuntur dolore doloribus eius fugit, porro quidem repellat temporibus unde?
                </p>
            </success>

            <info>
                <p class="leading-tight">
                    Lorem ipsum dolor sit amet, consectetur adipisicing elit. Blanditiis, labore, voluptatem. Deserunt illum laborum maxime?
                </p>
            </info>

            <error>
                <p class="leading-tight">
                    Lorem ipsum dolor sit amet, consectetur adipisicing elit. Aperiam, nemo!
                </p>
            </error>

            <pretty-code>
                local function HUDELEMENT:TestFunction(test, test){
                    local ply = player.GetAll()[1]
                    ply:WriteLine("tests")
                }
            </pretty-code>

            <docu-param name="test" type="string" text="Some description" />
            <docu-param name="test" type="string" text="Some description" />
        </div>
    </main-layout>
</template>
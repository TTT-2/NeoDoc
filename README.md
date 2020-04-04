# NeoDoc
A simple documentation tool focused on language extensibility and simplicity, written from scratch in C#.


## TODO
- Add multiline comment support (`--[[ ... ]]` or `/* ... */`)
- Add exit codes for CI Jenkins
- Add hints for wrong documentation style in common cases
- Remove Check function of Datastructures (redundant)
- If a Datastructure has an IgnoreParam, ignore the Datastructure. This can happen if not placing the IgnoreParam directly in the line before the Datastructure

# Setup

## How to add new languages?
1. Duplicate [Lua.cs](https://github.com/TTT-2/NeoDoc/blob/master/source/Langs/Lua.cs)
2. Rename the file into your new language (keep the file type `.cs`)
3. Change the class name into the same as the file's name (for consistency)
4. Modify the following function returns to support matching the new language:

Function | Returns
--- | ---
GetName | The name of the new language
GetFileExtension | The file type of the new language
GetCommentStyleRegex | RegEx of the a default file comment, `-{2,}` e.g. matches every line with two or more `-`
GetSingleCommentChar | Character of a single line comment, e.g. `-` or `/`
GetCommentStartRegex | RegEx of the default file comment block start, currently it's made to automatically register the `@desc` param, so you can start creating a description directly after using e.g. three dashes (`---`). Same as `GetCommentStyleRegex`

5. Your language is now registered and can match the default params :) But this tool still don't know who to assing the params to.

## Datastructures
Datastructures are the wrappers that stores the params. If no Datastructure is defined for your language, no param can be stored and no documentation can be created.

### Hot to add Datastructures to your language?
1. Create a folder with the name of your registered language in the [Datastructure folder](https://github.com/TTT-2/NeoDoc/tree/master/source/DataStructures)
2. It's common to use a documentation tool to document functions, so we start with functions. Copy the [Function Datastructure file of the Lua language](https://github.com/TTT-2/NeoDoc/blob/master/source/DataStructures/Lua/Function.cs) and paste it into the created Datastructure folder of the new language
3. Rename the last ending of the namespace in this file into your new language
4. Modify the following function returns to support matching the new language:

Function | Returns
--- | ---
GetName | The identification name
GetRegex | RegEx to match the current Datastructure in a line. 
Check | Check whether the current line matched this Datastructure [Depricated]
Process | If the RegEx for this function matches, the Process function is run. Here, you can extract special data like arguments to keep them in your documentation.
GetData | The exported data for the documentation
GetDatastructureName | The name of the current Datastructure in the file, e.g. the line with the string `CreateDocumentation(x, y, z)` should return here `CreateDocumentation`
CheckDataStructureTransformation (optional) | Used to check for a Datastructure transformation. Useful for Subtype matching or similar things
GetFullJSONData (optional) | The full JSON data of this Datastructure
IsGlobal (optional) | Return true to move these Datastructure out of the current wrapper, into the `_global` wrapper

If you wanna get more data, take a look into [this file](https://github.com/TTT-2/NeoDoc/blob/master/source/DataStructures/DataStructure.cs)

### How to add custom Params?
Params are used for any language, so you don't need to create a Param for any single language.

#### BaseParams

Most of all, they are derived from BaseParams (which doesn't get matched in the documentation) to avoid code redundancy.
These are the BaseParams which already exists:

Param | Utilization | Arguments
--- | --- | ---
[Param](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/Param.cs) | This is the main BaseParam. Any new param needs to be derived from it | 0
[MarkParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/MarkParam.cs) | Used to mark a Datastructure (adding static data) | 0
[StateParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/StateParam.cs) | Used to assign one single data to a Datastructure | 1
[TextParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/TextParam.cs) | Used to add a text to the Datastructure | 1
[TypeTextParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/TypeTextParam.cs) | Used to assign type and a desciption to a Datastructure | 2
[ParameterParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/ParameterParam.cs) | Used to assign a type, a name and a description to a Datastructure | 3

#### Params

Currently, there already are these preregistered Params:

##### [MarkParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/MarkParam.cs)

Param | File | Utilization
--- | --- | ---
`@2D` | [2DParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/2DParam.cs) | Functions that just run inside 2D hooks
`@3D` | [3DParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/3DParam.cs) | Functions that just run inside 3D hooks
`@deprecated` | [DeprecatedParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/DeprecatedParam.cs) | Mark a Datastructure as deprecated
`@hook` | [HookParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/HookParam.cs) | Mark a Datastructure as a function, e.g. used in Lua
`@ignore` | [IgnoreParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/IgnoreParam.cs) | Used to ignore the next line. To ignore a Datastructure, this needs to be placed directly the line before!
`@important` | [ImportantParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/ImportantParam.cs) | Mark a Datastructure as important
`@internal` | [InternalParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/InternalParam.cs) | Mark a Datastructure as internal
`@local` | [LocalParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/LocalParam.cs) | Mark a Datastructure as local. Currently just used by Lua, but can be used for `private`functions too
`@predicted` | [PredictedParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/PredictedParam.cs) | Mark a Datastructure as predicted (used in Lua)

##### [TextParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/TextParam.cs)

Param | File | Utilization
--- | --- | ---
`@author` | [AuthorParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/AuthorParam.cs) | Reference the creator of this function or module
`@desc` | [DescParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/DescParam.cs) | Automatically created if starting a comment block with e.g. `---` in Lua. Used to add a desciption to the Datastructure
`@function` | [FunctionParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/FunctionParam.cs) | _TODO_
`@note` | [NoteParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/NoteParam.cs) | Used to add a note to a Datastructure
`@ref` | [RefParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/RefParam.cs) | Used to reference a ressource. Derived from the `@see` Param
`@see` | [SeeParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/SeeParam.cs) | Used to give a hint for the user to another information
`@todo` | [TodoParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/TodoParam.cs) | Used to add information about what to do next for a Datastructure
`@usage` | [UsageParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/UsageParam.cs) | Used to give a usage example for a Datastructure. It's common to add a code example here
`@warning` | [WarningParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/WarningParam.cs) | Used to add a warning with a desciption to a Datastructure

##### [StateParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/StateParam.cs)

Param | File | Utilization
--- | --- | ---
`@name` | [NameParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/NameParam.cs) | Used to override the name of the following Datastructure. Currently used by Lua for the `CreateConVar` Datastructure. Useful for dynamic names with a static structure
`@realm` | [RealmParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/RealmParam.cs) | Used to set a realm of a Datastructure. In Lua, this can be `client`, `server` or `shared` (for both)

##### [ParameterParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/ParameterParam.cs)

Param | File | Utilization
--- | --- | ---
`@param` | [ParamParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/ParamParam.cs) | Used to add Parameter to a Datastructure, e.g. to register arguments of a function

##### [TypeTextParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/TypeTextParam.cs)

Param | File | Utilization
--- | --- | ---
`@return` | [ReturnParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/ReturnParam.cs) | Used to add the return types to a Datastructure, e.g. of a function

### How is the data structured?
The data is structured in differnt layers. The order is the following:
1. Wrapper (to add a custom Wrapper, use the custom `@wrapper` Param with one argument, the Wrapper's name)
2. Section (to add a custom Section, use the custom `@section` Param with one argument, the Section's name)
3. Datastructure
4. Param

If there is no Wrapper or Section given, the default Wrapper or Section have the name "none". If you wanna add Datastructures into a Wrapper or Section and later switch back to the default Wrapper, type `@wrapper none` or `@section none`.
Globals (global Datastructures) are placed in the `_globals` Wrapper. They are **not** assigned to any section!

### How to add a custom Param?
1. At first, you need to decide for a BaseParam. If there is no matching BaseParam, feel free to add your own one (It needs to be derived from the BaseParam [Param](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/Param.cs) and to be placed in the [BaseParams folder](https://github.com/TTT-2/NeoDoc/tree/master/source/Params/BaseParams)) :)
2. Then, give a look into the referenced Params above.
3. Duplicate the param's file
4. Rename the file into your new Param's name
5. Change the class' name to your new Param's name and adjust the BaseParam if you wanna use another one
6. Modify the return of the `GetName` to match your new Param's name
7. ...

# Examples
_TODO_

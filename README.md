# NeoDoc
NeoDoc is a simple documentation tool focused on language extensibility and simplicity, written from scratch in C#.
It's one part of a duo project. NeoDoc is the backend, made to create different json files, used by the other frontend project, [NeoVis](https://github.com/TTT-2/NeoVis).


## TODO
- Add multiline comment support (`--[[ ... ]]` or `/* ... */`)
- Add exit codes for CI Jenkins
- Add hints for wrong documentation style in common cases
- Remove Check function of `Datastructure`s (redundant)
- If a `Datastructure` has an `IgnoreParam`, ignore the `Datastructure`. This can happen if not placing the `IgnoreParam` directly in the line before the `Datastructure`

# Setup

## How to add new languages?
1. Duplicate [Lua.cs](https://github.com/TTT-2/NeoDoc/blob/master/source/Langs/Lua.cs).
2. Rename the file into your new language (keep the file type `.cs`).
3. Change the class name into the same as the file's name (for consistency).
4. Modify the following function returns to support matching the new language:

Function | Returns
--- | ---
`GetName` | The name of the new language
`GetFileExtension` | The file type of the new language
`GetCommentStyleRegex` | RegEx of the a default file comment, `-{2,}` e.g. matches every line with two or more `-`
`GetSingleCommentChar` | Character of a single line comment, e.g. `-` or `/`
`GetCommentStartRegex` | RegEx of the default file comment block start, currently it's made to automatically register the `@desc` param, so you can start creating a description directly after using e.g. three dashes (`---`). Same as `GetCommentStyleRegex`

5. Your language is now registered and can match the default `Param`s :) But this tool still don't know who to assing the params to.

## Datastructures
`Datastructure`s are the wrappers that stores the `Param`s. If no `Datastructure` is defined for your language, no `Param` can be stored and no documentation can be created.

### Hot to add Datastructures to your language?
1. Create a folder with the name of your registered language in the [Datastructure folder](https://github.com/TTT-2/NeoDoc/tree/master/source/DataStructures).
2. It's common to use a documentation tool to document functions, so we start with functions. Copy the [Function Datastructure file of the Lua language](https://github.com/TTT-2/NeoDoc/blob/master/source/DataStructures/Lua/Function.cs) and paste it into the created `Datastructure` folder of the new language.
3. Rename the last ending of the namespace in this file into your new language.
4. Modify the following function returns to support matching the new language:

Function | Returns
--- | ---
`GetName` | The identification name
`GetRegex` | RegEx to match the current `Datastructure` in a line. 
`Check` | Check whether the current line matched this `Datastructure` [Depricated]
`Process` | If the RegEx for this `Datastructure` matches, the Process function is run. Here, you can extract special data like arguments to keep them in your documentation.
`GetData` | The exported data for the documentation
`GetDatastructureName` | The name of the current `Datastructure` in the file, e.g. the line with the string `CreateDocumentation(x, y, z)` should return here `CreateDocumentation`
`CheckDataStructureTransformation` (optional) | Used to check for a `Datastructure` transformation. Useful for subtype matching or similar things
`GetFullJSONData` (optional) | The full JSON data of this `Datastructure`
`IsGlobal` (optional) | Return true to move these `Datastructure` out of the current `Wrapper`, into the `_global` `Wrapper`

If you wanna get more data, take a look into [this file](https://github.com/TTT-2/NeoDoc/blob/master/source/DataStructures/DataStructure.cs).

### Documentation parameter list
`Param`s are used for any language, so you don't need to create a `Param` for any single language.

#### BaseParams

Most of all, they are derived from `BaseParam`s (which doesn't get matched in the documentation) to avoid code redundancy.
These are the `BaseParam`s which already exists:

Param | Utilization | Arguments
--- | --- | ---
[Param](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/Param.cs) | This is the main `BaseParam`. Any new `Param` needs to be derived from it | 0
[MarkParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/MarkParam.cs) | Used to mark a `Datastructure` (adding static data) | 0
[StateParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/StateParam.cs) | Used to assign one single data to a `Datastructure` | 1
[TextParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/TextParam.cs) | Used to add a text to the `Datastructure` | 1
[TypeTextParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/TypeTextParam.cs) | Used to assign type and a desciption to a `Datastructure` | 2
[ParameterParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/ParameterParam.cs) | Used to assign a type, a name and a description to a `Datastructure`. This `Param` supports multiple types splitted by `\|`. E.g. `@param string\|number x ...` | 3

#### Params

Currently, there already are these preregistered `Param`s:

##### [MarkParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/MarkParam.cs)

Param | File | Utilization
--- | --- | ---
`@2D` | [2DParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/2DParam.cs) | Functions that just run inside 2D hooks
`@3D` | [3DParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/3DParam.cs) | Functions that just run inside 3D hooks
`@deprecated` | [DeprecatedParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/DeprecatedParam.cs) | Mark a `Datastructure` as deprecated
`@hook` | [HookParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/HookParam.cs) | Mark a `Datastructure` as a function, e.g. used in Lua
`@ignore` | [IgnoreParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/IgnoreParam.cs) | Used to ignore the next line. To ignore a `Datastructure`, this needs to be placed directly the line before!
`@important` | [ImportantParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/ImportantParam.cs) | Mark a `Datastructure` as important
`@internal` | [InternalParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/InternalParam.cs) | Mark a `Datastructure` as internal
`@local` | [LocalParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/LocalParam.cs) | Mark a `Datastructure` as local. Currently just used by Lua, but can be used for `private` functions too. **`Datastructure`s marked as `local` are excluded from any documentation output (json file)!**
`@predicted` | [PredictedParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/PredictedParam.cs) | Mark a `Datastructure` as predicted (used in Lua)

##### [TextParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/TextParam.cs)

Param | File | Utilization
--- | --- | ---
`@author` | [AuthorParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/AuthorParam.cs) | Reference the creator of this function or module
`@desc` | [DescParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/DescParam.cs) | Automatically created if starting a comment block with e.g. `---` in Lua. Used to add a desciption to the `Datastructure`
`@function` | [FunctionParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/FunctionParam.cs) | Used to create a function `Datastructure` in a comment, e.g. used in Lua. This needs to be the last `Param`, otherwise, the following `Param`s will not be assigned to this new `Datastructure`
`@note` | [NoteParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/NoteParam.cs) | Used to add a note to a `Datastructure`
`@ref` | [RefParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/RefParam.cs) | Used to reference a ressource. Derived from the `@see` `Param`
`@see` | [SeeParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/SeeParam.cs) | Used to give a hint for the user to another information
`@todo` | [TodoParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/TodoParam.cs) | Used to add information about what to do next for a `Datastructure`
`@usage` | [UsageParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/UsageParam.cs) | Used to give a usage example for a `Datastructure`. It's common to add a code example here
`@warning` | [WarningParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/WarningParam.cs) | Used to add a warning with a desciption to a `Datastructure`

##### [StateParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/StateParam.cs)

Param | File | Utilization
--- | --- | ---
`@name` | [NameParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/NameParam.cs) | Used to override the name of the following `Datastructure`. Currently used by Lua for the `CreateConVar` `Datastructure`. Useful for dynamic names with a static structure
`@realm` | [RealmParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/RealmParam.cs) | Used to set a realm of a `Datastructure`. In Lua, this can be `client`, `server` or `shared` (for both)

##### [ParameterParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/ParameterParam.cs)

Param | File | Utilization
--- | --- | ---
`@param` | [ParamParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/ParamParam.cs) | Used to add Parameter to a `Datastructure`, e.g. to register arguments of a function

##### [TypeTextParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/TypeTextParam.cs)

Param | File | Utilization
--- | --- | ---
`@return` | [ReturnParam](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/ReturnParam.cs) | Used to add the return types to a `Datastructure`, e.g. of a function

### How is the data structured?
The data is structured in differnt layers. The order is the following:
1. `Wrapper` (to add a custom `Wrapper`, use the custom `@wrapper` `Param` with one argument, the `Wrapper`'s name)
2. `Section` (to add a custom `Section`, use the custom `@section` `Param` with one argument, the `Section`'s name)
3. `Datastructure`
4. `Param`

If there is no `Wrapper` or `Section` given, the default `Wrapper` or `Section` have the name "none". If you wanna add `Datastructure`s into a `Wrapper` or `Section` and later switch back to the default `Wrapper` or `Section`, type `@wrapper none` or `@section none`.
Globals (global `Datastructure`s) are placed in the `_globals` `Wrapper`. They are **not** assigned to any `Section`!

### How to add a custom Param?
1. At first, you need to decide for a `BaseParam`. If there is no matching `BaseParam`, feel free to add your own one (It needs to be derived from the `BaseParam` [Param](https://github.com/TTT-2/NeoDoc/blob/master/source/Params/BaseParams/Param.cs) and to be placed in the [BaseParams folder](https://github.com/TTT-2/NeoDoc/tree/master/source/Params/BaseParams)) :)
2. Then, give a look into the referenced `Param`s above.
3. Duplicate the `Param`'s file
4. Rename the file into your new `Param`'s name
5. Change the class' name to your new `Param`'s name and adjust the `BaseParam` if you wanna use another one
6. Modify the return of the `GetName` to match your new `Param`'s name
7. ...

### Param Settings
`Param Settings` are made to define some default data or mark a `Param` e.g. as optional with `opt`.
Values are assigned with `=`. `Param Settings` needs to be concatenated **directly** to the `Param`. **Otherwise, it's handled as an argument!**

These are the default `Param Settings`:

Param Setting | Utilization | Value | Example
--- | --- | --- | ---
`default` | Used to set a default value | ✓ | `@param[default=true] boolean isUsed ...`
`opt` | Used to mark an `Param` as optional | × | `@param[opt] Player target ...`
`optchain` | Used to mark an `Param` as optional chain | × | `@param[optchain] Entity inflictor ...`

Most of all, these `Param Settings` just makes sense in the `ReturnParam` or `ParamParam`.

# Examples
```lua
---
-- This is a default description without a param
-- @param string x The test text
-- @return boolean Whether the test function was successful
function TestFunction1(x)
{
  -- ...
}

---
-- @author AuthorName
-- @wrapper TestWrapper

---
-- This function will be placed in the "TestWrapper" Wrapper, Section "none"
-- @realm client
function TestFunction2(x)
{
  -- ...
}

---
-- Another function that will be placed in the "TestWrapper" Wrapper
function TestFunction3(x)
{
  -- ...
}

-- @wrapper none

---
-- This function is placed in the "none" Wrapper
-- @function TestFunction4(x)
-- @param string x This will not be assigned to the previous function.

```

# Output
There are 3 different outputs, which can be used to create a documentation frontend.

1. The `overview.json` file, containing a complete list with the structure `Wrapper` -> `Section` -> `Datastructure`
2. The `search.json` file. containing a shrinked list with the structure `Wrapper` -> `Datastructure` (so without `section`), commonly used for an improved searching
3. The documentation of any `Datastructure` in the matching folder structure:
    - `Wrapper` name -> `Datastructure` name 
    - or for global `Datastructure`s: `_globals` as a `Wrapper` name -> `Datastructure` type name -> `Datastructure` name

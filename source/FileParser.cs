﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoDoc.DataStructures;
using NeoDoc.DataStructures.Lua;
using NeoDoc.Langs;
using NeoDoc.Params;

// Structure:
// 1. Wrapper, e.g. "module" or "class"
// 2. Section, e.g. "section"
// 3. Datastructures, e.g. "function"
// 4. Param, e.g. "desc" or "param"

namespace NeoDoc
{
    public class FileParser
    {
        private readonly LangMatcher langMatcher;
        private readonly Lang lang;
        private readonly string path;
        private readonly ParamMatcher paramMatcher;

        public SortedDictionary<string, WrapperParam> WrapperDict { get; set; }

        private WrapperParam CurrentWrapper { get; set; }
        private SectionParam CurrentSection { get; set; }

        public string[] Lines { get; set; }

        public FileParser(LangMatcher langMatcher, Lang lang, string path)
        {
            this.langMatcher = langMatcher;
            this.lang = lang;
            this.path = path;

            paramMatcher = new ParamMatcher(lang);

            Lines = File.ReadAllLines(path); // Load each line of the file in the buffer

            ModuleParam moduleParam = new ModuleParam(); // adds a new wrapper with default "none" data

            // initialize wrapper list
            WrapperDict = new SortedDictionary<string, WrapperParam>
            {
                { moduleParam.WrapperName, moduleParam }
            };

            // initializes the current vars for easy and fast access
            CurrentWrapper = moduleParam;
            CurrentSection = CurrentWrapper.GetSectionNone();
        }

        public void CleanUp()
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = Lines[i].TrimStart(); // clear every space in front
            }
        }

        public void Process()
        {
            List<Param> paramsList = new List<Param>();
            Param lastParam = null;

            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Lines[i];

                if (string.IsNullOrEmpty(line)) // ignore empty lines
                    continue;

                if (!paramMatcher.IsLineComment(line)) // if there is no comment 
                {
                    if (lastParam != null) // if there is something else than a comment. That means the doc comment section has end
                    {
                        // add the last param before setting it to null
                        paramsList.Add(lastParam);

                        lastParam = null;
                    }

                    DataStructure dataStructure = langMatcher.GetDataStructureType(lang, line);

                    if (dataStructure != null)
                    {
                        dataStructure.ParamsList = new List<Param>(paramsList); // set the params with a copy of the list
                        dataStructure.ProcessDatastructure(line);

                        if (!dataStructure.Ignore)
                        {
                            dataStructure = dataStructure.CheckDataStructureTransformation() ?? dataStructure;

                            // set meta information
                            dataStructure.FoundLine = i;
                            dataStructure.FoundPath = path;

                            dataStructure.Check();

                            // now add the datastructure into the current section of the current container
                            if (!CurrentSection.DataStructureDict.TryGetValue(dataStructure.GetName(), out List<DataStructure> dsList))
                            {
                                dsList = new List<DataStructure>();

                                CurrentSection.DataStructureDict.Add(dataStructure.GetName(), dsList);
                            }

                            dsList.Add(dataStructure);
                        }
                    }

                    // cleans the params list to be used for the next function or whatever, even if there is no dataStructure match
                    paramsList.Clear();

                    continue;
                }

                Param lineParam = paramMatcher.GetLineParam(line);

                if (lineParam == null) // if there is no line param
                {
                    if (paramMatcher.IsLineCommentStart(line))
                    {
                        if (lastParam != null)
                        {
                            // add the last param before replacing it
                            paramsList.Add(lastParam);
                        }

                        lastParam = new DescParam(); // start with a new description per default if matching e.g. "---"
                    }

                    string foundLineParamString = paramMatcher.GetLineParamString(line);

                    if (!string.IsNullOrEmpty(foundLineParamString)) // if there is a not registered param
                    {
                        NeoDoc.WriteErrors(new List<string>() {
                            "UNREGISTERED PARAM: " + foundLineParamString,
                            line,
                            "Source: '" + path + "' (ll. " + i + ")"
                        });

                        continue;
                    }

                    if (lastParam == null) // if there is no last param
                    {
                        continue;
                    }

                    lineParam = lastParam; // use last param as new line param to support multiline commenting style

                    lineParam.ProcessAddition(paramMatcher.GetLineCommentData(line)); // add additional content
                }
                else
                {
                    if (lastParam != null)
                    {
                        // add the last param before setting it to null
                        paramsList.Add(lastParam);

                        lastParam = null;
                    }

                    if (lineParam is WrapperParam || lineParam is SectionParam)
                    {
                        if (lineParam is WrapperParam tmpWrapperParam) // TODO what if Wrapper already exists
                        {
                            tmpWrapperParam.ProcessParamsList(paramsList); // e.g. add @author to the wrapper's data

                            if (!WrapperDict.TryGetValue(tmpWrapperParam.WrapperName, out WrapperParam foundWrapperParam))
                            {
                                foundWrapperParam = tmpWrapperParam;

                                WrapperDict.Add(foundWrapperParam.WrapperName, foundWrapperParam); // adds the new wrapper into the list
                            }
                            else
                            {
                                foundWrapperParam.Merge(tmpWrapperParam);
                            }

                            CurrentWrapper = foundWrapperParam; // updates the new wrapper
                            CurrentSection = CurrentWrapper.GetSectionNone(); // reset the section
                        }
                        else
                        {
                            SectionParam tmpSectionParam = (SectionParam)lineParam;

                            if (!CurrentWrapper.SectionDict.TryGetValue(tmpSectionParam.SectionName, out SectionParam foundSectionParam))
                            {
                                foundSectionParam = tmpSectionParam;

                                CurrentWrapper.SectionDict.Add(foundSectionParam.SectionName, foundSectionParam); // adds the new section into the list
                            }
                            else
                            {
                                foundSectionParam.Merge(tmpSectionParam);
                            }

                            CurrentSection = foundSectionParam; // update the section
                        }

                        // cleans the params list to be used for the next function or whatever, even if there is no dataStructure match
                        paramsList.Clear();
                    }
                    else if (lineParam is FunctionParam) // match @functions and transform them into DataStructures
                    {
                        DataStructure dataStructure = new Function
                        {
                            ParamsList = new List<Param>(paramsList) // set the params with a copy of the list
                        };

                        dataStructure.ProcessDatastructure(line);

                        if (!dataStructure.Ignore)
                        {
                            dataStructure = dataStructure.CheckDataStructureTransformation() ?? dataStructure;

                            // set meta information
                            dataStructure.FoundLine = i;
                            dataStructure.FoundPath = path;

                            dataStructure.Check();

                            // now add the datastructure into the current section of the current container
                            if (!CurrentSection.DataStructureDict.TryGetValue(dataStructure.GetName(), out List<DataStructure> dsList))
                            {
                                dsList = new List<DataStructure>();

                                CurrentSection.DataStructureDict.Add(dataStructure.GetName(), dsList);
                            }

                            dsList.Add(dataStructure);
                        }

                        // cleans the params list to be used for the next function or whatever, even if there is no dataStructure match
                        paramsList.Clear();
                    }
                    else if (lineParam is IgnoreParam)
                    {
                        i++; // ignore the next line

                        paramsList.Clear(); // and clean the current params list

                        lastParam = null; // and the last param too

                        continue;
                    }
                    else
                    {
                        lastParam = lineParam; // update the last param
                    }
                }
            }
        }

        public string GetPath()
        {
            return path;
        }

        public Lang GetLang()
        {
            return lang;
        }
    }
}

using System;
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
// 3. Param, e.g. "desc" or "param"

namespace NeoDoc
{
    public class FileParser
    {
        private readonly LangMatcher langMatcher;
        private readonly Lang lang;
        private readonly string path;
        private readonly ParamMatcher paramMatcher;

        private List<WrapperParam> WrapperList { get; set; }

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

            // initialize wrapper list
            WrapperList = new List<WrapperParam>
            {
                new ModuleParam() // adds a new wrapper with default "none" data
            };

            // initializes the current vars for easy and fast access
            CurrentWrapper = WrapperList.Last();
            CurrentSection = CurrentWrapper.SectionList.Last();
        }

        public void CleanUp()
        {
            List<string> tmpArr = new List<string>();

            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Lines[i];

                line = line.TrimStart(); // clear every space in front

                // just insert relevant lines (ignore empty lines)
                if (!string.IsNullOrEmpty(line))
                {
                    tmpArr.Add(line);
                }
            }

            Lines = tmpArr.ToArray();
        }

        public void Process()
        {
            List<Param> paramsList = new List<Param>();
            Param lastParam = null;

            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Lines[i];

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
                        dataStructure.ParamsList = paramsList.ToArray(); // set the params with an array copy of the list
                        dataStructure.Process(line);

                        DataStructure transformation = dataStructure.CheckDataStructureTransformation();

                        if (transformation != null)
                        {
                            dataStructure = transformation;
                        }

                        // now add the datastructure into the current section of the current container
                        CurrentSection.DataStructureList.Add(dataStructure);
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
                        Console.WriteLine("UNREGISTERED PARAM: " + foundLineParamString);
                        Console.WriteLine(line);

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
                    if (lineParam is WrapperParam || lineParam is SectionParam)
                    {
                        if (lineParam is WrapperParam)
                        {
                            CurrentWrapper = (WrapperParam)lineParam; // updates the new wrapper

                            WrapperList.Add(CurrentWrapper); // adds the new wrapper into the list

                            CurrentSection = CurrentWrapper.SectionList.Last(); // reset the section
                        }
                        else
                        {
                            CurrentSection = (SectionParam)lineParam; // update the section

                            CurrentWrapper.SectionList.Add(CurrentSection); // adds the new section into the list
                        }

                        lastParam = null; // reset last param and wrapper don't need to support multiline
                    }
                    else
                    {
                        if (lastParam != null)
                        {
                            paramsList.Add(lastParam); // add the last param before replacing it
                        }

                        lastParam = lineParam; // update the last param
                    }
                }
            }

            // HINT: if there is a docu comment at the EOF, it won't get included because there need to be a function afterwards

            // TODO just debugging
            foreach (WrapperParam wrapper in WrapperList)
            {
            //    Console.WriteLine("Found wrapper '" + wrapper.GetName() + "'");

                foreach (SectionParam section in wrapper.SectionList)
                {
                //    Console.WriteLine("Found section '" + section.GetName() + "'");

                    foreach (DataStructure dataStructure in section.DataStructureList)
                    {
                        if (!(dataStructure is Function))
                            continue;

                    //    Console.WriteLine("Found dataStructure '" + dataStructure.GetData() + "'");

                    /*    foreach (Param p in dataStructure.ParamsList)
                        {
                            Console.WriteLine("Found: " + p.GetName() + " with data '" + p.GetOutput() + "'");
                        }*/
                    }
                }

            //    Console.WriteLine("");
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

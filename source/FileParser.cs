using System;
using System.Collections.Generic;
using System.IO;
using NeoDoc.Langs;
using NeoDoc.Params;

namespace NeoDoc
{
    public class FileParser
    {
        private readonly Lang lang;
        private readonly string path;
        private readonly ParamMatcher paramMatcher;

        private bool Ignored { get; set; }

        public string[] Lines { get; set; }
        public List<Param> paramsList { get; set; }

        public FileParser(Lang lang, string path)
        {
            this.lang = lang;
            this.path = path;

            paramMatcher = new ParamMatcher(lang);

            Lines = File.ReadAllLines(path); // Load each line of the file in the buffer

            paramsList = new List<Param>();
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

        // Preprocessing the file, e.g. to detect whether it's ignored or whether it's a module
        public void PreProcess()
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Lines[i];

                if (paramMatcher.GetLineParam(line) is Params.Ignore) // @ignore support
                {
                    Ignored = true;

                    return; // don't do anything
                }
                // TODO check for @module and @class
            }
        }

        public void Process()
        {
            if (Ignored)
                return;

            // we don't need to declare it everytime, so keep it simple and do it one time outside the loop
            Param lineParam, lastParam = null;
            string line;

            for (int i = 0; i < Lines.Length; i++)
            {
                line = Lines[i];

                if (!paramMatcher.IsLineComment(line)) // if there is no comment 
                {
                    continue;
                }

                lineParam = paramMatcher.GetLineParam(line);

                if (lineParam == null) // if there is no line param
                {
                    if (paramMatcher.IsLineCommentStart(line))
                    {
                        if (lastParam != null)
                        {
                            // add the last param before replacing it
                            paramsList.Add(lastParam);
                        }

                        lastParam = new Params.Desc(); // start with a new description per default if matching e.g. "---"
                    }

                    string foundLineParamString = paramMatcher.GetLineParamString(line);

                    if (!string.IsNullOrEmpty(foundLineParamString)) // if there is a not registered param
                    {
                        Console.WriteLine("UNREGISTERED PARAM: " + foundLineParamString);

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
                        // add the last param before replacing it
                        paramsList.Add(lastParam);
                    }

                    lastParam = lineParam; // update the last param
                }
            }

            // add the last param of the file
            if (lastParam != null)
            {
                paramsList.Add(lastParam);
            }

            // TODO
            // first: match all params - DONE
            // second: process all params (e.g. replace classes refs)
            // third: process params output and generate docs

            // TODO just debugging
            foreach (Param p in paramsList)
            {
                Console.WriteLine("Found: " + p.GetName() + " with data '" + p.GetOutput() + "'");
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

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

        public FileParser(Lang lang, string path)
        {
            this.lang = lang;
            this.path = path;

            paramMatcher = new ParamMatcher(lang);

            Lines = File.ReadAllLines(path); // Load each line of the file in the buffer
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

            for (int i = 0; i < Lines.Length; i++)
            {
                string line = Lines[i];
                Param param = paramMatcher.GetLineParam(line);

                if (param == null)
                    continue;

                Console.WriteLine("Found: " + param.GetName() + " with data '" + param.GetOutput() + "'");
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

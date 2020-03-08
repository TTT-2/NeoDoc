using System;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures.Lua
{
    public class Function : DataStructure
    {
        private string Line { get; set; }
        private bool Local { get; set; }

        public override Regex GetRegex()
        {
            return new Regex(@"^(\s*local\s+){0,1}\s*function\s*\w+((\.|\:)\w+)*\s*\(([\w" + "\"" + @"]+\s*(,\s*[\w" + "\"" + @"]+\s*)*){0,1}\)"); // RegEx matches "function opt.name(param, opt)"
        }

        public override bool Check(string line)
        {
            return GetRegex().Match(line).Success;
        }

        public override void Process(string line)
        {
            Line = line;

            Match match = new Regex(@"^\s*local\s*").Match(line);

            Local = match.Success;
        }

        public bool IsLocal()
        {
            return Local;
        }

        public override string GetName()
        {
            return "function";
        }

        public override string GetData()
        {
            return "'" + GetName() + " with data '" + Line + "'";
        }
    }
}

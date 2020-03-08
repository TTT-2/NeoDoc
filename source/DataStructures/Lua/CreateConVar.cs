using System;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures.Lua
{
    public class CreateConVar : DataStructure
    {
        private string Line { get; set; }
        public string ConVarName { get; set; }

        public override Regex GetRegex()
        {
            return new Regex(@"\s*CreateConVar\s*\([\w" + "\"" + @"]+\s*,\s*[\w" + "\"" + @"]+\s*(,\s*[\w" + "\"" + @"\{\}]+\s*){0,4}\)"); // TODO match {} table, string creation and parentheses too!
        }

        public override bool Check(string line)
        {
            return GetRegex().Match(line).Success;
        }

        public override void Process(string line)
        {
            Line = line;

            Regex splitRegex1 = new Regex(@"CreateConVar\s*\(");
            Match splitMatch1 = splitRegex1.Match(line);
            int splitPos1 = splitMatch1.Index + splitMatch1.Length;

            Regex splitRegex2 = new Regex(@"CreateConVar\s*\([\w" + "\"" + @"]+\s*");
            Match splitMatch2 = splitRegex2.Match(line);
            int splitPos2 = splitMatch2.Index + splitMatch2.Length;

            ConVarName = line.Substring(splitPos1, splitPos2 - splitPos1);
        }

        public override string GetName()
        {
            return "createconvar";
        }

        public override string GetData()
        {
            return ConVarName;
        }
    }
}

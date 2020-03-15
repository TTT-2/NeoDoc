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
            return new Regex(@"\s*CreateConVar\s*\(");
        }

        public override bool Check(string line)
        {
            return GetRegex().Match(line).Success;
        }

        public override void Process(string line)
        {
            Line = line;

            string name = null;

            if (ParamsList != null && ParamsList.Length > 0)
            {
                foreach (Param param in ParamsList)
                {
                    if (param is NameParam)
                    {
                        name = param.GetData();

                        break;
                    }
                }
            }

            if (name == null)
            {
                Regex splitRegex1 = GetRegex();
                Match splitMatch1 = splitRegex1.Match(line);
                int splitPos1 = splitMatch1.Index + splitMatch1.Length;

                Match splitMatch2 = Regex.Match(line, @"\)", RegexOptions.RightToLeft);
                int splitPos2 = splitMatch2.Index;

                string result = line.Substring(splitPos1, splitPos2 - splitPos1);
                string[] splits = result.Split(',');

                if (splits.Length > 0)
                {
                    name = splits[0];
                }
                else
                {
                    name = result;
                }
            }

            ConVarName = name.TrimEnd(')').Trim().Trim('"').Trim().Replace("\"", "\\\"");
        }

        public override string GetName()
        {
            return "createconvar";
        }

        public override string GetData()
        {
            return ConVarName;
        }

        public override bool IsGlobal()
        {
            return true;
        }

        public override string GetHTML()
        {
            string html = @"
<template>
    <main-layout>
        <p>Site of '" + ConVarName + @"'</p>
    </main-layout>
</template>
";

            return html;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures.Lua
{
    public class Hook : DataStructure
    {
        private string Line { get; set; }
        public string HookName { get; set; }
        public string HookData { get; set; }

        public override Regex GetRegex()
        {
            return new Regex(@"\s*hook\.(Run|Call)\s*\("); // RegEx matches "hook.Run(" or "hook.Call("
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
                        name = ((NameParam)param).Value;

                        break;
                    }
                }
            }

            Match splitMatch = GetRegex().Match(line);

            string result = line.Substring(splitMatch.Index, line.Length - splitMatch.Index);

            bool mode = new Regex(@"\s*hook\.Call\s*\(").Match(Line).Success; // if false, "hook.Run(" is found

            List<string> tmpData = GetVarsFromFunction(result);

            HookName = (name ?? tmpData[0]).Trim('"');

            HookData = "GM:" + HookName + "(" + string.Join(", ", tmpData.GetRange(mode ? 2 : 1, tmpData.Count - (mode ? 2 : 1)).ToArray()) + ")"; // "hook.Call( string eventName, table gamemodeTable, vararg args )" or "hook.Run( string eventName, vararg args )"
        }

        public override string GetName()
        {
            return "hook";
        }

        public override object GetData()
        {
            return HookData;
        }

        public override string GetDatastructureName()
        {
            return HookName;
        }
    }
}

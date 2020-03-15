using System;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures.Lua
{
    public class Hook : DataStructure
    {
        private string Line { get; set; }
        public string HookName { get; set; }

        public override Regex GetRegex()
        {
            return new Regex(@"^\s*hook\.(Run|Call)\s*\("); // RegEx matches "hook.Run(" or "hook.Call("
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

                string result = line.Substring(splitPos1, line.Length - splitPos1);
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

            HookName = name.TrimEnd(')').Trim().Trim('"').Trim().Replace("\"", "\\\"");
        }

        public override string GetName()
        {
            return "hook";
        }

        public override string GetData()
        {
            return HookName;
        }

        public override bool IsGlobal()
        {
            return true;
        }

        public override string GetHTML()
        {
            return "Hook: " + HookName;
        }
    }
}

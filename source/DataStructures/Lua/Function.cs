using System;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures.Lua
{
    public class Function : DataStructure
    {
        private string Line { get; set; }
        private bool Local { get; set; }
        private string Name { get; set; }

        public override Regex GetRegex()
        {
            return new Regex(@"(^\s*(local\s+)?\s*|@)function\s*\w+((\.|\:)\w+)*\s*\((\w+\s*(,\s*\w+\s*)*)?\)"); // RegEx matches "@function opt.name(param, opt)" or "local function opt:name()"
        }

        public override bool Check(string line)
        {
            return GetRegex().Match(line).Success;
        }

        public override void Process(string line)
        {
            // if param "@local" found
            if (ParamsList != null && ParamsList.Length > 0)
            {
                foreach (Param param in ParamsList)
                {
                    if (param is LocalParam)
                    {
                        Local = true;

                        break;
                    }
                }
            }

            Line = line;

            if (!Local)
            {
                Match match = new Regex(@"^\s*local\s*").Match(line);

                Local = match.Success;
            }

            Name = GetRegex().Match(line).Value;
        }

        public bool IsLocal()
        {
            return Local;
        }

        public override string GetName()
        {
            return "function";
        }

        public override string GetJSONData()
        {
            if (Local)
                return null;

            return "\"" + Name.TrimStart('@') + "\"";
        }

        public override DataStructure CheckDataStructureTransformation()
        {
            // check whether it's a hook

            // if param "@hook" found
            if (ParamsList != null && ParamsList.Length > 0)
            {
                foreach (Param param in ParamsList)
                {
                    if (param is HookParam)
                    {
                        return new Hook
                        {
                            HookName = Name.Replace("function GM:", "").Replace("function GAMEMODE:", "").Split('(')[0].Trim()
                        };
                    }
                }
            }

            // if "GM" or "GAMEMODE" found
            Regex regex = new Regex(@"\s*function\s*(GAMEMODE|GM)\:\w+\s*\(");

            if (regex.Match(Line).Success)
            {
                return new Hook
                {
                    HookName = Name.Replace("function GM:", "").Replace("function GAMEMODE:", "").Split('(')[0].Trim()
                };
            }

            return null;
        }
    }
}

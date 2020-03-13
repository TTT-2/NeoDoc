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
            return new Regex(@"^(\s*local\s+){0,1}\s*function\s*\w+((\.|\:)\w+)*\s*\((\w+\s*(,\s*\w+\s*)*){0,1}\)"); // RegEx matches "function opt.name(param, opt)"
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

        public override string GetData()
        {
            return "'" + GetName() + " with data '" + Name + "'";
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
                            HookName = Name
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
                    HookName = Name
                };
            }

            return null;
        }
    }
}

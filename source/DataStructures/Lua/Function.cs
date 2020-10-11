using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;
using Newtonsoft.Json;

namespace NeoDoc.DataStructures.Lua
{
    public class Function : DataStructure
    {
        private string Line { get; set; }
        private bool Local { get; set; }
        private string FunctionData { get; set; }
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

            Ignore = Local;

            FunctionData = GetRegex().Match(line).Value.TrimStart('@');
            Name = FunctionData.Replace("function ", "").Split('(')[0].Trim();
        }

        public bool IsLocal()
        {
            return Local;
        }

        public override string GetName()
        {
            return "function";
        }

        public override object GetData()
        {
            return FunctionData;
        }

        public override string GetDatastructureName()
        {
            return Name;
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
                            HookName = FunctionData.Split(':')[1].Split('(')[0].Trim(),
                            HookData = FunctionData
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
                    HookName = FunctionData.Split(':')[1].Split('(')[0].Trim(),
                    HookData = FunctionData
                };
            }

            return null;
        }

        public override void Check()
        {
            if (!Ignore)
            {
                string dataStructureData = (string)GetData();
                List<string> expectedParams = GetVarsFromFunction(dataStructureData);
                List<Param> paramParams = new List<Param>();

                foreach (Param param in ParamsList)
                {
                    if (param is ParamParam)
                        paramParams.Add(param);
                }

                if (paramParams.Count != expectedParams.Count)
                {
                    List<string> errors = new List<string>()
                    {
                        "Param mismatch in " + GetName() + " (" + dataStructureData + ")!",
                        "Given params (" + paramParams.Count + "): "
                    };

                    foreach (Param paramParam in paramParams)
                    {
                        errors.Add(paramParam.GetName());
                    }

                    errors.Add("");
                    errors.Add("Expected Params (" + expectedParams.Count + "): ");

                    foreach (string paramParamName in expectedParams)
                    {
                        errors.Add(paramParamName);
                    }

                    errors.Add("----");
                    errors.Add("");

                    NeoDoc.WriteErrors(errors);
                }
            }
        }
    }
}

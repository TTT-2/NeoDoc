using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures.Lua
{
    public class CreateConVar : DataStructure
    {
        private string Line { get; set; }
        public string ConVarName { get; set; }
        private string[] ConVarData { get; set; }

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
                        name = ((NameParam)param).Value;

                        break;
                    }
                }
            }

            Match splitMatch = GetRegex().Match(line);
            string result = line.Substring(splitMatch.Index, line.Length - splitMatch.Index);

            ConVarData = GetVarsFromFunction(result).ToArray();
            ConVarName = (name ?? ConVarData[0]).Trim('"');
        }

        public override string GetName()
        {
            return "createconvar";
        }

        public override object GetData()
        {
            // ConVar CreateConVar( string name, string value, number flags = FCVAR_NONE, string helptext, number min = nil, number max = nil )
            Dictionary<string, string> retDict = new Dictionary<string, string>();

            int length = ConVarData.Length;

            if (length < 2)
                return null;
            else
                retDict.Add("value", ConVarData[1]);

            if (length > 2)
                retDict.Add("flags", ConVarData[2]);

            if (length > 3)
                retDict.Add("helptext", ConVarData[3]);

            if (length > 4)
                retDict.Add("min", ConVarData[4]);

            if (length > 5)
                retDict.Add("max", ConVarData[5]);

            return retDict;
        }

        public override bool IsGlobal()
        {
            return true;
        }

        public override string GetDatastructureName()
        {
            return ConVarName;
        }
    }
}

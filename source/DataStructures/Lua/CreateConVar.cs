using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;
using Newtonsoft.Json;

namespace NeoDoc.DataStructures.Lua
{
	public class CreateConVar : DataStructure
	{
		private string Line { get; set; }
		private string[] ConVarData { get; set; }

		[JsonProperty("name")]
		public string ConVarName { get; set; }

		public override Regex GetRegex()
		{
			return new Regex(@"\s*CreateConVar\s*\(.*\)");
		}

		public override bool CheckMatch(string line)
		{
			return GetRegex().Match(line).Success;
		}

		public override void Process(FileParser fileParser)
		{
			string name = null;

			if (ParamsList != null && ParamsList.Count > 0)
			{
				for (int i = 0; i < ParamsList.Count; i++)
				{
					if (ParamsList[i] is NameParam nameParam)
					{
						name = nameParam.Value;

						ParamsList.RemoveAt(i);

						break;
					}
				}

				if (ParamsList.Count == 0)
					ParamsList = null;
			}

			for (int j = 0; j < fileParser.CurrentMatchedLines; j++)
			{
				Line = Line + fileParser.Lines[fileParser.CurrentLineCount + j];
			}

			Match splitMatch = GetRegex().Match(Line);
			string result = Line.Substring(splitMatch.Index, Line.Length - splitMatch.Index);

			ConVarData = NeoDoc.GetEntriesFromString(result, out _).ToArray();
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

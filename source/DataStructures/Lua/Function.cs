using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;
using Newtonsoft.Json;

namespace NeoDoc.DataStructures.Lua
{
	public class Function : DataStructure
	{
		internal string Line { get; set; }
		internal bool Local { get; set; }

		[JsonProperty("data")]
		public string FunctionData { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		public override Regex GetRegex()
		{
			// RegEx matches "@function opt.name(param, opt)" or "local function opt:name()"
			return new Regex(@"((local\s+)?|@)function\s*\w+((\.|\:)\w+)?\s*\((\s*\w+\s*(,\s*\w+\s*)*)?\)");
		}

		public override bool CheckMatch(string line)
		{
			return GetRegex().Match(line).Success;
		}

		public override void Process(FileParser fileParser)
		{
			// if param "@local" found
			if (ParamsList != null && ParamsList.Count > 0)
			{
				for (int i = 0; i < ParamsList.Count; i++)
				{
					if (ParamsList[i] is LocalParam)
					{
						Local = true;

						ParamsList.RemoveAt(i);

						break;
					}
				}
			}

			for (int j = 0; j < fileParser.CurrentMatchedLines; j++)
			{
				Line = Line + fileParser.Lines[fileParser.CurrentLineCount + j];
			}

			if (!Local)
			{
				Match match = new Regex(@"^\s*local\s*").Match(Line);

				Local = match.Success;
			}

			if (Local)
				Ignore = true;

			FunctionData = GetRegex().Match(Line).Value.TrimStart('@');
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
			return null;
		}

		public override string GetDatastructureName()
		{
			return Name;
		}

		public override DataStructure CheckDataStructureTransformation()
		{
			// check whether it's a hook

			// if param "@hook" found
			if (ParamsList != null && ParamsList.Count > 0)
			{
				for (int i = 0; i < ParamsList.Count; i++)
				{
					if (ParamsList[i] is HookParam)
					{
						ParamsList.RemoveAt(i);

						if (ParamsList.Count == 0)
							ParamsList = null;

						return new Hook
						{
							HookName = FunctionData.Replace("function ", "").Split('(')[0].Trim().Replace("GAMEMODE", "GM"),
							HookData = FunctionData.Replace("GAMEMODE", "GM"),
							GlobalWrapper = FunctionData.Replace("function ", "").Split(':')[0].Trim().Replace("GAMEMODE", "GM"),
							Line = Line,
							ParamsList = ParamsList,
							Realm = Realm,
							Ignore = Ignore,
							FoundLine = FoundLine,
							FoundPath = FoundPath,
							IsMain = true
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
					HookName = FunctionData.Replace("function ", "").Split('(')[0].Trim().Replace("GAMEMODE", "GM"),
					HookData = FunctionData.Replace("GAMEMODE", "GM"),
					Line = Line,
					ParamsList = ParamsList,
					Realm = Realm,
					Ignore = Ignore,
					FoundLine = FoundLine,
					FoundPath = FoundPath,
					IsMain = true
				};
			}

			return base.CheckDataStructureTransformation();
		}

		public override void Check()
		{
			List<string> expectedParams = NeoDoc.GetEntriesFromString(FunctionData, out _);
			List<ParamParam> paramParams = new List<ParamParam>();

			if (ParamsList != null)
			{
				foreach (Param param in ParamsList)
				{
					if (param is ParamParam paramParam)
						paramParams.Add(paramParam);
				}
			}

			if (paramParams.Count != expectedParams.Count)
			{
				List<string> errors = new List<string>()
				{
					"In '" + GetName() + "' datastructure ('" + FunctionData + "'), detected params (" + paramParams.Count + "): "
				};

				foreach (ParamParam paramParam in paramParams)
				{
					errors.Add("- '" + paramParam.Name + "'");
				}

				errors.Add("Expected Params (" + expectedParams.Count + "): ");

				foreach (string paramParamName in expectedParams)
				{
					errors.Add("- '" + paramParamName + "'");
				}

				NeoDoc.WriteErrors("Param mismatch", errors, FoundPath, FoundLine, (int)NeoDoc.ERROR_CODES.PARAM_MISMATCH);
			}
		}
	}
}

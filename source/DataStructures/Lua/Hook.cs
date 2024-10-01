using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;
using Newtonsoft.Json;

namespace NeoDoc.DataStructures.Lua
{
	public class Hook : DataStructure
	{
		[JsonIgnore]
		public string Line { get; set; }

		[JsonIgnore]
		public override string GlobalWrapper { get; set; } = "GM";

		[JsonIgnore]
		public bool IsMain { get; set; } = false;

		[JsonIgnore]
		public List<Hook> Calls { get; set; } = new List<Hook>();

		[JsonIgnore]
		public string HookName { get; set; }

		[JsonProperty("data")]
		public string HookData { get; set; }

		public override Regex GetRegex()
		{
			return new Regex(@"hook\.(Run|Call)\s*\(.*\)"); // RegEx matches "hook.Run(" or "hook.Call("
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

			if (splitMatch.NextMatch().Success) // there are multiple hooks in this line
				NeoDoc.WriteErrors("Multiple datastructures found", null, fileParser.relPath, fileParser.CurrentLineCount + 1, (int)NeoDoc.ERROR_CODES.MULTIPLE_DS_IN_LINE);

			string result = Line.Substring(splitMatch.Index, Line.Length - splitMatch.Index);

			bool hookRun = new Regex(@"hook\.Run\s*\(").Match(Line).Success; // if false, "hook.Call(" is found

			List<string> tmpData = NeoDoc.GetEntriesFromString(result, out _);

			HookName = GlobalWrapper + ":" + (name ?? tmpData[0]).Trim('"');

			// "hook.Call( string eventName, table gamemodeTable, vararg args )" or "hook.Run( string eventName, vararg args )"
			HookData = HookName + "(" + string.Join(", ", tmpData.GetRange(hookRun ? 1 : 2, tmpData.Count - (hookRun ? 1 : 2)).ToArray()) + ")";
		}

		public override string GetName()
		{
			return "hook";
		}

		public override object GetData()
		{
			if (Calls.Count == 0)
				return null;

			return new Dictionary<string, object>{
				{ "calls", Calls }
			};
		}

		public override string GetDatastructureName()
		{
			return HookName;
		}

		public override bool IsGlobal()
		{
			return true;
		}

		public override DataStructure Merge(DataStructure dataStructure)
		{
			if (dataStructure is Hook hook)
			{
				if (hook.IsMain)
				{
					if (IsMain) // both are main
					{
						return null; // error
					}
					else // just new one is main
					{
						hook.Calls = Calls; // set reference
						Calls = new List<Hook>(); // clear reference

						hook.Calls.Add(this); // add this as a call too

						return hook; // replace the current ds with the new hook in the final ds list
					}
				}
				else // new one isn't main
				{
					Calls.Add(hook);
				}

				return this; // this has merged
			}

			return base.Merge(dataStructure);
		}
	}
}

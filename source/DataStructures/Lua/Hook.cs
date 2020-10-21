using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures.Lua
{
	public class Hook : DataStructure
	{
		public string Line { get; set; }
		public string HookName { get; set; }
		public string HookData { get; set; }
		public override string GlobalWrapper { get; set; } = "GM";
		public List<Hook> Calls { get; set; } = new List<Hook>();
		public bool IsMain { get; set; } = false;

		public override Regex GetRegex()
		{
			return new Regex(@"\s*hook\.(Run|Call)\s*\("); // RegEx matches "hook.Run(" or "hook.Call("
		}

		public override bool CheckMatch(string line)
		{
			return GetRegex().Match(line).Success;
		}

		public override void Process(FileParser fileParser)
		{
			Line = fileParser.Lines[fileParser.CurrentLineCount];

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
			}

			Match splitMatch = GetRegex().Match(Line);

			string result = Line.Substring(splitMatch.Index, Line.Length - splitMatch.Index);

			bool mode = new Regex(@"\s*hook\.Call\s*\(").Match(Line).Success; // if false, "hook.Run(" is found

			List<string> tmpData = GetVarsFromFunction(result);

			HookName = GlobalWrapper + ":" + (name ?? tmpData[0]).Trim('"');

			HookData = HookName + "(" + string.Join(", ", tmpData.GetRange(mode ? 2 : 1, tmpData.Count - (mode ? 2 : 1)).ToArray()) + ")"; // "hook.Call( string eventName, table gamemodeTable, vararg args )" or "hook.Run( string eventName, vararg args )"
		}

		public override string GetName()
		{
			return "hook";
		}

		public override object GetData()
		{
			return Calls;
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

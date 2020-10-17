using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NeoDoc.Params
{
	public abstract class URLParam : Param
	{
		public string Value { get; set; } = "";
		private readonly Regex URLRegex = new Regex(@"(?<Protocol>\w+):\/\/(?<Domain>[\w@][\w.:@]+)\/?[\w\.?=%&=\-@/$,]*");

		public override Dictionary<string, object> GetData()
		{
			Dictionary<string, object> tmpDict = new Dictionary<string, object>();

			if (!string.IsNullOrEmpty(Value))
				tmpDict.Add("value", Value);

			return tmpDict;
		}

		public override void Process(string[] paramData)
		{
			if (paramData.Length < 1)
				return;

			string tmp = paramData[0];

			if (CheckURL(tmp))
				Value = tmp;
		}

		public override void ProcessAddition(string[] paramData)
		{
			Process(paramData);
		}

		private bool CheckURL(string url)
		{
			return URLRegex.Match(url).Success;
		}

		public override void ModifyFileParser(FileParser fileParser)
		{
			if (string.IsNullOrEmpty(Value))
				NeoDoc.WriteErrors(new List<string>
				{
					"Detected missing or wrong argument format for '@" + GetName() + "' param, Source: '" + fileParser.relPath + "' (ll. " + (fileParser.CurrentLineCount + 1) + ")"
				});

			base.ModifyFileParser(fileParser);
		}
	}
}

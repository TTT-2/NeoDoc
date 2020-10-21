using System.Collections.Generic;

namespace NeoDoc.Params
{
	public abstract class StateParam : Param
	{
		public string Value { get; set; } = "";

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

			Value = paramData[0];
		}

		public override void ProcessAddition(string[] paramData)
		{
			Process(paramData);
		}

		public override void ModifyFileParser(FileParser fileParser)
		{
			if (string.IsNullOrEmpty(Value))
				NeoDoc.WriteErrors("Invalid param argument format", new List<string>
				{
					"In '@" + GetName() + "' param"
				}, fileParser.relPath, fileParser.CurrentLineCount + 1, (int)NeoDoc.ERROR_CODES.INVALID_PARAM_ARGS_FORMAT);

			base.ModifyFileParser(fileParser);
		}
	}
}

using System.Collections.Generic;

namespace NeoDoc.Params
{
	public abstract class TextParam : Param
	{
		public string Text { get; set; } = "";

		public override Dictionary<string, object> GetData()
		{
			Dictionary<string, object> tmpDict = new Dictionary<string, object>();

			if (!string.IsNullOrEmpty(Text))
				tmpDict.Add("text", Text);

			return tmpDict;
		}

		public override void Process(string[] paramData)
		{
			if (paramData.Length < 1)
				return;

			Text = string.Join(" ", paramData);
		}

		public override void ProcessAddition(string[] paramData)
		{
			if (paramData.Length < 1)
				return;

			if (!string.IsNullOrEmpty(Text))
			{
				Text += " ";
			}

			Text += string.Join(" ", paramData);
		}
	}
}

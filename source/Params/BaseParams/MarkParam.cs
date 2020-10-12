using System.Collections.Generic;

namespace NeoDoc.Params
{
	public abstract class MarkParam : Param
	{
		public override Dictionary<string, object> GetData()
		{
			return null;
		}

		public override void Process(string[] paramData)
		{
			// nothing to process
		}

		public override void ProcessAddition(string[] paramData)
		{
			// nothing to process
		}
	}
}

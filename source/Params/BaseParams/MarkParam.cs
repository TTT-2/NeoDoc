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

		public override Dictionary<string, object> GetJSONData()
		{
			Dictionary<string, object> tmpData = GetData();

			if (tmpData == null) // if there is no json data, settings will not make sense by default (like for a MarkParam), so those are excluded as well
			{
				if (SettingsDict != null && SettingsDict.Count > 0)
					NeoDoc.WriteErrors("SettingsData detected in a non-json param", null, FoundPath, FoundLine, (int)NeoDoc.ERROR_CODES.NO_SETTINGS_PARAM);

				return null;
			}

			return base.GetJSONData();
		}
	}
}

﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeoDoc.Params
{
	public abstract class Param
	{
		[JsonIgnore]
		public string FoundPath;

		[JsonIgnore]
		public int FoundLine;

		public Dictionary<string, object> SettingsDict { get; set; }

		public abstract string GetName(); // returns the name of the param
		public abstract void Process(string[] paramData); // paramData = everything except the @param prefix part
		public abstract void ProcessAddition(string[] paramData); // paramData = everything of the line that already hadn't a param part
		public abstract Dictionary<string, object> GetData(); // returns the data that should get returned, e.g. Name, Text, etc.

		public virtual Dictionary<string, object> GetJSONData() // returns the json output used for the website
		{
			Dictionary<string, object> tmpDict = new Dictionary<string, object>();

			Dictionary<string, object> tmpData = GetData();

			if (tmpData != null && tmpData.Count > 0)
			{
				foreach (KeyValuePair<string, object> entry in tmpData)
				{
					tmpDict.Add(entry.Key, entry.Value);
				}
			}

			if (SettingsDict != null && SettingsDict.Count > 0)
				tmpDict.Add("settings", SettingsDict);

			return tmpDict;
		}

		public virtual void ProcessSettings(string[] paramSettings) // paramData = everything except the @param prefix part
		{
			if (paramSettings.Length < 1)
				return;

			SettingsDict = new Dictionary<string, object>();

			foreach (string settingConstruct in paramSettings)
			{
				string[] settingSplit = settingConstruct.Trim().Split('='); // split e.g. "default=true"

				if (settingSplit.Length > 1)
					SettingsDict.Add(settingSplit[0].Trim(), settingSplit[1].Trim());
				else
					SettingsDict.Add(settingSplit[0].Trim(), true);
			}
		}

		public virtual void ModifyFileParser(FileParser fileParser)
		{
			FoundPath = fileParser.relPath;
			FoundLine = fileParser.CurrentLineCount + 1;

			fileParser.paramsList.Add(this); // add self into the list
		}
	}
}

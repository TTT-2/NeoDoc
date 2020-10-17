using System;
using System.Collections.Generic;
using System.Linq;
using NeoDoc.DataStructures;
using Newtonsoft.Json;

namespace NeoDoc.Params
{
	public abstract class WrapperParam : Param
	{
		public List<string> Authors { get; set; }

		public string WrapperName { get; set; } = "none";
		public SortedDictionary<string, SectionParam> SectionDict { get; set; }

		public WrapperParam()
		{
			SectionDict = new SortedDictionary<string, SectionParam>
			{
				{ "none", new SectionParam() } // adds a new section with default "none" data
			};

			Authors = new List<string>();
		}

		public SectionParam GetSection(string name)
		{
			SectionDict.TryGetValue(name, out SectionParam tmpSection);

			return tmpSection;
		}

		public SectionParam GetSectionNone()
		{
			SectionDict.TryGetValue("none", out SectionParam tmpSection);

			return tmpSection;
		}

		public override Dictionary<string, object> GetData()
		{
			return new Dictionary<string, object>
			{
				{ "name", WrapperName }
			};
		}

		public override void Process(string[] paramData)
		{
			if (paramData.Length < 1)
				return;

			WrapperName = paramData[0];
		}

		public override void ProcessAddition(string[] paramData)
		{
			// nothing to process
		}

		public void ProcessParamsList(List<Param> paramsList)
		{
			for (int i = 0; i < paramsList.Count; i++)
			{
				if (paramsList[i] is AuthorParam authorParam)
				{
					Authors.Add(authorParam.Text);
				}
			}
		}

		public Dictionary<string, object> GetDataDict()
		{
			Dictionary<string, object> jsonDict = GetData();

			if (Authors.Count > 0)
				jsonDict.Add("authors", Authors);

			Dictionary<string, object> sectionsDict = new Dictionary<string, object>();

			foreach (KeyValuePair<string, SectionParam> keyValuePair in SectionDict)
			{
				sectionsDict.Add(keyValuePair.Key, keyValuePair.Value.GetDataDict());
			}

			jsonDict.Add("sections", sectionsDict);

			return jsonDict;
		}

		public void Merge(WrapperParam wrapperParam)
		{
			foreach (string author in wrapperParam.Authors)
			{
				if (!Authors.Contains(author))
					Authors.Add(author);
			}

			// now we need to search for any section and add it into the wrapper AS WELL AS merging same sections of same wrappers together
			foreach (SectionParam section in wrapperParam.SectionDict.Values)
			{
				if (section.DataStructureDict.Count < 1) // do not include empty sections
					continue;

				// section already exists?
				SectionParam finalSection = null;

				foreach (SectionParam tmpSection in SectionDict.Values)
				{
					if (tmpSection.SectionName == section.SectionName)
					{
						finalSection = tmpSection;

						break;
					}
				}

				if (finalSection == null)
					SectionDict.Add(section.SectionName, section); // add missing section directly into the wrappers section list
				else
					finalSection.Merge(section);
			}
		}

		internal void ProcessGlobals(SortedDictionary<string, Dictionary<string, List<DataStructure>>> globalsDict)
		{
			SortedDictionary<string, SectionParam> tmpSectionDict = new SortedDictionary<string, SectionParam>();

			foreach (KeyValuePair<string, SectionParam> keyValuePair in SectionDict)
			{
				keyValuePair.Value.ProcessGlobals(globalsDict);

				if (keyValuePair.Value.DataStructureDict.Count > 0)
					tmpSectionDict.Add(keyValuePair.Key, keyValuePair.Value); // exclude empty sections
			}

			SectionDict = tmpSectionDict;
		}

		public override void ModifyFileParser(FileParser fileParser)
		{
			// TODO what if Wrapper already exists

			ProcessParamsList(fileParser.paramsList); // e.g. add @author to the wrapper's data

			if (!fileParser.WrapperDict.TryGetValue(WrapperName, out WrapperParam foundWrapperParam))
			{
				foundWrapperParam = this;

				fileParser.WrapperDict.Add(WrapperName, this); // adds the new wrapper into the list
			}
			else
			{
				foundWrapperParam.Merge(this);
			}

			fileParser.CurrentWrapper = foundWrapperParam; // updates the new wrapper
			fileParser.CurrentSection = foundWrapperParam.GetSectionNone(); // reset the section

			// cleans the params list to be used for the next function or whatever, even if there is no dataStructure match
			fileParser.paramsList.Clear();
		}
	}
}

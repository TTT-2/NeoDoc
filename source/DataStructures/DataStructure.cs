using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;
using Newtonsoft.Json;

namespace NeoDoc.DataStructures
{
	public abstract class DataStructure
	{
		public bool Ignore { get; set; } = false;
		public string Realm { get; set; } = "shared";
		public virtual string GlobalWrapper { get; set; } = "none";
		public List<Param> ParamsList;
		public string FoundPath;
		public int FoundLine;

		public abstract Regex GetRegex(); // returns the exact RegEx to match e.g. the Function
		public abstract void Process(FileParser fileParser); // process data based on given line string
		public abstract bool CheckMatch(string line); // returns whether the current DocTarget is matched in this line
		public abstract string GetName(); // returns an identification name
		public abstract string GetDatastructureName(); // returns the individual name of the matched datasctructure
		public abstract object GetData(); // returns data

		public virtual void ProcessDatastructure(FileParser fileParser) // used to set default data
		{
			bool foundRealm = false;

			// if param "@realm" or "@ignore" found
			if (ParamsList != null)
			{
				List<Param> copyParamList = new List<Param>();

				for (int i = 0; i < ParamsList.Count; i++)
				{
					Param curParam = ParamsList[i];

					if (curParam is RealmParam realmParam)
					{
						Realm = realmParam.Value;

						foundRealm = true;
					}
					else if (curParam is IgnoreParam)
						Ignore = true;
					else
						copyParamList.Add(curParam);
				}

				ParamsList = copyParamList;
			}

			if (!Ignore)
				Process(fileParser);

			if (!Ignore && !foundRealm)
				NeoDoc.WriteErrors(new List<string>{
					"Missing '@realm' in " + GetName() + " '" + GetDatastructureName() + "', Source: '" + FoundPath + "' (ll. " + FoundLine + ")"
				});
		}

		public virtual void Check() // used to finally check for errors and to print them into the console
		{

		}

		public virtual string GetFullJSONData() // returns full json data, used for the entire page structure
		{
			Dictionary<string, object> jsonDict = new Dictionary<string, object>
			{
				{ "type", "datastructure" },
				{ "subtype", GetName() },
				{ "name", GetDatastructureName() },
				{ "realm", Realm },
				{ "source", new Dictionary<string, object>()
					{
						{ "file", FoundPath },
						{ "line", FoundLine }
					}
				}
			};

			object data = GetData();

			if (data != null)
				jsonDict.Add("data", data);

			if (ParamsList != null && ParamsList.Count > 0)
			{
				// merge same datastructures together
				SortedDictionary<string, List<object>> paramsDict = new SortedDictionary<string, List<object>>();

				foreach (Param param in ParamsList)
				{
					if (!paramsDict.TryGetValue(param.GetName(), out List<object> paramsJSONList))
					{
						paramsJSONList = new List<object>();

						paramsDict.Add(param.GetName(), paramsJSONList);
					}

					Dictionary<string, object> jsonData = param.GetJSONData();

					if (jsonData.Count > 0)
						paramsJSONList.Add(jsonData);
				}

				jsonDict.Add("params", paramsDict);
			}

			return JsonConvert.SerializeObject(jsonDict, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});
		}

		public virtual DataStructure CheckDataStructureTransformation() // checks whether the data structure should be transformed
		{
			return this;
		}

		public virtual bool IsGlobal() // whether the DataStructure should be excluded from the sections / wrappers data structuring
		{
			return false;
		}

		internal int CompareTo(DataStructure y)
		{
			return GetDatastructureName().CompareTo(y.GetDatastructureName());
		}

		internal List<string> GetVarsFromFunction(string result)
		{
			int bracketsDeepness = 0;
			string tmpString = "";

			List<string> varsList = new List<string>();

			foreach (char c in result)
			{
				switch (c)
				{
					case '(':
					case '{':
						if (bracketsDeepness > 0) // if we are in the function hook
							tmpString += c;

						bracketsDeepness++;

						break;
					case ')':
					case '}':
						bracketsDeepness--;

						if (bracketsDeepness > 0)
							tmpString += c;

						break;
					case ',':
						if (bracketsDeepness == 1) // if we are directly in the function hook's instance
						{
							if (!string.IsNullOrEmpty(tmpString))
								varsList.Add(tmpString);

							tmpString = "";
						}
						else
						{
							tmpString += c;
						}

						break;
					default:
						if (bracketsDeepness > 0) // if we are in the function hook
							tmpString += c;

						break;
				}

				if (bracketsDeepness < 0) // don't continue, even if the line hasn't ended yet
					break;
			}

			if (!string.IsNullOrEmpty(tmpString))
				varsList.Add(tmpString);

			List<string> finalList = new List<string>();

			for (int i = 0; i < varsList.Count; i++)
			{
				finalList.Add(varsList[i].Trim());
			}

			return finalList;
		}

		public virtual void Initialize(FileParser fileParser)
		{
			ParamsList = new List<Param>(fileParser.paramsList); // set the params with a copy of the list

			// set meta information
			FoundLine = fileParser.CurrentLineCount + 1;
			FoundPath = fileParser.relPath;

			ProcessDatastructure(fileParser);

			if (Ignore) // just add this datastructure if it does not contain the "Ignore" flag
				return;

			DataStructure dataStructure = CheckDataStructureTransformation();

			if (dataStructure == null)
				return;

			dataStructure.Check();

			// now add the datastructure into the current section of the current container
			if (!fileParser.CurrentSection.DataStructureDict.TryGetValue(dataStructure.GetName(), out List<DataStructure> dsList))
			{
				dsList = new List<DataStructure>();

				fileParser.CurrentSection.DataStructureDict.Add(dataStructure.GetName(), dsList);
			}

			dsList.Add(dataStructure);
		}

		public virtual DataStructure Merge(DataStructure dataStructure)
		{
			return null;
		}
	}

	class DataStructureComparator : IComparer<DataStructure>
	{
		public int Compare(DataStructure x, DataStructure y)
		{
			if (x == null || y == null)
			{
				return 0;
			}

			// "CompareTo()" method 
			return x.CompareTo(y);
		}
	}
}

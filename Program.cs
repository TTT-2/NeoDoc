using NeoDoc.DataStructures;
using NeoDoc.Langs;
using NeoDoc.Params;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/*
 * TODO
 * Enums, global table, roleData etc. fetch (class attributes)
 */

namespace NeoDoc
{
	internal static class NeoDoc
	{
		public const bool DEBUGGING = false;
		public static int Progress = 0;
		public static string NEWDIR = Directory.GetCurrentDirectory() + "../../../output";

		public enum ERROR_CODES: int
		{
			INVALID_COMMAND_LINE = 0x667,
			BAD_ARGUMENTS = 0xA0,
			NOT_EXISTS = 0x194,
			MISSING_ESSENTIAL_PARAMS = 0x1000,
			UNREGISTERED_PARAM = 0x2000,
			PARAM_MISMATCH = 0x3000,
			MERGING_ISSUE = 0x4000,
			INVALID_PARAM_ARGS_FORMAT = 0x5000
		}

		public static void Main()
		{
			string[] args = Environment.GetCommandLineArgs();

			if (args.Length == 1)
			{
				Console.Error.WriteLine("Invalid command line (missing folder path)!");

				Environment.ExitCode = (int)ERROR_CODES.INVALID_COMMAND_LINE;

				return;
			}

			string folder = args[1];

			if (string.IsNullOrEmpty(folder))
			{
				Console.Error.WriteLine("Provided folder path is null or empty!");

				Environment.ExitCode = (int)ERROR_CODES.BAD_ARGUMENTS;

				return;
			}

			if (!Directory.Exists(folder))
			{
				Console.Error.WriteLine("Provided folder '" + folder + "' does not exists!");

				Environment.ExitCode = (int)ERROR_CODES.NOT_EXISTS;

				return;
			}

			// Build the file tree
			string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

			// Prepare the files
			List<FileParser> fileParsers = new List<FileParser>();

			int amount = files.Length;

			LangMatcher langMatcher = new LangMatcher(); // lang processing system

			for (int n = 0; n < amount; n++)
			{
				string file = files[n];

				string relPath = file.Remove(0, folder.Length);
				relPath = relPath.TrimStart('\\');
				relPath = relPath.Replace('\\', '/');

				Progress = (int)Math.Floor((n + 1) / (double)amount * 100.0);

				WriteDebugInfo("[" + Progress + "%] '" + relPath + "'");

				// get the lang based on the file extension
				Lang lang = langMatcher.GetByFileExtension(Path.GetExtension(file));

				if (lang == null)
					continue;

				WriteDebugInfo("Running '" + lang.GetName() + "' parser");

				FileParser fileParser = new FileParser(langMatcher, lang, file, relPath); // fileParser is used to process a file
				fileParser.CleanUp();
				fileParser.Process();

				fileParsers.Add(fileParser);

				WriteDebugInfo("Finished parsing");
				WriteDebugInfo("");
			}

			List<WrapperParam> wrapperList = new List<WrapperParam>(ProcessFileParsers(fileParsers, out SortedDictionary<string, SortedDictionary<string, List<DataStructure>>> globalsDict));

			// Generate Folders
			if (Directory.Exists(NEWDIR))
				Directory.Delete(NEWDIR, true);

			Directory.CreateDirectory(NEWDIR);

			// Write single files
			GenerateDocumentationData(wrapperList, globalsDict);

			// Write overviews JSON
			GenerateJSONIndex(wrapperList, globalsDict);

			// Write search JSON
			GenerateJSONSearch(wrapperList, globalsDict);

			WriteDebugInfo("Finished generating documentation.");
		}

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
		internal static void WriteDebugInfo(string debugInfo)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
		{
#pragma warning disable CS0162 // Unerreichbarer Code wurde entdeckt.
			if (!DEBUGGING)
				return;

			ConsoleColor oldColor = Console.ForegroundColor;

			Console.ForegroundColor = ConsoleColor.Yellow;

			Console.Out.WriteLine(debugInfo);

			Console.ForegroundColor = oldColor;
#pragma warning restore CS0162 // Unerreichbarer Code wurde entdeckt.
		}

		internal static void WriteErrors(string title, List<string> errors, string relPath, int? foundLine, int? exitCode)
		{
			if (exitCode != null)
			{
				Console.WriteLine("Error " + exitCode);

				Environment.ExitCode = (int)exitCode;
			}

			ConsoleColor oldColor = Console.ForegroundColor;

			Console.ForegroundColor = ConsoleColor.Red;

			TextWriter textWriter = Console.Error;

			StringBuilder errorBuilder = new StringBuilder();

			errorBuilder.AppendLine((relPath ?? "?") + ": [Warning] line " + (foundLine != null ? ((int)foundLine).ToString() : "?") + ": " + title);

			if (errors != null)
				foreach (string error in errors)
					errorBuilder.AppendLine(error);

			textWriter.WriteLine(errorBuilder.ToString());

			Console.ForegroundColor = oldColor;
		}

		private static WrapperParam[] ProcessFileParsers(List<FileParser> fileParsers, out SortedDictionary<string, SortedDictionary<string, List<DataStructure>>> globalsDict)
		{
			SortedDictionary<string, WrapperParam> wrapperParamsDict = new SortedDictionary<string, WrapperParam>();

			globalsDict = new SortedDictionary<string, SortedDictionary<string, List<DataStructure>>>();

			foreach (FileParser fileParser in fileParsers)
			{
				foreach (WrapperParam wrapper in fileParser.WrapperDict.Values)
				{
					if (wrapper.SectionDict.Count < 1) // do not process empty wrappers
						continue;

					bool exists = wrapperParamsDict.TryGetValue(wrapper.WrapperName, out WrapperParam finalWrapper);

					// at first, we need to add the wrappers
					if (!exists)
						finalWrapper = wrapper;
					else
						finalWrapper.Merge(wrapper); // e.g. merge Authors

					finalWrapper.ProcessGlobals(globalsDict);

					if (!exists && wrapper.SectionDict.Count > 0) // exclude empty wrapper
						wrapperParamsDict.Add(wrapper.WrapperName, wrapper); // add this wrapper as main wrapper if not already exists
				}
			}

			// sort
			DataStructureComparator dataStructureComparator = new DataStructureComparator();

			// sort wrappers
			foreach (WrapperParam wrapper in wrapperParamsDict.Values)
			{
				foreach (SectionParam section in wrapper.SectionDict.Values)
				{
					foreach (KeyValuePair<string, List<DataStructure>> dsListEntry in section.DataStructureDict)
					{
						dsListEntry.Value.Sort(dataStructureComparator);
					}
				}
			}

			// sort globals list
			foreach (KeyValuePair<string, SortedDictionary<string, List<DataStructure>>> keyValuePair in globalsDict)
			{
				foreach (KeyValuePair<string, List<DataStructure>> dsListEntry in keyValuePair.Value)
				{
					dsListEntry.Value.Sort(dataStructureComparator);
				}
			}

			WrapperParam[] wrapperParams = new WrapperParam[wrapperParamsDict.Count];

			wrapperParamsDict.Values.CopyTo(wrapperParams, 0);

			return wrapperParams;
		}

		private static void GenerateJSONIndex(List<WrapperParam> wrapperParams, SortedDictionary<string, SortedDictionary<string, List<DataStructure>>> globalsDict)
		{
			Dictionary<string, object> jsonDict = new Dictionary<string, object>
			{
				{ "type", "overview" },
				{ "name", "CompleteOverview" }
			};

			foreach (WrapperParam wrapper in wrapperParams) // wrappers
			{
				if (!jsonDict.TryGetValue(wrapper.GetName(), out object wrapperDict))
				{
					wrapperDict = new Dictionary<string, object>();

					jsonDict.Add(wrapper.GetName(), wrapperDict);
				}

				Dictionary<string, object> wrapperData = wrapper.GetDataDict();

				((Dictionary<string, object>) wrapperDict).Add(wrapper.WrapperName, wrapperData);

				// detail.json
				string wrapperDir = NEWDIR + "/" + wrapper.GetName() + "/" + RemoveSpecialCharacters(wrapper.WrapperName);

				// section detail.json
				foreach (SectionParam section in wrapper.SectionDict.Values) // sections
				{
					Dictionary<string, object> sectionDataJson = new Dictionary<string, object>
					{
						{ "type", "overview" },
						{ "name", "SectionOverview" },
						{ "data", section.GetDataDict() }
					};

					File.WriteAllText(wrapperDir + "/" + section.SectionName + "/detail.json", JsonConvert.SerializeObject(sectionDataJson, Formatting.None, new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore
					}));
				}

				// wrapper detail.json
				Dictionary<string, object> wrapperDataJson = new Dictionary<string, object>
				{
					{ "type", "overview" },
					{ "name", "WrapperOverview" },
					{ "data", wrapperData }
				};

				File.WriteAllText(wrapperDir + "/detail.json", JsonConvert.SerializeObject(wrapperDataJson, Formatting.None, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}));
			}

			// globals
			foreach (KeyValuePair<string, SortedDictionary<string, List<DataStructure>>> entry in globalsDict) // key = type, value = Dictionary<wrapper, List<DataStructure>>
			{
				if (entry.Value.Count < 0)
					continue; // don't include empty globals

				// { WrapperParam, [{WrapperParam, [DataStructure]}] }
				Dictionary<string, List<Dictionary<string, string>>> dsDictList = new Dictionary<string, List<Dictionary<string, string>>>();

				foreach (KeyValuePair<string, List<DataStructure>> globalsEntry in entry.Value) // key = wrapper, value = List<DataStructures>
				{
					foreach (DataStructure dataStructure in globalsEntry.Value)
					{
						if (!dsDictList.TryGetValue(globalsEntry.Key, out List<Dictionary<string, string>> tmpDsDict))
						{
							tmpDsDict = new List<Dictionary<string, string>>();

							dsDictList.Add(globalsEntry.Key, tmpDsDict);
						}

						tmpDsDict.Add(new Dictionary<string, string>() {
							{ "name", dataStructure.GetDatastructureName() },
							{ "realm", dataStructure.Realm }
						});
					}
				}

				if (dsDictList.Count == 0)
					continue;

				jsonDict.Add(entry.Key, dsDictList); // type, data

				// wrapper-only
				foreach (KeyValuePair<string, List<Dictionary<string, string>>> listEntry in dsDictList) // key = wrapper, value = Dictionary<dsName, dsRealm>
				{
					Dictionary<string, object> wrapperDataJson = new Dictionary<string, object>
					{
						{ "type", "overview" },
						{ "name", listEntry.Key + "Overview" },
						{ "data", listEntry.Value }
					};

					File.WriteAllText(NEWDIR + "/" + RemoveSpecialCharacters(entry.Key) + "/" + RemoveSpecialCharacters(listEntry.Key) + "/detail.json", JsonConvert.SerializeObject(wrapperDataJson, Formatting.None, new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore
					}));
				}

				// wrappers
				Dictionary<string, object> wrappersDataJson = new Dictionary<string, object>
				{
					{ "type", "overview" },
					{ "name", entry.Key + "Overview" },
					{ "data", dsDictList }
				};

				File.WriteAllText(NEWDIR + "/" + RemoveSpecialCharacters(entry.Key) + "/detail.json", JsonConvert.SerializeObject(wrappersDataJson, Formatting.None, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}));
			}

			File.WriteAllText(NEWDIR + "/overview.json", JsonConvert.SerializeObject(jsonDict, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			}));
		}

		private static void GenerateJSONSearch(List<WrapperParam> wrapperParams, SortedDictionary<string, SortedDictionary<string, List<DataStructure>>> globalsDict)
		{
			// transform into structure "WRAPPER_TYPE[] -> WRAPPER -> DATASTRUCTURES[]"
			Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> wrapperTypesDict = new Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>();

			foreach (WrapperParam wrapper in wrapperParams) // wrappers
			{
				if (!wrapperTypesDict.TryGetValue(wrapper.GetName(), out Dictionary<string, List<Dictionary<string, string>>> wrappersDict))
				{
					wrappersDict = new Dictionary<string, List<Dictionary<string, string>>>();

					wrapperTypesDict.Add(wrapper.GetName(), wrappersDict);
				}

				List<Dictionary<string, string>> dsDict = new List<Dictionary<string, string>>();

				wrappersDict.Add(wrapper.WrapperName, dsDict);

				foreach (SectionParam section in wrapper.SectionDict.Values) // sections
				{
					foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in section.DataStructureDict) // ds types
					{
						foreach (DataStructure ds in keyValuePair.Value) // ds
						{
							dsDict.Add(new Dictionary<string, string>()
							{
								{ "name", ds.GetDatastructureName() },
								{ "realm", ds.Realm },
								{ "type", ds.GetName() }
							});
						}
					}
				}
			}

			Dictionary<string, object> globalsShortDict = new Dictionary<string, object>();

			foreach (KeyValuePair<string, SortedDictionary<string, List<DataStructure>>> entry in globalsDict)
			{
				if (entry.Value.Count < 0)
					continue; // don't include empty globals

				Dictionary<string, List<Dictionary<string, string>>> dsList = new Dictionary<string, List<Dictionary<string, string>>>();

				foreach (KeyValuePair<string, List<DataStructure>> globalsEntry in entry.Value)
				{
					foreach (DataStructure dataStructure in globalsEntry.Value)
					{
						if (!dsList.TryGetValue(globalsEntry.Key, out List<Dictionary<string, string>> tmpDsDict))
						{
							tmpDsDict = new List<Dictionary<string, string>>();

							dsList.Add(globalsEntry.Key, tmpDsDict);
						}

						tmpDsDict.Add(new Dictionary<string, string>() {
							{ "name", dataStructure.GetDatastructureName() },
							{ "realm", dataStructure.Realm }
						});
					}
				}

				if (dsList.Count > 0)
					globalsShortDict.Add(entry.Key, dsList);
			}

			Dictionary<string, object> tmpDict = new Dictionary<string, object>
			{
				{ "type", "search" },
				{ "name", "Search" }
			};

			foreach (KeyValuePair<string, Dictionary<string, List<Dictionary<string, string>>>> entry in wrapperTypesDict)
			{
				tmpDict.Add(entry.Key, entry.Value);
			}

			foreach (KeyValuePair<string, object> entry in globalsShortDict)
			{
				tmpDict.Add(entry.Key, entry.Value);
			}

			File.WriteAllText(NEWDIR + "/search.json", JsonConvert.SerializeObject(tmpDict, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			}));
		}

		private static void GenerateDocumentationData(List<WrapperParam> wrapperList, SortedDictionary<string, SortedDictionary<string, List<DataStructure>>> globalsDict)
		{
			foreach (WrapperParam wrapper in wrapperList)
			{
				string wrapperTypDir = NEWDIR + "/" + wrapper.GetName();

				if (!Directory.Exists(wrapperTypDir))
					Directory.CreateDirectory(wrapperTypDir);

				string wrapperDir = wrapperTypDir + "/" + RemoveSpecialCharacters(wrapper.WrapperName);

				Directory.CreateDirectory(wrapperDir);

				foreach (SectionParam section in wrapper.SectionDict.Values)
				{
					string sectionDir = wrapperDir + "/" + section.SectionName;

					Directory.CreateDirectory(sectionDir);

					foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in section.DataStructureDict)
					{
						foreach (DataStructure dataStructure in keyValuePair.Value)
						{
							string fileDir = sectionDir + "/" + dataStructure.Realm;

							if (!Directory.Exists(fileDir))
								Directory.CreateDirectory(fileDir);

							File.WriteAllText(fileDir + "/" + RemoveSpecialCharacters(dataStructure.GetDatastructureName()) + ".json", dataStructure.GetFullJSONData());
						}
					}
				}
			}

			// add globals too
			foreach (KeyValuePair<string, SortedDictionary<string, List<DataStructure>>> entry in globalsDict)
			{
				if (entry.Value.Count < 0)
					continue; // don't include empty globals

				string globalsPath = NEWDIR + "/" + entry.Key;

				Directory.CreateDirectory(globalsPath);

				foreach (KeyValuePair<string, List<DataStructure>> globalsEntry in entry.Value)
				{
					if (globalsEntry.Value.Count < 1)
						continue;

					string globalTypePath = globalsPath + "/" + globalsEntry.Key;

					Directory.CreateDirectory(globalTypePath);

					foreach (DataStructure dataStructure in globalsEntry.Value)
					{
						string globalTypeDsPath = globalTypePath + "/" + dataStructure.Realm;

						if (!Directory.Exists(globalTypeDsPath))
							Directory.CreateDirectory(globalTypeDsPath);

						File.WriteAllText(globalTypeDsPath + "/" + RemoveSpecialCharacters(dataStructure.GetDatastructureName()) + ".json", dataStructure.GetFullJSONData());
					}
				}
			}
		}

		public static string RemoveSpecialCharacters(string str)
		{
			StringBuilder sb = new StringBuilder();

			foreach (char c in str)
			{
				if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' || c == '.')
					sb.Append(c);
				else if (c == ':')
					sb.Append("..");
			}

			return sb.ToString();
		}

		public static List<string> GetEntriesFromString(string str, out int lastPos, int deepness = 0, int list = 0) // list = which list (index) of found lists
		{
			int bracketsDeepness = 0;
			bool string1Active = false;
			bool string2Active = false;
			string tmpString = "";
			int currentList = 0;

			List<string> entriesList = new List<string>();

			for (lastPos = 0; lastPos < str.Length; lastPos++)
			{
				char c = str[lastPos];

				switch (c)
				{
					case '(':
					case '{':
					case '[':
						if (bracketsDeepness > deepness) // if we are in a scope
							tmpString += c;

						if (!string1Active && !string2Active)
							bracketsDeepness++;

						break;
					case ')':
					case '}':
					case ']':
						if (!string1Active && !string2Active)
						{
							if (bracketsDeepness == deepness + 1)
							{
								if (!string.IsNullOrEmpty(tmpString) && currentList == list) // insert before starting a new list
									entriesList.Add(tmpString.Trim());

								tmpString = "";

								currentList++; // increase current list count because the previous list has ended
							}

							bracketsDeepness--;
						}

						if (bracketsDeepness > deepness) // if we are in a scope
							tmpString += c;

						break;
					case '"':
						string1Active = !string1Active;

						if (bracketsDeepness > deepness) // if we are in a scope
							tmpString += c;

						break;
					case '\'':
						string2Active = !string2Active;

						if (bracketsDeepness > deepness) // if we are in a scope
							tmpString += c;

						break;
					case ',':
						if (bracketsDeepness == deepness + 1 && !string1Active && !string2Active) // if we are directly in the list and not in a string
						{
							if (!string.IsNullOrEmpty(tmpString) && currentList == list)
								entriesList.Add(tmpString.Trim());

							tmpString = "";
						}
						else
						{
							tmpString += c;
						}

						break;
					default:
						if (bracketsDeepness > deepness) // if we are in a scope
							tmpString += c;

						break;
				}

				if (bracketsDeepness < deepness || currentList > list) // don't continue, even if the line hasn't ended yet
					break;
			}

			return entriesList;
		}
	}
}

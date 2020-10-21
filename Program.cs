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
 * 
- detail.json -> for Wrapper and Sections: class/Armor/detail.json and class/ARMOR/none/detail.json
  - detail.json: DataStructures, Descs, Authors
 * param settings Color(255, 255, 255, 255) fix
 * sort datastructures in lists
 * 
 * TODO
 * Enums, global table, roleData etc. fetch (class attributes)
 */

namespace NeoDoc
{
	internal static class NeoDoc
	{
		public const bool DEBUGGING = false;
		public static int Progress = 0;

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

			List<WrapperParam> wrapperList = new List<WrapperParam>(ProcessFileParsers(fileParsers, out SortedDictionary<string, Dictionary<string, List<DataStructure>>> globalsDict));

			// Generate Folders
			string newDir = Directory.GetCurrentDirectory() + "../../../output";

			if (Directory.Exists(newDir))
				Directory.Delete(newDir, true);

			Directory.CreateDirectory(newDir);

			// Write overview JSON
			File.WriteAllText(newDir + "/overview.json", GenerateJSONIndex(wrapperList, globalsDict));

			// Write search JSON
			File.WriteAllText(newDir + "/search.json", GenerateJSONSearch(wrapperList, globalsDict));

			GenerateDocumentationData(wrapperList, globalsDict);

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

			StringBuilder errorBuilder = new StringBuilder((relPath ?? "?") + ": [Warning] line " + (foundLine != null ? ((int)foundLine).ToString() : "?") + ": " + title);

			if (errors != null)
				foreach (string error in errors)
					errorBuilder.AppendLine(error);

			textWriter.WriteLine(errorBuilder.ToString());

			Console.ForegroundColor = oldColor;
		}

		private static WrapperParam[] ProcessFileParsers(List<FileParser> fileParsers, out SortedDictionary<string, Dictionary<string, List<DataStructure>>> globalsDict)
		{
			SortedDictionary<string, WrapperParam> wrapperParamsDict = new SortedDictionary<string, WrapperParam>();

			globalsDict = new SortedDictionary<string, Dictionary<string, List<DataStructure>>>();

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

			/* sort globals list
			DataStructureComparator dataStructureComparator = new DataStructureComparator();

			foreach (KeyValuePair<string, Dictionary<string, List<DataStructure>>> keyValuePair in globalsDict)
			{
				keyValuePair.Value.Sort(dataStructureComparator);
			}
			*/

			WrapperParam[] wrapperParams = new WrapperParam[wrapperParamsDict.Count];

			wrapperParamsDict.Values.CopyTo(wrapperParams, 0);

			return wrapperParams;
		}

		private static string GenerateJSONIndex(List<WrapperParam> wrapperParams, SortedDictionary<string, Dictionary<string, List<DataStructure>>> globalsDict)
		{
			Dictionary<string, object> jsonDict = new Dictionary<string, object>
			{
				{ "type", "overview" },
				{ "name", "Overview" }
			};

			foreach (WrapperParam wrapper in wrapperParams)
			{
				if (!jsonDict.TryGetValue(wrapper.GetName(), out object wrapperDict))
				{
					wrapperDict = new Dictionary<string, object>();

					jsonDict.Add(wrapper.GetName(), wrapperDict);
				}

				((Dictionary<string, object>) wrapperDict).Add(wrapper.WrapperName, wrapper.GetDataDict());
			}

			foreach (KeyValuePair<string, Dictionary<string, List<DataStructure>>> entry in globalsDict)
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
					jsonDict.Add(entry.Key, dsList);
			}

			return JsonConvert.SerializeObject(jsonDict, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});
		}

		private static string GenerateJSONSearch(List<WrapperParam> wrapperParams, SortedDictionary<string, Dictionary<string, List<DataStructure>>> globalsDict)
		{
			// transform into structure "WRAPPER_TYPE[] -> WRAPPER -> DATASTRUCTURES[]"
			Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> wrapperTypesDict = new Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>>();

			foreach (WrapperParam wrapper in wrapperParams)
			{
				if (!wrapperTypesDict.TryGetValue(wrapper.GetName(), out Dictionary<string, List<Dictionary<string, string>>> wrappersDict))
				{
					wrappersDict = new Dictionary<string, List<Dictionary<string, string>>>();

					wrapperTypesDict.Add(wrapper.GetName(), wrappersDict);
				}

				List<Dictionary<string, string>> dsDict = new List<Dictionary<string, string>>();

				wrappersDict.Add(wrapper.WrapperName, dsDict);

				foreach (SectionParam section in wrapper.SectionDict.Values)
				{
					foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in section.DataStructureDict)
					{
						foreach (DataStructure ds in keyValuePair.Value)
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

			foreach (KeyValuePair<string, Dictionary<string, List<DataStructure>>> entry in globalsDict)
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

			return JsonConvert.SerializeObject(tmpDict, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});
		}

		private static void GenerateDocumentationData(List<WrapperParam> wrapperList, SortedDictionary<string, Dictionary<string, List<DataStructure>>> globalsDict)
		{
			string newDir = Directory.GetCurrentDirectory() + "../../../output";

			foreach (WrapperParam wrapper in wrapperList)
			{
				string wrapperTypDir = newDir + "/" + wrapper.GetName();

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
			foreach (KeyValuePair<string, Dictionary<string, List<DataStructure>>> entry in globalsDict)
			{
				if (entry.Value.Count < 0)
					continue; // don't include empty globals

				string globalsPath = newDir + "/" + entry.Key;

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
	}
}

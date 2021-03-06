﻿using NeoDoc.DataStructures;
using NeoDoc.Langs;
using NeoDoc.Params;
using Newtonsoft.Json;
using CommandLine;
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
		public static bool DEBUGGING;
		public static int Progress;
		public static string NEWDIR = "";
        public static string ErrorLogFormat = "auto";

		public enum ERROR_CODES: int
		{
			BAD_ARGUMENTS = 2,
			NOT_EXISTS = 3,
			MISSING_ESSENTIAL_PARAMS = 4,
			UNREGISTERED_PARAM = 5,
			PARAM_MISMATCH = 6,
			MERGING_ISSUE = 7,
			INVALID_PARAM_ARGS_FORMAT = 8,
			NO_SETTINGS_PARAM = 9,
			MULTIPLE_DS_IN_LINE = 10
		}

		public static void Empty(this DirectoryInfo directory)
		{
			foreach(FileInfo file in directory.GetFiles())
				file.Delete();

			foreach(DirectoryInfo subDirectory in directory.GetDirectories())
				subDirectory.Delete(true);
		}

        public class Options
        {
            [Option('f', "format", Default = "auto", Required = false, HelpText = "Choose a log message format (auto, standard, github).")]
            public string LogFormat { get; set; }

            [Option('v', "verbose", Default = false, Required = false, HelpText = "Should the output be more verbose and contain debugging logs?")]
            public bool Verbose { get; set; }

            [Value(0, Required = true, HelpText = "The source folder to check and read files from.")]
            public string Folder { get; set; }

            [Value(1, HelpText = "Set this to a path and you will generate json output (usually used by external applications such as NeoVis).")]
            public string OutputFolder { get; set; }
        }

        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    Run,
                    error => (int) ERROR_CODES.BAD_ARGUMENTS
                );
        }

        public static int Run(Options options) {
            NEWDIR = options.OutputFolder;
            ErrorLogFormat = options.LogFormat;
            DEBUGGING = options.Verbose;

            if (!Directory.Exists(options.Folder))
            {
                Console.Error.WriteLine("Provided folder '" + options.Folder + "' does not exists!");

                return (int) ERROR_CODES.NOT_EXISTS;
            }

            // Build the file tree
            string[] files = Directory.GetFiles(options.Folder, "*.*", SearchOption.AllDirectories);

			// Prepare the files
			List<FileParser> fileParsers = new List<FileParser>();

			int amount = files.Length;

			LangMatcher langMatcher = new LangMatcher(); // lang processing system

			for (int n = 0; n < amount; n++)
			{
				string file = files[n];

				string relPath = file.Remove(0, options.Folder.Length);
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

			if (string.IsNullOrEmpty(NEWDIR))
			{
				WriteDebugInfo("Finished checking the documentation.");

				return Environment.ExitCode;
			}

			// Generate Folders
			DirectoryInfo outputFolderInfo = new DirectoryInfo(NEWDIR);

			if (outputFolderInfo.Exists)
				outputFolderInfo.Empty();
			else
				Directory.CreateDirectory(NEWDIR);

			// Write single files
			GenerateDocumentationData(wrapperList, globalsDict);

			// Write overviews JSON
			GenerateJSONIndex(wrapperList, globalsDict);

			// Write search JSON
			GenerateJSONSearch(wrapperList, globalsDict);

			WriteDebugInfo("Finished generating the documentation.");

			return Environment.ExitCode;
		}

		internal static void WriteDebugInfo(string debugInfo)
		{
			if (!DEBUGGING)
				return;

			ConsoleColor oldColor = Console.ForegroundColor;

			Console.ForegroundColor = ConsoleColor.Yellow;

			Console.Out.WriteLine(debugInfo);

			Console.ForegroundColor = oldColor;
		}

		internal static void WriteErrors(string message, List<string> errors, string relPath, int? foundLine, int? exitCode)
		{
			if (exitCode != null)
				Environment.ExitCode = (int)exitCode;

			ConsoleColor oldColor = Console.ForegroundColor;

			Console.ForegroundColor = ConsoleColor.Red;

			TextWriter textWriter = Console.Error;

			textWriter.WriteLine(formatLogMessage(message, errors, relPath, foundLine, exitCode));

			Console.ForegroundColor = oldColor;
		}

		private static string formatLogMessage(string message, List<string> errors, string relPath, int? foundLine, int? exitCode) {
			switch (ErrorLogFormat) {
				case "auto":
					if (System.Environment.GetEnvironmentVariable("GITHUB_ACTIONS") != null && System.Environment.GetEnvironmentVariable("GITHUB_WORKFLOW") != null) {
						return formatGithub(message, errors, relPath, foundLine, exitCode);
					}
					return formatStandard(message, errors, relPath, foundLine, exitCode);
				case "standard":
					return formatStandard(message, errors, relPath, foundLine, exitCode);
				case "github":
					return formatGithub(message, errors, relPath, foundLine, exitCode);
				default:
					Console.Error.WriteLine("Could not understand the given format param, falling back to mode: standard");
					return formatStandard(message, errors, relPath, foundLine, exitCode);
			}
		}

		private static string formatStandard(string message, List<string> errors, string relPath, int? foundLine, int? exitCode) {
			StringBuilder errorBuilder = new StringBuilder();

			errorBuilder.AppendLine("Error " + (exitCode != null ? ((int)exitCode).ToString() : "???") + ": " + (relPath ?? "?") + ": [Warning] line " + (foundLine != null ? ((int)foundLine).ToString() : "?") + ": " + message);

			if (errors != null)
				foreach (string error in errors)
					errorBuilder.AppendLine(error);
			
			return errorBuilder.ToString();
		}

		private static string formatGithub(string message, List<string> errors, string relPath, int? foundLine, int? exitCode) {
			string output = "::error ";
			
			output += "file=" + (relPath ?? "?") + ",";
			output += "line=" + (foundLine != null ? ((int)foundLine).ToString() : "?");
			output += "::";
			output += message;

			if (errors != null)
				output += " -->";
				foreach (string error in errors)
					output += " \"" + error + "\"";
			
			// Also output human readable format for the log files
			return output + "\n" + formatStandard(message, errors, relPath, foundLine, exitCode);
		}

		private static WrapperParam[] ProcessFileParsers(List<FileParser> fileParsers, out SortedDictionary<string, SortedDictionary<string, List<DataStructure>>> globalsDict)
		{
			SortedDictionary<string, WrapperParam> wrapperParamsDict = new SortedDictionary<string, WrapperParam>();

			globalsDict = new SortedDictionary<string, SortedDictionary<string, List<DataStructure>>>();

			foreach (FileParser fileParser in fileParsers)
			{
				foreach (WrapperParam wrapper in fileParser.WrapperDict.Values)
				{
					if (wrapper.SectionDict.Count == 0) // do not process empty wrappers
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
				string wrapperDir = NEWDIR + "/" + wrapper.GetName().ToLower() + "/" + RemoveSpecialCharacters(wrapper.WrapperName).ToLower();

				// section detail.json
				foreach (SectionParam section in wrapper.SectionDict.Values) // sections
				{
					Dictionary<string, object> sectionDataJson = new Dictionary<string, object>
					{
						{ "type", "overview" },
						{ "name", "SectionOverview" },
						{ "data", section.GetDataDict() }
					};
					
					File.WriteAllText(wrapperDir + "/" + RemoveSpecialCharacters(section.SectionName).ToLower() + "/detail.json", JsonConvert.SerializeObject(sectionDataJson, Formatting.None, new JsonSerializerSettings
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

					File.WriteAllText(NEWDIR + "/" + RemoveSpecialCharacters(entry.Key).ToLower() + "/" + RemoveSpecialCharacters(listEntry.Key).ToLower() + "/detail.json", JsonConvert.SerializeObject(wrapperDataJson, Formatting.None, new JsonSerializerSettings
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

				File.WriteAllText(NEWDIR + "/" + RemoveSpecialCharacters(entry.Key).ToLower() + "/detail.json", JsonConvert.SerializeObject(wrappersDataJson, Formatting.None, new JsonSerializerSettings
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
				string wrapperTypDir = NEWDIR + "/" + wrapper.GetName().ToLower();

				if (!Directory.Exists(wrapperTypDir))
					Directory.CreateDirectory(wrapperTypDir);

				string wrapperDir = wrapperTypDir + "/" + RemoveSpecialCharacters(wrapper.WrapperName).ToLower();

				Directory.CreateDirectory(wrapperDir);

				foreach (SectionParam section in wrapper.SectionDict.Values)
				{
					string sectionDir = wrapperDir + "/" + RemoveSpecialCharacters(section.SectionName).ToLower();

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

				string globalsPath = NEWDIR + "/" + RemoveSpecialCharacters(entry.Key).ToLower();

				Directory.CreateDirectory(globalsPath);

				foreach (KeyValuePair<string, List<DataStructure>> globalsEntry in entry.Value)
				{
					if (globalsEntry.Value.Count < 1)
						continue;

					string globalTypePath = globalsPath + "/" + RemoveSpecialCharacters(globalsEntry.Key).ToLower();

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

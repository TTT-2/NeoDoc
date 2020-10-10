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
 * FIX multiple hooks (multiple hook.Run / hook.Call of the same hook)
 * FIX multiple functions (server and client realm created (same) functions)
 * 
 * TODO inside of the TTT2 documentation (not in this project)
 *
 * Remove @register
 * Rework documentation: put @module and @function right below the previous param descriptions
 * Improve @function calls, e.g. add PANEL in front of function
 * Add @module in documentation on your own on top of a module("...", ...) call or a "ITEM = {}" declaration
 * Cleanup wrong parameters, e.g. "deprecTated"
 * put [opt] and [default] directly after the param (not the type)
 */

namespace NeoDoc
{
    internal static class NeoDoc
	{
		private static void Main(string[] args)
		{
			if (args.Length < 1)
				return;

		    string folder = args[0];

            if (string.IsNullOrEmpty(folder))
                return;

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

				Console.WriteLine("[" + (int)Math.Floor((n + 1) / (double)amount * 100.0) + "%] '" + relPath + "'");

                // get the lang based on the file extension
				Lang lang = langMatcher.GetByFileExtension(Path.GetExtension(file));

				if (lang == null)
					continue;

				Console.WriteLine("Running '" + lang.GetName() + "' parser");

				FileParser fileParser = new FileParser(langMatcher, lang, file); // fileParser is used to process a file
				fileParser.CleanUp();
				fileParser.Process();

				fileParsers.Add(fileParser);

				Console.WriteLine("Finished parsing");
				Console.WriteLine("");
			}

            List<WrapperParam> wrapperList = new List<WrapperParam>(ProcessFileParsers(fileParsers, out SortedDictionary<string, List<DataStructure>> globalsDict));

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

            Console.WriteLine("Finished generating documentation.");
        }

        internal static void WriteErrors(List<string> errors)
        {
            ConsoleColor oldColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;

            foreach (string error in errors)
                Console.WriteLine(error);

            Console.ForegroundColor = oldColor;
        }

        private static WrapperParam[] ProcessFileParsers(List<FileParser> fileParsers, out SortedDictionary<string, List<DataStructure>> globalsDict)
        {
            SortedDictionary<string, WrapperParam> wrapperParamsDict = new SortedDictionary<string, WrapperParam>();

            globalsDict = new SortedDictionary<string, List<DataStructure>>();

            foreach (FileParser fileParser in fileParsers)
            {
                foreach (WrapperParam wrapper in fileParser.WrapperDict.Values)
                {
                    // at first, we need to add the wrappers
                    bool wrapperExists = wrapperParamsDict.TryGetValue(wrapper.WrapperName, out WrapperParam finalWrapper);

                    if (!wrapperExists)
                    {
                        // create a new wrapper of the same type
                        WrapperParam tmpWrapper = (WrapperParam)Activator.CreateInstance(wrapper.GetType());
                        tmpWrapper.WrapperName = wrapper.WrapperName;

                        wrapperParamsDict.Add(tmpWrapper.WrapperName, tmpWrapper); // add this wrapper as main wrapper if not already exists

                        finalWrapper = tmpWrapper;
                    }

                    finalWrapper.Merge(wrapper); // e.g. merge Authors
                    finalWrapper.ProcessGlobals(globalsDict);
                }
            }

            // sort globals list
            DataStructureComparator dataStructureComparator = new DataStructureComparator();

            foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in globalsDict)
            {
                keyValuePair.Value.Sort(dataStructureComparator);
            }

            WrapperParam[] wrapperParams = new WrapperParam[wrapperParamsDict.Count];

            wrapperParamsDict.Values.CopyTo(wrapperParams, 0);

            return wrapperParams;
        }

        private static string GenerateJSONIndex(List<WrapperParam> wrapperParams, SortedDictionary<string, List<DataStructure>> globalsDict)
        {
            Dictionary<string, object> jsonDict = new Dictionary<string, object>
            {
                { "type", "overview" },
                { "name", "Overview" }
            };

            foreach (WrapperParam wrapper in wrapperParams)
            {
                bool exists = jsonDict.TryGetValue(wrapper.GetName(), out object wrapperDict);

                if (!exists)
                {
                    wrapperDict = new Dictionary<string, object>();

                    jsonDict.Add(wrapper.GetName(), wrapperDict);
                }

                ((Dictionary<string, object>) wrapperDict).Add(wrapper.WrapperName, wrapper.GetDataDict());
            }

            Dictionary<string, object> globalsShortDict = new Dictionary<string, object>();

            foreach (KeyValuePair<string, List<DataStructure>> entry in globalsDict)
            {
                if (entry.Value.Count < 0)
                    continue; // don't include empty globals

                List<string> dsList = new List<string>();

                foreach (DataStructure dataStructure in entry.Value)
                {
                    if (dataStructure.Ignore)
                        continue;

                    dsList.Add(dataStructure.GetDatastructureName());
                }

                if (dsList.Count > 0)
                    globalsShortDict.Add(entry.Key, dsList);
            }

            foreach (KeyValuePair<string, object> entry in globalsShortDict)
            {
                jsonDict.Add(entry.Key, entry.Value);
            }

            return JsonConvert.SerializeObject(jsonDict, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static string GenerateJSONSearch(List<WrapperParam> wrapperParams, SortedDictionary<string, List<DataStructure>> globalsDict)
        {
            // transform into structure "WRAPPER -> TYPE[] -> DATASTRUCTURES[]"
            Dictionary<string, Dictionary<string, List<string>>> wrapperDict = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (WrapperParam wrapper in wrapperParams)
            {
                Dictionary<string, List<string>> typesDict = new Dictionary<string, List<string>>();

                wrapperDict.Add(wrapper.WrapperName, typesDict);

                foreach (SectionParam section in wrapper.SectionDict.Values)
                {
                    foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in section.DataStructureDict)
                    {
                        bool exists = typesDict.TryGetValue(keyValuePair.Key, out List<string> foundList);

                        if (!exists)
                        {
                            foundList = new List<string>();

                            typesDict.Add(keyValuePair.Key, foundList);
                        }

                        foreach (DataStructure ds in keyValuePair.Value)
                        {
                            if (ds.Ignore)
                                continue;

                            foundList.Add(ds.GetDatastructureName());
                        }
                    }
                }
            }

            Dictionary<string, object> globalsShortDict = new Dictionary<string, object>();

            foreach (KeyValuePair<string, List<DataStructure>> entry in globalsDict)
            {
                if (entry.Value.Count < 0)
                    continue; // don't include empty globals

                List<string> dsList = new List<string>();

                foreach (DataStructure dataStructure in entry.Value)
                {
                    if (dataStructure.Ignore)
                        continue;

                    dsList.Add(dataStructure.GetDatastructureName());
                }

                if (dsList.Count > 0)
                    globalsShortDict.Add(entry.Key, dsList);
            }

            Dictionary<string, object> tmpDict = new Dictionary<string, object>
            {
                { "type", "overview" },
                { "name", "Search" },
                { "data", wrapperDict }
            };

            foreach (KeyValuePair<string, object> entry in globalsShortDict)
            {
                tmpDict.Add(entry.Key, entry.Value);
            }

            return JsonConvert.SerializeObject(tmpDict, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static void GenerateDocumentationData(List<WrapperParam> wrapperList, SortedDictionary<string, List<DataStructure>> globalsDict)
        {
            string newDir = Directory.GetCurrentDirectory() + "../../../output";

            foreach (WrapperParam wrapper in wrapperList)
            {
                string wrapperDir = newDir + "/" + RemoveSpecialCharacters(wrapper.WrapperName);

                Directory.CreateDirectory(wrapperDir);

                foreach (SectionParam section in wrapper.SectionDict.Values)
                {
                    foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in section.DataStructureDict)
                    {
                        foreach (DataStructure dataStructure in keyValuePair.Value)
                        {
                            if (dataStructure.Ignore)
                                continue;

                            File.WriteAllText(wrapperDir + "/" + RemoveSpecialCharacters(dataStructure.GetDatastructureName()) + ".json", dataStructure.GetFullJSONData());
                        }
                    }
                }
            }

            // add globals too
            string globalDir = newDir + "/_globals";

            Directory.CreateDirectory(globalDir);

            foreach (KeyValuePair<string, List<DataStructure>> entry in globalsDict)
            {
                foreach (DataStructure dataStructure in entry.Value)
                {
                    if (dataStructure.Ignore)
                        continue;

                    File.WriteAllText(globalDir + "/" + RemoveSpecialCharacters(dataStructure.GetDatastructureName()) + ".json", dataStructure.GetFullJSONData());
                }
            }
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("_");
                }
            }

            return sb.ToString();
        }
    }
}

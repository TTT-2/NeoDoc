using NeoDoc.DataStructures;
using NeoDoc.Langs;
using NeoDoc.Params;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/* 
 * TODO inside of the TTT2 documentation (not in this project)
 *
 * Remove @register
 * Rework documentation: put @module and @function right below the previous param descriptions
 * Improve @function calls, e.g. add PANEL in front of function
 * Add @module in documentation on your own on top of a module("...", ...) call or a "ITEM = {}" declaration
 * Cleanup wrong parameters, e.g. "deprecTated"
 *
 * TODO for the doc generation part
 * (convert param[…] and return[default=…] into the doc pages too)
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

            List<WrapperParam> wrapperList = new List<WrapperParam>(ProcessFileParsers(fileParsers, out Dictionary<string, List<DataStructure>> globalsDict));

            CleanupEmptyDataKeeper(wrapperList); // remove empty sections and wrappers that were created e.g. because of hooks, ConVars or local functions

            // Generate Folders
            string newDir = Directory.GetCurrentDirectory() + "../../../output";

            if (Directory.Exists(newDir))
                Directory.Delete(newDir, true);

            Directory.CreateDirectory(newDir);

            string jsonString = GenerateJSONSearchIndex(wrapperList, globalsDict);

            // Write JSON
            File.WriteAllText(newDir + "/jsonList.json", jsonString);

            GenerateDocumentationData(wrapperList, globalsDict);
        }

        private static void CleanupEmptyDataKeeper(List<WrapperParam> wrapperParams)
        {
            for (int i2 = 0; i2 < wrapperParams.Count; i2++)
            {
                WrapperParam wrapper = wrapperParams[i2];

                for (int i3 = 0; i3 < wrapper.SectionList.Count; i3++)
                {
                    SectionParam section = wrapper.SectionList[i3];

                    if (section.DataStructureList.Count < 1)
                    {
                        wrapper.SectionList.Remove(section);

                        i3 -= 1;

                        if (i3 >= wrapper.SectionList.Count)
                            break;
                    }
                }

                if (wrapper.SectionList.Count < 1)
                {
                    wrapperParams.Remove(wrapper);

                    i2 -= 1;

                    if (i2 >= wrapperParams.Count)
                        break;
                }
            }
        }

        private static WrapperParam[] ProcessFileParsers(List<FileParser> fileParsers, out Dictionary<string, List<DataStructure>> globalsDict)
        {
            Dictionary<string, WrapperParam> wrapperParamsDict = new Dictionary<string, WrapperParam>();

            globalsDict = new Dictionary<string, List<DataStructure>>();

            foreach (FileParser fileParser in fileParsers)
            {
                foreach (WrapperParam wrapper in fileParser.WrapperList)
                {
                    // at first, we need to add the wrappers
                    bool wrapperExists = wrapperParamsDict.TryGetValue(wrapper.GetData(), out WrapperParam finalWrapper);

                    if (!wrapperExists)
                    {
                        // create a new wrapper of the same type
                        WrapperParam tmpWrapper = (WrapperParam)Activator.CreateInstance(wrapper.GetType());
                        tmpWrapper.WrapperName = wrapper.WrapperName;

                        wrapperParamsDict.Add(tmpWrapper.WrapperName, tmpWrapper); // add this wrapper as main wrapper if not already exists

                        finalWrapper = tmpWrapper;
                    }

                    finalWrapper.MergeData(wrapper); // e.g. merge Authors

                    // now we need to search for any section and add it into the wrapper AS WELL AS merging same sections of same wrappers together
                    foreach (SectionParam section in wrapper.SectionList)
                    {
                        // section already exists?
                        SectionParam finalSection = null;

                        foreach (SectionParam tmpSection in finalWrapper.SectionList)
                        {
                            if (tmpSection.GetData() == section.GetData())
                            {
                                finalSection = tmpSection;

                                break;
                            }
                        }

                        bool sectionExists = finalSection != null;

                        if (!sectionExists)
                        {
                            // create a new section of the same type
                            SectionParam tmpSection = (SectionParam)Activator.CreateInstance(section.GetType());
                            tmpSection.SectionName = section.SectionName;

                            finalWrapper.SectionList.Add(tmpSection); // add this section as new section into the wrappers section list

                            finalSection = tmpSection;
                        }

                        foreach (DataStructure dataStructure in section.DataStructureList)
                        {
                            if (!dataStructure.IsGlobal())
                            {
                                // just insert if not already exists
                                bool alreadyExists = false;

                                foreach (DataStructure tmpDs in finalSection.DataStructureList)
                                {
                                    if (tmpDs.GetJSONData() == dataStructure.GetJSONData())
                                    {
                                        alreadyExists = true;

                                        break;
                                    }
                                }

                                if (!alreadyExists)
                                    finalSection.DataStructureList.Add(dataStructure);
                            }
                            else
                            {
                                bool exists = globalsDict.TryGetValue(dataStructure.GetName(), out List<DataStructure> ds);

                                if (!exists)
                                {
                                    ds = new List<DataStructure>();

                                    globalsDict.Add(dataStructure.GetName(), ds);
                                }

                                // just insert if not already exists
                                bool alreadyExists = false;

                                foreach (DataStructure tmpDs in ds)
                                {
                                    if (tmpDs.GetJSONData() == dataStructure.GetJSONData())
                                    {
                                        alreadyExists = true;

                                        break;
                                    }
                                }

                                if (!alreadyExists)
                                    ds.Add(dataStructure);
                            }
                        }
                    }
                }
            }

            WrapperParam[] wrapperParams = new WrapperParam[wrapperParamsDict.Count];

            wrapperParamsDict.Values.CopyTo(wrapperParams, 0);

            return wrapperParams;
        }

        private static string GenerateJSONSearchIndex(List<WrapperParam> wrapperParams, Dictionary<string, List<DataStructure>> globalsDict)
        {
            string json = "{";

            foreach (WrapperParam wrapper in wrapperParams)
            {
                json += wrapper.GetJSONData() + ",";
            }

            // add globals too
            foreach (KeyValuePair<string, List<DataStructure>> entry in globalsDict)
            {
                if (entry.Value.Count < 0)
                    continue; // don't include empty globals

                json += "\"" + entry.Key + "s\":[";

                foreach (DataStructure dataStructure in entry.Value)
                {
                    json += dataStructure.GetJSONData() + ",";
                }

                json = json.Remove(json.Length - 1, 1) + "],"; // remove last "," and close data structure
            }

            return json.Remove(json.Length - 1, 1) + "}"; // remove last "," and close json
        }

        private static void GenerateDocumentationData(List<WrapperParam> wrapperList, Dictionary<string, List<DataStructure>> globalsDict)
        {
            string newDir = Directory.GetCurrentDirectory() + "../../../output";

            foreach (WrapperParam wrapper in wrapperList)
            {
                string wrapperDir = newDir + "/" + wrapper.GetData();

                Directory.CreateDirectory(wrapperDir);

                foreach (SectionParam section in wrapper.SectionList)
                {
                    foreach (DataStructure dataStructure in section.DataStructureList)
                    {
                        File.WriteAllText(wrapperDir + "/" + RemoveSpecialCharacters(dataStructure.GetData()) + ".html", dataStructure.GetHTML());
                    }
                }
            }

            // add globals too
            foreach (KeyValuePair<string, List<DataStructure>> entry in globalsDict)
            {
                if (entry.Value.Count < 0)
                    continue; // don't include empty globals

                string globalDir = newDir + "/" + entry.Key + "s";

                Directory.CreateDirectory(globalDir);

                foreach (DataStructure dataStructure in entry.Value)
                {
                    File.WriteAllText(globalDir + "/" + RemoveSpecialCharacters(dataStructure.GetData()) + ".html", dataStructure.GetHTML());
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
                    sb.Append("__");
                }
            }
            return sb.ToString();
        }
    }
}

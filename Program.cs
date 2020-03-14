﻿using NeoDoc.DataStructures;
using NeoDoc.Langs;
using NeoDoc.Params;
using System;
using System.Collections.Generic;
using System.IO;

/*
 * TODO
 *
 * @author
 * @register
 *
 * Rework documentation: put @module and @function right below the previous param descriptions
 * Improve @function calls, e.g. add PANEL in front of function
 * Add @module in documentation on your own on top of a module("...", ...) call or a "ITEM = {}" declaration
 * Cleanup wrong parameters, e.g. "deprecTated"
 * (convert param[…] and return[default=…] into the doc pages too)
 *
 * Export Hooks and ConVars from Sections and Wrappers ! -> add IsGlobal var and extract them in the end
 * Remove @ in front of function's json extraction
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

            List<WrapperParam> wrapperList = new List<WrapperParam>(ProcessFileParsers(fileParsers));

            CleanupEmptyDataKeeper(wrapperList); // remove empty sections and wrappers that were created e.g. because of hooks, ConVars or local functions

            // Generate Folders
            string newDir = Directory.GetCurrentDirectory() + "../../../output";

            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            string jsonString = GenerateJSONSearchIndex(wrapperList);

            // Write JSON
            File.WriteAllText(newDir + "/jsonList.json", jsonString);
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

        private static WrapperParam[] ProcessFileParsers(List<FileParser> fileParsers)
        {
            Dictionary<string, WrapperParam> wrapperParamsDict = new Dictionary<string, WrapperParam>();

            foreach (FileParser fileParser in fileParsers)
            {
                foreach (WrapperParam wrapper in fileParser.WrapperList)
                {
                    // at first, we need to add the wrappers
                    bool wrapperExists = wrapperParamsDict.TryGetValue(wrapper.GetData(), out WrapperParam finalWrapper);

                    if (!wrapperExists)
                    {
                        wrapperParamsDict.Add(wrapper.GetData(), wrapper); // add this wrapper as main wrapper if not already exists

                        continue; // we don't need to do any other thing here because a new wrapper will come with it's sections automatically
                    }
                    else
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
                            finalWrapper.SectionList.Add(section); // add this section as new section into the wrappers section list
                        else // otherwise, the section already exists and need to copy all functions of the current section
                        {
                            foreach (DataStructure dataStructure in section.DataStructureList)
                            {
                                if (dataStructure.IsGlobal())
                                {
                                    // TODO add to global list

                                    continue;
                                }

                                finalSection.DataStructureList.Add(dataStructure);
                            }
                        }
                    }
                }
            }

            WrapperParam[] wrapperParams = new WrapperParam[wrapperParamsDict.Count];

            wrapperParamsDict.Values.CopyTo(wrapperParams, 0);

            return wrapperParams;
        }

        private static string GenerateJSONSearchIndex(List<WrapperParam> wrapperParams)
        {
            string json = "{";

            foreach (WrapperParam wrapper in wrapperParams)
            {
                json += wrapper.GetJSONData() + ",";
            }

            return json.Remove(json.Length - 1, 1) + "}"; // remove last ","
        }
	}
}

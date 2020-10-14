using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoDoc.DataStructures;
using NeoDoc.DataStructures.Lua;
using NeoDoc.Langs;
using NeoDoc.Params;

// Structure:
// 1. Wrapper, e.g. "module" or "class" // TODO make namespace (multiple) support
// 2. Section, e.g. "section"
// 3. Datastructures, e.g. "function"
// 4. Param, e.g. "desc" or "param"

namespace NeoDoc
{
	public class FileParser
	{
		private readonly LangMatcher langMatcher;
		private readonly Lang lang;
		private readonly string path;
		private readonly string relPath;
		private readonly ParamMatcher paramMatcher;

		public SortedDictionary<string, WrapperParam> WrapperDict { get; set; }

		private WrapperParam CurrentWrapper { get; set; }
		private SectionParam CurrentSection { get; set; }

		public string[] Lines { get; set; }

		public FileParser(LangMatcher langMatcher, Lang lang, string path, string relPath)
		{
			this.langMatcher = langMatcher;
			this.lang = lang;
			this.path = path;
			this.relPath = relPath;

			paramMatcher = new ParamMatcher(lang);

			Lines = File.ReadAllLines(path); // Load each line of the file in the buffer

			ModuleParam moduleParam = new ModuleParam(); // adds a new wrapper with default "none" data

			// initialize wrapper list
			WrapperDict = new SortedDictionary<string, WrapperParam>
			{
				{ moduleParam.WrapperName, moduleParam }
			};

			// initializes the current vars for easy and fast access
			CurrentWrapper = moduleParam;
			CurrentSection = CurrentWrapper.GetSectionNone();
		}

		public void CleanUp()
		{
			for (int i = 0; i < Lines.Length; i++)
			{
				Lines[i] = Lines[i].TrimStart(); // clear every space in front
			}
		}

		public void Process()
		{
			List<Param> paramsList = new List<Param>();

			for (int i = 0; i < Lines.Length; i++)
			{
				string line = Lines[i];

				if (string.IsNullOrEmpty(line)) // ignore empty lines
					continue;

				if (!paramMatcher.IsLineComment(line)) // if there is no comment 
				{
					// if there is something else than a comment. That means the doc comment section has end

					DataStructure dataStructure = langMatcher.GetDataStructureType(lang, line);

					if (dataStructure != null)
					{
						dataStructure.ParamsList = new List<Param>(paramsList); // set the params with a copy of the list
						dataStructure.ProcessDatastructure(line);

						if (!dataStructure.Ignore)
						{
							dataStructure = dataStructure.CheckDataStructureTransformation() ?? dataStructure;

							// set meta information
							dataStructure.FoundLine = i;
							dataStructure.FoundPath = relPath;

							dataStructure.Check();

							// now add the datastructure into the current section of the current container
							if (!CurrentSection.DataStructureDict.TryGetValue(dataStructure.GetName(), out List<DataStructure> dsList))
							{
								dsList = new List<DataStructure>();

								CurrentSection.DataStructureDict.Add(dataStructure.GetName(), dsList);
							}

							dsList.Add(dataStructure);
						}
					}

					// cleans the params list to be used for the next function or whatever, even if there is no dataStructure match
					paramsList.Clear();

					continue;
				}

				Param lineParam = paramMatcher.GetLineParam(line);

				if (lineParam == null) // if there is no line param
				{
					string foundLineParamString = paramMatcher.GetLineParamString(line);

					if (!string.IsNullOrEmpty(foundLineParamString)) // if there is a not registered param
					{
						NeoDoc.WriteErrors(new List<string>() {
							"UNREGISTERED PARAM: " + foundLineParamString,
							line,
							"Source: '" + relPath + "' (ll. " + i + ")"
						});

						continue;
					}

					int size = paramsList.Count;

					if (size > 0) // if there are params in the list
						lineParam = paramsList.ElementAt(size - 1); // use last param as new line param to support multiline commenting style e.g.

					if (paramMatcher.IsLineCommentStart(line))
					{
						// use already existing description if available, otherwise create a new one
						if (!(lineParam is DescParam))
							lineParam = new DescParam();

						paramsList.Add(lineParam); // start with a new description per default if matching e.g. "---"
					}

					if (lineParam != null)
						lineParam.ProcessAddition(paramMatcher.GetLineCommentData(line)); // add additional content
				}
				else
				{
					if (lineParam is WrapperParam || lineParam is SectionParam) // TODO rework in post-processing, otherwise defined e.g. "@author" wouldn't work!
					{
						if (lineParam is WrapperParam tmpWrapperParam) // TODO what if Wrapper already exists
						{
							tmpWrapperParam.ProcessParamsList(paramsList); // e.g. add @author to the wrapper's data

							if (!WrapperDict.TryGetValue(tmpWrapperParam.WrapperName, out WrapperParam foundWrapperParam))
							{
								foundWrapperParam = tmpWrapperParam;

								WrapperDict.Add(foundWrapperParam.WrapperName, foundWrapperParam); // adds the new wrapper into the list
							}
							else
							{
								foundWrapperParam.Merge(tmpWrapperParam);
							}

							CurrentWrapper = foundWrapperParam; // updates the new wrapper
							CurrentSection = CurrentWrapper.GetSectionNone(); // reset the section
						}
						else
						{
							SectionParam tmpSectionParam = (SectionParam)lineParam;

							if (!CurrentWrapper.SectionDict.TryGetValue(tmpSectionParam.SectionName, out SectionParam foundSectionParam))
							{
								foundSectionParam = tmpSectionParam;

								CurrentWrapper.SectionDict.Add(foundSectionParam.SectionName, foundSectionParam); // adds the new section into the list
							}
							else
							{
								foundSectionParam.Merge(tmpSectionParam);
							}

							CurrentSection = foundSectionParam; // update the section
						}

						// cleans the params list to be used for the next function or whatever, even if there is no dataStructure match
						paramsList.Clear();
					}
					else if (lineParam is FunctionParam) // match @functions and transform them into DataStructures // TODO rework in post-processing: Just if params contains FunctionParam, do it as a function in FunctionParam! (so @function don't have to be the last entry)
					{
						DataStructure dataStructure = new Function
						{
							ParamsList = new List<Param>(paramsList) // set the params with a copy of the list
						};

						dataStructure.ProcessDatastructure(line);

						if (!dataStructure.Ignore)
						{
							dataStructure = dataStructure.CheckDataStructureTransformation() ?? dataStructure;

							// set meta information
							dataStructure.FoundLine = i;
							dataStructure.FoundPath = relPath;

							dataStructure.Check();

							// now add the datastructure into the current section of the current container
							if (!CurrentSection.DataStructureDict.TryGetValue(dataStructure.GetName(), out List<DataStructure> dsList))
							{
								dsList = new List<DataStructure>();

								CurrentSection.DataStructureDict.Add(dataStructure.GetName(), dsList);
							}

							dsList.Add(dataStructure);
						}

						// cleans the params list to be used for the next function or whatever, even if there is no dataStructure match
						paramsList.Clear();
					}
					else
					{
						paramsList.Add(lineParam); // update the last param
					}
				}
			}
		}

		public string GetPath()
		{
			return path;
		}

		public string GetRelativePath()
		{
			return relPath;
		}

		public Lang GetLang()
		{
			return lang;
		}
	}
}

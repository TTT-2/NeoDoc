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
		private readonly ParamMatcher paramMatcher;

		internal readonly string relPath;
		internal int CurrentLineCount { get; set; }
		internal List<Param> paramsList { get; set; }

		public SortedDictionary<string, WrapperParam> WrapperDict { get; set; }

		internal WrapperParam CurrentWrapper { get; set; }
		internal SectionParam CurrentSection { get; set; }

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
			paramsList = new List<Param>();

			for (CurrentLineCount = 0; CurrentLineCount < Lines.Length; CurrentLineCount++)
			{
				string line = Lines[CurrentLineCount];

				if (string.IsNullOrEmpty(line)) // ignore empty lines
					continue;

				if (!paramMatcher.IsLineComment(line)) // if there is no comment but something else. That means the doc comment section has end
				{
					langMatcher.GetDataStructureType(lang, line)?.Initialize(this);

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
							"Source: '" + relPath + "' (ll. " + CurrentLineCount + ")"
						});
					}
					else
					{
						int size = paramsList.Count;

						if (size > 0) // if there are params in the list
							lineParam = paramsList.ElementAt(size - 1); // use last param as new line param to support multiline commenting style e.g.

						if (paramMatcher.IsLineCommentStart(line)) // if matching e.g. "---"
						{
							// use already existing description if available, otherwise create a new one
							if (!(lineParam is DescParam))
							{
								lineParam = new DescParam(); // start with a new description per default 

								paramsList.Add(lineParam);
							}
						}

						lineParam?.ProcessAddition(paramMatcher.GetLineCommentData(line)); // add additional content
					}
				}
				else
				{
					lineParam.ModifyFileParser(this); // e.g. function or wrapper params will clean params list if it already contains a wrapper / ...
				}
			}

			paramsList = null;
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

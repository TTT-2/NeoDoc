﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoDoc.DataStructures;
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
		internal int CurrentMatchedLines { get; set; }
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

				if (string.IsNullOrEmpty(line))
				{
					// There shouldn't be empty lines between docs and their datastructure
					// we therefore discard the params list here.
					paramsList.Clear();
					continue;
				}

				// if this line is not a comment it means the doc comment section has ended
				// and we should now be able to find a datastructure.
				if (!paramMatcher.IsLineComment(line))
				{
					line = line.TrimEnd();
					DataStructure dataStructure = langMatcher.GetDataStructureType(lang, line);
					int counter = 1;

					// Search the next lines incrementally for a datastructure and only stop once
					// we find one or when the next line would be empty or a comment
					while(dataStructure == null
							&& CurrentLineCount + counter < Lines.Length
							&& !string.IsNullOrEmpty(Lines[CurrentLineCount + counter])
							&& !paramMatcher.IsLineComment(Lines[CurrentLineCount + counter]))
					{
						line = line + Lines[CurrentLineCount + counter].TrimEnd();
						dataStructure = langMatcher.GetDataStructureType(lang, line);
						counter++;
					}

					if (dataStructure != null)
					{
						CurrentMatchedLines = counter;
						dataStructure.Initialize(this);
						paramsList.Clear();
						CurrentLineCount = CurrentLineCount + counter - 1;
					}

					continue;
				}

				Param lineParam = paramMatcher.GetLineParam(line);

				if (lineParam == null) // if there is no line param
				{
					string foundLineParamString = paramMatcher.GetLineParamString(line);

					if (paramMatcher.IsLineCommentStart(line)) // if matching e.g. "---"
					{
						// start with a new description by default.
						// HINT: That means that if using e.g. `---` instead of `--` while
						// documenting e.g. a param addition in a new line, this line will be
						// handled as a new description entry instead of a continued
						// param addition / param description.
						lineParam = new DescParam();

						paramsList.Add(lineParam);
					}
					else if (paramsList.Count > 0) // if there are params in the list
					{
						lineParam = paramsList.ElementAt(paramsList.Count - 1); // use last param as new line param to support multiline commenting style e.g.
					}

					lineParam?.ProcessAddition(paramMatcher.GetLineCommentData(line)); // add additional content
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

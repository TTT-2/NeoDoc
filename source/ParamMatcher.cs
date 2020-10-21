using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NeoDoc.Langs;
using NeoDoc.Params;

namespace NeoDoc
{
	public class ParamMatcher
	{
		private readonly Lang lang;
		private readonly SortedDictionary<string, Param> cachedParams;
		private readonly Regex commentStartRegex, commentRegex, paramRegex;

		public ParamMatcher(Lang lang)
		{
			this.lang = lang;

			cachedParams = GenerateParamsList();

			// e.g. check for "-- " or "////"
			string commentRegexStr = @"^\s*" + lang.GetCommentStyleRegex();

			commentStartRegex = new Regex(@"^\s*" + lang.GetCommentStartRegex()); // e.g. check for "---" or "///"
			commentRegex = new Regex(commentRegexStr);
			paramRegex = new Regex(commentRegexStr + @"\s*@\w+"); // e.g. check for "-- @param" or "//// @param"
		}

		private SortedDictionary<string, Param> GenerateParamsList()
		{
			// Get each class in the "NeoDoc.Params" namespace, that is not abstract
			IEnumerable<Type> q = from t in Assembly.GetExecutingAssembly().GetTypes()
				where t.IsClass && t.Namespace == "NeoDoc.Params" && !t.IsAbstract
				select t;

			SortedDictionary<string, Param> dict = new SortedDictionary<string, Param>();

			// now create the dict with <paramName, class>
			foreach (Type type in q.ToList())
			{
				Param param = (Param)Activator.CreateInstance(type);

				dict.Add(param.GetName().ToLower(), param);
			}

			return dict;
		}

		// returns the param class by it's file extension
		public Param GetByName(string name)
		{
			cachedParams.TryGetValue(name, out Param val);

			if (val == null)
				return null;

			return (Param)Activator.CreateInstance(val.GetType()); // return a new instance of this Param
		}

		// returns regex to check for comments, e.g. "---" or "///"
		public Regex GetCommentStartRegex()
		{
			return commentStartRegex;
		}

		// returns regex to check for comments, e.g. "-- " or "////"
		public Regex GetCommentRegex()
		{
			return commentRegex;
		}

		// returns the regex for matching params, e.g. "-- @param" or "//// @param"
		public Regex GetParamRegex()
		{
			return paramRegex;
		}

		// extracts the line's param string
		private string ExtractLineParam(string line, out string[] paramSettings, out string[] paramData)
		{
			paramSettings = null;
			paramData = new string[0];

			string param = line.TrimStart(lang.GetSingleCommentChar()).TrimStart();

			if (!param.StartsWith("@"))
			{
				return null;
			}

			string[] paramDataArr = param.Split(' ')[0].Split('['); // e.g. split the default param data from "-- @return[default=false] type text with [ etc." or "-- @note text with h[ .." too
			string extractedParam = paramDataArr[0].TrimStart('@').ToLower();

			int lastPos = param.IndexOf(' ');

			if (paramDataArr.Length > 1) // there are settings
			{
				paramSettings = NeoDoc.GetEntriesFromString(param, out lastPos).ToArray(); // get entries of the first list based on the first layer

				if (lastPos == 0)
					lastPos = param.IndexOf('[');
			}

			if (lastPos == -1)
				lastPos = 0;

			// paramData
			string[] tmpParamData = param.Substring(lastPos).TrimStart('@').TrimStart().Split(' ');

			// clean the param data
			paramData = new string[tmpParamData.Length];

			for (int i = 0; i < paramData.Length; i++)
			{
				paramData[i] = tmpParamData[i].Trim();
			}

			return extractedParam.Trim();
		}

		// returns whether the current string / line is a comment
		public bool IsLineCommentStart(string line)
		{
			Match match = GetCommentStartRegex().Match(line);

			return match.Success;
		}

		// returns whether the current string / line is a comment
		public bool IsLineComment(string line)
		{
			Match match = GetCommentRegex().Match(line);

			return match.Success;
		}

		// returns the line param as string
		public string GetLineParamString(string line)
		{
			Match match = GetParamRegex().Match(line);

			return match.Value;
		}

		// returns whether the current string / line contains param and returns the class
		public Param GetLineParam(string line)
		{
			// extract possible param from line
			string param = ExtractLineParam(line, out string[] paramSettings, out string[] paramData);

			if (param == null)
				return null;

			Param val = GetByName(param);

			if (val == null)
				return null;

			// let each val process the data on it's own (without the param prefix stuff)
			val.Process(paramData);

			if (paramSettings != null)
				val.ProcessSettings(paramSettings);

			return val;
		}

		// returns all the comment data, should used if there isn't a @param part inside so used as multiline 
		public string[] GetLineCommentData(string line)
		{
			// removes the e.g. "-- " or "//// " in front of the line
			return line.TrimStart(lang.GetSingleCommentChar()).TrimStart().Split(' '); // every line is by default trimmed (in the cleanup)
		}
	}
}

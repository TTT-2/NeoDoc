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
        private readonly Dictionary<string, Param> cachedParams;
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

        private Dictionary<string, Param> GenerateParamsList()
        {
            // Get each class in the "NeoDoc.Params" namespace, that is not abstract
            IEnumerable<Type> q = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.Namespace == "NeoDoc.Params" && !t.IsAbstract
                select t;

            Dictionary<string, Param> dict = new Dictionary<string, Param>();

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

            return val;
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
        private string ExtractLineParam(string line)
        {
            string param = line.TrimStart(lang.GetSingleCommentChar()).TrimStart();

            if (!param.StartsWith("@"))
            {
                return null;
            }

            string extractedParam = param.TrimStart('@').Split(' ')[0].ToLower();
            string[] paramDataArr = extractedParam.Split('['); // e.g. split the default param data from "-- @return[default=false]"

            if (paramDataArr.Length > 1)
                extractedParam = paramDataArr[0];

            return extractedParam;
        }

        // extracts the line's param string
        private string[] ExtractLineParamData(string line)
        {
            string param = line.TrimStart(lang.GetSingleCommentChar()).TrimStart();

            if (!param.StartsWith("@"))
            {
                return null;
            }

            // remove the param and get the other data
            string[] arr = param.TrimStart('@').Split(' ');
            string[] retArr = new string[arr.Length - 1];

            for (int x = 1; x < arr.Length; x++)
            {
                retArr[x - 1] = arr[x];
            }

            return retArr;
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
            string param = ExtractLineParam(line);

            if (param == null)
                return null;

            // search for the Param with the same name
            foreach (string name in cachedParams.Keys)
            {
                if (param == name)
                {
                    cachedParams.TryGetValue(name, out Param val);

                    // let each val process the data on it's own (without the param prefix stuff)
                    val.Process(ExtractLineParamData(line));

                    return val;
                }
            }

            return null;
        }

        // returns all the comment data, should used if there isn't a @param part inside so used as multiline 
        public string[] GetLineCommentData(string line)
        {
            // removes the e.g. "-- " or "//// " in front of the line
            return line.TrimStart(lang.GetSingleCommentChar()).TrimStart().Split(' ');
        }
    }
}

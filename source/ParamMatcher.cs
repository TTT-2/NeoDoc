using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeoDoc.Langs;
using NeoDoc.Params;

namespace NeoDoc
{
    public class ParamMatcher
    {
        private readonly Lang lang;
        private readonly Dictionary<string, Param> cachedParams;

        public ParamMatcher(Lang lang)
        {
            this.lang = lang;

            cachedParams = GenerateParamsList();
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

        // returns the lang class by it's file extension
        public Param GetByName(string name)
        {
            cachedParams.TryGetValue(name, out Param val);

            return val;
        }

        // returns the regex for matching params, e.g. "-- @param" or "//// @param"
        public string GetParamRegex()
        {
            return @"^\s*" + lang.GetCommentStyleRegex() + @"\s*@\w+";
        }

        // extracts the line's param string
        private string ExtractLineParam(string line)
        {
            string param = line.TrimStart(lang.GetSingleCommentChar()).TrimStart();

            if (!param.StartsWith("@"))
            {
                return null;
            }

            return param.TrimStart('@').Split(' ')[0];
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
    }
}

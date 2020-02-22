using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeoDoc.Langs;

namespace NeoDoc
{
    public class LangParser
    {
        private readonly Dictionary<string, Lang> cachedTypes;

        public LangParser()
        {
            cachedTypes = GenerateTypesList();
        }

        private Dictionary<string, Lang> GenerateTypesList()
        {
            // Get each class in the "NeoDoc.Langs" namespace, that is not abstract
            IEnumerable<Type> q = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.Namespace == "NeoDoc.Langs" && !t.IsAbstract
                select t;

            Dictionary<string, Lang> dict = new Dictionary<string, Lang>();

            // now create the dict with <fileExtension, class>
            foreach (Type type in q.ToList())
            {
                Lang lang = (Lang)Activator.CreateInstance(type);

                dict.Add(lang.GetFileExtension().ToLower(), lang);
            }

            return dict;
        }

        // returns the lang class by it's file extension
        public Lang GetByFileExtension(string ext)
        {
            cachedTypes.TryGetValue(ext, out Lang val);

            return val;
        }

        // returns the regex for matching params, e.g. "-- @param" or "//// @param"
        public string GetParamRegex(Lang lang)
        {
            return @"^\s*" + lang.GetCommentStyleRegex() + @"\s*@\w+";
        }
    }
}

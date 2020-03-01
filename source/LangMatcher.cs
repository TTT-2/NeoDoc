using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeoDoc.DataStructures;
using NeoDoc.Langs;

namespace NeoDoc
{
    public class LangMatcher
    {
        private readonly Dictionary<string, Lang> cachedTypes;
        private readonly Dictionary<Lang, DataStructure[]> documentationTypes;

        public LangMatcher()
        {
            cachedTypes = GenerateLangTypesList();

            // initializes the langs
            documentationTypes = GenerateStructureTypesList<DataStructure>(cachedTypes.Values.ToArray());
        }

        private IEnumerable<Type> GetTypesInNamespace(string nmspc)
        {
            // Get each class in the namespace, that is not abstract
            return from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.Namespace == nmspc && !t.IsAbstract
                select t;
        }

        private Dictionary<string, Lang> GenerateLangTypesList()
        {
            IEnumerable<Type> q = GetTypesInNamespace("NeoDoc.Langs");

            Dictionary<string, Lang> dict = new Dictionary<string, Lang>();

            // now create the dict with <fileExtension, class>
            foreach (Type type in q.ToList())
            {
                Lang lang = (Lang)Activator.CreateInstance(type);

                dict.Add(lang.GetFileExtension().ToLower(), lang);
            }

            return dict;
        }

        private Dictionary<Lang, T[]> GenerateStructureTypesList<T>(Lang[] langs)
        {
            Dictionary<Lang, T[]> langDict = new Dictionary<Lang, T[]>();

            foreach (Lang lang in langs)
            {
                List<T> dict = new List<T>();

                IEnumerable<Type> q = GetTypesInNamespace("NeoDoc.DataStructures." + lang.GetName());

                // now create the dict with <fileExtension, class>
                foreach (Type type in q.ToList())
                {
                    T instance = (T)Activator.CreateInstance(type);

                    dict.Add(instance);
                }

                langDict.Add(lang, dict.ToArray());
            }

            return langDict;
        }

        // returns the lang class by it's file extension
        public Lang GetByFileExtension(string ext)
        {
            cachedTypes.TryGetValue(ext, out Lang val);

            return val;
        }

        // returns the current matching data structure type in this lang
        public DataStructure GetDataStructureType(Lang lang, string line)
        {
            documentationTypes.TryGetValue(lang, out DataStructure[] dataStructures);

            foreach (DataStructure dataStructure in dataStructures)
            {
                if (dataStructure.Check(line))
                    return (DataStructure)Activator.CreateInstance(dataStructure.GetType()); // return a new instance of this dataStructure
            }

            return null;
        }
    }
}

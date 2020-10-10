using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;
using Newtonsoft.Json;

namespace NeoDoc.DataStructures
{
    public abstract class DataStructure
    {
        public bool Ignore { get; set; } = false;

        public Param[] ParamsList;
        public abstract Regex GetRegex(); // returns the exact RegEx to match e.g. the Function
        public abstract void Process(string line); // process data based on given line string
        public abstract bool Check(string line); // returns whether the current DocTarget is matched in this line
        public abstract string GetName(); // returns an identification name
        public abstract string GetDatastructureName(); // returns the individual name of the matched datasctructure
        public abstract object GetData(); // returns data

        public virtual string GetFullJSONData() // returns full json data, used for the entire page structure
        {
            Dictionary<string, object> jsonDict = new Dictionary<string, object>
            {
                { "type", "datastructure" },
                { "subtype", GetName() },
                { "name", GetDatastructureName() }
            };

            /* This is not needed because it can be builded based on the given params easily
            object jsonData = GetData();

            if (jsonData != null)
                jsonDict.Add("data", jsonData);
            */

            if (ParamsList != null && ParamsList.Length > 0)
            {
                // merge same datastructures together
                SortedDictionary<string, List<object>> paramsDict = new SortedDictionary<string, List<object>>();

                foreach (Param param in ParamsList)
                {
                    bool exists = paramsDict.TryGetValue(param.GetName(), out List<object> paramsJSONList);

                    if (!exists)
                    {
                        paramsJSONList = new List<object>();

                        paramsDict.Add(param.GetName(), paramsJSONList);
                    }

                    Dictionary<string, object> jsonData = param.GetJSONData();

                    if (jsonData.Count > 0)
                        paramsJSONList.Add(jsonData);
                }

                jsonDict.Add("params", paramsDict);
            }

            return JsonConvert.SerializeObject(jsonDict, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public virtual DataStructure CheckDataStructureTransformation() // checks whether the data structure should be transformed
        {
            return null;
        }

        public virtual bool IsGlobal() // whether the DataStructure should be excluded from the sections / wrappers data structuring
        {
            return false;
        }

        internal int CompareTo(DataStructure y)
        {
            return GetDatastructureName().CompareTo(y.GetDatastructureName());
        }

        internal List<string> GetVarsFromFunction(string result)
        {
            int bracketsDeepness = 0;
            string tmpString = "";

            List<string> varsList = new List<string>();

            foreach (char c in result)
            {
                switch (c)
                {
                    case '(':
                    case '{':
                        if (bracketsDeepness > 0) // if we are in the function hook
                            tmpString += c;

                        bracketsDeepness++;

                        break;
                    case ')':
                    case '}':
                        bracketsDeepness--;

                        if (bracketsDeepness > 0)
                            tmpString += c;

                        break;
                    case ',':
                        if (bracketsDeepness == 1) // if we are directly in the function hook's instance
                        {
                            if (tmpString != "")
                                varsList.Add(tmpString);

                            tmpString = "";
                        }
                        else
                        {
                            tmpString += c;
                        }

                        break;
                    default:
                        if (bracketsDeepness > 0) // if we are in the function hook
                            tmpString += c;

                        break;
                }

                if (bracketsDeepness < 0) // don't continue, even if the line hasn't ended yet
                    break;
            }

            if (tmpString != "")
                varsList.Add(tmpString);

            List<string> finalList = new List<string>();

            for (int i = 0; i < varsList.Count; i++)
            {
                finalList.Add(varsList[i].Trim());
            }

            return finalList;
        }
    }

    class DataStructureComparator : IComparer<DataStructure>
    {
        public int Compare(DataStructure x, DataStructure y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            // "CompareTo()" method 
            return x.CompareTo(y);
        }
    }
}

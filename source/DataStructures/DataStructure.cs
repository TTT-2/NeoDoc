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
        public abstract string GetData(); // returns data

        public virtual string GetJSONData() // returns json data
        {
            string data = GetData();

            if (data == null)
                return null;

            return JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public virtual string GetFullJSONData() // returns full json data, used for the entire page structure
        {
            Dictionary<string, object> jsonDict = new Dictionary<string, object>
            {
                { "type", "datastructure" },
                { "subtype", GetName() },
                { "name", GetDatastructureName() }
            };

            string jsonData = GetJSONData();

            if (jsonData != null)
                jsonDict.Add("data", jsonData);

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

                    paramsJSONList.Add(param.GetJSONData());
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

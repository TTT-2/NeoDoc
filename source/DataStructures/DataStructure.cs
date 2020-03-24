using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;
using Newtonsoft.Json;

namespace NeoDoc.DataStructures
{
    public abstract class DataStructure
    {
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

            return JsonConvert.SerializeObject(data);
        }

        public virtual string GetFullJSONData() // returns full json data, used for the entire page structure
        {
            string dsName = JsonConvert.SerializeObject(GetDatastructureName());

            if (dsName == null)
                dsName = "\"\"";

            string json = "{\"type\":\"datastructure\",\"subtype\":\"" + GetName() + "\",\"name\":" + dsName + ",\"params\":{";

            if (ParamsList != null && ParamsList.Length > 0)
            {
                // merge same datastructures together
                SortedDictionary<string, List<Param>> paramsDict = new SortedDictionary<string, List<Param>>();

                foreach (Param param in ParamsList)
                {
                    bool exists = paramsDict.TryGetValue(param.GetName(), out List<Param> paramsList);

                    if (!exists)
                    {
                        paramsList = new List<Param>();

                        paramsDict.Add(param.GetName(), paramsList);
                    }

                    paramsList.Add(param);
                }

                foreach (KeyValuePair<string, List<Param>> keyValuePair in paramsDict)
                {
                    json += JsonConvert.SerializeObject(keyValuePair.Key) + ":[";

                    foreach (Param param in keyValuePair.Value)
                    {
                        json += param.GetJSON() + ",";
                    }

                    json = json.Remove(json.Length - 1, 1) + "],";
                }

                json = (paramsDict.Count > 0) ? json.Remove(json.Length - 1, 1) : json;
            }

            json += "}";

            string jsonData = GetJSONData();

            if (jsonData != null)
                json += ",\"data\":[" + jsonData + "]";

            return json + "}";
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

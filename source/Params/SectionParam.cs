using System.Collections.Generic;
using NeoDoc.DataStructures;
using Newtonsoft.Json;

namespace NeoDoc.Params
{
    public class SectionParam : Param
    {
        public string SectionName { get; set; } = "none";
        public SortedDictionary<string, List<DataStructure>> DataStructureDict;

        public SectionParam()
        {
            DataStructureDict = new SortedDictionary<string, List<DataStructure>>();
        }

        public override string GetData()
        {
            return SectionName;
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            SectionName = paramData[0];
        }

        public override void ProcessAddition(string[] paramData)
        {
            // nothing to process
        }

        public override string GetName()
        {
            return "section";
        }

        public string GetJSONData()
        {
            string json = JsonConvert.SerializeObject(GetData()) + ":{";


            // loop through datastructure types
            foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in DataStructureDict)
            {
                json += JsonConvert.SerializeObject(keyValuePair.Key + "s") + ":["; // data structures

                bool entry = false;

                foreach (DataStructure dataStructure in keyValuePair.Value)
                {
                    string tmpJSON = JsonConvert.SerializeObject(dataStructure.GetDatastructureName());

                    if (tmpJSON == null)
                        continue;

                    json += tmpJSON + ",";

                    entry = true;
                }

                json = entry ? json.Remove(json.Length - 1, 1) : json;

                json += "],";
            }

            return (DataStructureDict.Count > 0 ? json.Remove(json.Length - 1, 1) : json) + "}"; // close dataStructures and section and remove last ","
        }
    }
}

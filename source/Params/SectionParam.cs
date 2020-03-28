using System.Collections.Generic;
using System.Linq;
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

        public override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "name", SectionName }
            };
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

        public Dictionary<string, object> GetDataDict()
        {
            Dictionary<string, object> jsonDict = new Dictionary<string, object>();

            foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in DataStructureDict)
            {
                List<string> dsNames = new List<string>();

                foreach (DataStructure dataStructure in keyValuePair.Value)
                {
                    if (dataStructure.Ignore)
                        continue;

                    dsNames.Add(dataStructure.GetDatastructureName());
                }

                jsonDict.Add(keyValuePair.Key, dsNames);
            }

            return jsonDict;
        }

        public void Merge(SectionParam sectionParam)
        {
            foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in sectionParam.DataStructureDict)
            {
                foreach (DataStructure dataStructure in keyValuePair.Value)
                {
                    // just insert if not already exists
                    bool match = DataStructureDict.TryGetValue(keyValuePair.Key, out List<DataStructure> finalDSList);

                    if (!match) // initialize if not already exists
                    {
                        finalDSList = new List<DataStructure>();

                        DataStructureDict.Add(keyValuePair.Key, finalDSList);
                    }

                    bool alreadyExists = false;

                    foreach (DataStructure tmpDs in finalDSList)
                    {
                        if (tmpDs.GetDatastructureName() == dataStructure.GetDatastructureName())
                        {
                            alreadyExists = true;

                            break;
                        }
                    }

                    if (!alreadyExists)
                        finalDSList.Add(dataStructure);
                }
            }
        }

        public void ProcessGlobals(SortedDictionary<string, List<DataStructure>> globalsDict)
        {
            SortedDictionary<string, List<DataStructure>> finalDict = new SortedDictionary<string, List<DataStructure>>();

            foreach (KeyValuePair<string, List<DataStructure>> keyValuePair in DataStructureDict)
            {
                List<DataStructure> finalDSList = new List<DataStructure>();

                foreach (DataStructure dataStructure in keyValuePair.Value)
                {
                    if (!dataStructure.IsGlobal())
                    {
                        finalDSList.Add(dataStructure);

                        continue;
                    }

                    bool exists = globalsDict.TryGetValue(dataStructure.GetName(), out List<DataStructure> ds);

                    if (!exists)
                    {
                        ds = new List<DataStructure>();

                        globalsDict.Add(dataStructure.GetName(), ds);
                    }

                    // just insert if not already exists
                    bool alreadyExists = false;

                    foreach (DataStructure tmpDs in ds)
                    {
                        if (tmpDs.GetDatastructureName() == dataStructure.GetDatastructureName())
                        {
                            alreadyExists = true;

                            break;
                        }
                    }

                    if (!alreadyExists)
                        ds.Add(dataStructure);
                }

                if (finalDSList.Count > 0)
                    finalDict.Add(keyValuePair.Key, finalDSList);
            }

            DataStructureDict = finalDict;
        }
    }
}

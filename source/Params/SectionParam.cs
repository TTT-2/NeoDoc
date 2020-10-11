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
                List<Dictionary<string, string>> dsNames = new List<Dictionary<string, string>>();

                foreach (DataStructure dataStructure in keyValuePair.Value)
                {
                    if (dataStructure.Ignore)
                        continue;

                    dsNames.Add(new Dictionary<string, string>()
                    {
                        { "name", dataStructure.GetDatastructureName() },
                        { "realm", dataStructure.Realm }
                    });
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

        public void ProcessGlobals(SortedDictionary<string, Dictionary<string, List<DataStructure>>> globalsDict)
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

                    bool exists = globalsDict.TryGetValue(dataStructure.GetName(), out Dictionary<string, List<DataStructure>> dsList);

                    if (!exists)
                    {
                        dsList = new Dictionary<string, List<DataStructure>>();

                        globalsDict.Add(dataStructure.GetName(), dsList);
                    }

                    // just insert if not already exists
                    bool dsExists = dsList.TryGetValue(dataStructure.GlobalWrapper, out List<DataStructure> dsWrapperList);

                    if (!dsExists)
                    {
                        dsWrapperList = new List<DataStructure>();

                        dsList.Add(dataStructure.GlobalWrapper, dsWrapperList);
                    }

                    bool alreadyExists = false;

                    foreach (DataStructure entry in dsWrapperList)
                    {
                        if (entry.GetDatastructureName() == dataStructure.GetDatastructureName())
                        {
                            alreadyExists = true;

                            break;
                        }
                    }

                    if (alreadyExists)
                        continue;

                    dsWrapperList.Add(dataStructure);
                }

                if (finalDSList.Count > 0)
                    finalDict.Add(keyValuePair.Key, finalDSList);
            }

            DataStructureDict = finalDict;
        }
    }
}

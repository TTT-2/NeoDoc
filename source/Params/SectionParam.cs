using System.Collections.Generic;
using NeoDoc.DataStructures;

namespace NeoDoc.Params
{
    public class SectionParam : Param
    {
        public string SectionName { get; set; } = "none";
        public List<DataStructure> DataStructureList;

        public SectionParam()
        {
            DataStructureList = new List<DataStructure>();
        }

        public override string GetData()
        {
            return SectionName;
        }

        public override string GetOutput()
        {
            return ""; // nothing to output
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
            string json = "\"" + GetData() + "\":{";

            // data structures
            json += "\"dataStructures\":[";

            bool entry = false;

            foreach (DataStructure dataStructure in DataStructureList)
            {
                if (dataStructure.IsGlobal())
                    continue;

                string tmpJSON = dataStructure.GetJSONData();

                if (tmpJSON == null)
                    continue;

                json += tmpJSON + ",";

                entry = true;
            }

            return (entry ? json.Remove(json.Length - 1, 1) : json) + "]}"; // close dataStructures and section and remove last ","
        }
    }
}

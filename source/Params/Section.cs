using System.Collections.Generic;
using NeoDoc.DataStructures;

namespace NeoDoc.Params
{
    public class Section : Param
    {
        public string SectionName { get; set; } = "none";
        public List<DataStructure> DataStructureList;

        public Section()
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
    }
}

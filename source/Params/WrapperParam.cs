using System.Collections.Generic;

namespace NeoDoc.Params
{
    public abstract class WrapperParam : Param
    {
        public string WrapperName { get; set; } = "none";
        public List<SectionParam> SectionList { get; set; }

        public WrapperParam()
        {
            SectionList = new List<SectionParam>
            {
                new SectionParam() // adds a new section with default "none" data
            };
        }

        public override string GetData()
        {
            return WrapperName;
        }

        public override string GetOutput()
        {
            return ""; // nothing to output
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            WrapperName = paramData[0];
        }

        public override void ProcessAddition(string[] paramData)
        {
            // nothing to process
        }
    }
}

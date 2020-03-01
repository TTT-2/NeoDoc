using System.Collections.Generic;

namespace NeoDoc.Params
{
    public abstract class Wrapper : Param
    {
        public string WrapperName { get; set; } = "none";
        public List<Section> SectionList { get; set; }

        public Wrapper()
        {
            SectionList = new List<Section>
            {
                new Section() // adds a new section with default "none" data
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

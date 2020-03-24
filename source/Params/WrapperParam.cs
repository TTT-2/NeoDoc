using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeoDoc.Params
{
    public abstract class WrapperParam : Param
    {
        public List<string> Authors { get; set; }

        public string WrapperName { get; set; } = "none";
        public List<SectionParam> SectionList { get; set; }

        public WrapperParam()
        {
            SectionList = new List<SectionParam>
            {
                new SectionParam() // adds a new section with default "none" data
            };

            Authors = new List<string>();
        }

        public override string GetData()
        {
            return WrapperName;
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

        public void ProcessParamsList(List<Param> paramsList)
        {
            foreach (Param param in paramsList)
            {
                if (param is AuthorParam)
                {
                    Authors.Add(param.GetData());
                }
            }
        }

        public void MergeData(WrapperParam wrapper)
        {
            foreach (string author in wrapper.Authors)
            {
                if (Authors.Contains(author))
                    continue;

                Authors.Add(author);
            }
        }

        public string GetJSONData()
        {
            string json = JsonConvert.SerializeObject(GetData()) + ":{";

            // add auhtors
            if (Authors.Count > 0)
                json += "\"authors\":{" + JsonConvert.SerializeObject(string.Join("\",", Authors)) + "},";

            // sections
            json += "\"sections\":{";

            foreach (SectionParam section in SectionList)
            {
                json += section.GetJSONData() + ",";
            }

            return json.Remove(json.Length - 1, 1) + "}}"; // close sections and wrapper and remove last ","
        }
    }
}

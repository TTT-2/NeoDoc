using System.Collections.Generic;

namespace NeoDoc.Params
{
    public abstract class TypeTextParam : Param
    {
        public string[] Typs { get; set; }
        public string Description { get; set; } = "";

        public override Dictionary<string, object> GetData()
        {
            Dictionary<string, object> tmpDict = new Dictionary<string, object>
            {
                { "typs", Typs }
            };

            if (!string.IsNullOrEmpty(Description))
                tmpDict.Add("description", Description);

            return tmpDict;
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            Typs = paramData[0].Split('|');

            if (paramData.Length < 2)
                return;

            for (int i = 1; i < paramData.Length; i++)
            {
                Description += paramData[i] + " ";
            }

            Description = Description.Trim();
        }

        public override void ProcessAddition(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            if (!string.IsNullOrEmpty(Description))
            {
                Description += " ";
            }

            Description += string.Join(" ", paramData);
        }
    }
}

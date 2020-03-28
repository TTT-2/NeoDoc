using System.Collections.Generic;

namespace NeoDoc.Params
{
    public abstract class StateParam : Param
    {
        public string Value { get; set; } = "";

        public override Dictionary<string, object> GetData()
        {
            Dictionary<string, object> tmpDict = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(Value))
                tmpDict.Add("value", Value);

            return tmpDict;
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            Value = paramData[0];
        }

        public override void ProcessAddition(string[] paramData)
        {
            Process(paramData);
        }
    }
}

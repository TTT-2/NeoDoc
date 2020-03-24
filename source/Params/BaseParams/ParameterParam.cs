using Newtonsoft.Json;

namespace NeoDoc.Params
{
    public abstract class ParameterParam : Param
    {
        public string Name { get; set; }
        public string Typ { get; set; }
        public string Description { get; set; } = "";

        public override string GetData()
        {
            return Name + " - " + Typ + " - " + Description;
        }

        public override string GetJSON()
        {
            string json = "{";

            json += "\"name\":" + JsonConvert.SerializeObject(Name) + ",";
            json += "\"type\":" + JsonConvert.SerializeObject(Typ) + ",";
            json += "\"description\":" + JsonConvert.SerializeObject(Description);

            if (Default != null && Default != "")
                json += ",\"default\":" + JsonConvert.SerializeObject(Default);

            if (Optional)
                json += ",\"optional\":\"true\"";

            return json + "}";
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            Typ = paramData[0];

            if (paramData.Length < 2)
                return;

            Name = paramData[1];

            if (paramData.Length < 3)
                return;

            for (int i = 2; i < paramData.Length; i++)
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

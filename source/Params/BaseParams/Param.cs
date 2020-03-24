using Newtonsoft.Json;

namespace NeoDoc.Params
{
    public abstract class Param
    {
        public string Default { get; set; }
        public bool Optional { get; set; } = false;
        public bool OptionalChain { get; set; } = false;

        public abstract string GetName(); // returns the name of the param
        public abstract void Process(string[] paramData); // paramData = everything except the @param prefix part
        public abstract void ProcessAddition(string[] paramData); // paramData = everything of the line that already hadn't a param part
        public abstract string GetData(); // returns the data that should get returned, e.g. Name, Text, etc.

        public virtual string GetJSON() // returns the json output used for the website
        {
            string json = "{";

            json += "\"data\":" + JsonConvert.SerializeObject(GetData());

            if (Default != null && Default != "")
                json += ",\"default\":" + JsonConvert.SerializeObject(Default);

            if (Optional)
                json += ",\"optional\":\"true\"";

            if (OptionalChain)
                json += ",\"optionalchain\":\"true\"";

            return json + "}";
        }

        public virtual void ProcessSettings(string[] paramSettings) // paramData = everything except the @param prefix part
        {
            if (paramSettings.Length < 1)
                return;

            foreach (string settingConstruct in paramSettings)
            {
                string[] settingSplit = settingConstruct.Trim().Split('='); // split e.g. "default=true"
                string settingStart = settingSplit[0].Trim();

                if (settingStart == "default")
                {
                    Default = settingSplit[1].Trim();
                }
                else if (settingStart == "opt")
                {
                    Optional = true;
                }
                else if (settingStart == "optchain")
                {
                    OptionalChain = true;
                }
            }
        }
    }
}

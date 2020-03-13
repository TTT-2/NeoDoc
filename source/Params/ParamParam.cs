namespace NeoDoc.Params
{
    public class ParamParam : Param
    {
        public string Name { get; set; }
        public string Typ { get; set; }
        public string Description { get; set; } = "";

        public override string GetData()
        {
            return Name + " - " + Typ + " - " + Description;
        }

        public override string GetName()
        {
            return "param";
        }

        public override string GetOutput()
        {
            return GetData();
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            Name = paramData[0];

            if (paramData.Length < 2)
                return;

            Typ = paramData[1];

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

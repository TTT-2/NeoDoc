namespace NeoDoc.Params
{
    public class RealmParam : Param
    {
        public string Name { get; set; } = "";

        public override string GetData()
        {
            return Name;
        }

        public override string GetName()
        {
            return "realm";
        }

        public override string GetOutput()
        {
            return Name;
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            Name = paramData[0];
        }

        public override void ProcessAddition(string[] paramData)
        {
            Process(paramData);
        }
    }
}

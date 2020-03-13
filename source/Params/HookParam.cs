namespace NeoDoc.Params
{
    public class HookParam : Param
    {
        public override string GetData()
        {
            return "";
        }

        public override string GetName()
        {
            return "hook";
        }

        public override string GetOutput()
        {
            return ""; // nothing to output
        }

        public override void Process(string[] paramData)
        {
            // nothing to process
        }

        public override void ProcessAddition(string[] paramData)
        {
            // nothing to process
        }
    }
}

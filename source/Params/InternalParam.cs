namespace NeoDoc.Params
{
    public class InternalParam : Param
    {
        public override string GetData()
        {
            return "";
        }

        public override string GetName()
        {
            return "internal";
        }

        public override string GetOutput()
        {
            return "INTERNAL"; // nothing to output
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

namespace NeoDoc.Params
{
    public class TwoDParam : Param
    {
        public override string GetData()
        {
            return "";
        }

        public override string GetName()
        {
            return "2D";
        }

        public override string GetOutput()
        {
            return "This is a 2D method"; // nothing to output
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

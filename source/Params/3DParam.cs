namespace NeoDoc.Params
{
    public class ThreeDParam : Param
    {
        public override string GetData()
        {
            return "";
        }

        public override string GetName()
        {
            return "3D";
        }

        public override string GetOutput()
        {
            return "This is a 3D method"; // nothing to output
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

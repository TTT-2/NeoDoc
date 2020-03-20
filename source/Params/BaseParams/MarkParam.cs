namespace NeoDoc.Params
{
    public abstract class MarkParam : Param
    {
        public override string GetData()
        {
            return "";
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

namespace NeoDoc.Params
{
    public class InternalParam : MarkParam
    {
        public override string GetName()
        {
            return "internal";
        }

        public override string GetOutput()
        {
            return "INTERNAL"; // nothing to output
        }
    }
}

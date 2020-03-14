namespace NeoDoc.Params
{
    public class TwoDParam : MarkParam
    {
        public override string GetName()
        {
            return "2D";
        }

        public override string GetOutput()
        {
            return "This is a 2D method"; // nothing to output
        }
    }
}

namespace NeoDoc.Params
{
    public class DeprecatedParam : MarkParam
    {
        public override string GetName()
        {
            return "deprecated";
        }

        public override string GetOutput()
        {
            return "DEPRECATED!"; // nothing to output
        }
    }
}

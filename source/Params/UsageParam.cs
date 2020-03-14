namespace NeoDoc.Params
{
    public class UsageParam : TextParam
    {
        public override string GetName()
        {
            return "usage";
        }

        public override string GetOutput()
        {
            return Text;
        }
    }
}

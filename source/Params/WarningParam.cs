namespace NeoDoc.Params
{
    public class WarningParam : TextParam
    {
        public override string GetName()
        {
            return "warning";
        }

        public override string GetOutput()
        {
            return "Warning: " + Text;
        }
    }
}

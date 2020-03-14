namespace NeoDoc.Params
{
    public class ImportantParam : TextParam
    {
        public override string GetName()
        {
            return "Important";
        }

        public override string GetOutput()
        {
            return "Important: " + Text;
        }
    }
}

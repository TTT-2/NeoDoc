namespace NeoDoc.Params
{
    public class SeeParam : TextParam
    {
        public override string GetName()
        {
            return "see";
        }

        public override string GetOutput()
        {
            return Text;
        }
    }
}

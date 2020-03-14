namespace NeoDoc.Params
{
    public class TodoParam : TextParam
    {
        public override string GetName()
        {
            return "todo";
        }

        public override string GetOutput()
        {
            return "TODO: " + Text;
        }
    }
}

namespace NeoDoc.Params
{
    public class ReturnParam : ParameterParam
    {
        public override string GetData()
        {
            return Name + " - " + Typ + " - " + Description;
        }

        public override string GetName()
        {
            return "return";
        }

        public override string GetOutput()
        {
            return GetData();
        }
    }
}

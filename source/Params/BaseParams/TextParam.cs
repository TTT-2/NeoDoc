namespace NeoDoc.Params
{
    public abstract class TextParam : Param
    {
        public string Text { get; set; } = "";

        public override string GetData()
        {
            return Text;
        }

        public override string GetOutput()
        {
            return Text;
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            Text = string.Join(" ", paramData);
        }

        public override void ProcessAddition(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            if (!string.IsNullOrEmpty(Text))
            {
                Text += " ";
            }

            Text += string.Join(" ", paramData);
        }
    }
}

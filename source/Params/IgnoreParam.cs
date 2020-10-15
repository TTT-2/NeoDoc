namespace NeoDoc.Params
{
	public class IgnoreParam : MarkParam
	{
		public override string GetName()
		{
			return "ignore";
		}

		public override void ModifyFileParser(FileParser fileParser)
		{
			if (fileParser.CurrentLineCount == 0)
			{
				fileParser.CurrentLineCount = fileParser.Lines.Length; // ignore the whole line if the @ignore is in the first line of the file

				return;
			}

			base.ModifyFileParser(fileParser);
		}
	}
}
